<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="RegisterSuccess.aspx.cs" Inherits="StageBitz.UserWeb.Account.RegisterSuccess" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div style="padding-top:25px;">
        <h1>
            StageBitz™ account successfully created.</h1>
        <div class="message success">
            You have successfully created a StageBitz™ user account and an activation email
            has been sent to your email address. You will need to click on the link in that
            email to activate your account before you log on. If you don't receive the email,
            please check in your junk mail folder. <a class="messageSucessLink" href="~/Account/Login.aspx" runat="server">(Return to logon)</a>
        </div>
    </div>
</asp:Content>
