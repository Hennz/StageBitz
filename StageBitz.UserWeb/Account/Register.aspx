<%@ Page Title="Welcome to StageBitz!" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="StageBitz.UserWeb.Account.Register" %>

<%@ Register src="~/Controls/Common/CountryList.ascx" tagname="CountryList" tagprefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">

        function ClearError() {
            $('#<%= errormsg.ClientID %>').hide();
        }
    
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upd" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <div style="margin-left: 25px;padding-bottom:4px;">
                <h3>
                    Create your StageBitz ID</h3>
                Already have a StageBitz login? <a id="lnkLogin" href="~/Account/Login.aspx" runat="server">
                    Sign in here</a>
            </div>
            <div class="BlueBoxesS" style="width: 800px;">
                <div style="width: 430px; float: left;">
                    <div style="padding-bottom: 2px;" class="blueText">
                        Required Information:</div>
                    <table style="width: 90%;">
                        <tbody>
                            <tr>
                                <td style="width: 200px;" class="labelField">
                                    First Name:
                                </td>
                                <td style="width: 250px;">
                                    <asp:TextBox runat="server" ID="txtFirstName" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RqdFirstName" runat="server" ControlToValidate="txtFirstName" ValidationGroup="FieldsValidation"
                                        ErrorMessage="First Name is required."></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Last Name:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtLastName" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RqdLastName" runat="server" ControlToValidate="txtLastName" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Last Name is required."></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Email Address:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtEmail" onkeypress="ClearError()" MaxLength="50"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegExEmail" runat="server" ErrorMessage="Invalid email address." ValidationGroup="FieldsValidation"
                                        ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                    <asp:RequiredFieldValidator ID="RqdEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Email is required."></asp:RequiredFieldValidator>
                                    <span id="errormsg" class="inputError" runat="server" visible="false"></span>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Confirm Email:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtConfirmEmail" MaxLength="50"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegExConfirmEmail" runat="server" ErrorMessage="Invalid email address." ValidationGroup="FieldsValidation"
                                        ControlToValidate="txtConfirmEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                    <asp:RequiredFieldValidator ID="RqdConfirmEmail" runat="server" ControlToValidate="txtConfirmEmail" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Email confirmation is required."></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="cmpEmail" ErrorMessage="Please ensure emails match." ControlToCompare="txtEmail" ValidationGroup="FieldsValidation"
                                        ControlToValidate="txtConfirmEmail" runat="server"></asp:CompareValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Password:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" TextMode="Password" ID="txtPassWord" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RqdPassword" runat="server" ControlToValidate="txtPassword" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Password is required."></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="revNewPW0" runat="server" ControlToValidate="txtPassWord" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Password must be at least 6 characters long." ValidationExpression="^.{6,}$"></asp:RegularExpressionValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Confirm Password:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" TextMode="Password" ID="txtConfirmPassword" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RqdConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ValidationGroup="FieldsValidation"
                                        ErrorMessage="Password confirmation is required."></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="cmpPassWord" ErrorMessage="Please ensure passwords match." ValidationGroup="FieldsValidation"
                                        ControlToCompare="txtPassWord" ControlToValidate="txtConfirmPassword" runat="server"></asp:CompareValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField">
                                    Country:
                                </td>
                                <td>
                                    <uc1:CountryList ID="ucCountryList" DropDownWidth="250" ValidatorPossitionMode="Left" runat="server" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="width: 320px; float: left;">
                    <div class="blueText">
                        Optional Information:
                    </div>
                    <table>
                        <tbody>
                            <tr>
                                <td colspan="2" style="width: 50%">
                                    When you are working on a project with other users in StageBitz, they will be able
                                    to see your name and your role on that project. If you'd like them to be able to
                                    access other contact information, please include it here. This information will not
                                    be visible to any one who is not working on a project with you.
                                </td>
                            </tr>
                            <tr>
                            <td colspan="2">
                            &nbsp;
                            </td>
                            </tr>
                            <tr>
                                <td class="labelField" style="width: 25%">
                                    Phone 1:
                                </td>
                                <td style="width: 25%; text-align: left">
                                    <asp:TextBox ID="txtPhone1" runat="server" Width="200" MaxLength="50"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="labelField" style="width: 25%">
                                    Phone 2:
                                </td>
                                <td style="width: 25%; text-align: left;">
                                    <asp:TextBox ID="txtPhone2" runat="server" Width="200" MaxLength="50"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:CheckBox ID="chkEmailVisibletoAll" runat="server" Text="Also make my email address visible to others on my Projects." />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="text-align: right">
                                    <asp:Button ID="btnSubmit" OnClick="RegisterUser" ValidationGroup="FieldsValidation" CssClass="buttonStyle" runat="server" Text="Start Using StageBitz" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="clear: both;">
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
