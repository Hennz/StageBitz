<%@ Page Title="" Language="C#" MasterPageFile="~/Content.Master" AutoEventWireup="true"
    CodeBehind="ProjectSchedule.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectSchedule" %>

<%@ Register Src="~/Controls/Project/ProjectEvents.ascx" TagName="Event" TagPrefix="uc1" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectID %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectID %>', 2]);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<a id="linkTaskManager" runat="server">Task Manager</a>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <telerik:RadWindowManager ID="mgr" runat="server"></telerik:RadWindowManager>

    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>

    <strong>Edit Project Start and End Dates :</strong>
    <%--    <asp:UpdatePanel ID="upnl" runat="server">
        <ContentTemplate>--%>
    <div style="height: 420px; margin-top: 5px;">
        <uc1:Event ID="ucProjectEvents" EventsGridHeight="240" runat="server" />
    </div>
    <div class="buttonArea">
        &nbsp;
        <asp:Button ID="btnDone" runat="server" OnClick="SaveSchedule" Text="Done" ValidationGroup="valgroup" />
        <asp:Button ID="btnCancel" CausesValidation="false" runat="server" Text="Cancel" />
    </div>
    <%--</ContentTemplate>
    </asp:UpdatePanel>--%>
</asp:Content>
