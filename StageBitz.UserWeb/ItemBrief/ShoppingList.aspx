<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="ShoppingList.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.ShoppingList" %>

<%@ Register Src="~/Controls/ItemBrief/ItemBriefTasks.ascx" TagName="ItemBriefTasks"
    TagPrefix="uc1" %>
<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectId %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectId %>', 2]);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<asp:HyperLink ID="hyperLinkTaskManager" runat="server">Task Manager</asp:HyperLink>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>

    <div style="height: 405px;">
        <sb:ExportData ID="ExportData1" runat="server" />
        <uc1:ItemBriefTasks ID="taskList" DisplayMode="TaskListView" runat="server" />
        <asp:Button ID="btnDone" runat="server" Text="Done" CssClass="buttonStyle" OnClick="btnDone_Click" />
        &nbsp;
        <asp:Button ID="btnDeleteList" OnClientClick="showPopup('popupShoppingListConfirmation'); return false;"
            runat="server" Text="Delete List" CssClass="buttonStyle" />
    </div>
    <div>
        <sb:PopupBox ID="popupShoppingListConfirmation" Title="Delete List Confirmation"
            Height="100" runat="server">
            <BodyContent>
                <div style="white-space: nowrap;">
                    Are you sure you want to delete this list?
                </div>
            </BodyContent>
            <BottomStripeContent>
                <input type="button" class="popupBoxCloser buttonStyle" value="No" />
                <asp:Button ID="btnRemoveConfirm" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnRemoveConfirm_Click"
                    runat="server" Text="Yes" />
            </BottomStripeContent>
        </sb:PopupBox>
    </div>
</asp:Content>
