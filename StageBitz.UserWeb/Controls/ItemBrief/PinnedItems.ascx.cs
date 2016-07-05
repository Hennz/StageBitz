using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Notification;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Inventory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.ItemBrief
{
    /// <summary>
    /// Delegate for inform item brief detail to load
    /// </summary>
    /// <param name="itemBriefIdStatus">The item brief identifier status.</param>
    public delegate void InformItemBriefDetailToLoad(int itemBriefIdStatus);

    /// <summary>
    /// Delegate for inform item brief detail to reload attachments from pin tab
    /// </summary>
    public delegate void InformItemBriefDetailToReloadAttachmentsFromPinTab();

    /// <summary>
    /// Delegate forinform item brief detail to reload complete item tab
    /// </summary>
    public delegate void InformItemBriefDetailToReloadAttachmentsFromCompleteItemTab();

    /// <summary>
    /// Delegate for inform item brief detail to get complete item dirty status
    /// </summary>
    public delegate void InformItemBriefDetailToGetCompleteItemDirtyStatus();

    /// <summary>
    /// Delegate for update pinned items count
    /// </summary>
    /// <param name="count">The count.</param>
    public delegate void UpdatePinnedItemsCount(int count);

    /// <summary>
    /// Delegate for inform parent to update complete item tab
    /// </summary>
    public delegate void InformParentToUpdateCompleteItemTab();

    /// <summary>
    /// Delegate for inform parent to show delayed popup
    /// </summary>
    /// <param name="itemId">The item identifier.</param>
    public delegate void InformParentToShowDelayedPopup(int itemId);

    /// <summary>
    /// User control for pinned items in item brief details page.
    /// </summary>
    public partial class PinnedItems : UserControlBase
    {
        #region Fields

        /// <summary>
        /// The is read only access var
        /// </summary>
        private bool IsReadOnlyAccess = false;

        #endregion Fields

        #region Enums

        /// <summary>
        /// Enum for pinned items concurrency type.
        /// </summary>
        public enum PinnedItemsConcurrencyType
        {
            InvalidAvailableQuntity,
            ItemAlreadyConfirmed
        }

        #endregion Enums

        #region Events

        /// <summary>
        /// The inform item brief detail to load
        /// </summary>
        public InformItemBriefDetailToLoad InformItemBriefDetailToLoad;

        /// <summary>
        /// The inform item brief detail to reload attachments from pin tab
        /// </summary>
        public InformItemBriefDetailToReloadAttachmentsFromPinTab InformItemBriefDetailToReloadAttachmentsFromPinTab;

        /// <summary>
        /// The inform item brief detail to reload complete item tab
        /// </summary>
        public InformItemBriefDetailToReloadAttachmentsFromCompleteItemTab InformItemBriefDetailToReloadCompleteItemTab;

        /// <summary>
        /// The inform item brief detail to get complete item dirty status
        /// </summary>
        public InformItemBriefDetailToGetCompleteItemDirtyStatus InformItemBriefDetailToGetCompleteItemDirtyStatus;

        /// <summary>
        /// The update pinned items count
        /// </summary>
        public UpdatePinnedItemsCount UpdatePinnedItemsCount;

        /// <summary>
        /// The inform parent to update complete item tab
        /// </summary>
        public InformParentToUpdateCompleteItemTab InformParentToUpdateCompleteItemTab;

        /// <summary>
        /// The inform parent to show delayed popup
        /// </summary>
        public InformParentToShowDelayedPopup InformParentToShowDelayedPopup;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the item brief identifier.
        /// </summary>
        /// <value>
        /// The item brief identifier.
        /// </value>
        public int ItemBriefId
        {
            get
            {
                if (ViewState["ItemBriefId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["ItemBriefId"];
            }
            set
            {
                ViewState["ItemBriefId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item booking identifier.
        /// </summary>
        /// <value>
        /// The item booking identifier.
        /// </value>
        private int ItemBookingId
        {
            get
            {
                if (ViewState["ItemBookingId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["ItemBookingId"];
            }
            set
            {
                ViewState["ItemBookingId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected item identifier.
        /// </summary>
        /// <value>
        /// The selected item identifier.
        /// </value>
        public int SelectedItemId
        {
            get
            {
                if (ViewState["SelectedItemId"] == null)
                {
                    ViewState["SelectedItemId"] = 0;
                }

                return (int)ViewState["SelectedItemId"];
            }
            set
            {
                ViewState["SelectedItemId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is complete item tab dirty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is complete item tab dirty; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleteItemTabDirty
        {
            get
            {
                if (ViewState["IsCompleteItemTabDirty"] == null)
                    return false;
                return (bool)ViewState["IsCompleteItemTabDirty"];
            }
            set
            {
                ViewState["IsCompleteItemTabDirty"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }

            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            documentPreview.FunctionPrefix = "PinTab";
            documentPreview.IsReadOnly = true;
            documentPreview.IsTextboxsDisabled = true;
        }

        /// <summary>
        /// Handles the ItemCreated event of the lvPinnedItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvPinnedItem_ItemCreated(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.EmptyItem)
            {
                Literal litBriefItemType = (Literal)e.Item.FindControl("litBriefItemType");
                string itemBriefType = GetBL<ItemBriefBL>().GetItemBriefType(ItemBriefId).ItemType.Name + " Brief";
                Literal litEmptyHeader = (Literal)e.Item.FindControl("litEmptyHeader");
                Literal litEmptyBodyText = (Literal)e.Item.FindControl("litEmptyBodyText");

                if (GetBL<ItemBriefBL>().IsItemBriefComplete(GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId)))
                {
                    litEmptyHeader.Text = "An Item has been used for this " + itemBriefType + " and released to the Inventory";
                    litEmptyBodyText.Text = "You can view a Historical Snapshot of this Item on the Complete Item tab.";
                }
                else
                {
                    litEmptyHeader.Text = "Is there something in your Company Inventory that could be perfect for this " + itemBriefType + "?";
                    litEmptyBodyText.Text = "You can pin them here as suggestions for approval.";
                }
            }
        }

        /// <summary>
        /// Handles the OnItemDataBound event of the lvPinnedItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvPinnedItem_OnItemDataBound(object sender, ListViewItemEventArgs e)
        {
            ListViewItem dataItem = (ListViewItem)e.Item;
            dynamic itemBriefItem = ((dynamic)e.Item.DataItem);
            int itemId = itemBriefItem.ItemId;

            InventoryBusinessCard businessCard = (InventoryBusinessCard)dataItem.FindControl("businessCard");
            businessCard.ItemId = itemId;
            businessCard.ItemBriefId = ItemBriefId;
            businessCard.RelatedTable = "Item";
            businessCard.IsReadOnly = this.IsReadOnly;
            businessCard.LoadData();

            ImageButton imgKeep = (ImageButton)dataItem.FindControl("imgKeep");
            ImageButton imgRemove = (ImageButton)dataItem.FindControl("imgRemove");
            //Check for Security
            imgKeep.Visible = imgRemove.Visible = !IsReadOnlyAccess;
            if (itemBriefItem.ItemBookingStatusCodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId || itemBriefItem.ItemBookingStatusCodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId)
            {
                //Check for the status
                imgKeep.Visible = false;
            }
        }

        /// <summary>
        /// Handles the OnItemCommand event of the lvPinnedItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvPinnedItem_OnItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                SelectedItemId = (int)lvPinnedItems.DataKeys[dataItem.DisplayIndex]["ItemId"];
                int itemBookingId = (int)lvPinnedItems.DataKeys[dataItem.DisplayIndex]["ItemBookingId"];

                switch (e.CommandName)
                {
                    case "Remove":

                        if (InformItemBriefDetailToGetCompleteItemDirtyStatus != null)
                        {
                            InformItemBriefDetailToGetCompleteItemDirtyStatus();
                        }

                        if (IsCompleteItemTabDirty)
                        {
                            popupConfirmDirtySave.ShowPopup();
                        }
                        else
                        {
                            ShowRemoveConfirmationMessage();
                        }
                        break;

                    case "Keep":
                        InventoryBL inventoryBL = new InventoryBL(DataContext);
                        Code bookingStatus = inventoryBL.GetItemBookingStatus(itemBookingId);

                        if (bookingStatus != null && bookingStatus.CodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNEDDELAYED"))
                        {
                            if (InformParentToShowDelayedPopup != null)
                            {
                                InformParentToShowDelayedPopup(SelectedItemId);
                            }
                        }
                        else if (inventoryBL.CanKeepItem(ItemBriefId, SelectedItemId))
                        {
                            popupBoxKeepItem.ShowPopup();
                        }
                        else if (!GetBL<InventoryBL>().IsItemDeleted(SelectedItemId))
                        {
                            //Needs to be provided by Mat
                            popUpPinError.ShowPopup();
                        }

                        break;
                }

                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmRemoveOfItemGeneratedFromIB control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmRemoveOfItemGeneratedFromIB_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                IsCompleteItemTabDirty = false;
                //Call the SP 'ReleaseItemBriefToInventory'
                DataContext.ReleaseItemBriefToInventory(ProjectId, UserID, ItemBookingId, SelectedItemId, ItemBriefId, true);
                GetBL<NotificationBL>().GenerateNotificationsForBookings(this.UserID, ItemBriefId, SelectedItemId, ProjectId, NotificationBL.BookingAction.RemoveWithSnapshot);

                popupRemoveItemGeneratedFromIB.HidePopup();

                if (InformItemBriefDetailToLoad != null)
                {
                    InformItemBriefDetailToLoad(DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ItemBriefId).FirstOrDefault().ItemBriefStatusCodeId);
                }

                if (InformItemBriefDetailToReloadCompleteItemTab != null)
                {
                    InformItemBriefDetailToReloadCompleteItemTab();
                    RegisterScript("InformItemBriefDetailToReloadCompleteItemTab", false, true);
                }
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmDirtyChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmDirtyChanges_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                IsCompleteItemTabDirty = false;
                popupConfirmDirtySave.HidePopup();
                ShowRemoveConfirmationMessage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmSaveDirtyChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmSaveDirtyChanges_Click(object sender, EventArgs e)
        {
            //Inform parent to inform "CompleteItem Control" to save its content
            if (!PageBase.StopProcessing)
            {
                RegisterScript("InformItemBriefDetailToSaveCompleteItem", string.Empty);

                IsCompleteItemTabDirty = false;
                popupConfirmDirtySave.HidePopup();
                ShowRemoveConfirmationMessage();
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnKeep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnKeep_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                DataContext.BookItem(ItemBriefId, SelectedItemId, UserID);
                GetBL<NotificationBL>().GenerateNotificationsForBookings(this.UserID, ItemBriefId, SelectedItemId, ProjectId, NotificationBL.BookingAction.Keep);

                divDefaultMessage.Visible = false;
                bool hasGeneratedFromItemBrief = GetBL<InventoryBL>().IsItemGeneretedFromGivenItemBrief(SelectedItemId, ItemBriefId);
                divMsgCompletedItemBrief.Visible = !hasGeneratedFromItemBrief;
                divMsgCompletedItemBriefNew.Visible = hasGeneratedFromItemBrief;

                popupBoxKeepItem.HidePopup();
                FireEventsRelatedToKeep();
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemovePinnedItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemovePinnedItem_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                GetBL<InventoryBL>().RemoveItemFromItemBrief(SelectedItemId, ItemBriefId);
                GetBL<NotificationBL>().GenerateNotificationsForBookings(this.UserID, ItemBriefId, SelectedItemId, ProjectId, NotificationBL.BookingAction.Remove);
                popupRemovePinnedItem.HidePopup();
                if (InformItemBriefDetailToReloadCompleteItemTab != null)
                {
                    InformItemBriefDetailToReloadCompleteItemTab();
                    RegisterScript("InformItemBriefDetailToReloadCompleteItemTab", true, true);
                }

                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveInUseItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveInUseItem_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                NotificationBL.BookingAction action = NotificationBL.BookingAction.Remove;
                if (rdKeepaCopy.Checked)
                {
                    action = NotificationBL.BookingAction.RemoveWithSnapshot;
                    DataContext.ReleaseItemBriefToInventory(ProjectId, UserID, ItemBookingId, SelectedItemId, ItemBriefId, true);
                }
                else
                {
                    action = NotificationBL.BookingAction.Remove;
                    GetBL<InventoryBL>().RemoveInUseItemFromItemBrief(ItemBriefId, UserID);
                }

                GetBL<NotificationBL>().GenerateNotificationsForBookings(this.UserID, ItemBriefId, SelectedItemId, ProjectId, action);
                //DataContext.RemoveInUseItemFromItemBrief(ItemBriefId, true, true, UserID);

                divDefaultMessage.Visible = true;
                divMsgCompletedItemBrief.Visible = false;
                divMsgCompletedItemBriefNew.Visible = false;

                popupRemoveInUseItem.HidePopup();

                if (InformItemBriefDetailToReloadAttachmentsFromPinTab != null)
                {
                    InformItemBriefDetailToReloadAttachmentsFromPinTab();
                }

                if (InformItemBriefDetailToLoad != null)
                {
                    InformItemBriefDetailToLoad(DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ItemBriefId).FirstOrDefault().ItemBriefStatusCodeId);
                }

                if (InformItemBriefDetailToReloadCompleteItemTab != null)
                {
                    InformItemBriefDetailToReloadCompleteItemTab();
                    RegisterScript("InformItemBriefDetailToReloadCompleteItemTab", rdKeepaCopy.Checked ? false : true, true);
                }

                LoadData();
            }
        }

        /// <summary>
        /// Businesses the card_ show concurency error popup.
        /// </summary>
        /// <param name="concurrencyType">Type of the concurrency.</param>
        protected void businessCard_ShowConcurencyErrorPopup(PinnedItemsConcurrencyType concurrencyType)
        {
            switch (concurrencyType)
            {
                case PinnedItemsConcurrencyType.InvalidAvailableQuntity:
                    popupConcurrencyInvalidAvailableQuntity.ShowPopup();
                    break;

                case PinnedItemsConcurrencyType.ItemAlreadyConfirmed:
                    popupConcurrencyItemAlreadyConfirmed.ShowPopup();
                    FireEventsRelatedToKeep();
                    break;
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            int pinStatusCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "PINNED").CodeId;

            //Load all the Pinned Items
            List<ItemBooking> itemBriefItems = this.GetBL<ItemBriefBL>().GetAllPinnedItems(ItemBriefId);
            IsReadOnlyAccess = Support.IsReadOnlyRightsForProject(ProjectId);
            lvPinnedItems.DataSource = itemBriefItems;

            if (UpdatePinnedItemsCount != null)
            {
                UpdatePinnedItemsCount(itemBriefItems.Count());
            }

            divDefaultMessage.Visible = (itemBriefItems.Where(ibi => ibi.ItemBookingStatusCodeId == pinStatusCodeId).Count() > 0);
            lvPinnedItems.DataBind();
            DisplayCompleteItemBriefMessage(itemBriefItems);
            upnel.Update();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Displays the complete item brief message.
        /// </summary>
        /// <param name="itemBriefItems">The item brief items.</param>
        private void DisplayCompleteItemBriefMessage(List<ItemBooking> itemBriefItems)
        {
            int inUseCompleteStatusCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;
            int inUseStatusCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;

            if (itemBriefItems.Where(ibi => (ibi.ItemBookingStatusCodeId == inUseStatusCodeId || ibi.ItemBookingStatusCodeId == inUseCompleteStatusCodeId)).Count() == 1)
            {
                if (lvPinnedItems.Items.Count == 1)
                {
                    int itemId = (int)lvPinnedItems.DataKeys[0].Value;
                    divMsgCompletedItemBriefNew.Visible = GetBL<InventoryBL>().IsItemGeneretedFromGivenItemBrief(itemId, ItemBriefId);
                    divMsgCompletedItemBrief.Visible = !divMsgCompletedItemBriefNew.Visible;
                }
                else
                {
                    divMsgCompletedItemBrief.Visible = true;
                }
            }
            else
            {
                divMsgCompletedItemBrief.Visible = divMsgCompletedItemBriefNew.Visible = false;
            }
        }

        /// <summary>
        /// Shows the remove confirmation message.
        /// </summary>
        private void ShowRemoveConfirmationMessage()
        {
            Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetItemBookingByItemID(SelectedItemId, ItemBriefId, "ItemBrief");
            //Check if a new Item is created and appears on the pinboard tab and the 'X' button is selected
            if (itemBooking != null)
            {
                ItemBookingId = itemBooking.ItemBookingId;
                bool isItemBriefItemInUse = itemBooking.ItemBookingStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE")
                    || itemBooking.ItemBookingStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");

                //Make sure that there is no Item already attached.
                if (isItemBriefItemInUse && GetBL<InventoryBL>().IsItemGeneretedFromGivenItemBrief(SelectedItemId, ItemBriefId))
                {
                    popupRemoveItemGeneratedFromIB.ShowPopup();
                }
                //Check the status of the Item. If it is "In Use" with the IB.
                //If it is Just "Pinned" just show the short confirmation
                else if (isItemBriefItemInUse || GetBL<ItemBriefBL>().IsItemBriefComplete(GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId)))
                {
                    //Display InUse message
                    popupRemoveInUseItem.ShowPopup();
                }
                else if (itemBooking.ItemBookingStatusCodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "PINNED").CodeId)
                {
                    //Display Pinned message
                    popupRemovePinnedItem.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Registers the script.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="showEmptyText">if set to <c>true</c> [show empty text].</param>
        /// <param name="isReleaseItem">if set to <c>true</c> [is release item].</param>
        private void RegisterScript(string method, bool showEmptyText, bool isReleaseItem)
        {
            string script = string.Concat("Client", method);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), script, string.Format("{0}('{1}', '{2}');", script,
                showEmptyText.ToString(CultureInfo.InvariantCulture).ToLower(),
                isReleaseItem.ToString(CultureInfo.InvariantCulture).ToLower()), true);
        }

        /// <summary>
        /// Registers the script.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="param">The parameter.</param>
        private void RegisterScript(string method, string param)
        {
            string script = string.Concat("Client", method);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), script, string.Format("{0}('{1}');", script, param), true);
        }

        /// <summary>
        /// Fires the events related to keep.
        /// </summary>
        private void FireEventsRelatedToKeep()
        {
            if (InformItemBriefDetailToReloadAttachmentsFromPinTab != null)
            {
                InformItemBriefDetailToReloadAttachmentsFromPinTab();
            }

            if (InformItemBriefDetailToLoad != null)
            {
                InformItemBriefDetailToLoad(DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ItemBriefId).FirstOrDefault().ItemBriefStatusCodeId);
            }

            if (InformItemBriefDetailToReloadCompleteItemTab != null)
            {
                InformItemBriefDetailToReloadCompleteItemTab();
                RegisterScript("InformItemBriefDetailToReloadCompleteItemTab", false, false);
            }

            if (InformParentToUpdateCompleteItemTab != null)
            {
                InformParentToUpdateCompleteItemTab();
            }
        }

        #endregion Private Methods
    }
}