<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PinnedItems.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.ItemBrief.PinnedItems" %>
<%@ Register Src="~/Controls/Inventory/InventoryBusinessCard.ascx" TagName="BusinessCard"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentPreview.ascx" TagName="DocumentPreview"
    TagPrefix="sb" %>

<script type="text/javascript">
    function ClientInformItemBriefDetailToReloadCompleteItemTab(showEmptyTextVal, isReleaseItemVal) {
        var showEmptyText = (showEmptyTextVal === 'true');
        var isReleaseItem = (isReleaseItemVal === 'true');
        $(document).trigger('onClientInformItemBriefDetailToReloadCompleteItemTab', [showEmptyText, isReleaseItem]);
    };

    function ClientInformItemBriefDetailToSaveCompleteItem() {
        $(document).trigger('onClientInformItemBriefDetailToSaveCompleteItem');
    };

    $(document).on('onShowEditArea_InventoryBusinessCard', function (e, elementId) {
        $('table.BusinessCardTileDisplay:has(#' + elementId + ')').find('.actionButtons input').hide(500);
    });

    $(document).on('onHideEditArea_InventoryBusinessCard', function (e, elementId) {
        $('table.BusinessCardTileDisplay:has(#' + elementId + ')').find('.actionButtons input').show(500);
    });
</script>

<div style="overflow-x: hidden; overflow-y: hidden;">
    <sb:DocumentPreview ID="documentPreview" runat="server" />
    <asp:UpdatePanel ID="upnel" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:PopupBox ID="popupRemovePinnedItem" Title="Remove" runat="server">
                <BodyContent>
                    <div style="text-align: left; width: 300px;">
                        <p>
                            <strong>Are you sure? </strong>
                            <br />
                            This Item will be removed from your Pinboard tab and will show as 'Available' in
                            your Company Inventory so other people can use it.
                        </p>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnRemovePinnedItem" runat="server" OnClick="btnRemovePinnedItem_Click"
                        CssClass="buttonStyle" Text="I'm sure" />
                    <asp:Button ID="btnCancel" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupRemoveInUseItem" Title="Release" runat="server">
                <BodyContent>
                    <div style="text-align: left; width: 500px;">
                        <p>
                            <strong>Just checking...</strong>
                            <br />
                            <br />
                            Do you need to record that this Item was booked to this Project?
                             <br />
                            <br />
                            <asp:RadioButton ID="rdRelease" GroupName="grp1" runat="server" Text="No, just release it to the Inventory <br /><i> No record will be kept of this Item in the Project </i><br />" />
                            <asp:RadioButton ID="rdKeepaCopy" GroupName="grp1" Checked="true" runat="server" Text="Yes, Keep a record and then release it to the Inventory <br /><i>We'll keep a historical snapshot of this Item on the Complete Item tab</i><br />" />
                        </p>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnRemoveInUseItem" runat="server" OnClick="btnRemoveInUseItem_Click"
                        CssClass="buttonStyle" Text="Confirm" />
                    <asp:Button ID="Button1" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupRemoveItemGeneratedFromIB" Title="Release" runat="server">
                <BodyContent>
                    <div style="text-align: left; width: 300px;">
                        <p>
                            <strong>Just checking...</strong>
                            <br />
                            Do you want to release this Item to the Company Inventory?
                            <br />
                            <br />
                            We'll keep a historical snapshot of this Item on the Complete Item tab.
                        </p>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnConfirmRemoveOfItemGeneratedFromIB" runat="server" OnClick="btnConfirmRemoveOfItemGeneratedFromIB_Click"
                        CssClass="buttonStyle" Text="Confirm" />
                    <asp:Button ID="Button5" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popUpPinError" Title="Cannot Keep" runat="server">
                <BodyContent>
                    <div style="text-align: left; width: 300px;">
                        <p>
                            This Item has passed its booking period so cannot be kept for use at this time. Please check in the Inventory to see if it's available to book for a later period.
                        </p>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="Button4" runat="server" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox Width="300" ID="popupBoxKeepItem" Title="Keep" runat="server">
                <BodyContent>
                    <div style="text-align: left; line-height: 18px; width: 450px;">
                        <p>
                            <strong>Just checking...</strong>
                            <br />
                            <br />
                            When you select 'Keep':
                            <br />
                            1. This Item will be booked to your Project and its details will go into your Complete Item tab (but you can still add tasks, etc, if it needs 
                            'stuff' done to it).
                            <br />
                            2. Any other Items on your Pinboard will be removed and marked as 'Available' in the Company Inventory of other people can use them.
                            <br />
                            <br />
                            What do you want to do?
                        </p>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnKeep" runat="server" OnClick="btnKeep_Click" CssClass="buttonStyle"
                        Text="Keep it" />
                    <asp:Button ID="Button3" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupConfirmDirtySave" runat="server" Title="Save unsaved changes">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in this page. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnConfirm" CssClass="buttonStyle" runat="server" Text="Yes" OnClick="btnConfirmSaveDirtyChanges_Click" />
                    <asp:Button ID="btnCancelConfirmDirtyChanges" CssClass="buttonStyle" runat="server"
                        OnClick="btnCancelConfirmDirtyChanges_Click" Text="No" />
                </BottomStripeContent>
            </sb:PopupBox>
            <div id="divDefaultMessage" runat="server" class="left" style="margin-bottom: 10px;">
                <h2>
                    <div class="left">
                        Decisions, decisions...     
                    </div>
                    <div style="clear: both;"></div>
                </h2>
                If you decide to keep any of the Inventory Items shown here, selecting the 'tick' will confirm the booking.
            </div>
            <div id="divMsgCompletedItemBrief" visible="false" runat="server" style="margin-bottom: 10px;">
                <b>This Item has been booked from the Inventory.</b>
                <br />
                If you need to release it back to the Inventory, select the 'X' on the details card, otherwise all completed Items will be released once this Project is wrapped up.                
            </div>
            <div id="divMsgCompletedItemBriefNew" visible="false" runat="server" style="margin-bottom: 10px;">
                <b>This Item has been newly created for this Project...</b>
                <br />
                If you need to release it to the Inventory, select the 'X' on the details card, otherwise all completed Items will be released once this Project is wrapped up.                
            </div>
            <div style="clear: both;"></div>
            <asp:ListView runat="server" DataKeyNames="ItemId,ItemBookingId" OnItemDataBound="lvPinnedItem_OnItemDataBound" OnItemCreated="lvPinnedItem_ItemCreated"
                ClientIDMode="AutoID" OnItemCommand="lvPinnedItem_OnItemCommand" ID="lvPinnedItems">
                <LayoutTemplate>
                    <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                </LayoutTemplate>
                <EmptyDataTemplate>
                    <div class="lightNotice">
                        <h2>
                            <div style="text-align: center; padding-top: 30px; margin-left: 130px; width: 650px;">
                                <asp:Literal ID="litEmptyHeader" runat="server"></asp:Literal>
                                <asp:Literal ID="litBriefItemType" runat="server"></asp:Literal>
                                
                                
                            </div>
                        </h2>
                        <div style="text-align: center; padding-top: 20px; margin-left: 200px; margin-bottom: 40px; width: 450px;">
                            
                            <asp:Literal ID="litEmptyBodyText" runat="server"></asp:Literal>
                        </div>
                    </div>
                </EmptyDataTemplate>
                <ItemTemplate>
                    <div style="margin-bottom: 20px; width: 400px; margin-top: 10px; max-height: 205px; min-height: 205px;" class="left">
                        <table class="BusinessCardTileDisplay">
                            <tr>
                                <td style="vertical-align: top; text-align:right;" class="actionButtons">
                                    <asp:ImageButton ID="imgKeep" runat="server" ToolTip="Keep" CommandName="Keep" ImageUrl="~/Common/Images/tick.gif" />
                                    <asp:ImageButton ID="imgRemove" runat="server" ToolTip="Remove from Pinboard and release to Company Inventory"
                                        CommandName="Remove" ImageUrl="~/Common/Images/dialog_cancel.png" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <sb:BusinessCard ID="businessCard" onShowConcurencyErrorPopup="businessCard_ShowConcurencyErrorPopup" runat="server" />
                                </td>
                            </tr>
                        </table>
                    </div>
                </ItemTemplate>
            </asp:ListView>
            <sb:PopupBox ID="popupConcurrencyInvalidAvailableQuntity" runat="server" Title="Hold on... another booking has just been made.">
                <BodyContent>
                    <div style="width: 500px;">
                        Someone else has booked at least one of the units for the Item you're trying to book. When you close this pop-up, the units available for the Item will be refreshed.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" class="ignoreDirtyFlag popupBoxCloser buttonStyle" value="Ok" />
                </BottomStripeContent>
            </sb:PopupBox>

            <sb:PopupBox ID="popupConcurrencyItemAlreadyConfirmed" runat="server" Title="Hold on... Someone else has just confirmed this Item.">
                <BodyContent>
                    <div style="width: 500px;">
                        Once an Item's booking has been confirmed the quantity cannot be changed. You can contact the Inventory Administrator to let them know you would like to change the quantity or make another booking.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" class="ignoreDirtyFlag popupBoxCloser buttonStyle" value="Ok" />
                </BottomStripeContent>
            </sb:PopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
