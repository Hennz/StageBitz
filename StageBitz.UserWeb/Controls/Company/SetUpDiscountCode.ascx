<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SetUpDiscountCode.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.SetUpDiscountCode" %>
<asp:UpdatePanel ID="upnlDiscountCode" runat="server">
    <ContentTemplate>
        
        <div style="float: right;">
            <div style="float: left;" id="divDiscountSet" runat="server" visible="false">
                <div style="float: left; font-weight: bold;">
                    <asp:Literal ID="litDiscountApplied" runat="server"></asp:Literal>
                </div>
                <img src="~/Common/Images/msginfo.png" alt="InformationIcon" runat="server" id="imgDiscountCode"
                    style="display: block; float: left; margin-left: 3px;" />
                &nbsp;|&nbsp;
            </div>
            <asp:LinkButton ID="lnkSetUpDiscountCode" OnClick="ShowSetUpDiscountCode" runat="server"></asp:LinkButton>
        </div>
        <div id="divSuccessDiscount" style="margin-left: 5px; display: none; color: Gray; float: left;">
            Changes saved.
        </div>
        <sb:PopupBox ID="popupManageDiscount" Title="Apply Promotional Code" runat="server">
            <BodyContent>
                <div style="width: 350px;">
                    Promotional Code:
                            <asp:TextBox ID="txtDiscountCode" ValidationGroup="discountCreateValGroup" Width="100"
                                MaxLength="20" runat="server"></asp:TextBox>
                    <span id="spanErrorMsg" visible="true" style="margin-top: 15px; min-height: 20px;"
                        class="message error" runat="server" /><span id="spanDiscountavailable" visible="false"
                            runat="server">
                            <p>
                                This will replace the existing Promotional Code that is currently being applied to
                                        the Company.
                            </p>
                        </span>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnOk" CssClass="buttonStyle" runat="server" Text="OK" OnClick="SetUpDiscount" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>
        <div style="clear: both;">
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
