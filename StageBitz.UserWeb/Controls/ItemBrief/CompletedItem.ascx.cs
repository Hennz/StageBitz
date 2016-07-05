using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Data;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.ItemBrief
{
    public delegate void InformItemBriefDetailToSave();

    /// <summary>
    /// Delegate for inform item brief detail page to complete
    /// </summary>
    /// <param name="itembriefStatusCodeID">The itembrief status code identifier.</param>
    public delegate void InformItemBriefDetailToComplete(int itembriefStatusCodeID);

    /// <summary>
    /// Delegate for inform item brief detail to reload attachments
    /// </summary>
    public delegate void InformItemBriefDetailToReloadAttachments();

    /// <summary>
    /// Delegate for inform item brief detail to load pin tab
    /// </summary>
    public delegate void InformItemBriefDetailToLoadPinTab();

    /// <summary>
    /// User control for Completed Item tab of the Item brief details page.
    /// </summary>
    public partial class CompletedItem : UserControlBase
    {
        #region Events

        /// <summary>
        /// The inform item brief detail page to complete
        /// </summary>
        public InformItemBriefDetailToComplete InformItemBriefDetailToComplete;

        /// <summary>
        /// The inform item brief detail to reload attachments
        /// </summary>
        public InformItemBriefDetailToReloadAttachments InformItemBriefDetailToReloadAttachments;

        /// <summary>
        /// The inform item brief detail to load pin tab
        /// </summary>
        public InformItemBriefDetailToLoadPinTab InformItemBriefDetailToLoadPinTab;

        #endregion Events

        #region Properties and Fields

        /// <summary>
        /// The code task complete var
        /// </summary>
        private Code codeTaskComplete = Support.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED");

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
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

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
                    ViewState["ItemBriefId"] = 0;
                }

                return (int)ViewState["ItemBriefId"];
            }
            set
            {
                ViewState["ItemBriefId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is item brief read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is item brief read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsItemBriefReadOnly
        {
            get
            {
                if (ViewState["IsItemBriefReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsItemBriefReadOnly"];
                }
            }
            set
            {
                ViewState["IsItemBriefReadOnly"] = value;
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
                return ciItemCompleteTab.IsDirty || cihItemCompleteTab.IsDirty;
            }
            set
            {
                ciItemCompleteTab.IsDirty = value;
                cihItemCompleteTab.IsDirty = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                if (ViewState["ValidationGroup"] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ViewState["ValidationGroup"].ToString();
                }
            }

            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item brief has item.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this item brief has item; otherwise, <c>false</c>.
        /// </value>
        public bool HasItem
        {
            get
            {
                if (ViewState["HasItem"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["HasItem"];
                }
            }
            set
            {
                ViewState["HasItem"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirty
        {
            get
            {
                return cihItemCompleteTab.IsDirty || ciItemCompleteTab.IsDirty;
            }
        }

        #endregion Properties and Fields

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            cihItemCompletePopup.ItemDocumentList = ciItemCompletePopup.ItemDocumentList;
            cihItemCompleteTab.ItemDocumentList = ciItemCompleteTab.ItemDocumentList;

            if (!IsPostBack)
            {
                LoadData();
                //Get ItemType of the ItemBrief
                ItemBriefType itemBriefType = GetBL<ItemBriefBL>().GetItemBriefType(ItemBriefId);
                if (itemBriefType != null)
                    popupConfirmCompleteItemBrief.Title = string.Concat("Confirm Complete ", Utils.GetItemTypeById(itemBriefType.ItemTypeId).Name, " Brief");
            }

            InitailizeValidationGroup();

            cihItemCompleteTab.InformCompleteItemToInforToItemBrief += delegate()
            {
                if (InformItemBriefDetailToLoadPinTab != null)
                {
                    InformItemBriefDetailToLoadPinTab();
                }
            };
        }

        /// <summary>
        /// Handles the Click event of the btnSendEmailToIM control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendEmailToBookingManager_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Data.Item item = GetBL<InventoryBL>().GetItem(this.ciItemCompleteTab.ItemId);
                if (item != null)
                {
                    Data.User user = GetBL<PersonalBL>().GetUser(this.UserID);
                    Data.Booking booking = GetBL<InventoryBL>().GetBooking(this.ProjectId, GlobalConstants.RelatedTables.Bookings.Project);
                    Data.User bookingManager = GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);

                    if (user != null && booking != null && bookingManager != null)
                    {
                        string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
                        string userInput = HttpUtility.HtmlDecode(txtContactIMEmailBody.Text.Trim());
                        string emailContent = Utils.GetSafeHtmlFragments(userInput);
                        string itemUrl = string.Format("{0}/Inventory/ItemDetails.aspx?ItemId={1}&CompanyId={2}", userWebUrl, item.ItemId, item.CompanyId.Value);
                        string bookingUrl = string.Format("{0}/Inventory/BookingDetails.aspx?BookingId={1}&CompanyId={2}", userWebUrl, booking.BookingId, item.CompanyId.Value);

                        EmailSender.SendContactInventoryManagerForItemChangesDuetoBookingOverlapEmail(bookingManager.Email1,
                                bookingManager.FirstName, user.FirstName, item.Name, itemUrl,
                                GetBL<InventoryBL>().GetCompanyBookingNumber(booking.BookingId, item.CompanyId.Value).BookingNumber.ToString(CultureInfo.InvariantCulture),
                                bookingUrl, emailContent);
                        popupBookingOverlapContactBookingManager.HidePopup();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnVerifyBeforeComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnVerifyBeforeComplete_Click(object sender, EventArgs e)
        {
            if (PageBase.IsValid && !PageBase.StopProcessing)
            {
                if (PageBase.IsPageDirty)
                {
                    popupConfirmCompleteItemBrief.ShowPopup();
                }
                else
                {
                    if (!CanCompleteItemBrief())
                    {
                        popupErrorItemNotKept.ShowPopup();
                        return;
                    }
                    InitializeItemCompletePopup();
                    popupItemComplete.ShowPopup();
                    LoadCompleteItemPopupData();
                }
            }
        }

        /// <summary>
        /// Handles the item complete popup document list document changed event.
        /// </summary>
        protected void ciItemCompletePopup_CompleteItemDocumentListDocumentChanged()
        {
            if (!PageBase.StopProcessing)
            {
                cihItemCompletePopup.LoadControl();

                if (InformItemBriefDetailToReloadAttachments != null)
                {
                    InformItemBriefDetailToReloadAttachments();
                }
            }
        }

        /// <summary>
        /// Handles the item complete tab document list document changed event.
        /// </summary>
        protected void ciItemCompleteTab_CompleteItemDocumentListDocumentChanged()
        {
            if (!PageBase.StopProcessing)
            {
                cihItemCompleteTab.LoadControl();

                if (InformItemBriefDetailToReloadAttachments != null)
                {
                    InformItemBriefDetailToReloadAttachments();
                }

                if (InformItemBriefDetailToLoadPinTab != null)
                {
                    InformItemBriefDetailToLoadPinTab();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCompleteItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCompleteItem_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (ciItemCompleteTab.IsDirty || cihItemCompleteTab.IsDirty)
                {
                    popupConfirmSaveItemTab.ShowPopup();
                }
                else
                {
                    InitializeItemCompletePopup();
                    popupItemComplete.ShowPopup();
                    LoadCompleteItemPopupData();
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            bool canEditIteminItemBrief = GetBL<InventoryBL>().CanEditIteminItemBrief(ItemBriefId);
            this.IsReadOnly = IsItemBriefReadOnly || !canEditIteminItemBrief;

            Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetInUseOrCompleteItemBooking(ItemBriefId);
            Data.Item completedItem = null;
            Data.ItemVersionHistory itemVersionHistory = null;
            if (itemBooking != null)
            {
                completedItem = itemBooking.Item;
            }
            else
            {
                itemVersionHistory = GetBL<InventoryBL>().GetItemVersionHistoryByItemBriefId(ItemBriefId);
            }

            if (completedItem == null && itemVersionHistory == null)
            {
                ShowHideItemCompleteTab(false);
                this.HasItem = false;
                if (HasCompanyReachedInventoryLimit())
                {
                    if (this.GetBL<ItemBriefBL>().HasPinnedItems(ItemBriefId))
                    {
                        liInventoryLimitReachedMsg.Visible = true;
                        liInventoryLimitReachedMsg.InnerText = "Update its listing in the Company Inventory";
                    }
                    else
                    {
                        liInventoryLimitReachedMsg.Visible = false;
                    }
                }
                else
                {
                    liInventoryLimitReachedMsg.Visible = true;
                }

                var pinnedItems = this.GetBL<ItemBriefBL>().GetAllPinnedItems(ItemBriefId);
                int pinnedItemCount = pinnedItems.Count();

                //If the Item(s) were pined from Inventory. If so, display the text.
                if (pinnedItemCount > 1 || pinnedItemCount == 1 && !this.GetBL<InventoryBL>().IsItemGeneretedFromGivenItemBrief(pinnedItems.FirstOrDefault().ItemId, ItemBriefId))
                {
                    string headerText = (string.Format("<b>{0} {1}  been suggested for this {2} Brief. </b>", pinnedItemCount, pinnedItemCount == 1 ?
                        "Item has" : "Items have", this.GetBL<ItemBriefBL>().GetItemBriefType(ItemBriefId).ItemType.Name));
                    string bodyText = "</br>It's decision time...Before you can complete this you'll need to confirm if you wish to use an Item booked from the Inventory that is currently showing on the Pinboard tab. Once you've done that the details will appear here for you to check.</br></br>";
                    litForNotYetKeptItem.Text = string.Concat(headerText, bodyText);
                    divBlankNotice.Visible = false;
                }
                else
                {
                    litForNotYetKeptItem.Visible = false;
                    divBlankNotice.Visible = true;
                }
            }
            else
            {
                ShowHideItemCompleteTab(true);
                litForNotYetKeptItem.Visible = false;
                this.HasItem = true;
                if (itemVersionHistory == null)
                {
                    litNormalText.Visible = true;

                    litNormalText.Text = this.GetBL<InventoryBL>().GetDefaultMessageToDisplayInCompleteItemTab(completedItem.ItemId, ItemBriefId);
                }
                else
                {
                    divOriginalVersionText.Visible = true;
                    litNormalText.Visible = false;
                    if (GetBL<ProjectBL>().IsProjectClosed(ProjectId))
                    {
                        litProjCloseDate.Text = "when the project was closed on " + Support.FormatDate(GetBL<ProjectBL>().GetProjectArchive(ProjectId).ProjectClosedDate) + ".";
                    }
                    else
                    {
                        litProjCloseDate.Text = "when it was released to the Inventory on " + Support.FormatDate(GetBL<InventoryBL>().GetItemBriefItemReleaseDate(ItemBriefId)) + ".";
                    }

                    if (!this.GetBL<InventoryBL>().IsItemHidden(itemVersionHistory.ItemId)
                        && itemVersionHistory.Item.Code.SortOrder >= GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(itemVersionHistory.Item.CompanyId.Value, this.UserID, itemVersionHistory.Item.LocationId, false).SortOrder)
                    {
                        lnkItemName.Visible = true;
                        lblItemName.Visible = false;
                        lnkItemName.Text = Support.TruncateString(itemVersionHistory.Name, 40);
                        if (itemVersionHistory.Name.Length > 40)
                        {
                            lnkItemName.ToolTip = itemVersionHistory.Name;
                        }
                        lnkItemName.NavigateUrl = ResolveUrl(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}", itemVersionHistory.ItemId, Support.GetCompanyByProjectId(ProjectId).CompanyId));
                    }
                    else
                    {
                        lnkItemName.Visible = false;
                        lblItemName.Visible = true;
                        lblItemName.Text = Support.TruncateString(itemVersionHistory.Name, 40);
                        if (itemVersionHistory.Name.Length > 40)
                        {
                            lblItemName.ToolTip = itemVersionHistory.Name;
                        }
                    }
                }
            }

            InitailizeValidationGroup();

            InitializeItemCompletePopup();
            InitializeItemCompleteTab();
            IntializeCompleteButton();

            UpdateUpdatePanels();
        }

        /// <summary>
        /// Resets the properties.
        /// </summary>
        public void ResetProperties()
        {
            // Reset item tab controls.
            cihItemCompleteTab.ItemId = 0;
            ciItemCompleteTab.ItemId = 0;

            // Reset item popup controls.
            ciItemCompletePopup.ItemId = 0;
            cihItemCompletePopup.ItemId = 0;

            this.HasItem = false;
            InitailizeValidationGroup();
        }

        /// <summary>
        /// Hides the complete item popup.
        /// </summary>
        public void HideCompleteItemPopup()
        {
            popupItemComplete.HidePopup();
        }

        /// <summary>
        /// Intializes the complete button.
        /// </summary>
        public void IntializeCompleteButton()
        {
            bool canComplete = false;
            bool canEditIteminItemBrief = this.GetBL<InventoryBL>().CanEditIteminItemBrief(ItemBriefId, out canComplete);

            btnVerifyBeforeComplete.Enabled = !IsItemBriefReadOnly;

            Data.ItemBrief itemBrief = GetItemBrief(this.ItemBriefId);

            btnCompleteItem.Enabled = !IsItemBriefReadOnly;

            DataContext.Refresh(RefreshMode.StoreWins, itemBrief);

            bool isItemBriefCompleted = (itemBrief != null && itemBrief.ItemBriefStatusCodeId == Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED"));

            btnCompleteItem.Visible = (!isItemBriefCompleted && (canEditIteminItemBrief || canComplete));
            if (!canEditIteminItemBrief && canComplete)
            {
                btnCompleteItem.Text = "Complete";
            }

            UpdateUpdatePanels();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Determines whether [has company reached inventory limit].
        /// </summary>
        /// <returns></returns>
        private bool HasCompanyReachedInventoryLimit()
        {
            int companyId = this.GetBL<ItemBriefBL>().GetCompanyByItemBriefId(ItemBriefId).CompanyId;
            return this.GetBL<FinanceBL>().HasCompanyReachedInventoryLimit(companyId);
        }

        /// <summary>
        /// Updates the update panels.
        /// </summary>
        private void UpdateUpdatePanels()
        {
            uplCompleteItemBlankNotice.Update();
            uplCompleteItemPopups.Update();
            uplCompleteItemTabButton.Update();
            uplCompleteItemTabContent.Update();
            uplCompleteItemPopupContent.Update();
        }

        /// <summary>
        /// Shows the hide item complete tab.
        /// </summary>
        /// <param name="show">if set to <c>true</c> [show].</param>
        private void ShowHideItemCompleteTab(bool show)
        {
            string divBlankNoticeClientId = divBlankNotice.ClientID;
            string pnlCompletedItemClientId = pnlCompletedItemTab.ClientID;
            string script = string.Empty;

            if (show)
            {
                script = string.Format("$('#{0}').show();$('#{1}').hide();", pnlCompletedItemClientId, divBlankNoticeClientId);
            }
            else
            {
                script = string.Format("$('#{0}').hide();$('#{1}').show();", pnlCompletedItemClientId, divBlankNoticeClientId);
            }

            ScriptManager.RegisterStartupScript(this.Page, GetType(), "ShowHideItemCompleteTab", script, true);
        }

        /// <summary>
        /// Determines whether [is in use item hidden].
        /// </summary>
        /// <returns></returns>
        private bool IsInUseItemHidden()
        {
            return this.GetBL<InventoryBL>().GetItemBookingByRelatedTable(ItemBriefId, "ItemBrief", true).Item.IsHidden;
        }

        /// <summary>
        /// Gets the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        private StageBitz.Data.ItemBrief GetItemBrief(int itemBriefId)
        {
            return DataContext.ItemBriefs.FirstOrDefault(ib => ib.ItemBriefId == itemBriefId);
        }

        /// <summary>
        /// Initializes the item complete popup.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void InitializeItemCompletePopup()
        {
            ciItemCompletePopup.ClearInMemoryData();
            cihItemCompletePopup.ClearInMemoryData();

            ciItemCompletePopup.IsReadOnly = IsReadOnly;
            ciItemCompletePopup.ItemBriefId = ItemBriefId;
            ciItemCompletePopup.LoadControl();

            cihItemCompletePopup.ItemBriefId = ItemBriefId;
            cihItemCompletePopup.IsReadOnly = IsReadOnly;
            cihItemCompletePopup.FunctionPrefix = ciItemCompletePopup.ClientID;
            cihItemCompletePopup.LoadControl();

            divInventoryLimitReached.Visible = divInventoryLimitNotReached.Visible = false;
            if (HasCompanyReachedInventoryLimit())
            {
                if (this.GetBL<ItemBriefBL>().HasPinnedItems(ItemBriefId))
                {
                    if (IsInUseItemHidden())
                    {
                        divInventoryLimitReached.Visible = true;
                    }
                    else
                    {
                        divInventoryLimitNotReached.Visible = true;
                    }
                }
                else
                {
                    divInventoryLimitReached.Visible = true;
                }
            }
            else
            {
                divInventoryLimitNotReached.Visible = true;
            }
        }

        /// <summary>
        /// Initializes the item complete tab.
        /// </summary>
        private void InitializeItemCompleteTab()
        {
            ciItemCompleteTab.ClearInMemoryData();
            cihItemCompleteTab.ClearInMemoryData();

            ciItemCompleteTab.IsReadOnly = IsReadOnly;

            ciItemCompleteTab.ItemBriefId = cihItemCompleteTab.ItemBriefId = ItemBriefId;

            ciItemCompleteTab.LoadControl();

            cihItemCompleteTab.IsReadOnly = IsReadOnly;
            cihItemCompleteTab.FunctionPrefix = ciItemCompleteTab.ClientID;
            cihItemCompleteTab.LoadControl();
        }

        /// <summary>
        /// Determines whether this instance [can complete item brief].
        /// </summary>
        /// <returns></returns>
        private bool CanCompleteItemBrief()
        {
            List<ItemBooking> itemBookingList = GetBL<InventoryBL>().GetAllItemBookingByRelatedTable(ItemBriefId, "ItemBrief", true);

            //If no item is Pinned, the user should be able to complete the ItemBrief.
            //if there are Pinned items exist and if they are yet being confirmed
            if (itemBookingList.Count() > 0 && itemBookingList.Where(ibs => ibs.ItemBookingStatusCodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "PINNED").CodeId).Count() == itemBookingList.Count)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Initailizes the validation group.
        /// </summary>
        private void InitailizeValidationGroup()
        {
            // if item is not completed set fake validation group id
            string validationGroupTab = this.HasItem ? this.ValidationGroup : string.Concat(this.ValidationGroup, "Not");

            btnCompleteItem.ValidationGroup = validationGroupTab;
            cihItemCompleteTab.ValidationGroup = validationGroupTab;
            ciItemCompleteTab.ValidationGroup = validationGroupTab;
            btnVerifyBeforeComplete.ValidationGroup = this.ValidationGroup;

            // Popup is not validation for done button click
            string validationGroupPopup = string.Concat(this.ValidationGroup, "Popup");
            cihItemCompletePopup.ValidationGroup = validationGroupPopup;
            ciItemCompletePopup.ValidationGroup = validationGroupPopup;
            btnConfirmDetails.ValidationGroup = validationGroupPopup;
        }

        /// <summary>
        /// Loads the complete item popup data.
        /// </summary>
        private void LoadCompleteItemPopupData()
        {
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "InitializeCompleteItemPopup", "InitializeCompleteItemPopup();", true);
        }

        #endregion Private Methods
    }
}