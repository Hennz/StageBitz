<%@ Page DisplayTitle="User Details" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="UserDetails.aspx.cs" Inherits="StageBitz.UserWeb.Personal.UserDetails" %>

<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/CountryList.ascx" TagName="CountryList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Personal/UserProjects.ascx" TagName="UserProjects" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Personal/UserSkills.ascx" TagName="UserSkills" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Personal/UserEmailNotifications.ascx" TagPrefix="sb" TagName="UserEmailNotifications" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">

        function OnClientTabSelected(sender, eventArgs) {
            var tab = eventArgs.get_tab();

            if (tab.get_value() == "ChangePassword") {
                //Clear password textboxes
                $("#<%= txtCurrentPassword.ClientID %>").val("");
                $("#<%= txtNewPassword.ClientID %>").val("");
                $("#<%= txtConfirmPassword.ClientID %>").val("");
            }
        }

        function ClearErrorMessages() {
            $("#<%=divEmailRequestError.ClientID %>").hide();
        }
    
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div style="margin: 10px 0px 15px 0px;">
        <div class="left">
            <asp:UpdatePanel ID="upnlThumbnail" UpdateMode="Conditional" runat="server">
                <ContentTemplate>
                    <sb:ImageDisplay ID="profileImage" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="left" style="margin-left: 10px; max-width: 350px; overflow: hidden;">
            <div style="line-height: 20px; margin-bottom: 5px;">
                <asp:UpdatePanel ID="upnlBasicDetails" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <strong>
                            <asp:Literal ID="ltrlName" runat="server"></asp:Literal></strong><br />
                        <asp:Literal ID="ltrlHeadingPosition" runat="server"></asp:Literal>
                        <asp:Literal ID="ltrlHeadingCompany" runat="server"></asp:Literal>
                        <asp:Literal ID="ltrlMembershipPeriod" runat="server"></asp:Literal><br />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <asp:UpdatePanel ID="upnlImageUpload" UpdateMode="Conditional" runat="server">
                <ContentTemplate>
                    <div style="display: inline-block;">
                        <sb:FileUpload ID="fileUpload" Title="Change Image" DisplayLauncherAsLink="true"
                            OnFileUploaded="fileUpload_FileUploaded" runat="server" />
                    </div>
                    <div id="divRemoveImage" runat="server" style="display: inline-block;">
                        | <a class="smallText" href="#" onclick="showPopup('popupImageRemoveConfirmation'); return false;"
                            title="Remove profile image">Remove</a>
                    </div>
                    <sb:PopupBox ID="popupImageRemoveConfirmation" Title="Remove Profile Image" Height="100"
                        runat="server">
                        <BodyContent>
                            <div style="white-space: nowrap;">
                                Are you sure you want to remove the profile image?
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <input type="button" class="popupBoxCloser buttonStyle" value="No" />
                            <asp:Button ID="btnRemoveImageConfirm" OnClick="btnRemoveImageConfirm_Click" CssClass="buttonStyle"
                                runat="server" Text="Yes" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="divPrivacyNotice" runat="server" class="right highlightBox dirtyValidationArea"
            style="width: 430px; padding: 5px;">
            <strong>Please note:</strong><br />
            Any information included in your Personal Details will be visible to the team members
            for each of your Projects. If you don't want to share anything, leave the fields
            blank and make sure the checkbox below is unticked.
            <asp:CheckBox ID="chkEmailVisible" Style="margin-top: 10px; display: block;" Text="Make my Primary Email visible to others on my Projects"
                runat="server" />
        </div>
        <div style="clear: both;">
        </div>
    </div>
    <asp:UpdatePanel ID="upnlTabs" runat="server">
        <ContentTemplate>
            <telerik:RadTabStrip ID="profileTabStrip" Width="920px" MultiPageID="profileMultiPage"
                OnClientTabSelected="OnClientTabSelected" runat="server" SelectedIndex="0">
                <Tabs>
                    <telerik:RadTab runat="server" Text="Personal Details" Value="PersonalDetails" Selected="True">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Notifications" Value="Notifications">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="StageBitz Projects" Value="Projects">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Job History" Value="JobHistory">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Portfolio" Value="Portfolio">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Change Password" Value="ChangePassword">
                    </telerik:RadTab>
                </Tabs>
            </telerik:RadTabStrip>
            <div class="tabPage" style="width: 880px;">
                <telerik:RadMultiPage ID="profileMultiPage" runat="server" SelectedIndex="0">
                    <telerik:RadPageView ID="PersonalDetailTab" runat="server">
                        <asp:Panel ID="pnlPersonalDetails" runat="server">
                            <div id="divPersonalDetailsEditableLabels" runat="server" class="left" style="line-height: 30px;
                                margin-right: 10px; width: 100px; white-space: nowrap;">
                                First Name:<br />
                                Last Name:<br />
                                Position:<br />
                                Company:<br />
                                Primary Email:<br />
                                Secondary Email:<br />
                                Phone 1:<br />
                                Phone 2:<br />
                                Address Line 1:<br />
                                Address Line 2:<br />
                                City:<br />
                                State:<br />
                                Postal/Zip code:<br />
                                Country:<br />
                            </div>
                            <div id="divPersonalDetailsReadOnlyLabels" runat="server" class="left" style="line-height: 30px;
                                margin-right: 10px; width: 120px;">
                                First Name:<br />
                                Last Name:<br />
                                Position:<br />
                                Company:<br />
                                <asp:PlaceHolder ID="plcEmailLabels" runat="server">Primary Email:<br />
                                </asp:PlaceHolder>
                                Secondary Email:<br />
                                Phone 1:<br />
                                Phone 2:<br />
                                Address:<br />
                            </div>
                            <div id="divPersonalDetailsEditable" runat="server" class="left dirtyValidationArea sideErrorContainer"
                                style="line-height: 30px; width: 310px;">
                                <asp:Panel ID="pnlPersonalDetailsEdit" DefaultButton="btnDummy" runat="server">
                                    <asp:TextBox ID="txtFirstName" MaxLength="50" Width="210" runat="server"></asp:TextBox>
                                    <asp:RequiredFieldValidator ControlToValidate="txtFirstName" runat="server" ErrorMessage="* required"
                                        ValidationGroup="personalDetailsFields"></asp:RequiredFieldValidator><br />
                                    <asp:TextBox ID="txtLastName" MaxLength="50" Width="210" runat="server"></asp:TextBox>
                                    <asp:RequiredFieldValidator ControlToValidate="txtLastName" runat="server" ErrorMessage="* required"
                                        ValidationGroup="personalDetailsFields"></asp:RequiredFieldValidator><br />
                                    <asp:TextBox ID="txtPosition" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtCompany" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:Literal ID="ltrlEmail1" runat="server"></asp:Literal>
                                    <asp:LinkButton ID="lbtnChangePrimaryEmail" runat="server" Text="Change" OnClick="lbtnChangePrimaryEmail_Click"></asp:LinkButton>
                                    <div style="position: relative; display: inline; top: 5px;">
                                        <asp:ImageButton runat="server" ID="iBtnEmailAlreadySent" ImageUrl="~/Common/Images/msginfo.png"
                                            Visible="false" OnClick="iBtnEmailAlreadySent_Click" /></div>
                                    <br />
                                    <asp:TextBox ID="txtEmail2" MaxLength="50" Width="210" runat="server"></asp:TextBox>
                                    <asp:RegularExpressionValidator runat="server" ErrorMessage="* invalid" ValidationGroup="personalDetailsFields"
                                        ControlToValidate="txtEmail2" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator><br />
                                    <asp:TextBox ID="txtPhone1" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtPhone2" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtAddressLine1" MaxLength="100" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtAddressLine2" MaxLength="100" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtCity" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtState" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <asp:TextBox ID="txtPostCode" MaxLength="50" Width="210" runat="server"></asp:TextBox><br />
                                    <sb:CountryList ID="countryList" DropDownWidth="210" runat="server" ValidationGroup="personalDetailsFields" />
                                    <asp:Button ID="btnDummy" runat="server" Style="display: none;" OnClientClick="return false;"
                                        Text="Button" />
                                    <asp:Literal ID="Literal1" runat="server"></asp:Literal>
                                </asp:Panel>
                            </div>
                            <sb:PopupBox ID="popupEditPrimaryEmail" runat="server" Width="400">
                                <BodyContent>
                                    <div style="display: table">
                                        <div class="left" style="line-height: 30px;">
                                            Current Email:<br />
                                            New Email:
                                            <br />
                                        </div>
                                        <div class="left" style="padding-left: 5px">
                                            <asp:TextBox ID="txtCurrentEmail" runat="server" ReadOnly="true" Enabled="false"
                                                ValidationGroup="PrimaryEmailChange" MaxLength="50" Width="210"></asp:TextBox><br />
                                            <asp:TextBox ID="txtNewEmail" MaxLength="50" Width="210" runat="server" ValidationGroup="PrimaryEmailChange"></asp:TextBox><br />
                                        </div>
                                    </div>
                                    <div style="color: Gray; width: 400px; line-height: 20px;">
                                        Once you click OK, an activation email will be sent to your new email address. You
                                        will need to click on the link in the email to confirm your new address.
                                    </div>
                                    <div class="message error" id="divEmailRequestError" runat="server" visible="false">
                                        <asp:Literal runat="server" ID="ltrlEmailRequestError"></asp:Literal>
                                    </div>
                                    <div>
                                        <asp:RequiredFieldValidator ID="rfvNewEmail" runat="server" SkinID="Hidden" ValidationGroup="PrimaryEmailChange"
                                            ControlToValidate="txtNewEmail" ErrorMessage="Please enter new primary email address."></asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="revNewEmail" runat="server" ErrorMessage="Invalid email Address."
                                            SkinID="Hidden" ValidationGroup="PrimaryEmailChange" ControlToValidate="txtNewEmail"
                                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                        <asp:CompareValidator ID="cvEmail" ControlToCompare="txtCurrentEmail" ControlToValidate="txtNewEmail"
                                            runat="server" ErrorMessage="Please enter a different email address." Operator="NotEqual"
                                            ValidationGroup="PrimaryEmailChange" SkinID="Hidden"></asp:CompareValidator>
                                        <asp:ValidationSummary ID="valSumPrimaryEmailChange" ValidationGroup="PrimaryEmailChange"
                                            DisplayMode="SingleParagraph" runat="server" CssClass="message error" />
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnSavePrimaryEmail" class="buttonStyle" Text="OK" runat="server"
                                        OnClick="btnSavePrimaryEmail_Click" ValidationGroup="PrimaryEmailChange" OnClientClick="ClearErrorMessages();" />
                                    <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupResendEmail" runat="server" Width="400">
                                <BodyContent>
                                    <asp:Literal runat="server" ID="ltrlResendEmail"></asp:Literal>
                                    <div class="message error" id="divResendError" runat="server" visible="false">
                                        <asp:Literal runat="server" ID="ltrlResendError"></asp:Literal>
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <input type="button" class="buttonStyle popupBoxCloser" value="Close" />
                                    <asp:Button ID="btnResendEmail" class="buttonStyle" Text="Resend Verification Email"
                                        runat="server" OnClick="btnResendEmail_Click" Style="float: left;" />
                                    <asp:Button ID="btnCancelRequest" class="buttonStyle" Text="Cancel Request" runat="server"
                                        OnClick="btnCancelRequest_Click" Style="float: left;" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <div id="divPersonalDetailsReadOnly" runat="server" class="left" style="line-height: 30px;
                                width: 310px;">
                                <asp:Literal ID="ltrlFirstName" runat="server"></asp:Literal><br />
                                <asp:Literal ID="ltrlLastName" runat="server"></asp:Literal><br />
                                <asp:Literal ID="ltrlPosition" runat="server"></asp:Literal><br />
                                <asp:Literal ID="ltrlCompany" runat="server"></asp:Literal><br />
                                <asp:PlaceHolder ID="plcEmailTexts" runat="server">
                                    <asp:HyperLink ID="hypEmail1" runat="server"></asp:HyperLink><br />
                                </asp:PlaceHolder>
                                <asp:HyperLink ID="hypEmail2" runat="server"></asp:HyperLink><br />
                                <asp:Literal ID="ltrlPhone1" runat="server"></asp:Literal><br />
                                <asp:Literal ID="ltrlPhone2" runat="server"></asp:Literal><br />
                                <div style="line-height: 18px; padding-top: 6px;">
                                    <asp:Literal ID="ltrlAddress" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="right" style="width: 430px;">
                                <div class="blueText" style="margin-bottom: 5px;">
                                    Skills</div>
                                <sb:UserSkills ID="userSkills" runat="server" />
                            </div>
                            <div style="clear: both;">
                            </div>
                            <asp:Button ID="btnSavePersonalDetails" runat="server" CssClass="buttonStyle" ValidationGroup="personalDetailsFields"
                                OnClick="btnSavePersonalDetails_Click" Text="Save" />
                            <div id="personalDetailsSavedNotice" class="inlineNotification right">
                                Changes saved.</div>
                            <div style="clear: both;">
                            </div>
                        </asp:Panel>
                    </telerik:RadPageView>

                    <telerik:RadPageView ID="NotificationsTab" runat="server">
                        <sb:UserEmailNotifications runat="server" id="sbUserEmailNotifications" />
                    </telerik:RadPageView>

                    <telerik:RadPageView ID="ProjectsTab" runat="server">
                        <sb:UserProjects ID="userProjects" runat="server" />
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="JobHistoryTab" runat="server">
                        <div style="margin: 40px; text-align: center;">
                            Job History/Portfolio coming soon.
                        </div>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="PortfolioTab" runat="server">
                        <div style="margin: 40px; text-align: center;">
                            Job History/Portfolio coming soon.
                        </div>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="ChangePasswordTab" runat="server">
                        <asp:UpdatePanel ID="upnlChangePassword" UpdateMode="Conditional" runat="server">
                            <ContentTemplate>
                                <asp:Panel ID="pnlChangePassword" DefaultButton="btnUpdatePassword" runat="server">
                                    <div class="left" style="line-height: 30px; margin-right: 10px; width: 120px; white-space: nowrap;">
                                        Current Password:<br />
                                        New Password:<br />
                                        Confirm password:<br />
                                    </div>
                                    <div class="left sideErrorContainer" style="width: 600px; line-height: 30px;">
                                        <asp:TextBox ID="txtCurrentPassword" MaxLength="50" Width="210" runat="server" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="reqCurrentPassword" ControlToValidate="txtCurrentPassword"
                                            runat="server" ErrorMessage="* required" ValidationGroup="changePasswordFields"></asp:RequiredFieldValidator>
                                        <asp:CustomValidator ID="cusvalCurrentPassword" runat="server" ControlToValidate="txtCurrentPassword"
                                            OnServerValidate="cusvalCurrentPassword_ServerValidate" ErrorMessage="Incorrect password."
                                            ValidationGroup="changePasswordFields"></asp:CustomValidator><br />
                                        <asp:TextBox ID="txtNewPassword" MaxLength="50" Width="210" runat="server" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="reqNewPassword" ControlToValidate="txtNewPassword"
                                            runat="server" ErrorMessage="* required" ValidationGroup="changePasswordFields"></asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="regexNewPassword" runat="server" ControlToValidate="txtNewPassword"
                                            ValidationGroup="changePasswordFields" ErrorMessage="Password must be at least 6 characters long."
                                            ValidationExpression="^.{6,}$"></asp:RegularExpressionValidator><br />
                                        <asp:TextBox ID="txtConfirmPassword" MaxLength="50" Width="210" runat="server" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="reqConfirmPassword" ControlToValidate="txtConfirmPassword"
                                            runat="server" ErrorMessage="* required" ValidationGroup="changePasswordFields"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compChangePassword" ControlToValidate="txtConfirmPassword"
                                            ControlToCompare="txtNewPassword" runat="server" ErrorMessage="Please ensure passwords match."
                                            ValidationGroup="changePasswordFields"></asp:CompareValidator><br />
                                    </div>
                                    <br style="clear: both;" />
                                    <div style="width: 345px; height: 30px; margin-top: 10px;">
                                        <asp:Button ID="btnUpdatePassword" CssClass="buttonStyle" OnClick="btnUpdatePassword_Click"
                                            runat="server" Text="Update Password" ValidationGroup="changePasswordFields" />
                                        <div id="passwordUpdatedNotice" class="inlineNotification right">
                                            Password updated.</div>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </telerik:RadPageView>
                </telerik:RadMultiPage>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
