<%@ Page DisplayTitle="Reset Password" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ResetUserPassword.aspx.cs" Inherits="StageBitz.UserWeb.Account.ResetUserPassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div id="divLoginName" style="font-weight:bold;padding-bottom:5px;" runat="server">
    </div>
    <asp:UpdatePanel UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <div id="panel" class="tabPage" style="width: 880px; border: 1px solid #E6E2EB;"
                runat="server">
                <div>
                    <table>
                        <tr>
                            <td>
                                New Password:
                            </td>
                            <td>
                                <asp:TextBox ID="txtNewPassword" MaxLength="50" Width="210" TextMode="Password" runat="server"></asp:TextBox>
                            </td>
                            <td style="padding-top: 5px;">
                                &nbsp<asp:RequiredFieldValidator ID="RqdPassword" runat="server" ControlToValidate="txtNewPassword"
                                    ValidationGroup="FieldsValidation" ErrorMessage="Password is required."></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="revNewPW0" runat="server" ControlToValidate="txtNewPassword"
                                    ValidationGroup="FieldsValidation" ErrorMessage="Password must be at least 6 characters long."
                                    ValidationExpression="^.{6,}$"></asp:RegularExpressionValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Confirm Password:&nbsp
                            </td>
                            <td>
                                <asp:TextBox ID="txtConfirmPassword" MaxLength="50" TextMode="Password" Width="210"
                                    runat="server"></asp:TextBox>
                            </td>
                            <td style="padding-top: 5px;">
                                &nbsp<asp:RequiredFieldValidator ID="RqdConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                    ValidationGroup="FieldsValidation" ErrorMessage="Password confirmation is required."></asp:RequiredFieldValidator>
                                <asp:CompareValidator ID="cmpPassWord" ErrorMessage="Please ensure passwords match."
                                    ValidationGroup="FieldsValidation" ControlToCompare="txtNewPassword" ControlToValidate="txtConfirmPassword"
                                    runat="server"></asp:CompareValidator>
                            </td>
                        </tr>
                    </table>
                </div>
                <div style="width: 333px; height: 35px;">
                    <asp:Button ID="btnResetPassword" CssClass="buttonStyle" runat="server" ValidationGroup="FieldsValidation"
                        Text="Reset Password" OnClick="btnResetPassword_Click" />
                </div>
            </div>
            <div id="divNotifications" runat="server" visible="false" class="divSingleColumnL notices">
        <span id="errormsg" runat="server">It seems that confirmation link you clicked to reset
            your StageBitz™ password is invalid. We are sorry for the inconvenience. <a id="linkHomePage"
                href="~/Account/Login.aspx" runat="server">Please try to reset the password again</a></span>
    </div>
    <div id="divSucess" runat="server" visible="false" class="divSingleColumnL notices">
        Password reset successfully. Please use your email address and password to <a id="A1"
            href="~/Account/Login.aspx" runat="server">log on now</a>.
    </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    
</asp:Content>
