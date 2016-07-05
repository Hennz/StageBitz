<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentPreview.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.DocumentPreview" %>

<script type="text/javascript">
    function <%= this.FunctionPrefix %>showDocumentPreview(documentMediaId) {
        if (documentMediaId > 0) {
            $("#<%= hdnDocumentMediaId.ClientID %>").val(documentMediaId);
            $("#<%= btnDocumentPreviewLaunch.ClientID %>").click();
        }
    }
</script>

<asp:UpdatePanel ID="upnlDocumentPreview" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hdnDocumentMediaId" runat="server" />

        <!-- This button is used to initiate a postback from client side. It's not displayed to the user. -->
        <asp:Button ID="btnDocumentPreviewLaunch" runat="server" Style="display: none;" OnClick="btnDocumentPreviewLaunch_Click" Text="DocumentPreview" />

        <sb:PopupBox ID="popupDocumentPreview" Title="File Properties" runat="server">
            <BodyContent>

                <div style="min-width: 200px; min-height: 150px; text-align: center;">
                    <img id="imgPreview" runat="server" src="" alt="Preview" />
                    <asp:Label Style="font-size: 120%; font-weight: bold; display: block;" ID="lblDocumentExtension" runat="server"></asp:Label>
                </div>

                <asp:TextBox ID="txtName" CssClass="focusEditField largeText" Style="text-align: center;" ToolTip="File label" MaxLength="200" Width="100%" runat="server"></asp:TextBox>
                <asp:TextBoxWatermarkExtender ID="documentLabelWatermark" SkinID="NoCSS" WatermarkCssClass="focusEditField largeText watermark" TargetControlID="txtName" WatermarkText="Click to set file label" runat="server">
                </asp:TextBoxWatermarkExtender>

                <sb:PopupBox ID="popupDocumentPreviewRemoveConfirmation" Title="Remove File" Height="100" runat="server">
                    <BodyContent>
                        <div style="white-space: nowrap;">
                            <asp:Literal ID="ltrlRemoveConfirmText" runat="server">Are you sure you want to remove this file?</asp:Literal>
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnRemoveConfirm" OnClick="btnRemoveConfirm_Click" CssClass="buttonStyle" runat="server" Text="Yes" />
                                 <input type="button" class="popupBoxCloser buttonStyle" value="No" />
                    </BottomStripeContent>
                </sb:PopupBox>

            </BodyContent>
            <BottomStripeContent>
                <a id="lnkDownload" runat="server" class="ignoreDirtyFlag" style="float: left; margin-left: 5px;" title="Download file" href="">
                    <img id="imgDownload" runat="server" style="width: 20px; height: 20px;" alt="Download" />
                </a>
                <asp:ImageButton ID="imgbtnRemoveDocument" Width="20" Height="20" Style="float: left; margin-left: 5px; cursor: pointer;" ToolTip="Remove" runat="server" />

                <asp:Button ID="btnDone" runat="server" OnClick="btnDone_Click" CssClass="buttonStyle" Text="Done" />
            </BottomStripeContent>
        </sb:PopupBox>

    </ContentTemplate>
</asp:UpdatePanel>
