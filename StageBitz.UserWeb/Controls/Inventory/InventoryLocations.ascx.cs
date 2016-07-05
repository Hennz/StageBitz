using StageBitz.Common;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// Delegate for inform inventory location change.
    /// </summary>
    public delegate void LocationChanged();

    /// <summary>
    /// Display warning banner when company is suspended by Stagebitz admin.
    /// </summary>
    public partial class InventoryLocations : UserControlBase
    {
        #region Enum

        /// <summary>
        /// Enum for display mode.
        /// </summary>
        public enum DisplayMode
        {
            SearchInventory, // Sever postback on node select
            Generic, //
            LocationAdmin,
            Admin, // Client side function call on node select
        }

        #endregion Enum

        #region Properties

        /// <summary>
        /// The location changed.
        /// </summary>
        public LocationChanged LocationChanged;

        /// <summary>
        /// Gets or sets the inventory location display mode.
        /// </summary>
        /// <value>
        /// The inventory location display mode.
        /// </value>
        public DisplayMode InventoryLocationDisplayMode
        {
            get
            {
                if (ViewState["InventoryLocationDisplayMode"] == null)
                {
                    ViewState["InventoryLocationDisplayMode"] = DisplayMode.Admin;
                }

                return (DisplayMode)ViewState["InventoryLocationDisplayMode"];
            }

            set
            {
                ViewState["InventoryLocationDisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the company ID.
        /// </summary>
        /// <value>
        /// The company ID.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (this.ViewState["CompanyId"] == null)
                {
                    this.ViewState["CompanyId"] = 0;
                }
                return (int)this.ViewState["CompanyId"];
            }
            set
            {
                this.ViewState["CompanyId"] = value;
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
                return txtLocation.ValidationGroup;
            }
            set
            {
                txtLocation.ValidationGroup = value;
                rfvLocation.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>
        /// The access key.
        /// </value>
        public string AccessKey
        {
            get
            {
                return txtLocation.AccessKey;
            }
            set
            {
                txtLocation.AccessKey = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get
            {
                return txtLocation.Enabled;
            }
            set
            {
                txtLocation.Enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected location identifier.
        /// </summary>
        /// <value>
        /// The selected location identifier.
        /// </value>
        public int? SelectedLocationId
        {
            get
            {
                int locationId = 0;
                if (int.TryParse(rtvLocations.SelectedValue, out locationId))
                {
                    return locationId;
                }
                return null;
            }
            set
            {
                RadTreeNode node = rtvLocations.FindNodeByValue(value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                if (node != null)
                {
                    node.Selected = true;
                    string fullpath = Support.GetFullPath(node);
                    txtLocation.Text = Utils.ReverseEllipsize(fullpath, 25);
                    txtLocation.ToolTip = fullpath;
                }
                else
                {
                    txtLocation.Text = string.Empty;
                    rtvLocations.UnselectAllNodes();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width
        {
            get
            {
                if (this.ViewState["Width"] == null)
                {
                    this.ViewState["Width"] = 200;
                }
                return (int)this.ViewState["Width"];
            }
            set
            {
                this.ViewState["Width"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                return txtLocation.CssClass;
            }
            set
            {
                txtLocation.CssClass = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show only editable locations].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show only editable locations]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableViewOnlyLocations
        {
            get
            {
                if (this.ViewState["DisableViewOnlyLocations"] == null)
                {
                    this.ViewState["DisableViewOnlyLocations"] = false;
                }
                return (bool)this.ViewState["DisableViewOnlyLocations"];
            }
            set
            {
                this.ViewState["DisableViewOnlyLocations"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            if (InventoryLocationDisplayMode == DisplayMode.SearchInventory)
                rtvLocations.NodeClick += rtvLocations_NodeClick;

            txtLocation.Width = Width;
            if (!IsPostBack)
            {
                rtvLocations.OnClientNodeClicking = string.Concat(this.ClientID, "_OnClientNodeClicking");
            }
        }

        /// <summary>
        /// Handles the NodeClick event of the rtvLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadTreeNodeEventArgs"/> instance containing the event data.</param>
        protected void rtvLocations_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            if (LocationChanged != null)
            {
                LocationChanged();
            }
        }

        /// <summary>
        /// Handles the Click event of the ibtnClearSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ibtnClearSearch_Click(object sender, EventArgs e)
        {
            txtLocation.Text = string.Empty;
            rtvLocations.UncheckAllNodes();
            SelectedLocationId = -1;
            if (LocationChanged != null)
            {
                LocationChanged();
            }
        }

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="excludeIds">The exclude ids.</param>
        public void LoadData(List<int> excludeIds = null, bool showAll = false)
        {
            Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);
            if (company != null)
            {
                txtLocation.Text = string.Empty;

                var locations = GetBL<LocationBL>().GetLocations(this.CompanyId, this.UserID, showAll: showAll);
                var excludedLocations = excludeIds == null ? locations : locations.Where(l => !excludeIds.Contains(l.LocationId));
                rtvLocations.DataSource = excludedLocations;
                rtvLocations.DataBind();

                if (InventoryLocationDisplayMode == DisplayMode.SearchInventory || InventoryLocationDisplayMode == DisplayMode.Generic || InventoryLocationDisplayMode == DisplayMode.LocationAdmin)
                {
                    rfvLocation.Enabled = false;
                }
                else if (InventoryLocationDisplayMode == DisplayMode.Admin)
                {
                    ibtnClearSearch.OnClientClick = string.Concat("return ", ibtnClearSearch.ClientID, "_ClientClick();");
                    ibtnClearSearch.Attributes.Add("onkeypress", string.Concat("return ", ibtnClearSearch.ClientID, "_ClientKeyPress(event);"));
                }

                RadTreeNode node = new RadTreeNode(company.CompanyName, string.Empty);
                node.Expanded = true;
                node.Nodes.AddRange(rtvLocations.Nodes.Cast<RadTreeNode>());
                rtvLocations.Nodes.Add(node);

                // Disable location that has view rights
                if (DisableViewOnlyLocations)
                {
                    foreach (RadTreeNode tier2Loc in node.Nodes)
                    {
                        int locId;
                        if (int.TryParse(tier2Loc.Value, out locId))
                        {
                            bool enabled = Utils.HasLocationManagerPermission(CompanyId, this.UserID, locId) || (InventoryLocationDisplayMode != DisplayMode.LocationAdmin &&
                                    Utils.IsCompanyInventoryStaffMember(CompanyId, this.UserID, locId, DataContext));
                            if (InventoryLocationDisplayMode == DisplayMode.Generic || InventoryLocationDisplayMode == DisplayMode.LocationAdmin)
                            {
                                tier2Loc.Visible = enabled;
                            }
                            else
                            {
                                // Set visible false on client side. (see InventoryLocations_Jquery_SetValue method)
                                tier2Loc.Enabled = enabled;
                            }
                        }
                    }
                }
            }
        }

        #endregion Public Methods
    }
}