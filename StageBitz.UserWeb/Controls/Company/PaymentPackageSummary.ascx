<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentPackageSummary.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PaymentPackageSummary" %>
<%@ Register Src="~/Controls/Company/SetUpDiscountCode.ascx" TagName="SetUpDiscountCode"
    TagPrefix="uc" %>

<asp:UpdatePanel ID="upnlSummary" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hdnEducationalRate" runat="server" />
        <div class="PaymentSummary" style="width: 380px;">
            <table style="line-height: 20px;">
                <tr>
                    <td>Your Project level is: 
                    </td>
                    <td style="width: 120px; text-align: center;">
                        <b>
                            <asp:Literal ID="litProjectLevelPackage" runat="server"></asp:Literal>
                        </b>

                    </td>
                    <td style="width: 100px; text-align: right;">
                        <asp:Literal ID="litProjectLevelAmount" runat="server"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td>Your Inventory level is:
                    </td>
                    <td style="text-align: center;"><b>
                        <asp:Literal ID="litInventoryLevelPackage" runat="server"></asp:Literal>
                    </b>
                    </td>
                    <td style="width: 100px; text-align: right;">
                        <asp:Literal ID="litInventoryLevelAmount" runat="server"></asp:Literal>
                    </td>
                </tr>
            </table>
            <div runat="server" id="divCollegeSelection">
          <b>This account is for a school, college or university
                </b>
                <asp:CheckBox ID="chkIsCollege"  OnClick="ConfigureUI(this)" runat="server" />
                <br />
            </div>
            <div id="divEducationalDiscount" runat="server" style="margin-top: 5px; margin-left: 25px;" class="divEducationalDiscount">
                <div style="width: 350px; float: left;">
                    Your Educational discount is:
                    <b>
                        <asp:Literal ID="litEduDiscount" runat="server"></asp:Literal>
                        &nbsp;(Unlimited Users)
                    </b>
                </div>
                <br />
                <div style="clear: both;"></div>
            </div>
            <table style="line-height: 20px;">

                <tr class="trPromotionalCode" runat="server" id="trPromotionalCode">
                    <td>
                        <asp:Literal ID="litDiscountMsg" runat="server"></asp:Literal>&nbsp;
                    </td>
                    <td style="text-align: right;">
                        <uc:SetUpDiscountCode ID="setUpDiscountCode" runat="server" />
                    </td>
                </tr>
            </table>
            <div runat="server" style="text-align: right; border-top: 1px solid #665271;">
                <h2>Total: 
            <asp:Label class="lblTotaltoPay" ID="lblTotaltoPay" runat="server"></asp:Label>
                    <asp:Label class="hdnTotalAfterDiscount" ID="hdnTotalAfterDiscount" Style="display: none;" runat="server"></asp:Label>
                    <asp:Label class="hdnTotalAfterEducationalPackage" ID="hdnTotalAfterEducationalPackage" Style="display: none;" runat="server"></asp:Label>
                </h2>
                <div style="margin-top: -10px; margin-bottom: 5px;">
                    <h6>
                        <asp:Label class="lblDiscountedAmountText" ID="lblDiscountedAmountText" runat="server"></asp:Label></h6>
                </div>
                <asp:Label class="lblDiscountToSummary" ID="lblTotalAmountAfterPromotion" runat="server"></asp:Label>


            </div>

        </div>
    </ContentTemplate>

</asp:UpdatePanel>

