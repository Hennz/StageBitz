<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="StageBitz.AdminWeb.Account.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

    <script type="text/javascript">

        function signInClick() {

            if (!Page_ClientValidate()) {

                //Hide the login error box
                $(".message").hide();

                return false;
            }

            return true;
        }
    
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div style="width:290px; margin:0px auto;">
        <h2>Admin Portal - Sign In</h2>
        <asp:UpdatePanel ID="upnlLogin" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div style="margin:20px 0px; width:600px;" class="sideErrorContainer">
                    <asp:Label ID="lblUsername" AssociatedControlID="txtUsername" runat="server" Text="Email Address" style="display:inline-block; width:100px;"></asp:Label>
                    <asp:TextBox ID="txtUsername" Width="180" MaxLength="50" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="reqUsername" ControlToValidate="txtUsername" ValidationGroup="LoginValidation"
                        runat="server" ErrorMessage="Enter your email address."></asp:RequiredFieldValidator>
                    <br />
                    <asp:Label ID="lblPassword" AssociatedControlID="txtPassword" runat="server" Text="Password" style="display:inline-block; width:100px;"></asp:Label>
                    <asp:TextBox ID="txtPassword" Width="180" MaxLength="50" TextMode="Password" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="reqPassword" ControlToValidate="txtPassword" ValidationGroup="LoginValidation"
                        runat="server" ErrorMessage="Enter your password."></asp:RequiredFieldValidator>
                </div>
                <span>
                    <asp:Label ID="lblRememberMe" AssociatedControlID="chkRememberMe" runat="server"
                        Text="Keep me signed in"></asp:Label>
                    <asp:CheckBox ID="chkRememberMe" runat="server" />
                </span>
                <asp:Button ID="btnSignIn" runat="server" CssClass="buttonStyle" ValidationGroup="LoginValidation"
                    OnClientClick="return signInClick();" OnClick="btnSignIn_Click" Text="Sign In" />
                <div id="divInvalidLogin" runat="server" visible="false" class="message">
                    Username and/or password is not recognised. Please try again.</div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>
