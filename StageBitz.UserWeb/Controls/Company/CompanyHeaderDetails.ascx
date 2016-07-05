<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyHeaderDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.CompanyHeaderDetails" %>
<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>

    <div style="margin: 10px 0px 15px 0px;">
        <div class="left">
            <asp:UpdatePanel ID="upnlThumbnail" UpdateMode="Conditional" runat="server">
                <ContentTemplate>
                    <sb:ImageDisplay ID="companyImage" Visible="false" runat="server" />
                    <asp:Image ID="imgCompanies" ImageUrl="~/Common/Images/folder_Thumb.png" runat="server"
                        Visible="false" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="left" style="margin-left: 10px; max-width: 350px; overflow: hidden;">
            <div style="line-height: 20px; margin-bottom: 5px;">
                <asp:UpdatePanel ID="upnlBasicDetails" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <strong>
                            <asp:Literal ID="ltrlName" runat="server"></asp:Literal></strong><br />
                        <asp:Literal ID="ltrlMembershipPeriod" runat="server"></asp:Literal><br />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <asp:UpdatePanel ID="upnlFileUpload" UpdateMode="Conditional" runat="server">
                <ContentTemplate>
                    <div style="display: inline-block;">
                        <sb:FileUpload ID="fileUpload" Title="Change Image" DisplayLauncherAsLink="true"
                            OnFileUploaded="fileUpload_FileUploaded" runat="server" />
                    </div>
                    <div id="divRemoveImage" runat="server" style="display: inline-block;">
                        | <a class="smallText" href="#" onclick="showPopup('popupImageRemoveConfirmation'); return false;"
                            title="Remove profile image">Remove</a>
                    </div>
                    <sb:PopupBox ID="popupImageRemoveConfirmation" Title="Remove Company Logo" Height="100"
                        runat="server">
                        <BodyContent>
                            <div style="white-space: nowrap;">
                                Are you sure you want to remove the company logo?
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
            <p>
                This information will be shared with other StageBitz users. Fields marked with an
                <span class="requiredMarker">*</span>&nbsp are required information.
                
                <br />
            </p>
        </div>
        <div style="clear: both;">
        </div>
    </div>
    <div class="boxBorderDark" style="padding: 10px;">
        <table>
            <tr>
                <td>
                    <p>
                        <strong>Company Details</strong></p>
                </td>
                <td id="tdHelpTip" visible="false" runat="server">
                    <sb:HelpTip ID="helpTipEditCompany" Visible="true" runat="server">
                        <p>
                            Changes to the Company Name will be seen on all
                            <br />
                            past and present Projects by all Users.
                        </p>
                        <br />
                        <p>
                            If you require this information to stay the same do
                            <br />
                            not change the Company name.
                        </p>
                        <br />
                        <p>
                            Changing the country will alter the currency shown
                            <br />
                            in all Projects.
                        </p>
                    </sb:HelpTip>
                </td>
            </tr>
        </table>
        <br />
    </div>