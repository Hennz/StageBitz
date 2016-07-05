using StageBitz.Common;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Globalization;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// Delegete for show concurrency popup event.
    /// </summary>
    /// <param name="concurrencyType">Type of the concurrency.</param>
    public delegate void ShowConcurencyErrorPopup(StageBitz.UserWeb.Controls.ItemBrief.PinnedItems.PinnedItemsConcurrencyType concurrencyType);

    /// <summary>
    /// User control for Business card.
    /// </summary>
    public partial class InventoryBusinessCard : UserControlBase
    {
        /// <summary>
        /// Occurs when [show concurency error popup].
        /// </summary>
        public event ShowConcurencyErrorPopup ShowConcurencyErrorPopup;

        #region Properties

        /// <summary>
        /// Gets or sets the item brief id.
        /// </summary>
        /// <value>
        /// The item brief id.
        /// </value>
        public int? ItemBriefId
        {
            get
            {
                if (ViewState["ItemBriefId"] == null)
                {
                    return null;
                }

                return (int?)ViewState["ItemBriefId"];
            }
            set
            {
                ViewState["ItemBriefId"] = value;
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
                    return 0;
                }

                return (int)ViewState["ItemId"];
            }
            set
            {
                ViewState["ItemId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related table.
        /// </summary>
        /// <value>
        /// The related table.
        /// </value>
        public string RelatedTable
        {
            get
            {
                if (ViewState["RelatedTable"] == null)
                {
                    return string.Empty;
                }
                return (string)ViewState["RelatedTable"];
            }
            set
            {
                ViewState["RelatedTable"] = value;
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
            thumbItem.FunctionPrefix = "PinTab";
        }

        /// <summary>
        /// Handles the Click event of the lbtnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbtnOk_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (rntxtQuntity.Value.HasValue && RelatedTable == "Item" && ItemBriefId.HasValue)
                {
                    Data.Item item = this.GetBL<InventoryBL>().GetItem(ItemId);
                    Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetItemBookingByItemID(ItemId, ItemBriefId.Value, "ItemBrief");
                    if (itemBooking != null && itemBooking.ItemBookingStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED"))
                    {
                        if (item != null && itemBooking.ToDate.HasValue)
                        {
                            int? availableQuntity = this.GetBL<InventoryBL>().GetAvailableItemQuantity(this.ItemId, itemBooking.FromDate, itemBooking.ToDate.Value);
                            if (availableQuntity.HasValue && (availableQuntity + itemBooking.Quantity) >= rntxtQuntity.Value.Value)
                            {
                                itemBooking.Quantity = (int)rntxtQuntity.Value.Value;
                                itemBooking.LastUpdateDate = Now;
                                itemBooking.LastUpdatedBy = UserID;
                                this.GetBL<InventoryBL>().SaveChanges();
                            }
                            else
                            {
                                if (ShowConcurencyErrorPopup != null)
                                {
                                    ShowConcurencyErrorPopup(ItemBrief.PinnedItems.PinnedItemsConcurrencyType.InvalidAvailableQuntity);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ShowConcurencyErrorPopup != null)
                        {
                            ShowConcurencyErrorPopup(ItemBrief.PinnedItems.PinnedItemsConcurrencyType.ItemAlreadyConfirmed);
                        }
                    }
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
            int relatedId = (RelatedTable == "ItemBrief") && ItemBriefId.HasValue ? ItemBriefId.Value : ItemId;
            var defaultImage = (from m in DataContext.DocumentMedias
                                where m.RelatedTableName == RelatedTable && m.RelatedId == relatedId
                                && m.IsImageFile == true
                                orderby m.SortOrder descending
                                select new { m.DocumentMediaId, m.Name }).FirstOrDefault();

            if (defaultImage != null)
            {
                thumbItem.DocumentMediaId = defaultImage.DocumentMediaId;
                thumbItem.ImageTitle = defaultImage.Name;
            }

            if (RelatedTable == "ItemBrief")
            {
                if (ItemBriefId.HasValue)
                {
                    Data.ItemBrief itemBrief = GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId.Value);
                    if (itemBrief != null)
                    {
                        lblName.Text = Support.TruncateString(itemBrief.Name, 20);
                        if (itemBrief.Name.Length > 20)
                        {
                            lblName.ToolTip = itemBrief.Name;
                        }

                        if (itemBrief.Quantity != null)
                        {
                            lblQuantity.Text = itemBrief.Quantity.ToString();
                            rntxtQuntity.Value = itemBrief.Quantity;
                        }
                        else
                        {
                            trQuantity.Visible = false;
                        }

                        trStatus.Visible = false;
                        trCompany.Visible = false;
                        lblDescription.InnerText = Support.TruncateString(itemBrief.Description, 60);
                        if (itemBrief.Description != null && itemBrief.Description.Length > 60)
                            lblDescription.Attributes.Add("title", itemBrief.Description);
                    }

                    trLocation.Visible = false;
                }
            }
            else
            {
                Data.Item item = this.GetBL<InventoryBL>().GetItem(ItemId);
                if (item != null)
                {
                    lblName.Text = Support.TruncateString(item.Name, 20);
                    if (item.Name.Length > 20)
                    {
                        lblName.ToolTip = item.Name;
                    }

                    if (item.Quantity != null)
                    {
                        lblQuantity.Text = item.Quantity.ToString();
                    }
                    else
                    {
                        trQuantity.Visible = false;
                    }

                    trCompany.Visible = true;
                    string companyName = Support.GetCompanyNameById(Convert.ToInt32(item.CompanyId));
                    lblcompany.Text = Support.TruncateString(companyName, 20);
                    if (companyName.Length > 20)
                    {
                        lblcompany.ToolTip = companyName;
                    }

                    string path = GetBL<LocationBL>().GetLocationPath(item.LocationId, item.CompanyId.Value);
                    lblLocation.Text = Utils.ReverseEllipsize(path, 22);
                    if (path.Length > 22)
                    {
                        lblLocation.ToolTip = path;
                    }

                    // If both ItemBriefId and ItemId exists need to get information from ItemBooking (this is specific for Pinboard tab)
                    if (ItemBriefId.HasValue)
                    {
                        Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetItemBookingByItemID(ItemId, ItemBriefId.Value, "ItemBrief");

                        if (itemBooking != null && itemBooking.ToDate.HasValue)
                        {
                            litStatus.Text = GetBL<InventoryBL>().GetItemBookingStatus(itemBooking.ItemBookingId).Description;
                            lnkName.Visible = true;
                            lblName.Visible = false;

                            lnkName.Text = Support.TruncateString(item.Name, 20);
                            lnkName.NavigateUrl = string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}", this.ItemId, GetBL<ItemBriefBL>().GetItemBrief(itemBooking.RelatedId).Project.CompanyId);
                            if (item.Name.Length > 20)
                            {
                                lnkName.ToolTip = item.Name;
                            }

                            int availableQuntity = this.GetBL<InventoryBL>().GetAvailableItemQuantity(this.ItemId, itemBooking.FromDate, itemBooking.ToDate.Value);
                            int maxQty = availableQuntity + itemBooking.Quantity;
                            rntxtQuntity.MaxValue = maxQty;
                            lblAvailableQty.Text = maxQty.ToString(CultureInfo.InvariantCulture);

                            rntxtQuntity.MinValue = availableQuntity == 0 ? 0 : 1;
                            rntxtQuntity.Value = itemBooking.Quantity >= rntxtQuntity.MinValue ? itemBooking.Quantity : rntxtQuntity.MinValue;
                            lblQuantity.Text = itemBooking.Quantity.ToString(CultureInfo.InvariantCulture);

                            litFromDate.Text = Support.FormatDate(itemBooking.FromDate);
                            litToDate.Text = Support.FormatDate(itemBooking.ToDate);
                            trBookingFrom.Visible = true;
                            trBookingTo.Visible = true;
                        }
                        else
                        {
                            trStatus.Visible = false;
                        }

                        this.IsReadOnly = this.IsReadOnly ||
                            itemBooking.ItemBookingStatusCodeId != Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED") || !itemBooking.ToDate.HasValue ||
                            itemBooking.ToDate < Utils.Today;
                    }
                    else
                    {
                        trStatus.Visible = false;
                    }

                    lblDescription.InnerText = Support.TruncateString(item.Description, 60);
                    if (item.Description != null && item.Description.Length > 60)
                        lblDescription.Attributes.Add("title", item.Description);
                }
            }

            InitializeEditMode();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Initializes the edit mode.
        /// </summary>
        private void InitializeEditMode()
        {
            lbtnEdit.Visible = !this.IsReadOnly;
            divQtyEdit.Visible = !this.IsReadOnly;
        }

        #endregion Private Methods
    }
}