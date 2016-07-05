<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EmailChange.aspx.cs" Inherits="StageBitz.UserWeb.Public.EmailChange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h1 style="padding-bottom: 3px">
    </h1>
    <div id="divEmailChanged" runat="server">
        <h1>
            Your StageBitz™ account's primary email changed successfully.</h1>
        <div class="divSingleColumnL notices">
            Your primary email has changed successfully. Please use new email
            address to <a id="lnkEmailChanged" runat="server">logon now</a>.
        </div>
    </div>
    <div id="divEmailChangeFailed" runat="server">
        <h1>
            Your StageBitz™ account's primary email change failed.</h1>
        <div class="divSingleColumnL notices">
            <p>You have already clicked the link to change your primary email or the link is invalid.</p>
            <br />
            <a id="lnkEmailChangeFailed" runat="server">Return to StageBitz home</a></div>
    </div>
</asp:Content>
