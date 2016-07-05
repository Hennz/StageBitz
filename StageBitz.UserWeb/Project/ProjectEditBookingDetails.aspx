<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="ProjectEditBookingDetails.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectEditBookingDetails" %>
<%@ Register Src="~/Controls/Inventory/EditBookingDetails.ascx" TagName="EditBookingDetails" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="<%# ResolveUrl("../Common/Scripts/BookingDetails/BookingDetails.js?v="+ this.ApplicationVersionString) %>"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<a id="linkTaskManager" runat="server">Task Manager</a>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <sb:EditBookingDetails ID="editBookingDetails" runat="server" />
</asp:Content>
