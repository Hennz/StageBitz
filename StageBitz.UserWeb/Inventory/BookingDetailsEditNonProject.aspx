<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="BookingDetailsEditNonProject.aspx.cs" Inherits="StageBitz.UserWeb.Inventory.BookingDetailsEditNonProject" %>
<%@ Register Src="~/Controls/Inventory/EditBookingDetails.ascx" TagName="EditBookingDetails"
    TagPrefix="sb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="<%# ResolveUrl("../Common/Scripts/BookingDetails/BookingDetails.js?v="+ this.ApplicationVersionString) %>"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    <asp:PlaceHolder ID="plcHeaderLinks" runat="server"> | <a id="lnkCompanyInventory"
        runat="server">Company Inventory</a> | <asp:HyperLink ID="hyperLinkMyBooking" runat="server">My Bookings</asp:HyperLink> 
        | <asp:HyperLink ID="hyperLinkInventorySharing" runat="server">Manage Inventory</asp:HyperLink>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <sb:EditBookingDetails ID="editBookingDetails" runat="server" />
</asp:Content>
