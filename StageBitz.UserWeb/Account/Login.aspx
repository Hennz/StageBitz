<%@ Page Title="Welcome to StageBitz!" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="StageBitz.UserWeb.Account.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">

        function signInClick() {

            if (!Page_ClientValidate("LoginValidation")) {

                //Hide the login error box
                $(".message").hide();

                return false;
            }

            return true;
        }

        function CleanErrorMsg() {
            $('#<%= errormsg.ClientID %>').hide();
        }

        function CleanResetPasswordEmailText() {
            $('#<%= txtEmailAddress.ClientID %>').val("");
        }

        function CleanValidators() {
            //HIde validators
            for (i = 0; i < Page_Validators.length; i++) {
                Page_Validators[i].style.display = 'none';
            }

            return false;
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%--<script type="text/javascript">
        $(function () {
            var cookies = document.cookie.split(";");
            for (var i = 0; i < cookies.length; i++) {
                var cookie = cookies[i];
                var eqPos = cookie.indexOf("=");
                var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
                document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
            }
        })
    </script>--%>
    <div id="loginContainer">
        <div>
            <h1>Welcome to StageBitz&#8482;</h1>
            <h3>Project & Inventory Management for Props, Scenery and all those other Bitz.</h3>
        </div>
        <div>
            <%--    <div class="left" style="width: 530px;">
                <img src="~/Common/Images/home_main_image.png" alt="home" width="530" height="120"
                    runat="server" />
            </div>--%>
            <div id="divInvitation" runat="server" visible="false" class="notices" style="width: 500px; margin-bottom: 5px; margin-left: 140px;">
                Please sign in or register to accept your invitation.
            </div>
            <div id="loginBox">
                <h2>Sign In</h2>
                <p style="margin-bottom: 20px;">
                    If you do not have a StageBitz&#8482; registered account, it's easy to
                    <asp:HyperLink ID="lnkRegister" NavigateUrl="~/Account/Register.aspx" runat="server">get one now</asp:HyperLink>.
                </p>
                <asp:UpdatePanel ID="upnlLogin" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <div style="min-height: 105px;">
                            <asp:Label ID="lblUsername" AssociatedControlID="txtUsername" runat="server" Text="Email Address"></asp:Label>
                            <asp:TextBox ID="txtUsername" MaxLength="50" ValidationGroup="LoginValidation" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqUsername" ControlToValidate="txtUsername" ValidationGroup="LoginValidation"
                                runat="server" ErrorMessage="Enter your email address."></asp:RequiredFieldValidator>
                            <br />
                            <asp:Label ID="lblPassword" AssociatedControlID="txtPassword" runat="server" Text="Password"></asp:Label>
                            <asp:TextBox ID="txtPassword" MaxLength="50" ValidationGroup="LoginValidation" TextMode="Password"
                                runat="server"></asp:TextBox>
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
                            Username and/or password is not recognised. Please try again.
                        </div>
                        <div id="divPendingActivation" runat="server" visible="false" class="message">
                            Your account is not activated yet. The activation email was sent to
                            <asp:Literal ID="pendingActivationEmail" runat="server"></asp:Literal>. If you did
                            not receive this email,
                            <asp:LinkButton ID="linkSendEmail" Text="click here" runat="server" OnClick="linkSendEmail_Click"></asp:LinkButton>
                            to resend.
                        </div>
                        <div id="divActivationMailSentPrimaryEmailChange" runat="server" visible="false"
                            class="message">
                            Activation email has been sent to&nbsp;<asp:Literal ID="litPrimaryEmailSent" runat="server"></asp:Literal>.
                            You will need to click on the link in that email to confirm your new address.
                        </div>
                        <div id="divActivationMailSent" runat="server" visible="false" class="message">
                            Activation email has been sent to&nbsp;<asp:Literal ID="litSucessActivationMail"
                                runat="server"></asp:Literal>. You will need to click on the link in that email
                            to activate your account before you log on.
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <a href="#" id="forgotPasswordLink" onclick="showPopup('popupForgotPassword');CleanErrorMsg();CleanValidators();CleanResetPasswordEmailText(); return false;">Forgot your password?</a>
                <asp:UpdatePanel ID="upnlForgotPassWord" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <sb:PopupBox ID="popupForgotPassword" Height="200" Title="Forgot your password?"
                            runat="server">
                            <BodyContent>
                                <div style="text-align: center; width: 300px;">
                                    <p>
                                        To reset your password, type the full email address you use to sign in to your StageBitz™
                                        account.
                                    </p>
                                    <br />
                                    Email Address: &nbsp;<asp:TextBox ID="txtEmailAddress" MaxLength="50" Width="170"
                                        onchange="CleanErrorMsg();" runat="server"></asp:TextBox>
                                    <br />
                                </div>
                                <div style="padding-left: 109px; min-height: 15px; width: 170px;">
                                    <asp:RegularExpressionValidator ID="RegExEmail" runat="server" ErrorMessage="Invalid email address."
                                        ValidationGroup="ResetPasswordValidation" ControlToValidate="txtEmailAddress"
                                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                    <asp:RequiredFieldValidator ID="RqdEmail" runat="server" ControlToValidate="txtEmailAddress"
                                        ValidationGroup="ResetPasswordValidation" ErrorMessage="Email is required."></asp:RequiredFieldValidator>
                                    <span id="errormsg" class="inputError" runat="server"></span>
                                </div>
                            </BodyContent>
                            <BottomStripeContent>
                                <asp:Button ID="btnReset" runat="server" CssClass="buttonStyle" OnClick="btnReset_Click"
                                    ValidationGroup="ResetPasswordValidation" OnClientClick="CleanErrorMsg();" Text="Reset" />
                                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                            </BottomStripeContent>
                        </sb:PopupBox>
                        <sb:PopupBox ID="popupResetSucess" Title="Password Reset" runat="server">
                            <BodyContent>
                                <div style="text-align: center; width: 300px;">
                                    <p>
                                        StageBitz&#8482; has sent you an email to confirm
                                        <br />
                                        password reset.
                                    </p>
                                </div>
                            </BodyContent>
                            <BottomStripeContent>
                                <asp:Button ID="Button2" runat="server" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                            </BottomStripeContent>
                        </sb:PopupBox>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <br style="clear: both;" />
        </div>
    </div>
</asp:Content>
