<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Viewer.aspx.cs" Inherits="ReportViewer.Viewer" %>
<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web"
    TagPrefix="CR" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Crystal Report Viewer</title>
</head>
<body>
    <form id="form1" runat="server">
        <CR:CrystalReportViewer 
            ID="CrystalReportViewer1" 
            runat="server"
            AutoDataBind="true"
            EnableParameterPrompt="true"
            EnableDatabaseLogonPrompt="false"
            Width="100%" 
            Height="600px" />
    </form>
</body>
</html>
