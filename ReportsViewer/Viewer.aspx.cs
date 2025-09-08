using CrystalDecisions.CrystalReports.Engine;
using System;

namespace ReportViewer
{
    public partial class Viewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReport();
            }
        }

        private void LoadReport()
        {
            try
            {
                // Simple - just like your WinForms version
                ReportDocument report = new ReportDocument();
                report.Load(Server.MapPath("~/Reportss/Gyne+ecc clinics.rpt"));
                CrystalReportViewer1.ReportSource = report;
            }
            catch (Exception ex)
            {
                Response.Write("Error: " + ex.Message);
            }
        }
    }
}