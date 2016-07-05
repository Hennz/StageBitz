using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Item
{
    /// <summary>
    /// Delegate for event when fires document list changed.
    /// </summary>
    public delegate void CompleteItemDocumentListDocumentChanged();

    /// <summary>
    /// The Complete Item Body Control
    /// </summary>
    public partial class CompleteItem : UserControlBase
    {
        #region Events

        /// <summary>
        /// event for notify complete item header when item is deleted from complete item list.
        /// </summary>
        public event CompleteItemDocumentListDocumentChanged CompleteItemDocumentListDocumentChanged;

        #endregion Events

        #region Enums

        /// <summary>
        /// Enum for display mode of the control.
        /// </summary>
        public enum CompleteItemDisplayMode
        {
            ItemDetails,
            ItemBriefDetails
        }

        #endregion Enums

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
        public CompleteItemDisplayMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    return CompleteItemDisplayMode.ItemDetails;
                }
                else
                {
                    return (CompleteItemDisplayMode)ViewState["DisplayMode"];
                }
            }

            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets the item document list.
        /// </summary>
        /// <value>
        /// The item document list.
        /// </value>
        public DocumentList ItemDocumentList
        {
            get
            {
                return documentList;
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
                bool isDirty = false;
                bool.TryParse(hdnIsDirty.Value, out isDirty);
                return isDirty;
            }

            set
            {
                hdnIsDirty.Value = value.ToString();
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            documentList.FunctionPrefix = this.ClientID;
            documentPreview.FunctionPrefix = this.ClientID;
            documentPreview.IsTextboxsDisabled = IsReadOnly || ItemBriefId > 0;
            documentPreview.IsReadOnly = IsReadOnly;

            helpTipItemFiles.Position = Telerik.Web.UI.ToolTipPosition.TopRight;
            helpTipItemFiles.RelativeTo = Telerik.Web.UI.ToolTipRelativeDisplay.Mouse;

            documentPreview.InformItemIsDeleted += delegate
            {
                ShowItemDeletedPopUp();
            };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterDirtyValidationScripts();
            SendDocumentMediaIdsForCilentSideUsage();
        }

        /// <summary>
        /// Documents the preview_ on document delete clicked.
        /// </summary>
        /// <param name="documentMediaId">The document media id.</param>
        protected void documentPreview_OnDocumentDeleteClicked(int documentMediaId)
        {
            if (!PageBase.StopProcessing)
            {
                List<int> tempExclutionList = documentList.DocumentMediaIdsToExclude;
                tempExclutionList.Add(documentMediaId);
                documentList.DocumentMediaIdsToExclude = tempExclutionList;
                documentList.LoadData();
                upnlDocumentList.Update();
            }
        }

        /// <summary>
        /// Handles the OnDocumentDeleted event of the documentPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void documentPreview_OnDocumentDeleted(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (this.CompleteItemDocumentListDocumentChanged != null)
                {
                    // In the case we are handling the documents in memory for first time of the item completion. Fire an event to reload header control.
                    this.CompleteItemDocumentListDocumentChanged();
                }
            }
        }

        /// <summary>
        /// Handles the OnDocumentAttributesChanged event of the documentPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void documentPreview_OnDocumentAttributesChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                documentList.LoadData();
                upnlDocumentList.Update();

                if (this.CompleteItemDocumentListDocumentChanged != null)
                {
                    // In the case we are handling the documents in memory for first time of the item completion. Fire an event to reload header control.
                    this.CompleteItemDocumentListDocumentChanged();
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            InitializeIds();
            if (ItemId > 0)
            {
                StageBitz.Data.Item item = this.GetBL<InventoryBL>().GetItem(ItemId);

                if (item != null)
                {
                    LoadItem(item);
                    documentPreview.ShouldRemoveFromDatabase = true;
                }
            }
            else if (ItemBriefId > 0)
            {
                Data.ItemVersionHistory itemVersionHistory = this.GetBL<InventoryBL>().GetItemVersionHistoryByItemBriefId(ItemBriefId);

                if (itemVersionHistory != null)
                {
                    LoadItemVersionHistory(itemVersionHistory);
                }
                else
                {
                    StageBitz.Data.ItemBrief itemBrief = this.GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId);

                    if (itemBrief != null)
                    {
                        LoadItemBrief(itemBrief);
                        documentPreview.ShouldRemoveFromDatabase = false;
                    }
                }
            }

            pnlCompletedItem.Style["display"] = "block";
            documentPreview.IsTextboxsDisabled = IsReadOnly || ItemBriefId > 0;
            documentPreview.IsReadOnly = IsReadOnly;
            documentPreview.InitializeUI();

            LoadImageList();

            switch (this.DisplayMode)
            {
                case CompleteItemDisplayMode.ItemDetails:
                    divDescription.Visible = true;
                    break;

                case CompleteItemDisplayMode.ItemBriefDetails:
                    divDescription.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Loads the image list.
        /// </summary>
        public void LoadImageList()
        {
            documentList.FunctionPrefix = this.ClientID;
            documentPreview.FunctionPrefix = this.ClientID;
            documentList.LoadData();
            divDocumentList.Style["overflow-x"] = (documentList.LoadedDocumentCount > 3) ? "scroll" : "hidden";
            upnlDocumentList.Update();
        }

        /// <summary>
        /// Clears the in memory data.
        /// </summary>
        public void ClearInMemoryData()
        {
            documentList.DocumentMediaIdsToExclude = new List<int>();
        }

        #endregion Public Methods

        #region Private Methods

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
                    }
                    else
                    {
                        IsItemCreated = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the document media ids for cilent side usage.
        /// </summary>
        private void SendDocumentMediaIdsForCilentSideUsage()
        {
            hdnDocumentIds.Value = string.Join(",", this.ItemDocumentList.DocumentMediaIds.Select(dmi => dmi.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Loads the item brief.
        /// </summary>
        /// <param name="itemBrief">The item brief.</param>
        private void LoadItemBrief(Data.ItemBrief itemBrief)
        {
            documentList.RelatedTableName = "ItemBrief";
            documentList.RelatedId = itemBrief.ItemBriefId;
        }

        /// <summary>
        /// Loads the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void LoadItem(Data.Item item)
        {
            documentList.RelatedTableName = "Item";
            documentList.RelatedId = item.ItemId;
        }

        /// <summary>
        /// Loads the item version history.
        /// </summary>
        /// <param name="itemVersionHistory">The item version history.</param>
        private void LoadItemVersionHistory(Data.ItemVersionHistory itemVersionHistory)
        {
            documentList.RelatedTableName = "ItemVersionHistory";
            documentList.RelatedId = itemVersionHistory.ItemVersionHistoryId;
        }

        /// <summary>
        /// Registers the dirty validation scripts.
        /// </summary>
        private void RegisterDirtyValidationScripts()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), string.Concat("DirtyValidationScript", this.ClientID),
                string.Concat("CompleteItemDirtyValidation_", this.ClientID, "();"), true);
        }

        /// <summary>
        /// Shows the item deleted pop up.
        /// </summary>
        private void ShowItemDeletedPopUp()
        {
            if (this.GetBL<InventoryBL>().IsItemDeleted(ItemId))
            {
                int companyId = (int)(this.GetBL<InventoryBL>().GetItem(ItemId).CompanyId);
                popupItemDeletedWarning.ShowItemDeleteMessagePopup(ItemId, companyId);
            }
            else if (DisplayMode == CompleteItemDisplayMode.ItemDetails && this.GetBL<InventoryBL>().IsItemInUse(ItemId))
            {
                popupItemPinned.ShowPopup();
            }

            upnlDocumentList.Update();
        }

        #endregion Private Methods
    }
}