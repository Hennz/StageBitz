<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvitationViewer.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.InvitationViewer" %>

<sb:PopupBox ID="popupViewInvitation" Title="Invitation Pending" runat="server">
    <BodyContent>
    
        <div style="width:300px; text-align:center; padding:10px;">
            <asp:Literal ID="ltrlInvitationText" runat="server"></asp:Literal>
        </div>

    </BodyContent>
    <BottomStripeContent>
        <asp:Button ID="btnAccept" CssClass="buttonStyle" OnClick="btnAccept_Click" runat="server" Text="Accept" />
        <asp:Button ID="btnDecline" CssClass="buttonStyle" OnClick="btnDecline_Click" runat="server" Text="Decline" />
        <input type="button" class="popupBoxCloser buttonStyle" value="Not yet" />
    </BottomStripeContent>
</sb:PopupBox>
