<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="ProjectDashboard.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectDashboard" %>

<%@ Register Src="~/Controls/Project/ProjectSchedules.ascx" TagName="projectSchedules"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Project/ProjectItemBrief.ascx" TagName="ProjectItemBrief"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Project/ProjectAdministration.ascx" TagName="ProjectAdministration"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay"
    TagPrefix="sb" %>
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
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a> | <a id="lnkBookings" runat="server">Bookings</a> |<a id="linkTaskManager"
        runat="server">Task Manager</a> |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <asp:PlaceHolder ID="plcProjectContent" runat="server">
        <div style="text-align: right;">
            <a id="lnkProjectDetails" runat="server">Project Details</a>
        </div>
        <div style="margin-top: 20px;">
            <uc:projectSchedules ID="ucprojectSchedules" runat="server" />
        </div>
        <div>
            <uc:ProjectItemBrief ID="ucprojectItemBrief" runat="server" />
        </div>
        <uc:ProjectAdministration ID="ucProjectAdministration" runat="server" />
    </asp:PlaceHolder>
</asp:Content>
