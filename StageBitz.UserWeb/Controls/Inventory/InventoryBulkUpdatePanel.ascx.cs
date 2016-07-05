using StageBitz.Common;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// Delegate for Inform Bulk Update To Update.
    /// </summary>
    public delegate void InformCompanyInventoryToReloadBulkUpdate();

    public partial class InventoryBulkUpdatePanel : UserControlBase
    {
        /// <summary>
        /// The delimiter
        /// </summary>
        public const string Delimiter = ":";

        #region Events  and Delegates

        /// <summary>
        /// Delegate for inform company inventory to show error popup
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public delegate void InformCompanyInventoryToShowErrorPopup(ErrorCodes errorCode);

        /// <summary>
        /// The inform bulk update to update
        /// </summary>
        public InformCompanyInventoryToReloadBulkUpdate InformCompanyInventoryToReloadBulkUpdate;

        /// <summary>
        /// The inform company inventory to show error popup
        /// </summary>
        public InformCompanyInventoryToShowErrorPopup OnInformCompanyInventoryToShowErrorPopup;

        #endregion Events  and Delegates

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
        /// Gets the visibility update error item ids.
        /// </summary>
        /// <value>
        /// The visibility update error item ids.
        /// </value>
        public Dictionary<int, string> VisibilityUpdateErrorItemIds
        {
            get
            {
                return sbInventoryUpdateVisibility.ErrorItemIds;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            hdnBulkUpdateSelectedItems.Value = string.Empty;
            
            sbInventoryUpdateVisibility.CompanyId = CompanyId;
            sbInventoryUpdateVisibility.LoadData();
            divUpdateVisibility.Visible = Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID) || GetBL<InventoryBL>().IsCompanyLocationManagerAnyLocation(this.CompanyId, this.UserID);

            upnlBulkUpdate.Update();
        }

        public void LoadLoations()
        {
            sbInventoryLocations.CompanyId = CompanyId;
            sbInventoryLocations.LoadData();
        }

        #endregion Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the btnBulkUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBulkUpdate_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (this.Page.IsValid)
                {
                    if (this.GetBL<CompanyBL>().HasEditPermissionForInventoryStaff(this.CompanyId, UserID, sbInventoryLocations.SelectedLocationId))
                    {
                        int? locationId = sbInventoryLocations.SelectedLocationId;
                        Data.Code userVisibilityLevel = GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(this.CompanyId, UserID, sbInventoryLocations.SelectedLocationId, false);

                        if (locationId.HasValue)
                        {
                            Data.Location location = GetBL<LocationBL>().GetLocation(locationId.Value);
                            if (location == null)
                            {
                                popupInventoryLocationDeleted.ShowPopup();
                                sbInventoryLocations.SelectedLocationId = null;
                                upnlBulkUpdate.Update();
                                return;
                            }
                        }

                        lblToLocation.Text = Utils.ReverseEllipsize(GetBL<LocationBL>().GetLocationPath(locationId, this.CompanyId), 25);

                        string[] itemIdStrings = hdnBulkUpdateSelectedItems.Value.Split(Delimiter.ToCharArray());
                        bool hasChanges = false;

                        foreach (string itemIdString in itemIdStrings)
                        {
                            int itemId = 0;
                            if (int.TryParse(itemIdString, out itemId) && itemId > 0)
                            {
                                Data.Item item = GetBL<InventoryBL>().GetItem(itemId);
                                if (item != null && item.CompanyId.Value == this.CompanyId && item.Code.SortOrder >= userVisibilityLevel.SortOrder)
                                {
                                    item.LocationId = locationId;
                                    hasChanges = true;
                                }
                            }
                        }

                        if (hasChanges)
                        {
                            GetBL<InventoryBL>().SaveChanges();
                        }

                        if (InformCompanyInventoryToReloadBulkUpdate != null)
                        {
                            InformCompanyInventoryToReloadBulkUpdate();
                        }

                        ScriptManager.RegisterStartupScript(this.Page, GetType(), "ShowSavedMessage", "showNotification('" + bulkUpdateSavedNotice.ClientID + "', 5000);", true);
                        LoadData();
                    }
                    else
                    {
                        if (OnInformCompanyInventoryToShowErrorPopup != null)
                        {
                            OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                        }
                    }
                }
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
                if (this.Page.IsValid)
                {
                    if (Utils.IsCompanyInventoryAdmin(this.CompanyId, UserID) || GetBL<InventoryBL>().IsCompanyLocationManagerAnyLocation(this.CompanyId, UserID))
                    {
                        string[] itemIdStrings = hdnBulkUpdateSelectedItems.Value.Split(Delimiter.ToCharArray());
                        List<int> itemIds = new List<int>();

                        foreach (string itemIdString in itemIdStrings)
                        {
                            int itemId = 0;
                            if (int.TryParse(itemIdString, out itemId) && itemId > 0)
                            {
                                itemIds.Add(itemId);
                            }
                        }

                        sbInventoryUpdateVisibility.UpdateItems(itemIds);

                        if (InformCompanyInventoryToReloadBulkUpdate != null)
                        {
                            InformCompanyInventoryToReloadBulkUpdate();
                        }

                        if (itemIds.Count != sbInventoryUpdateVisibility.ErrorItemIds.Count) // Atlease few item has saved.
                        {
                            ScriptManager.RegisterStartupScript(this.Page, GetType(), "ShowSavedMessage", "showNotification('" + visibilitySavedNotice.ClientID + "', 5000);", true);
                        }

                        LoadData();
                    }
                    else
                    {
                        if (OnInformCompanyInventoryToShowErrorPopup != null)
                        {
                            OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                        }
                    }
                }
            }
        }

        #endregion Event Handlers
    }
}