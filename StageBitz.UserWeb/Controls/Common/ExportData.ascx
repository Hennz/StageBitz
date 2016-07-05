<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExportData.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.ExportData" %>
<asp:UpdatePanel runat="server" ID="upnlExport">
    <ContentTemplate>
        <div id="divPrintExport" runat="server" style="text-align: right; margin-right: -5px;">
            <div style="float: right;">
                <asp:Button ID="btnCreatePDF" runat="server" Text="Create PDF" ToolTip="Click to export as PDF"
                    CssClass="buttonStyle" Style="float: none;" OnClick="btnCreatePDF_Click" />
            </div>
            <div style="display: inline-block; margin-right: -2px;">
                <asp:Button ID="btnSendToExcel" runat="server" Text="Send to Excel" ToolTip="Click to export as Excel"
                    CssClass="buttonStyle" Style="float: none;" OnClick="btnSendToExcel_Click" />
            </div>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnSendToExcel" />
        <asp:PostBackTrigger ControlID="btnCreatePDF" />
    </Triggers>
</asp:UpdatePanel>

