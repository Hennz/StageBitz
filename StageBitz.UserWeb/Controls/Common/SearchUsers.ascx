<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchUsers.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.SearchUsers" %>

<%@ Register Src="~/Controls/Project/ProjectUserInvitation.ascx" TagName="UserInvitation" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="uc" TagName="PackageLimitsValidation" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocationRoles.ascx" TagPrefix="uc" TagName="InventoryLocationRoles" %>
<script type="text/javascript">

    function hideUserSearchNotifications() {
        $("#<%= divEmailNotFound.ClientID %>").hide();
        $("#<%= divNotification.ClientID %>").hide();
        $("#<%= divInviteSent.ClientID %>").hide();
    }

    function ValidateInventoryRole(sender, args) {
        args.IsValid = <%= sbInventoryLocationRoles.ClientID%>_ValidateUISelection();
    }

</script>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        <asp:PlaceHolder ID="plcCompanyAdminTitle" runat="server">
            <asp:Label runat="server" Text="Add Company Team Member"></asp:Label>
            <sb:HelpTip ID="HelpTip1" runat="server">
                <p>
                    Company Administrators can make decisions about your StageBitz Company account; they can change your Company’s contact details and create Projects.
                </p>
                <br />
                <p>
                    However, only the Primary Company Administrator can change payment details and manage other Administrators.
                </p>
                <br />
                <p>
                    Company Inventory Admin can add and edit Inventory Items and they will receive communication about the Inventory. Until this role is selected the Primary Company Administrator will receive the Inventory communication.
                </p>
            </sb:HelpTip>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="plcProjectTeamTitle" runat="server">Add Project Team Member: </asp:PlaceHolder>
        <asp:PlaceHolder ID="plcInventoryTeamTitle" runat="server">Add Inventory Team Member: </asp:PlaceHolder>
    </TitleLeftContent>
    <BodyContent>
        <asp:UpdatePanel ID="upnlSearchCriteria" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnlSearchCriteria" DefaultButton="btnSearch" runat="server">
                    <div style="text-align: center; padding: 0 20%">
                        Invite by email address:&nbsp;<asp:TextBox ID="txtEmail" MaxLength="50" Width="50%" runat="server"></asp:TextBox>
                        <asp:Button ID="btnSearch" CssClass="buttonStyle" OnClientClick="hideUserSearchNotifications();"
                            Style="vertical-align: bottom;" ValidationGroup="SearchVal" OnClick="btnSearch_Click"
                            runat="server" Text="Invite" />
                    </div>
                    <div style="clear: both;">
                    </div>
                </asp:Panel>
                <div>
                    <asp:RequiredFieldValidator runat="server" ID="rfvEmail" ControlToValidate="txtEmail"
                        ErrorMessage="Please enter an email address." ValidationGroup="SearchVal" SkinID="Hidden"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="regExSearchEmail" SkinID="Hidden" runat="server"
                        ErrorMessage="Invalid email address." ValidationGroup="SearchVal" ControlToValidate="txtEmail"
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                    <asp:ValidationSummary ID="valSummarySearch" ValidationGroup="SearchVal" DisplayMode="SingleParagraph"
                        CssClass="message error" runat="server" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="upnlSearchResults" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <uc:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />
                <sb:PopupBox ID="popupInviteProjectMember" Title="Invite to Project Team" runat="server">
                    <BodyContent>
                        <uc:UserInvitation ID="ucUserInvitationPopup" runat="server" />
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnInviteProjectMember" CssClass="buttonStyle" runat="server" OnClick="btnInviteProjectMember_Click"
                            ValidationGroup="ProjectInvitation" Text="Send Invitation" />
                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <sb:PopupBox ID="popupInviteCompanyAdmin" Title="Invite Company Adminstrator" runat="server">
                    <BodyContent>
                        <div style="max-width: 500px;">
                            <b>
                                <asp:Literal ID="ltrlCompanyAdminInvitationText" runat="server"></asp:Literal></b>
                            <table id="tblCompInviteName" runat="server" visible="false" style="margin-top: 10px; width: 700px;">
                                <tr>
                                    <td style="vertical-align: top; padding-top: 5px; width: 70px;">Name:&nbsp;&nbsp;&nbsp;&nbsp;
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtCompInvitePersonName" Width="170" MaxLength="100" runat="server"></asp:TextBox>
                                        <br />
                                        <asp:RequiredFieldValidator ID="reqCompInvitePersonName" ControlToValidate="txtCompInvitePersonName" SkinID="Hidden"
                                            ValidationGroup="CompanyInvitation" runat="server" ErrorMessage="Please enter the new user's name so we can address them properly in the email."></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>
                            <table style="margin-top: 10px; width: 500px;">
                                <tr>
                                    <td style="width: 70px; vertical-align: top;">As a Secondary Company Administrator, this person will be able to:
                                        <div>
                                            <ul style="margin-top: 5px;">
                                                <li>Make decisions about your StageBitz company account
                                                </li>
                                                <li>Create Projects
                                                </li>
                                                <li>Have full administrative access to your Company Inventory
                                                </li>
                                            </ul>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <asp:ValidationSummary ID="valsumCompanyAdminInvitation" ValidationGroup="CompanyInvitation"
                                DisplayMode="List" CssClass="message error" runat="server" />
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                        <asp:Button ID="btnInviteCompanyAdmin" CssClass="buttonStyle" runat="server" ValidationGroup="CompanyInvitation"
                            OnClick="btnInviteCompanyAdmin_Click" Text="Send Invitation" />
                    </BottomStripeContent>
                </sb:PopupBox>


                <sb:PopupBox ID="popupInviteInventoryUsers" Title="Invite to Inventory" runat="server">
                    <BodyContent>
                        <div style="width: 600px;">
                            <b>
                                <asp:Literal ID="ltrInviteInventoryUsersText" runat="server"></asp:Literal></b>
                            <table style="margin-top: 10px; width: 600px; margin-left: 30px;" id="tblInventoryUserName" runat="server" visible="false">
                                <tr>
                                    <td style="vertical-align: top; padding-top: 5px; width: 120px; text-align: right;">Name:&nbsp;
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtInventoryUserName" Width="170" MaxLength="100" runat="server"></asp:TextBox>
                                        <br />
                                        <asp:RequiredFieldValidator ID="rfvInventoryUserName" ControlToValidate="txtInventoryUserName" SkinID="Hidden"
                                            ValidationGroup="InventoryInvitation" runat="server" ErrorMessage="Please enter the new user's name so we can address them properly in the email."></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>
                            <div style="width: 100%">
                                <div style="padding: 5px 5%; margin: 5px 0;" class="infobox">
                                    <table>
                                        <tr>
                                            <td style="padding: 10px;">
                                                <i class="fa fa-lightbulb-o" style="font-size: 3em;"></i>
                                            </td>
                                            <td>
                                                <i>If you need to your new team member to become the <b>Inventory Administrator</b> or a <b>Location Manager</b>, 
                                                    then you'll need to invite them as an Inventory Team member first. You can change their role after they've accepted your invitation.</i>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                <div><b>Which StageBitz Role should this user have?</b></div>
                                <uc:InventoryLocationRoles runat="server" ID="sbInventoryLocationRoles" InventoryRolesDisplayMode="InviteMode" />
                                <asp:CustomValidator ID="cusvalInventoryRole" runat="server"
                                    ValidationGroup="InventoryInvitation" ClientValidationFunction="ValidateInventoryRole"
                                    ErrorMessage="Please select Inventory Role for all locations." SkinID="Hidden"></asp:CustomValidator>
                            </div>
                            <asp:ValidationSummary ID="vsInviteInventoryUser" ValidationGroup="InventoryInvitation"
                                DisplayMode="List" CssClass="message error" runat="server" />
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnInviteInventoryUsers" CssClass="buttonStyle" runat="server" ValidationGroup="InventoryInvitation"
                            OnClick="btnInviteInventoryUsers_Click" Text="Send invitation" />
                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                    </BottomStripeContent>
                </sb:PopupBox>

                <sb:PopupBox ID="popupNotification" Title="Invite User" runat="server">
                    <BodyContent>
                        <div style="max-width: 400px;">
                            <asp:Literal ID="ltrlPopupNotification" runat="server"></asp:Literal>

                            &nbsp;<asp:HyperLink ID="hyperLinkAdjustLimitPopup" Visible="false" Style="" runat="server"></asp:HyperLink>

                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <input type="button" class="buttonStyle popupBoxCloser" value="OK" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <div id="divEmailNotFound" runat="server" class="message" visible="false">
                    We have not found any matches in StageBitz.
                    <asp:LinkButton ID="lnkbtnInviteToStageBitz" OnClick="lnkbtnInviteToStageBitz_Click"
                        runat="server"></asp:LinkButton>
                    to StageBitz.
                </div>
                <div id="divNotification" runat="server" class="message warning" visible="false">
                </div>
                <div id="divInviteSent" runat="server" class="message success" visible="false">
                </div>
                <div id="divSearchResults" runat="server" style="border-top: 1px solid #A3D1FF; margin-top: 10px; padding-top: 10px; max-height: 320px; overflow-y: auto;"
                    visible="false">
                    <div style="margin-bottom: 5px; font-weight: bold;">
                        <asp:Literal ID="ltrlMatchCount" runat="server"></asp:Literal>
                        <span class="grayText" style="font-weight: normal;">(Click a result to invite)</span>
                    </div>
                    <asp:ListView ID="lvSearchResults" OnItemCommand="lvSearchResults_ItemCommand" OnItemDataBound="lvSearchResults_ItemDataBound"
                        DataKeyNames="UserId" runat="server">
                        <LayoutTemplate>
                            <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkbtnUser" CssClass="thumbListItem" CommandName="InviteUser"
                                ClientIDMode="AutoID" runat="server">
                                <sb:ImageDisplay ID="userThumbDisplay" ShowImagePreview="false" runat="server" />
                                <div>
                                    <asp:Literal ID="ltrlUsername" runat="server"></asp:Literal>
                                </div>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:ListView>
                    <div style="clear: both;">
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <sb:ErrorPopupBox ID="popupInventoryLocationDeleted" runat="server" Title="Error" ShowCornerCloseButton="false" ErrorCode="InventoryLocationDeleted">
            <BodyContent>
                <div style="width: 300px;">
                    Selected Inventory Location has already been deleted.
                        <br />
                </div>
            </BodyContent>
            <BottomStripeContent>
                <input type="button" value="Ok" onclick="location.reload();" class="buttonStyle" />
            </BottomStripeContent>
        </sb:ErrorPopupBox>
    </BodyContent>
</sb:GroupBox>
