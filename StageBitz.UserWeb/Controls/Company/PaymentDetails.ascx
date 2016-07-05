<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PaymentDetails" %>
<%@ Register Src="~/Controls/Company/CompanyDetails.ascx" TagPrefix="uc" TagName="CompanyDetails" %>
<%@ Register Src="~/Controls/Common/SetupCreditCardDetails.ascx" TagPrefix="uc" TagName="SetupCreditCardDetails" %>

<div>
    <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="upnlPaymentDetails">
        <ContentTemplate>
     
            <table style="min-height: 20px;">
                <tr>
                    <td style="width: 200px;">
                        <b>I have chosen to pay: </b>
                    </td>
                    <td>
                        <b>
                            <asp:Label runat="server" ID="lblPaymentDurationType"></asp:Label></b>
                    </td>
                </tr>
                <tr>
                    <td><b>Please charge me via: </b></td>
                    <td>
                        <asp:RadioButton runat="server" ID="rbtnViaCreditCard" Text="Credit Card" OnCheckedChanged="rbtnViaCreditCard_CheckedChanged" GroupName="PayVia" AutoPostBack="true" />
                        <asp:RadioButton runat="server" ID="rbtnViaInvoice" Text="Invoice" OnCheckedChanged="rbtnViaInvoice_CheckedChanged" GroupName="PayVia" AutoPostBack="true" />
                    </td>
                </tr>
                <tr runat="server" id="trEducational">
                    <td colspan="2"><b>What is your position at your School/University/College?:&nbsp;</b>

                        <asp:TextBox runat="server" ID="txtPosition" MaxLength="100" Width="200" CssClass="Position"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPosition" runat="server" ControlToValidate="txtPosition" ErrorMessage="Position is required." ValidationGroup="ValidateCompanyDetails"></asp:RequiredFieldValidator>
                    </td>
                </tr>
            </table>
            <asp:Panel ID="pnlPaymentType" Width="850" runat="server">
                <div style="padding-left: 20px; padding-bottom: 10px;">
                    <div style="padding-left: 83px;">
                        <uc:SetupCreditCardDetails runat="server" ID="pricingPlanSetupCreditCardDetails" OnCredidCardDetailsVisibilityChanged="pricingPlanSetupCreditCardDetails_CredidCardDetailsVisibilityChanged" DisplayMode="PricingPlan" />
                    </div>
                    <uc:CompanyDetails runat="server" ID="pricingPlanCompanyDetails" DisplayMode="PaymentDetail" />
                </div>
            </asp:Panel>
            <div>
                <div style="padding-left: 20px;">
                    <asp:Label runat="server" ID="lblAcceptPricing" />
                </div>
                <asp:CheckBox runat="server" ID="chkAcceptPricing" AutoPostBack="true" OnCheckedChanged="chkAcceptTerms_CheckedChanged" CssClass="chkTerms" />
                <asp:CheckBox runat="server" ID="chkAcceptTerms" AutoPostBack="true" CssClass="chkTerms" OnCheckedChanged="chkAcceptTerms_CheckedChanged" />
            </div>
            <div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
