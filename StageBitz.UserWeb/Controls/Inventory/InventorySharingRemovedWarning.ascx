<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventorySharingRemovedWarning.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Inventory.InventorySharingRemovedWarning" %>

    <sb:PopupBox ID="popupInventorySharingRemoved" Title="Company has been removed" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="width: 400px;">
                    <asp:Literal ID="ltrRemovedCompanyName" runat="server"/> is no longer sharing their Inventory. 
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnDoneInventorySharingRemoved" runat="server" CssClass="buttonStyle ignoreDirtyFlag" OnClick="btnDoneInventorySharingRemoved_Click" Text="OK" />
            </BottomStripeContent>
    </sb:PopupBox>