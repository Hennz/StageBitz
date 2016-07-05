using StageBitz.Common;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for update inventory item visibility.
    /// </summary>
    public partial class InventoryUpdateVisibility : UserControlBase
    {
        #region Fields And Properties
        /// <summary>
        /// The _error item ids
        /// </summary>
        private Dictionary<int, string> _errorItemIds = new Dictionary<int, string>();

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
        /// Gets or sets the error item ids.
        /// </summary>
        /// <value>
        /// The error item ids.
        /// </value>
        public Dictionary<int, string> ErrorItemIds
        {
            get
            {
                return _errorItemIds;
            }
            set
            {
                _errorItemIds = value;
            }
        }

        /// <summary>
        /// Gets the visibility level.
        /// </summary>
        /// <value>
        /// The visibility level.
        /// </value>
        public int VisibilityLevel
        {
            get
            {
                int aboveSharedInventoryCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");
                int aboveInventoryObserverCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IO");
                int aboveInventoryStaffCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IS");
                int aboveInventoryAdminCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IA");

                return chkVisibilitySharedInventory.Checked ? aboveSharedInventoryCodeId :
                                chkVisibilityInventoryObservers.Checked ? aboveInventoryObserverCodeId :
                                chkVisibilityInventoryStaff.Checked ? aboveInventoryStaffCodeId : aboveInventoryAdminCodeId;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            InitializeUI();
        }

        /// <summary>
        /// Updates the items.
        /// </summary>
        /// <param name="itemIds">The item ids.</param>
        public void UpdateItems(List<int> itemIds)
        {
            if (!PageBase.StopProcessing)
            {
                bool hasChanges = false;
                ErrorItemIds = new Dictionary<int, string>();

                foreach (int itemId in itemIds)
                {
                    Data.Item item = GetBL<InventoryBL>().GetItem(itemId);
                    if (item != null && item.IsActive && item.CompanyId.Value == this.CompanyId)
                    {
                        if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, item.LocationId))
                        {
                            if (GetBL<InventoryBL>().IsItemInUse(itemId) || GetBL<InventoryBL>().IsItemOverDue(itemId) || GetBL<InventoryBL>().HasFutureBookingsForItem(itemId))
                            {
                                ErrorItemIds.Add(itemId, "Please complete or remove the booking for this item before changing its visibility.");
                            }
                            else
                            {
                                item.VisibilityLevelCodeId = this.VisibilityLevel;
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            ErrorItemIds.Add(itemId, "Sorry, you're not allowed to change this item's visibility.");
                        }
                    }
                }

                if (hasChanges)
                {
                    GetBL<InventoryBL>().SaveChanges();
                }

                if (ErrorItemIds.Count > 0)
                {
                    if (this.ItemId > 0)
                    {
                        popupConcurrencyVisibilityItemDetails.ShowPopup();
                    }
                    else
                    {
                        popupConcurrencyVisibilityBulkEdit.ShowPopup();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        public void InitializeUI()
        {
            if (this.ItemId == 0)
            {
                chkVisibilityInventoryObservers.Checked =
                    chkVisibilityInventoryStaff.Checked =
                    chkVisibilitySharedInventory.Checked =
                    chkVisibilityInventoryTeam.Checked = false;
            }
            else
            {
                Data.Code aboveSharedInventoryCode = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");
                Data.Code aboveInventoryObserverCode = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IO");
                Data.Code aboveInventoryStaffCode = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IS");
                Data.Code aboveInventoryAdminCode = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IA");

                Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);
                if(item != null)
                {
                    int sortOrder = item.Code.SortOrder;

                    chkVisibilitySharedInventory.Checked = aboveSharedInventoryCode.SortOrder <= sortOrder;
                    chkVisibilityInventoryObservers.Checked = aboveInventoryObserverCode.SortOrder <= sortOrder;
                    chkVisibilityInventoryStaff.Checked = aboveInventoryStaffCode.SortOrder <= sortOrder;

                    chkVisibilityInventoryTeam.Checked = chkVisibilityInventoryObservers.Checked || chkVisibilityInventoryStaff.Checked;

                    this.CompanyId = item.CompanyId.Value;
                }
            }


            Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);
            if (company != null)
            {
                ltrlChkVisibilityInventoryTeam.Text = string.Format("Inventory Team Members from <span title='{0}'>{1}</span>", company.CompanyName.Length > 30 ? company.CompanyName : string.Empty,
                    Support.TruncateString(company.CompanyName, 30));
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chkVisibilityInventoryObservers.Attributes["onchange"] =
                chkVisibilityInventoryStaff.Attributes["onchange"] =
                chkVisibilitySharedInventory.Attributes["onchange"] = string.Format("{0}_OnClickAnyOption(this)", this.ClientID);
            chkVisibilityInventoryTeam.Attributes["onchange"] = string.Format("{0}_OnClickCompanyTeam()", this.ClientID);
        }
        #endregion
    }
}