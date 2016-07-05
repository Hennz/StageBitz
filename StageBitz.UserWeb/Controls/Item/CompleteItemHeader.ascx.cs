using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Item
{
    /// <summary>
    /// Delegate for inform complete item control to inform to item brief page.
    /// </summary>
    public delegate void InformCompleteItemToInforToItemBrief();

    /// <summary>
    /// Complete Item Header control
    /// </summary>
    public partial class CompleteItemHeader : UserControlBase
    {
        #region Events

        /// <summary>
        /// The inform complete item control to infor to item brief page.
        /// </summary>
        public InformCompleteItemToInforToItemBrief InformCompleteItemToInforToItemBrief;

        #endregion Events

        #region enum

        /// <summary>
        /// Enum for display mode of the control.
        /// </summary>
        public enum CompleteItemHeaderDisplayMode
        {
            ItemDetails,
            ItemBriefDetails
        }

        #endregion enum

        #region Properties

        /// <summary>
        /// Gets or sets the item brief id.
        /// </summary>
        /// <value>
        /// The item brief id.
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
        /// Gets or sets the item version id.
        /// </summary>
        /// <value>
        /// The item version id.
        /// </value>
        private int ItemVersionHistoryId
        {
            get
            {
                if (ViewState["ItemVersionHistoryId"] == null)
                {
                    ViewState["ItemVersionHistoryId"] = 0;
                }

                return (int)ViewState["ItemVersionHistoryId"];
            }

            set
            {
                ViewState["ItemVersionHistoryId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public int ItemId
        {
            get
            {
                if (ViewState["ItemId"] == null)
                {
                    ViewState["ItemId"] = 0;
                }

                return (int)ViewState["ItemId"];
            }

            set
            {
                ViewState["ItemId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    if (this.ItemId > 0)
                    {
                        Data.Item item = this.GetBL<InventoryBL>().GetItem(this.ItemId);

                        if (item != null)
                        {
                            ViewState["CompanyId"] = item.CompanyId;
                        }
                        else
                        {
                            ViewState["CompanyId"] = 0;
                        }
                    }
                    else
                    {
                        ViewState["CompanyId"] = 0;
                    }
                }

                return (int)ViewState["CompanyId"];
            }

            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
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
        /// Gets or sets a value indicating whether this instance is item created.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is item created; otherwise, <c>false</c>.
        /// </value>
        public bool IsItemCreated
        {
            get
            {
                if (ViewState["IsItemCreated"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsItemCreated"];
                }
            }

            set
            {
                ViewState["IsItemCreated"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default image id.
        /// </summary>
        /// <value>
        /// The default image id.
        /// </value>
        public int DefaultImageId
        {
            get
            {
                if (ViewState["DefaultImageId"] == null)
                {
                    ViewState["DefaultImageId"] = 0;
                }

                return (int)ViewState["DefaultImageId"];
            }

            set
            {
                ViewState["DefaultImageId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item type id.
        /// </summary>
        /// <value>
        /// The item type id.
        /// </value>
        public int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    ViewState["ItemTypeId"] = 0;
                }

                return (int)ViewState["ItemTypeId"];
            }

            set
            {
                ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the function prefix.
        /// </summary>
        /// <value>
        /// The function prefix.
        /// </value>
        public string FunctionPrefix
        {
            get
            {
                if (ViewState["FunctionPrefix"] == null)
                {
                    return string.Empty;
                }

                return ViewState["FunctionPrefix"].ToString();
            }

            set
            {
                ViewState["FunctionPrefix"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item brief document list.
        /// </summary>
        /// <value>
        /// The item brief document list.
        /// </value>
        public DocumentList ItemDocumentList
        {
            get;
            set;
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
                bool isDirty = false;
                bool.TryParse(hdnIsDirty.Value, out isDirty);
                return isDirty;
            }

            set
            {
                hdnIsDirty.Value = value.ToString();
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
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public CompleteItemHeaderDisplayMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    return CompleteItemHeaderDisplayMode.ItemDetails;
                }
                else
                {
                    return (CompleteItemHeaderDisplayMode)ViewState["DisplayMode"];
                }
            }

            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers and Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            thumbItemBrief.FunctionPrefix = this.FunctionPrefix;
            InitailizeValidationGroup();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterDirtyValidationScripts();
            hdnDefaultImageId.Value = this.DefaultImageId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Handles the Init event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Init(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the DocumentPicked event of the imagePickerDocumentList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Controls.Common.DocumentListDocumentPickedEventArgs" /> instance containing the event data.</param>
        protected void imagePickerDocumentList_DocumentPicked(object sender, Controls.Common.DocumentListDocumentPickedEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                DefaultImageId = e.DocumentMediaId;
                popupImagePicker.HidePopup();
                DisplayItemThumbnail();
                if (ItemId > 0)
                {
                    DataContext.SetItemDefaultImage(ItemId, DefaultImageId);
                }

                //Inform CompleteItem tab to inform ItemBrief
                if (InformCompleteItemToInforToItemBrief != null)
                {
                    InformCompleteItemToInforToItemBrief();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkbtnChangePreviewImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lnkbtnChangePreviewImage_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                DisplayItemThumbnail();
                imagePickerDocumentList.LoadData();
                popupImagePicker.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbtnChangeVisibility control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbtnChangeVisibility_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                sbInventoryUpdateVisibility.CompanyId = this.ItemId;
                sbInventoryUpdateVisibility.ItemId = this.ItemId;
                sbInventoryUpdateVisibility.LoadData();
                popupChangeVisibility.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateVisibility control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateVisibility_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                bool hasBooking = GetBL<InventoryBL>().IsItemInUse(this.ItemId) ||
                        GetBL<InventoryBL>().IsItemOverDue(this.ItemId) ||
                        GetBL<InventoryBL>().HasFutureBookingsForItem(this.ItemId);

                if (!hasBooking)
                {
                    if (GetBL<InventoryBL>().IsItemDeleted(this.ItemId))
                    {
                        this.PageBase.ShowErrorPopup(ErrorCodes.ItemDeleted);
                        popupChangeVisibility.HidePopup();
                        return;
                    }

                    List<int> items = new List<int>();
                    items.Add(this.ItemId);

                    sbInventoryUpdateVisibility.UpdateItems(items);
                    sbItemVisibilityToolTip.LoadData();
                    popupChangeVisibility.HidePopup();
                }
                else
                {
                    this.PageBase.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                }
            }
        }

        #endregion Event Handlers and Overrides

        #region Public Methods

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            InitailizeValidationGroup();
            LoadData();
            DisplayItemThumbnail();

            switch (this.DisplayMode)
            {
                case CompleteItemHeaderDisplayMode.ItemDetails:
                    divDescription.Visible = false;
                    trCreatedFor.Visible = true;
                    trLocation.Visible = true;
                    sbItemVisibilityToolTip.Visible = true;
                    Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);

                    upnlVisibility.Visible = Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, item.LocationId);
                    bool hasBooking = GetBL<InventoryBL>().IsItemInUse(this.ItemId) ||
                        GetBL<InventoryBL>().IsItemOverDue(this.ItemId) ||
                        GetBL<InventoryBL>().HasFutureBookingsForItem(this.ItemId);
                    lbtnChangeVisibility.Visible = !hasBooking;
                    lblChangeVisibility.Visible = hasBooking;
                    break;

                case CompleteItemHeaderDisplayMode.ItemBriefDetails:
                    divDescription.Visible = true;
                    trCreatedFor.Visible = false;
                    trLocation.Visible = false;
                    upnlVisibility.Visible = false;
                    sbItemVisibilityToolTip.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Clears the in memory data.
        /// </summary>
        public void ClearInMemoryData()
        {
            DefaultImageId = 0;
        }

        /// <summary>
        /// Sets up the thumbnail image display control with the default thumbnail
        /// image for this item brief.
        /// </summary>
        public void DisplayItemThumbnail()
        {
            dynamic defaultImage = null;

            if (DefaultImageId > 0 && !ItemDocumentList.DocumentMediaIdsToExclude.Contains(DefaultImageId))
            {
                defaultImage = (from m in DataContext.DocumentMedias
                                where m.DocumentMediaId == DefaultImageId && m.IsImageFile == true
                                select new { m.DocumentMediaId, m.Name }).FirstOrDefault();
            }
            else
            {
                if (ItemId > 0)
                {
                    defaultImage = (from m in DataContext.DocumentMedias
                                    where m.RelatedTableName == "Item" && m.RelatedId == ItemId
                                    && !ItemDocumentList.DocumentMediaIdsToExclude.Contains(m.DocumentMediaId)
                                    && m.IsImageFile == true
                                    orderby m.SortOrder descending
                                    select new { m.DocumentMediaId, m.Name }).FirstOrDefault();
                }
                else if (ItemBriefId > 0)
                {
                    //If the Proejct Status is Closed, Show the "ItemVersionHistory"
                    ItemVersionHistory itemVersionHistory = this.GetBL<InventoryBL>().GetItemVersionHistoryByItemBriefId(ItemBriefId);

                    if (itemVersionHistory == null)
                    {
                        defaultImage = (from m in DataContext.DocumentMedias
                                        where m.RelatedTableName == "ItemBrief" && m.RelatedId == ItemBriefId
                                        && !ItemDocumentList.DocumentMediaIdsToExclude.Contains(m.DocumentMediaId)
                                        && m.IsImageFile == true
                                        orderby m.SortOrder descending
                                        select new { m.DocumentMediaId, m.Name }).FirstOrDefault();
                    }
                    else
                    {
                        defaultImage = (from m in DataContext.DocumentMedias
                                        where m.RelatedTableName == "ItemVersionHistory" && m.RelatedId == itemVersionHistory.ItemVersionHistoryId
                                        && !ItemDocumentList.DocumentMediaIdsToExclude.Contains(m.DocumentMediaId)
                                        && m.IsImageFile == true
                                        orderby m.SortOrder descending
                                        select new { m.DocumentMediaId, m.Name }).FirstOrDefault();
                    }
                }
            }

            bool displayImagePicker = false;

            if (defaultImage != null)
            {
                DefaultImageId = defaultImage.DocumentMediaId;
                thumbItemBrief.DocumentMediaId = defaultImage.DocumentMediaId;
                thumbItemBrief.ImageTitle = defaultImage.Name;
                displayImagePicker = true;

                // If there is a default image, there should be more than one image for Image picking to work.
                displayImagePicker = ItemDocumentList.LoadedImageCount > 1;
            }
            else
            {
                DefaultImageId = 0;
                thumbItemBrief.DocumentMediaId = 0;
                thumbItemBrief.ImageTitle = string.Empty;

                // If there is no default image, there should be at least one image for Image picking to work.
                displayImagePicker = ItemDocumentList.LoadedImageCount > 0;
            }

            if (displayImagePicker && !IsReadOnly)
            {
                trChangePreviewImage.Visible = true;
                imagePickerDocumentList.Visible = true;
                imagePickerDocumentList.RelatedTableName = ItemDocumentList.RelatedTableName;
                imagePickerDocumentList.RelatedId = ItemDocumentList.RelatedId;
                imagePickerDocumentList.DocumentMediaIdsToExclude = ItemDocumentList.DocumentMediaIdsToExclude;

                if (defaultImage != null)
                {
                    imagePickerDocumentList.ExcludedDocumentMediaIds = new int[] { defaultImage.DocumentMediaId };
                }
            }
            else
            {
                trChangePreviewImage.Visible = false;
                imagePickerDocumentList.Visible = false;
            }

            upnlItemBriefThumb.Update();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            reqName.ControlToValidate = itemNameEdit.TextBox.ID;
            sbInventoryLocations.CompanyId = this.CompanyId;

            bool showAll = false;
            InitializeIds();
            LoadItemTypes();

            if (ItemId > 0)
            {
                sbItemVisibilityToolTip.ItemId = this.ItemId;
                sbItemVisibilityToolTip.LoadData();

                // Show/Hide in the place it's being used (ItemDetails or ItemBriefDetails)
                divBookedQty.Visible = !(ItemBriefId > 0);
                trItemStatus.Visible = false;
                Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);

                // Show location for users have No Access + Shared Inventory permissions
                if (item != null)
                {
                    Code visibilityCode = GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(item.CompanyId.Value,
                            this.UserID, item.LocationId, false);
                    showAll = visibilityCode.SortOrder >= Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IO").SortOrder;
                }

                if (ItemBriefId > 0)
                {
                    Data.ItemBooking itemBooking = this.GetBL<InventoryBL>().GetItemBookingByRelatedTable(ItemBriefId, "ItemBrief", true);
                    if (itemBooking != null && itemBooking.ToDate.HasValue)
                    {
                        trItemStatus.Visible = true;
                    }
                }
            }
            else if (ItemBriefId > 0)
            {
                trItemStatus.Visible = false;
                lblItemStatus.Text = string.Empty;
                divBookedQty.Visible = false;
            }

            sbInventoryLocations.LoadData(showAll: showAll);
            upnlItemBriefThumb.Update();
        }

        /// <summary>
        /// Loads the item types.
        /// </summary>
        private void LoadItemTypes()
        {
            //item type can be changed by the project admin and the company admin.
            bool canEdit = !IsReadOnly && DisplayMode == CompleteItemHeaderDisplayMode.ItemDetails;
            divItemTypeSelect.Visible = canEdit;
            divItemTypeStatic.Visible = !canEdit;

            Data.ItemType itemType = this.GetBL<ItemBriefBL>().GetItemType(this.ItemTypeId);
            if (canEdit)
            {
                this.ddItemTypes.Items.Clear();
                List<Data.ItemType> itemTypeList = Utils.GetALLItemTypes();
                this.ddItemTypes.Items.Add(new ListItem(itemType.Name, ItemTypeId.ToString(CultureInfo.InvariantCulture)));
                ddItemTypes.SelectedIndex = 0;
                if (itemTypeList.Count > 0)
                    this.ddItemTypes.AddItemGroup("Change to:");

                foreach (var it in itemTypeList.OrderBy(it => it.Name))
                {
                    if (it.ItemTypeId != this.ItemTypeId)
                    {
                        this.ddItemTypes.Items.Add(new ListItem(it.Name, it.ItemTypeId.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
            else if (itemType != null)
            {
                lblItemType.Text = itemType.Name;
            }
        }

        /// <summary>
        /// Initializes the ids.
        /// </summary>
        private void InitializeIds()
        {
            if (ItemId > 0)
            {
                StageBitz.Data.Item item = this.GetBL<InventoryBL>().GetItem(ItemId);

                if (item != null)
                {
                    IsItemCreated = true;
                    this.ItemTypeId = item.ItemTypeId.Value;
                }
            }
            else if (ItemBriefId > 0)
            {
                StageBitz.Data.ItemBrief itemBrief = this.GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId);

                if (itemBrief != null)
                {
                    Data.ItemBooking itemBooking = this.GetBL<InventoryBL>().GetItemBookingByRelatedTable(ItemBriefId, "ItemBrief", true);
                    Data.Item item = null;
                    if (itemBooking != null)
                    {
                        item = itemBooking.Item;
                    }

                    if (item != null)
                    {
                        ItemId = item.ItemId;
                        IsItemCreated = true;
                        this.ItemTypeId = item.ItemTypeId.Value;
                    }
                    else
                    {
                        IsItemCreated = false;
                        this.ItemTypeId = itemBrief.ItemBriefTypes.FirstOrDefault().ItemTypeId;
                    }
                }
            }
        }

        /// <summary>
        /// Registers the dirty validation scripts.
        /// </summary>
        private void RegisterDirtyValidationScripts()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), string.Concat("DirtyValidationScript", this.ClientID),
                string.Concat("CompleteItemHeaderDirtyValidation_", this.ClientID, "();"), true);
        }

        /// <summary>
        /// Initailizes the validation group.
        /// </summary>
        private void InitailizeValidationGroup()
        {
            itemNameEdit.TextBox.ValidationGroup = this.ValidationGroup;
            reqName.ValidationGroup = this.ValidationGroup;
            txtItemQuantity.ValidationGroup = this.ValidationGroup;
            rngItemQuantity.ValidationGroup = this.ValidationGroup;
            reqQuantity.ValidationGroup = this.ValidationGroup;
        }

        #endregion Private Methods
    }
}