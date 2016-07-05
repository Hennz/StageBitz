<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompleteItem.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Item.CompleteItem" %>
<%@ Register Src="~/Controls/Common/DocumentList.ascx" TagName="DocumentList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentPreview.ascx" TagName="DocumentPreview"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/ItemDeletedWarning.ascx" TagName="ItemDeletedWarning" TagPrefix="sb" %>
<script type="text/javascript">
    function CompleteItemDirtyValidation_<%= this.ClientID %>() {
        var select = '#<%=pnlCompletedItem.ClientID%> :not(:has(.dirtyValidationExclude)) :input:not([type=hidden],:submit,:password,:button)';
        $(document).off('change.dirtyValidationCompletedItem', select);
        $(document).on('change.dirtyValidationCompletedItem', select, function () {
            if (CanSetDirty(this)) {
                $("#<%= hdnIsDirty.ClientID %>").val("True");
            }
        });
    }
</script>
<asp:Panel ID="pnlCompletedItem" runat="server">
    <div class="left" style="width: 100%;" id="divDescription" runat="server">
        Description:<br />
        <asp:TextBox ID="txtDescription" TextMode="MultiLine" Rows="4" runat="server" ValidationGroup="ItemFields"></asp:TextBox>
    </div>
    <br style="clear: both" />
    <br style="clear: both" />
    <div>
        <div class="dirtyValidationExclude">
            <sb:DocumentPreview ID="documentPreview" runat="server" OnDocumentDeleteClicked="documentPreview_OnDocumentDeleteClicked" OnDocumentAttributesChanged="documentPreview_OnDocumentAttributesChanged"
                OnDocumentDeleted="documentPreview_OnDocumentDeleted" />
        </div>
        <table style="padding-bottom: 3px;">
            <tr>
                <td>Attachments:
                </td>
                <td style="padding-left: 2px;">
                    <sb:HelpTip ID="helpTipItemFiles" Visible="true" runat="server" Width="470">
                        <p>
                            Don't forget, if you don't need any of these files to stay with this Item in the
                            Company Inventory, just click on them to view and delete.
                        </p>
                        <br />
                        <p>
                            They will not be deleted from this Project. You can add more files from the Attachments
                            Tab.
                        </p>
                    </sb:HelpTip>
                </td>
            </tr>
        </table>
        <asp:UpdatePanel ID="upnlDocumentList" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div id="divDocumentList" runat="server" style="overflow-x: hidden; width: 98%; padding: 5px 0px; margin-bottom: 5px;"
                    class="boxBorder">
                    <sb:DocumentList ID="documentList" runat="server" />
                </div>
                <sb:ItemDeletedWarning runat="server" ID="popupItemDeletedWarning" />
                <sb:PopupBox ID="popupItemPinned" Title="This Item has been Pinned" runat="server">
                    <BodyContent>
                        <div style="width: 400px;">
                            You cannot delete because the Item Has been Pinned. 
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <input type="button" class="popupBoxCloser buttonStyle" value="OK" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <asp:HiddenField runat="server" ID="hdnDocumentIds" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="DynamicFields" style="padding-top: 10px; margin-right: 15px;">
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hdnIsDirty" Value="False" />
</asp:Panel>
