<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentDetailsValidation.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PaymentDetailsValidation" %>
<%@ Register Src="~/Controls/Company/PaymentDetails.ascx" TagPrefix="uc" TagName="PaymentDetails" %>
<%@ Register Src="~/Controls/Company/PaymentPackageSummary.ascx" TagName="PaymentPackageSummary" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Common/FutureRequestNotificationMessage.ascx" TagPrefix="uc" TagName="FutureRequestNotificationMessage" %>
<asp:UpdatePanel runat="server" ID="upnlPopUps" UpdateMode="Conditional">
    <ContentTemplate>
        <sb:PopupBox ID="popupNoProjectsPackageSelectionDuringFreeTrail" runat="server" ShowCornerCloseButton="false" Title="Are you sure?">
            <BodyContent>
                <div style="width: 520px;">
                    <p>
                        <b>The Pricing plan you have chosen does not include any active Projects at all.
                        </b>
                    </p>
                    <br />
                    <p>
                        This mean free trial Project will be suspended when it reaches the end of your free period on
                        <asp:Label runat="server" ID="lblFreeTrailProjectEndDate"></asp:Label>.
                    </p>
                    <br />
                    <p>
                        If you would like to keep working on
                        <asp:Label runat="server" ID="lblFreeTrailProject"></asp:Label>
                        after this date you will need to choose a pricing plan that includes Projects. 
    You will not be charged for anything until the end of the 3 week Free Trial and you can change your pricing plan at any time.
                    </p>

                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnContinueNoProjectsPackageDuringFreeTrail" Text="Continue" OnClick="btnContinueNoProjectsPackageDuringFreeTrail_Click" CssClass="ignoreDirtyFlag  buttonStyle" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>        

        <sb:PopupBox ID="popupNoPackageChanges" Title="Just checking..." ShowCornerCloseButton="false" runat="server">
            <BodyContent>
                <div style="width: 500px;">
                    You've already subscribed to this plan. Do you want to update your payment details? 
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnNoPackageChanges" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnNoPackageChanges_Click" Text="Update" CausesValidation="false" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupFreeInventoryOnly" runat="server" ShowCornerCloseButton="false" Title="Just Inventory? No need for Projects? ">
            <BodyContent>
                <div>
                    <uc:FutureRequestNotificationMessage runat="server" ID="FutureRequestNotificationMessageForFreeInventory" />
                </div>
                <div style="width: 520px;">
                    <p>
                        <b>You have chosen to have up to
                <asp:Literal ID="ltrMaxItemsForFreeInventory" runat="server" />
                            Inventory Items for free and no Projects.</b>
                    </p>
                    <br />
                    <p>
                        If you would like to be able to create Projects you will need to select a higher Project level.
                You can come back to the Pricing plan page at any point to upgrade your Inventory or Project levels.
                    </p>
                    <br />
                    <asp:CheckBox runat="server" Width="400" ID="chkAcceptTermsFreeInventory" AutoPostBack="true" OnCheckedChanged="chkAcceptTermsFreeInventory_CheckedChanged" />
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnConfirmFreeInventory" Text="Confirm" Enabled="false" OnClick="btnConfirmFreeInventory_Click" CssClass="ignoreDirtyFlag  buttonStyle" ToolTip="Please accept the Terms and Conditions" />
                <asp:Button runat="server" ID="btnCancelFreeInventory" CssClass="ignoreDirtyFlag  buttonStyle" Text="Cancel" CausesValidation="false" OnClick="btnClose_Click" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupHundredPercentDiscount" runat="server" ShowCornerCloseButton="false" Title="100% Promotional Code">
            <BodyContent>
                <div>
                    <uc:FutureRequestNotificationMessage runat="server" ID="FutureRequestNotificationMessageForHundredPercent" />
                </div>
                <div style="width: 510px;">
                    <p>
                        <b>Congratulations! you can use StageBitz for FREE until
                        <asp:Literal ID="ltrDiscountExpiryDate" runat="server" />.</b>
                    </p>
                    <br />
                    <p>
                        After this date you will need to have entered your payment details if you would like to continue using StageBitz. To make sure 
                    everything is smooth sailing, why not enter those details now?
                    </p>
                    <br />
                    <p>Either way we will send you an email to remind you when the code is about to expire.</p>
                    <br />
                    <asp:CheckBox runat="server" Width="400" ID="chkAcceptTermsDiscount" AutoPostBack="true" OnCheckedChanged="chkAcceptTermsDiscount_CheckedChanged" />
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnConfirmHundredPercentDiscount" Text="Confirm" Enabled="false" OnClick="btnConfirmHundredPercentDiscount_Click" CssClass="ignoreDirtyFlag  buttonStyle" ToolTip="Please accept the Terms and Conditions" />
                <asp:Button runat="server" ID="btnAddDetails" CssClass="buttonStyle" OnClick="btnAddDetails_Click" Text="Add details now" />
                <asp:Button runat="server" ID="btnCancelHundredDiscount" CssClass="ignoreDirtyFlag buttonStyle" Text="Cancel" CausesValidation="false" OnClick="btnClose_Click" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupConcurrentDiscountChanged" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="margin-left: 65px; width: 450px;">
                    <div runat="server" id="divDiscountRemoved">
                        <p>
                            You can either continue without entering a promotional code or contact <a runat="server" id="lnkEmail">StageBitz</a> to discuss accessing a current 
                    promotional code.
                        </p>
                        <br />
                    </div>
                    <div runat="server" id="divDiscountAddedBy">
                        <p>
                            <b>
                                <asp:Literal ID="ltrDiscountAddedBy" runat="server" /></b>
                        </p>
                        <br />
                    </div>
                    <div runat="server" id="divDiscountAdded">
                        <p>
                            This gives you <b>
                                <asp:Literal ID="ltrDiscountValue" runat="server" /></b> off your chosen pricing plan until <b>
                                    <asp:Literal ID="ltrChangedDiscountExpiryDate" runat="server" />.</b>
                        </p>
                        <br />
                    </div>
                    <div runat="server" id="divGeneralError">
                        <p>
                            <asp:Literal ID="litGeneralError" runat="server" />
                        </p>
                        <br />
                    </div>
                    This means your current pricing plan is...<br />
                    <br />
                </div>
                <div style="margin-left: 60px;">
                    <uc:PaymentPackageSummary ID="paymentPackageSummary" runat="server" />
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnRespondDiscountChanged" Text="OK" OnClick="btnRespondDiscountChanged_Click" CssClass="ignoreDirtyFlag buttonStyle" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupError" Title="Please choose a larger Pricing plan" runat="server">
            <BodyContent>
                <div style="width: 500px;">
                    <asp:Literal ID="litError" runat="server"></asp:Literal>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <input type="button" class="popupBoxCloser buttonStyle" value="OK" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox runat="server" ID="popupPaymentDetails" Title="Please enter payment details" ShowCornerCloseButton="false">
            <BodyContent>
                <div>
                    <uc:FutureRequestNotificationMessage runat="server" ID="FutureRequestNotificationForPaymentDetails" />
                </div>
                <div style="width: 850px;" id="divPaymentDetails" runat="server">
                    <uc:PaymentDetails runat="server" ID="paymentDetails" OnTermsAccepted="paymentDetails_TermsAccepted" OnAuthorizationEnabled="paymentDetails_AuthorizationEnabled" />
                </div>
                <br style="clear: both;" />
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnConfirmPaymentDetails" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnConfirmPaymentDetails_Click" Text="Confirm" Enabled="false" />
                <asp:Button runat="server" ID="btnClose" CssClass="ignoreDirtyFlag buttonStyle" Text="Cancel" CausesValidation="false" OnClick="btnClose_Click" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupCloseConfirm" Title="Confirm" runat="server">
            <BodyContent>
                <div style="width: 300px;">
                    Are you sure you want to cancel?
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnCloseConfirmAccept" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnCloseConfirmAccept_Click" Text="Yes" CausesValidation="false" />
                <input type="button" class="ignoreDirtyFlag popupBoxCloser buttonStyle" value="No" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupConcurrency" Title="Error !" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="width: 500px;">
                    <span runat="server" id="spanPaymentFailed" visible="false">Please note that you currently have a failed payment. You will need to make this payment from the
                        <asp:HyperLink runat="server" ID="lnkBillingPage" Text="Company Billing Page"></asp:HyperLink>
                        before you can continue.
                    </span>
                    <span runat="server" id="spanSBAdminSuspened" visible="false">Your Company has been suspended by StageBitz Administration. If you need any assistance please contact
                        <asp:HyperLink runat="server" ID="lnkContactSBSupport"></asp:HyperLink>
                        and we’ll be happy to help.
                    </span>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnReloadPage" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnReloadPage_Click" Text="Reload Page" CausesValidation="false" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupSaveChangesConfirm" Title="Changes Saved" ShowCornerCloseButton="false" runat="server">
            <BodyContent>
                <div style="width: 300px;">
                    Your payment choices have been confirmed.
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnChangesSaved" CssClass="ignoreDirtyFlag buttonStyle" OnClick="btnChangesSaved_Click" Text="OK" CausesValidation="false" />
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
</asp:UpdatePanel>
