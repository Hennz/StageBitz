<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentPackageSelector.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PaymentPackageSelector" %>

<div>
    <asp:UpdatePanel ID="upnlPckgeSelection" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <div style="float: right; clear: both; margin-right: 33px;" id="divUpgradeNotice" runat="server" visible="false">
                <p style="font-size: 11px;">* If you are upgrading, you'll pay a pro-rata amount for the remainder of the </p>
                    <p style="font-size: 11px; padding-left:9px;">current <asp:Label runat="server" ID="lblDuration"></asp:Label> to get your subscription up to date.</p>
            </div>
            <div style="width: 100%; margin: 10px;">
                <table>
                    <tr>
                        <td>Show prices as:
                        </td>
                        <td id="tdMonthlySelector" runat="server" class="paymentPackageDurationSelector">
                            <asp:LinkButton ID="lnkMonthlyPaymentPackage" Font-Underline="false" OnClick="lnkMonthlyPaymentPackage_Click" runat="server">Monthly</asp:LinkButton>
                        </td>
                        <td id="tdYearlySelector" runat="server" class="paymentPackageDurationSelector">
                            <asp:LinkButton ID="lnkYearlyPaymentPackage" Font-Underline="false" OnClick="lnkYearlyPaymentPackage_Click" Text="Yearly" runat="server">Yearly</asp:LinkButton>
                        </td>
                    </tr>
                </table>

            </div>
            <div class="left packageTypeTitle" style="margin-top: 10px; margin-right: 4px;">
                <h1>PROJECTS</h1>
            </div>
            <div class="left">
                <asp:ListView ID="lvProjectList" OnItemDataBound="lvProjectList_ItemDataBound" OnItemCommand="lvProjectList_ItemCommand" runat="server">
                    <LayoutTemplate>
                        <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkProjectPaymentPackage" ClientIDMode="AutoID" CommandArgument='<%# Eval("PackageTypeId") %>' CommandName="PackageSelected" runat="server">
                            <asp:Panel CssClass="left PaymentPackage" ID="tblPackage" runat="server">
                                <div style="min-height: 255px;">
                                    <div style="height: 50px; padding-top: 10px; vertical-align: top;">

                                        <h2>
                                            <asp:Literal ID="litPackageName" runat="server"></asp:Literal></h2>
                                    </div>

                                    <div style="min-height: 10px;">
                                        <asp:Literal ID="litPackageCharges" runat="server"></asp:Literal>
                                    </div>
                                    <div style="height: 0px; margin-top: 7px;">
                                        <asp:Literal ID="litTitleDescription" runat="server"></asp:Literal>
                                    </div>
                                    <div style="min-height: 50px;">
                                        <asp:Literal ID="litPackageLimits" runat="server"></asp:Literal>
                                    </div>
                                    <div style="min-height: 100px;">
                                        <asp:Literal ID="litPackageDisplayText" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </asp:Panel>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
            </div>
            <div style="clear: both"></div>
            <div class="left packageTypeTitle" style="margin-top: 33px; margin-right: 4px;">
                <h1>INVENTORY</h1>
            </div>
            <div class="left">
                <asp:ListView ID="lvInventoryList" OnItemDataBound="lvInventoryList_ItemDataBound" OnItemCommand="lvInventoryList_ItemCommand" runat="server">
                    <LayoutTemplate>
                        <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkInventoryPaymentPackage" ClientIDMode="AutoID" CommandArgument='<%# Eval("PackageTypeId") %>' CommandName="PackageSelected" runat="server">
                            <asp:Panel CssClass="left PaymentPackage" ID="tblInvPackage" runat="server">
                                <div style="min-height: 255px;">
                                    <div style="height: 50px; padding-top: 10px; vertical-align: top;">
                                        <h2>
                                            <asp:Literal ID="litPackageName" runat="server"></asp:Literal></h2>
                                    </div>
                                    <div style="min-height: 10px;">
                                        <asp:Literal ID="litPackageCharges" runat="server"></asp:Literal>
                                    </div>

                                    <div style="min-height: 50px;">
                                        <asp:Literal ID="litPackageLimits" runat="server"></asp:Literal>
                                    </div>

                                    <div style="min-height: 100px;">
                                        <asp:Literal ID="litPackageDisplayText" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </asp:Panel>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
            </div>
            <div style="clear: both"></div>
        </ContentTemplate>
    </asp:UpdatePanel>

</div>

