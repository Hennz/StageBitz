<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SetupCreditCardDetails.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.SetupCreditCardDetails" %>

<%@ Register Src="~/Controls/Common/CreditCardDetails.ascx" TagName="CredidCardDetails" TagPrefix="uc" %>

<script type="text/javascript">


    function HidePaymentSuccessMessage() {
        showNotification("divSuccess");
    }

    function ShowCreditCardDetailsExpander() {

    }


</script>

<asp:UpdatePanel runat="server" ID="upnlCreditCardDetails" UpdateMode="Conditional">
    <ContentTemplate>
        <div runat="server" id="paymentSetupDetails" style="float: left;" visible="false">
            <strong class="left" style="color: Red;">
                <asp:Literal ID="litPaymentDetailsNotSet" Visible="false" Text="Not set " runat="server"></asp:Literal>
                &nbsp;
            </strong>

            <div style="float: left;" id="divPaymentDetailsSet" runat="server" visible="false">
                <div style="float: left; font-weight: bold;">
                    <asp:Literal ID="litPaymentDetailsStatus" Text="Payments are set up" runat="server"></asp:Literal>
                </div>
                <img src="~/Common/Images/msginfo.png" alt="InformationIcon" runat="server" id="imgCreditCardNo"
                    style="display: block; float: left; margin-left: 3px;" />
                &nbsp;
            </div>
            <div style="float: left;">

                <asp:Literal ID="litpipePaymentDetailsNotSet" runat="server" Text=" | "></asp:Literal>
                <asp:LinkButton ID="lnkSetUp" Text="Set Up" OnClick="ShowCreditCardDetails"
                    runat="server"></asp:LinkButton>
                <asp:Literal ID="litPackageStatus" Visible="false" Text="Not set" runat="server"></asp:Literal>
                <span id="spnSeperator" visible="false" runat="server"></span>
                <asp:LinkButton ID="lnkChange" Visible="false" Text="Change" OnClick="ShowCreditCardDetails"
                    runat="server"></asp:LinkButton>
            </div>
            <div class="left" id="divSuccess" style="color: Gray;display: none;">
                &nbsp;Changes saved.
            </div>
        </div>
        <div style="clear: both;">
        </div>
        <asp:Panel ID="pnlPopup" runat="server">
            <sb:PopupBox ID="popupConfirmPaymentDetails" ShowCornerCloseButton="false" runat="server">
                <BodyContent>
                    <div style="padding: 10px;">
                        <uc:CredidCardDetails runat="server" ID="ucCreditCardDetailsPopup" />

                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <img id="imgCreditCard" style="margin-left: 15px; margin-top: -5px;" width="100"
                        height="30" runat="server" src="~/Common/Images/visa_mastercard_logo.gif" alt="creditcard" />
                    <asp:Button ID="btnConfirm" CssClass="buttonStyle" OnClick="btnConfirm_Click" Text="Confirm"
                        runat="server" />
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
        </asp:Panel>


        <asp:Panel ID="pnlExpander" runat="server" Visible="false">
            <div style="padding: 10px;">
                <uc:CredidCardDetails runat="server" ID="ucCreditCardDetailsExpander" />
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
