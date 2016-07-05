<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemDeletedWarning.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Item.ItemDeletedWarning" %>


    <sb:ErrorPopupBox ID="popupItemDeleted" Title="This Item has been deleted" runat="server" ErrorCode="ItemDeleted" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="width: 450px;">
                    <asp:Literal ID="ltrItemDeletedUser" runat="server"/> has just deleted this Item.
                    <br/><br />
                    Please contact <a runat="server"  id="lnkItemDeletedUserEmail"/> if you have any questions.
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnDoneItemDeleted" runat="server" CssClass="buttonStyle ignoreDirtyFlag" OnClick="btnDoneItemDeleted_Click" Text="Done" />
            </BottomStripeContent>
    </sb:ErrorPopupBox>