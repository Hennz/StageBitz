<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AttachHyperlink.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.AttachHyperlink" %>



<sb:PopupBox ID="popupAttachLinks" runat="server">
    <BodyContent>
        <table>
            <tr>
                <td>Hyperlink:
                </td>
                <td>
                    <div class="left sideErrorContainer">
                        <asp:TextBox ID="txtHyperlink" CssClass="" MaxLength="499" Width="300" runat="server" target="_blank"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="requiredHyperlink" runat="server" ToolTip="Hyperlink is required." CssClass="InlineError" ControlToValidate="txtHyperlink" ValidationGroup="valGroup"
                            ErrorMessage="*"></asp:RequiredFieldValidator>
                    </div>
                </td>
            </tr>
            <tr id="trDocumentLabel" runat="server">
                <td>Label:&nbsp;
                </td>
                <td>
                    <asp:TextBox ID="txtName" CssClass="" MaxLength="199" Width="100" runat="server"></asp:TextBox>
                </td>
            </tr>
        </table>

    </BodyContent>
    <BottomStripeContent>
        <asp:ImageButton ID="imgbtnRemoveLink" OnClick="imgbtnRemoveLink_Click" Width="20" Height="20" Style="float: left; margin-left: 5px; cursor: pointer;" ToolTip="Remove" runat="server" />
        <asp:Button ID="btnOK" runat="server" CssClass="buttonStyle" OnClick="btnOK_Click" Text="Ok" ValidationGroup="valGroup" />
        <input type="button" id="cancelButton" class="popupBoxCloser buttonStyle" value="Cancel" runat="server" />
        <%--     <asp:HiddenField ID="hdnAcceptStatus" runat="server" />--%>
    </BottomStripeContent>
</sb:PopupBox>
<sb:PopupBox ID="linkAlreadyDeleted" Title="Link Already Deletd" runat="server">
    <BodyContent>
        <div style="width: 300px;">
            This link has already been removed.
        </div>
    </BodyContent>
    <BottomStripeContent>
        <asp:Button runat="server" ID="btnConifrmAlreadyRemoved" CssClass="buttonStyle" OnClick="btnConifrmAlreadyRemoved_Click" Text="OK" CausesValidation="false" />

    </BottomStripeContent>
</sb:PopupBox>
<sb:PopupBox ID="popupLinkRemoveConfirmation" Title="Remove Hyperlink" Height="100" runat="server">
    <BodyContent>
        <div style="white-space: nowrap;">
            <asp:Literal ID="ltrlRemoveConfirmText" runat="server">Are you sure you want to remove this link?</asp:Literal>
        </div>
    </BodyContent>
    <BottomStripeContent>
        <asp:Button ID="btnRemoveConfirm" OnClick="btnRemoveConfirm_Click" CssClass="buttonStyle" runat="server" Text="Yes" />
        <input type="button" class="popupBoxCloser buttonStyle" value="No" />
    </BottomStripeContent>
</sb:PopupBox>




