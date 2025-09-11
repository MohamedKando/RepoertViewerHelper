using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using System;
using System.Configuration;
using System.IO;

namespace ReportViewer
{
    public partial class Viewer : System.Web.UI.Page
    {
        private ReportDocument currentReport;

        protected void Page_Init(object sender, EventArgs e)
        {
            // Reapply connection when Crystal navigates (after parameters submitted)
            CrystalReportViewer1.Navigate += CrystalReportViewer1_Navigate;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string rptFileName = Request.QueryString["path"];

                if (!string.IsNullOrEmpty(rptFileName))
                {
                    LoadReport(rptFileName);
                }
                else
                {
                    Response.Write("Error: No report path provided.<br/>");
                }
            }
            else
            {
                // Restore the report on postback
                RestoreReportFromSession();
            }
        }

        private void LoadReport(string rptFileName)
        {
            try
            {
                string reportPath;

                // Allow relative path (recommended) or absolute path (if needed)
                if (Path.IsPathRooted(rptFileName))
                    reportPath = rptFileName;
                else
                    reportPath = Server.MapPath("~/Reportss/" + rptFileName);

                if (!File.Exists(reportPath))
                    throw new FileNotFoundException($"Report file not found: {reportPath}");

                Response.Write($"Loading report from: {reportPath}<br/>");

                currentReport = new ReportDocument();
                currentReport.Load(reportPath);

                ConfigureOracleConnection(currentReport);

                CrystalReportViewer1.ReportSource = currentReport;
                CrystalReportViewer1.EnableDatabaseLogonPrompt = false;
                CrystalReportViewer1.EnableParameterPrompt = true;
                CrystalReportViewer1.HasRefreshButton = true;
                CrystalReportViewer1.HasSearchButton = false;
                CrystalReportViewer1.DisplayGroupTree = false;

                Session["CurrentReport"] = currentReport;

                Response.Write("Report loaded and connection applied<br/>");
            }
            catch (Exception ex)
            {
                Response.Write($"LoadReport Error: {ex.Message}<br/>");
                if (ex.InnerException != null)
                    Response.Write($"Inner Exception: {ex.InnerException.Message}<br/>");
            }
        }

        private void RestoreReportFromSession()
        {
            try
            {
                if (Session["CurrentReport"] is ReportDocument restoredReport)
                {
                    currentReport = restoredReport;
                    ConfigureOracleConnection(currentReport); // 🔑 REAPPLY CONNECTION
                    CrystalReportViewer1.ReportSource = currentReport;

                    Response.Write("Report restored from session and connection reapplied<br/>");
                }
                else
                {
                    Response.Write("No report found in session — reloading<br/>");
                    string rptFileName = Request.QueryString["path"];
                    if (!string.IsNullOrEmpty(rptFileName))
                        LoadReport(rptFileName);
                }
            }
            catch (Exception ex)
            {
                Response.Write($"Session restore error: {ex.Message}<br/>");
            }
        }

        private void ConfigureOracleConnection(ReportDocument reportDocument)
        {
            string serverName = ConfigurationManager.AppSettings["ServerName"];
            string userName = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];

            try
            {
                Response.Write($"Applying DB connection to {serverName} as {userName}<br/>");

                reportDocument.SetDatabaseLogon(userName, password, serverName, "");

                foreach (CrystalDecisions.CrystalReports.Engine.Table table in reportDocument.Database.Tables)
                {
                    TableLogOnInfo logonInfo = table.LogOnInfo;
                    logonInfo.ConnectionInfo = new ConnectionInfo
                    {
                        ServerName = serverName,
                        DatabaseName = "",
                        UserID = userName,
                        Password = password,
                        IntegratedSecurity = false
                    };
                    table.ApplyLogOnInfo(logonInfo);
                }

                // Handle subreports
                foreach (ReportDocument subreport in reportDocument.Subreports)
                {
                    subreport.SetDatabaseLogon(userName, password, serverName, "");

                    foreach (CrystalDecisions.CrystalReports.Engine.Table table in subreport.Database.Tables)
                    {
                        TableLogOnInfo logonInfo = table.LogOnInfo;
                        logonInfo.ConnectionInfo = new ConnectionInfo
                        {
                            ServerName = serverName,
                            DatabaseName = "",
                            UserID = userName,
                            Password = password,
                            IntegratedSecurity = false
                        };
                        table.ApplyLogOnInfo(logonInfo);
                    }
                }

                reportDocument.VerifyDatabase();
                Response.Write("Database connection verified<br/>");
            }
            catch (Exception ex)
            {
                Response.Write($"Database connection error: {ex.Message}<br/>");
                if (ex.InnerException != null)
                    Response.Write($"Inner exception: {ex.InnerException.Message}<br/>");
                throw;
            }
        }

        private void CrystalReportViewer1_Navigate(object source, NavigateEventArgs e)
        {
            if (currentReport != null)
            {
                Response.Write("Navigate event fired — reapplying DB connection<br/>");
                ConfigureOracleConnection(currentReport);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            try
            {
                if (Session["CurrentReport"] is ReportDocument report)
                {
                    report.Close();
                    report.Dispose();
                    Session.Remove("CurrentReport");
                }

                if (currentReport != null)
                {
                    currentReport.Close();
                    currentReport.Dispose();
                    currentReport = null;
                }

                CrystalReportViewer1.ReportSource = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup error: {ex.Message}");
            }

            base.OnUnload(e);
        }
    }
}
