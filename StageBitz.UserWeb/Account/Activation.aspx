<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Activation.aspx.cs" Inherits="StageBitz.UserWeb.Account.Actication" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h1 style="padding-bottom: 3px">
    </h1>
    <div id="divActivationSucess" runat="server">
        <h1>
            Your StageBitz™ account has been activated</h1>
        <div class="divSingleColumnL notices">
            Thank you! Your user account <strong>
                <asp:Literal ID="litSuccessEmail" runat="server"></asp:Literal></strong> is
            now ready for use. Please use your email address and password to<label>
            </label>
            <a id="linkSuccess" runat="server">logon now</a>.</div>        
    </div>
    <div id="divAlreadyActivated" runat="server">
        <h1>
            Your StageBitz™ account has already been activated</h1>
        <div class="divSingleColumnL notices">
            You have already activated your StageBitz™ account for
            <asp:Literal ID="userEmail" runat="server"></asp:Literal>. Please use your email
            address and password to <a id="linkAlreadyActivated" runat="server">logon now</a>.</div>
    </div>
    <div id="divInvalidLink" runat="server">
        <h1>
            Your StageBitz™ account activation failed</h1>
        <div class="divSingleColumnL notices">
            <p>Invalid activation link.</p>
            <br />
            <a id="linkInvalidLink" runat="server">Return to StageBitz home</a></div>
    </div>
</asp:Content>
