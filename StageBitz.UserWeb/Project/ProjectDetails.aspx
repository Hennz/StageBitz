<%@ Page Title="" Language="C#" MasterPageFile="~/Content.Master" AutoEventWireup="true"
    CodeBehind="ProjectDetails.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectDetails" %>

<%@ Register Src="~/Controls/Project/ProjectLocations.ascx" TagName="Location" TagPrefix="uc2" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ItemBriefTypeSummary.ascx" TagName="ItemBriefTypeSummary" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/ItemAttachments.ascx" TagName="Attachments" TagPrefix="uc2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyID %>' != '0' && '<%= this.ProjectID %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyID %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectID %>', 2]);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<a id="linkTaskManager" runat="server">Task Manager</a>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <telerik:RadWindowManager ID="mgr" runat="server">
    </telerik:RadWindowManager>
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <asp:UpdatePanel ID="updpnl" runat="server">
        <ContentTemplate>
            <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
            <div>
                <div style="float: left;">
                    <strong>Project Details</strong>
                </div>
                <div style="float: left; margin-left: 4px;">
                    <sb:HelpTip ID="HelpTip1" runat="server">
                        Changes to the Project Details will be seen by all members of the Project Team working
                on this Project.
                    </sb:HelpTip>
                </div>
                <div style="clear: both;">
                </div>
            </div>
            <div style="text-align: left; float: left; padding-top: 5px; margin-top: 5px; margin-right: 10px;">
                Project Name :
            </div>
            <div style="float: left; margin-top: 5px;" class="dirtyValidationArea">
                <asp:TextBox ID="txtName" MaxLength="100" Width="200" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ValidationGroup="projectDetails" ID="RequiredFieldValidator1"
                    ControlToValidate="txtName" runat="server" ErrorMessage="Project name is required."></asp:RequiredFieldValidator>
            </div>
            <div style="clear: both;">
            </div>
            <div style="height: auto;">
                <span id="errormsg" class="inputError" runat="server"></span>
            </div>

            <asp:UpdatePanel runat="server" ID="upnlProjectDetailsTabs" UpdateMode="Conditional">
                <ContentTemplate>
                    <telerik:RadTabStrip ID="projectDetailsTabs" Width="100%" MultiPageID="projectDetailsPages" runat="server">
                        <Tabs>
                            <telerik:RadTab runat="server" Text="Locations" Value="Locations">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Attachments" Value="Attachments">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Wrap up" Value="Wrapup">
                            </telerik:RadTab>
                        </Tabs>
                    </telerik:RadTabStrip>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="tabPage" style="width: 900px; min-height: 310px;">
                <telerik:RadMultiPage ID="projectDetailsPages" runat="server">
                    <telerik:RadPageView ID="LocationsTab" runat="server">
                        <div style="height: 308px; margin-top: 5px;">
                            <uc2:Location ID="ucLocation" LocationGridHeight="200" runat="server" />
                        </div>

                    </telerik:RadPageView>

                    <telerik:RadPageView ID="AttachmentsTab" runat="server">
                         <div style="height: 308px; margin-top: 5px;">
                            <uc2:Attachments ID="attachments" LocationGridHeight="200" runat="server" />
                        </div>
                    </telerik:RadPageView>

                    <telerik:RadPageView ID="WrapupTab" runat="server">
                        <div runat="server" id="divBeforeCloseProject">
                            <b>Before closing the Project check that all Items you wish to be released to the Inventory have been completed.</b>
                            <div style="padding: 20px 250px; min-height: 160px;">
                                <sb:ItemBriefTypeSummary runat="server" ID="itemBriefTypeSummaryBeforeClose" />
                            </div>
                            <b>Closing the finished Projects will mean no more changes can be made and the Team will no longer have access to the information. </b>
                            <b>Company Administrator will be able to access a read-only version from the Company Dashboard.</b>
                        </div>
<%--                        <div runat="server" id="divBeforeCloseProjectBlank" style="text-align: center; padding-top: 70px; padding-bottom: 100px;">
                            <p>
                                <h1>You have nothing to Wrap-up yet.</h1>
                            </p>
                            <p>This is where the finished Project is closed and all the Props, Costumes, Scenery and all those other Bitz </p>
                            <p>get released to the Company Inventory.</p>
                        </div>--%>
                        <div>
                            <br style="clear: both" />
                            <asp:Button runat="server" Text="Close finished Project" OnClick="btnProjectClose_Click" ID="btnProjectClose" Enabled="false" CssClass="buttonStyle right" ToolTip="Only Administrators can close a Project" />
                            <br style="clear: both" />
                        </div>
                        <div runat="server" id="divAfterCloseProject" style="text-align: center">
                            <p>
                                <h1>That’s a wrap!</h1>
                                This Project has been closed.
                            </p>
                            <p>You can still access all the information and print reports; you just won’t be able to change anything.</p>
                            <div style="padding: 20px 250px;">
                                <sb:ItemBriefTypeSummary runat="server" ID="itemBriefTypeSummaryAfterClose" />
                            </div>
                        </div>
                        <div class="dirtyValidationExclude">
                            <sb:PopupBox ID="popupConfirmSaveProjectDetails" runat="server" Title="Save unsaved changes">
                                <BodyContent>
                                    <div style="width: 300px;">
                                        You have unsaved changes in this page. Do you want to save them and proceed?<br />
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnConfirmSaveProjectDetails" CssClass="buttonStyle" runat="server"
                                        Text="Yes" OnClick="btnConfirmSaveProjectDetails_Click" />
                                    <asp:Button ID="btnCancelConfirmSaveProjectDetails" CssClass="buttonStyle" runat="server"
                                        Text="No" OnClick="btnCancelConfirmSaveProjectDetails_Click" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupConfirmCloseProject" runat="server" Title="Are you sure you are ready to close the finished Project?" Width="700">
                                <BodyContent>
                                    <div style="width: 600px;">
                                        <p>Closing the Project means:</p>
                                        <p>
                                            <ul>
                                                <li>All complete Items are released to the Company Inventory.</li>
                                                <li>The Project will be removed from the Personal Dashboard of each Team Member.</li>
                                                <li>
                                                    <p>Company Administrators will be able to access a read-only version of the Project from the Company Dashboard.</p>
                                                </li>
                                                <li>You will not be able to make ANY further changes to it.</li>
                                            </ul>
                                        </p>
                                        <i>
                                            <p>COMING SOON: “Clone Project”.</p>
                                            <p>
                                                This will allow you to make a copy of a Closed Project so that you can take up where you left off for the next season,&nbsp;
                                            tour or revival without over-writing the original Project. If you need to do this before we release ‘Clone Project’,&nbsp; 
                                            Please contact
                                            <asp:HyperLink runat="server" ID="lnkFeedBackEmail"></asp:HyperLink>
                                                and we’ll be happy to help.
                                            </p>
                                        </i>
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnConfirmCloseProject" CssClass="buttonStyle" runat="server"
                                        Text="Confirm" OnClick="btnConfirmCloseProject_Click" CommandName="ConfirmClose" />
                                    <asp:Button ID="btnCancelConfirmCloseProject" CssClass="buttonStyle" runat="server"
                                        Text="Cancel" OnClick="btnCancelConfirmCloseProject_Click" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupPaymentFailed" runat="server" Title="There are outstanding payments for this Project." Width="700">
                                <BodyContent>
                                    <div style="width: 600px;">
                                        <p>
                                            You are seeing this message because there has been a failed payment on this Project. This is because of invalid or incomplete credit card details.
                                        </p>
                                        <br />
                                        <p>
                                            It’s simple to fix:
                                            <ul>
                                                <li>
                                                    <p>
                                                        The Company Administrators can update the credit card details and pay the outstanding amount by going to Company Billing on the Company Dashboard.
                                                    </p>
                                                </li>
                                                <asp:Literal runat="server" ID="ltrlContactPrimaryAdmin"></asp:Literal>
                                            </ul>
                                        </p>
                                        <br />
                                        <p>Once the Payment has been made, you will be able to close the Project and release all completed Items to the Company Inventory.</p>
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnGotoCompanyBilling" CssClass="buttonStyle" runat="server"
                                        Text="Go to Company Billing" OnClick="btnGotoCompanyBilling_Click" Visible="false" />
                                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupCloseFreeTrailProject" runat="server" Title="Are you sure you are ready to close the finished Project?" Width="700">
                                <BodyContent>
                                    <div style="width: 600px;">
                                        <h4>This will end your Free trial!</h4>
                                        <br />
                                        <p>Closing the Project means:</p>
                                        <p>
                                            <ul>
                                                <li>All complete Items are released to the Company Inventory.</li>
                                                <li>The Project will be removed from the Personal Dashboard of each Team Member.</li>
                                                <li>
                                                    <p>Company Administrators will be able to access a read-only version of the Project from the Company Dashboard.</p>
                                                </li>
                                                <li>You will not be able to make ANY further changes to it.</li>
                                            </ul>
                                        </p>
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnConfirmCloseFreeTrailProject" CssClass="buttonStyle" runat="server"
                                        Text="Confirm" OnClick="btnConfirmCloseProject_Click" CommandName="FreeTrailEnd" />
                                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupProjectCloseInventoryLimitReached" runat="server" Title="Are you sure you are ready to close the finished Project?" Width="700">
                                <BodyContent>
                                    <div style="width: 800px;">
                                        <h4><b>Oops... your company has already reached the limit for the number of Items allowed in their Company Inventory.</b></h4>
                                        <p>
                                            <h4><b>What would you like to do? </b></h4>
                                        </p>
                                        <div style="margin-left:10px;">
                                        <p>
                                            <b>1. Close the Project anyway</b>
                                            <ul>
                                                <li>No new Items created for this Project will be added to your Company Inventory.</li>
                                                <li>All Items previously booked to this Project from the Company Inventory will be made available to other
                                                    projects.</li>
                                                <li>The Project will be removed from the Personal Dashboard of each Team member</li>
                                                <li>Company Administrators will be able to access a read-only version of the Project from the Company
                                                    Dashboard.</li>
                                                <li>You will not be able to make ANY further changes to it.</li>
                                            </ul>
                                        </p>
                                        </div>

                                        <div runat="server" id="divProjectAdminInventoryReached" style="margin-left:10px;">
                                           <b>2. Ask your Company Administrator to upgrade your Company Inventory </b> 
                                            <p>
                                                If you would like to increase the number of Items allowed in your Company Inventory, please email your Primary
                                                Company Administrator (<asp:Literal runat="server" ID="ltrCompanyPrimaryAdminName"/>) and ask them to upgrade
                                                their subscription. Then, once they've done that, come back and you'll be able to close the project AND add all 
                                                your new Items to the Company Inventory.
                                        </p>
                                        </div>
                                        <div runat="server" id="divCompanyAdminInventoryReached" style="margin-left:10px;">
                                           <b> 2. Upgrade your Company Inventory </b> 
                                                <p>If you would like to increase the number of Items allowed in your Company Inventory, please go to the pricing
                                              plan page. Then, once they've done that, come back and you'll be able to close the project AND add all 
                                                your new Items to the Company Inventory.
                                        </p>
                                        </div>
                                    </div>
                                </BodyContent>
                                <BottomStripeContent>
                                    <asp:Button ID="btnUpgradeProjectCloseInventoryLimitReached" CssClass="buttonStyle" runat="server"
                                        Text="Upgrade"  OnClick="btnUpgradeProjectCloseInventoryLimitReached_Click"/>
                                    <asp:Button ID="btnSendEmailProjectCloseInventoryLimitReached" CssClass="buttonStyle" runat="server"
                                        Text="Send email without closing the Project" OnClick="btnSendEmailProjectCloseInventoryLimitReached_Click" />
                                    <asp:Button ID="btnConfirmCloseInventoryLimitReached" CssClass="buttonStyle" runat="server"
                                        Text="Confirm and close Project" OnClick="btnConfirmCloseProject_Click" CommandName="InventoryLimitReached" />
                                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                            <sb:PopupBox ID="popupConfirmEmailSentToAdminToUpgrade" Title="Confirmation" runat="server">
                                <BodyContent>
                                    Email has been sent to <asp:Literal runat="server" ID="ltrPrimaryAdminName"/> requesting Upgrade.
                                </BodyContent>
                                <BottomStripeContent>
                                    <input type="button" class="popupBoxCloser buttonStyle" value="OK" />
                                </BottomStripeContent>
                            </sb:PopupBox>
                        </div>
                    </telerik:RadPageView>
                </telerik:RadMultiPage>

            </div>
            <br style="clear: both" />
            <div class="buttonArea" style="text-align: right;">
                <asp:Button ID="btnDone" ValidationGroup="projectDetails" OnClick="SaveDetails" runat="server"
                    Text="Done" CssClass="ignoreDirtyFlag buttonStyle" />
                <asp:Button ID="btnCancel" CausesValidation="false" runat="server" Text="Cancel" CssClass="ignoreDirtyFlag buttonStyle" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
