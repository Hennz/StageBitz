using StageBitz.Common;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for Inventory Location Roles.
    /// </summary>
    public partial class InventoryLocationRoles : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for display mode.
        /// </summary>
        public enum DisplayMode
        {
            InviteMode,
            EditMode
        }

        #endregion Enums

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
        /// Gets or sets the name attribute of the RadioButtons.
        /// </summary>
        /// <value>
        /// The name attribute of the RadioButtons.
        /// </value>
        public string RadioButtonGroupName
        {
            get
            {
                if (ViewState["RadioButtonGroupName"] == null)
                {
                    ViewState["RadioButtonGroupName"] = string.Empty;
                }

                return ViewState["RadioButtonGroupName"].ToString();
            }
            set
            {
                ViewState["RadioButtonGroupName"] = value;
            }
        }

        /// <summary>
        /// Gets the location permissions.
        /// </summary>
        /// <value>
        /// The location permissions.
        /// </value>
        public Dictionary<int, int> LocationPermissions
        {
            get
            {
                int inventoryStaffCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
                int inventoryObserverCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
                int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
                int noAccessCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "NOINVENTORYACCESS").CodeId;

                Dictionary<int, int> locationPermissions = new Dictionary<int, int>();

                foreach (RepeaterItem item in rptrLocations.Items)
                {
                    if (item.ItemType == ListItemType.Item
                        || item.ItemType == ListItemType.AlternatingItem)
                    {
                        HtmlInputRadioButton rbtnLocationManager = (HtmlInputRadioButton)item.FindControl("rbtnLocationManager");
                        HtmlInputRadioButton rbtnInventoryStaff = (HtmlInputRadioButton)item.FindControl("rbtnInventoryStaff");
                        HtmlInputRadioButton rbtnInventoryObserver = (HtmlInputRadioButton)item.FindControl("rbtnInventoryObserver");
                        HtmlInputRadioButton rbtnNoAccess = (HtmlInputRadioButton)item.FindControl("rbtnNoAccess");
                        HiddenField hdnLocationId = (HiddenField)item.FindControl("hdnLocationId");
                        HiddenField hdnCanEdit = (HiddenField)item.FindControl("hdnCanEdit");

                        if (hdnLocationId != null && hdnCanEdit != null &&
                            rbtnLocationManager != null && rbtnInventoryStaff != null && rbtnInventoryObserver != null && rbtnNoAccess != null)
                        {
                            int locationId = 0;
                            bool canEdit = false;
                            if (int.TryParse(hdnLocationId.Value, out locationId)
                                && bool.TryParse(hdnCanEdit.Value, out canEdit) && canEdit
                                && (rbtnLocationManager.Checked || rbtnInventoryStaff.Checked || rbtnInventoryObserver.Checked || rbtnNoAccess.Checked))
                            {
                                Data.Location location = GetBL<LocationBL>().GetLocation(locationId);
                                if (location != null && location.IsActive)
                                {
                                    int permissionCodeId = rbtnLocationManager.Checked ? locationManagerCodeId :
                                        rbtnInventoryStaff.Checked ? inventoryStaffCodeId :
                                        rbtnInventoryObserver.Checked ? inventoryObserverCodeId : noAccessCodeId;

                                    locationPermissions.Add(locationId, permissionCodeId);
                                }
                            }
                        }
                    }
                }

                return locationPermissions;
            }
        }

        /// <summary>
        /// Gets or sets the inventory roles display mode.
        /// </summary>
        /// <value>
        /// The inventory roles display mode.
        /// </value>
        public DisplayMode InventoryRolesDisplayMode
        {
            get
            {
                if (ViewState["InventoryRolesDisplayMode"] == null)
                {
                    ViewState["InventoryRolesDisplayMode"] = DisplayMode.InviteMode;
                }

                return (DisplayMode)ViewState["InventoryRolesDisplayMode"];
            }

            set
            {
                ViewState["InventoryRolesDisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            rbtnInventoryObserverAll.Attributes["onclick"] = string.Concat(this.ClientID, "_OnSelectLocationRoleHeader('IO')");
            rbtnInventoryStaffAll.Attributes["onclick"] = string.Concat(this.ClientID, "_OnSelectLocationRoleHeader('IS')");
            rbtnLocationManagerAll.Attributes["onclick"] = string.Concat(this.ClientID, "_OnSelectLocationRoleHeader('LM')");
            rbtnNoAccessAll.Attributes["onclick"] = string.Concat(this.ClientID, "_OnSelectLocationRoleHeader('NoAccess')");

            thAllLocationManager.Visible = (InventoryRolesDisplayMode == DisplayMode.EditMode);
            tdAllLocationManager.Visible = (InventoryRolesDisplayMode == DisplayMode.EditMode);
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="viewingUserId">The viewing user identifier.</param>
        /// <param name="locationPermissions">The location permissions.</param>
        /// <returns>Tier 2 Location count.</returns>
        public int LoadData(Dictionary<int, int> locationPermissions, bool isReadOnly = false)
        {
            List<Data.DataTypes.Tier2Location> locations = GetBL<LocationBL>().GetTier2Locations(this.CompanyId, this.UserID, isReadOnly);
            rptrLocations.DataSource = locations;
            rptrLocations.DataBind();

            if (locations.Count > 0)
            {
                //rptrLocations.Items[rptrLocations.Items.Count -1].Visible = false;
                rptrLocations.Controls[rptrLocations.Controls.Count - 1].Visible = false;
            }

            bool hasDisabledLocations = locations.Where(l => !l.CanEdit).FirstOrDefault() != null;

            if (locations.Count == 0 || hasDisabledLocations)
            {
                rbtnInventoryObserverAll.Attributes["disabled"] = "disabled";
                rbtnInventoryStaffAll.Attributes["disabled"] = "disabled";
                rbtnLocationManagerAll.Attributes["disabled"] = "disabled";
                rbtnNoAccessAll.Attributes["disabled"] = "disabled";
            }
            else
            {
                rbtnInventoryObserverAll.Attributes.Remove("disabled");
                rbtnNoAccessAll.Attributes.Remove("disabled");
                rbtnLocationManagerAll.Attributes.Remove("disabled");
                rbtnInventoryStaffAll.Attributes.Remove("disabled");
            }

            if (locationPermissions != null)
            {
                LoadLocationRoles(locationPermissions);
            }

            InitializeAllLocationRadioButtons();

            return locations.Count;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the location roles.
        /// </summary>
        /// <param name="locationPermissions">The location permissions.</param>
        private void LoadLocationRoles(Dictionary<int, int> locationPermissions)
        {
            int inventoryStaffCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
            int inventoryObserverCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
            int noAccessCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "NOINVENTORYACCESS").CodeId;

            foreach (RepeaterItem item in rptrLocations.Items)
            {
                if (item.ItemType == ListItemType.Item
                    || item.ItemType == ListItemType.AlternatingItem)
                {
                    HtmlInputRadioButton rbtnLocationManager = (HtmlInputRadioButton)item.FindControl("rbtnLocationManager");
                    HtmlInputRadioButton rbtnInventoryStaff = (HtmlInputRadioButton)item.FindControl("rbtnInventoryStaff");
                    HtmlInputRadioButton rbtnInventoryObserver = (HtmlInputRadioButton)item.FindControl("rbtnInventoryObserver");
                    HtmlInputRadioButton rbtnNoAccess = (HtmlInputRadioButton)item.FindControl("rbtnNoAccess");
                    HiddenField hdnLocationId = (HiddenField)item.FindControl("hdnLocationId");

                    if (hdnLocationId != null && rbtnLocationManager != null && rbtnInventoryStaff != null && rbtnInventoryObserver != null)
                    {
                        int locationId = 0;
                        if (int.TryParse(hdnLocationId.Value, out locationId))
                        {
                            if (locationPermissions.ContainsKey(locationId))
                            {
                                rbtnLocationManager.Checked = locationPermissions[locationId] == locationManagerCodeId;
                                rbtnInventoryStaff.Checked = locationPermissions[locationId] == inventoryStaffCodeId;
                                rbtnInventoryObserver.Checked = locationPermissions[locationId] == inventoryObserverCodeId;
                                rbtnNoAccess.Checked = locationPermissions[locationId] == noAccessCodeId;
                            }
                            else
                            {
                                rbtnNoAccess.Checked = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes all location radio buttons.
        /// </summary>
        private void InitializeAllLocationRadioButtons()
        {
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "InitializeRadioButtons", string.Concat(this.ClientID, "_InitializeUI();"), true);
        }

        #endregion Private Methods
    }
}