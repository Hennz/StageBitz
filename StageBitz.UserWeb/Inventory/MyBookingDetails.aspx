<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="MyBookingDetails.aspx.cs" Inherits="StageBitz.UserWeb.Inventory.MyBookingDetails" %>

<%@ Register Src="~/Controls/Inventory/BookingDetails.ascx" TagName="BookingDetails"
    TagPrefix="sb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    <asp:PlaceHolder ID="plcHeaderLinks" runat="server">| <a id="lnkCompanyInventory"
        runat="server">Company Inventory</a> | <asp:HyperLink ID="hyperLinkMyBooking" runat="server">My Bookings</asp:HyperLink> <span runat="server" id="spnInventorySharing">|<asp:HyperLink ID="hyperLinkInventorySharing" runat="server">Manage Inventory</asp:HyperLink></span>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <sb:BookingDetails ID="bookingDetails" runat="server" />
</asp:Content>
