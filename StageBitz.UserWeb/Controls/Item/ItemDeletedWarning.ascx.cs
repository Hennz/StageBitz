using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Item
{
    /// <summary>
    /// User control for item deleted warnings.
    /// </summary>
    public partial class ItemDeletedWarning : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item identifier.
        /// </summary>
        /// <value>
        /// The item identifier.
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
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault
        {
            get
            {
                if (ViewState["IsDefault"] == null)
                {
                    ViewState["IsDefault"] = false;
                }

                return (bool)ViewState["IsDefault"];
            }
            set
            {
                ViewState["IsDefault"] = value;
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
            if (!IsPostBack)
            {
                InitializePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDoneItemDeleted control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDoneItemDeleted_Click(object sender, EventArgs e)
        {
            //Data.Item item = this.GetBL<InventoryBL>().GetItem(this.ItemId);
            int companyId = int.Parse(btnDoneItemDeleted.CommandArgument);
            Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", companyId));
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Shows the item delete message popup.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        public void ShowItemDeleteMessagePopup(int itemId, int companyId)
        {
            this.CompanyId = companyId;
            this.ItemId = itemId;
            InitializePopup();
            popupItemDeleted.ShowPopup();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Initializes the popup.
        /// </summary>
        private void InitializePopup()
        {
            if (this.CompanyId > 0 && this.ItemId > 0)
            {
                btnDoneItemDeleted.CommandArgument = this.CompanyId.ToString();
                DeletedItemDatails deletedItemData = this.GetBL<InventoryBL>().GetDeleteItemData(this.ItemId);
                if (deletedItemData != null)
                {
                    ltrItemDeletedUser.Text = deletedItemData.ItemDeletedUser;
                    lnkItemDeletedUserEmail.InnerText = deletedItemData.ItemDeletedUserEmail;
                    lnkItemDeletedUserEmail.HRef = "mailto:" + deletedItemData.ItemDeletedUserEmail;
                }
                else
                {
                    Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);
                    if (item != null)
                    {
                        Data.User inventoryAdmin = GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                        if (inventoryAdmin != null)
                        {
                            ltrItemDeletedUser.Text = "An user";
                            lnkItemDeletedUserEmail.InnerText = inventoryAdmin.FirstName + " " + inventoryAdmin.LastName;
                            lnkItemDeletedUserEmail.HRef = "mailto:" + inventoryAdmin.Email1;
                        }
                    }
                }

                popupItemDeleted.IsDefault = this.IsDefault;
            }
        }

        #endregion Private Methods
    }
}