<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="StageBitz.AdminWeb.Public.Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

<h1>Uh oh!</h1>

<p>It would appear that StageBitz Admin portal has encountered an error. Our engineers have been notified and will
investigate the issue. In the meantime please <a href="~/Default.aspx" runat="server">return to the home page</a>.
Thanks for your patience.</p>

</asp:Content>
