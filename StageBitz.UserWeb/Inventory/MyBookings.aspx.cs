using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// Page for display my bookings.
    /// </summary>
    public partial class MyBookings : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the company identifier.
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
                    int companyId = 0;

                    if (Request["CompanyId"] != null)
                    {
                        int.TryParse(Request["CompanyId"], out companyId);
                    }

                    ViewState["CompanyId"] = companyId;
                }

                return (int)ViewState["CompanyId"];
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Company not found</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (this.CompanyId > 0)
                {
                    DisplayTitle = "My Bookings";
                    LoadBreadCrumbs();
                    InitializeNavigationLinks();
                    sbManageBookings.CompanyId = this.CompanyId;
                    sbManageBookings.CreatedByUserId = this.UserID;
                    sbManageBookings.LoadData();
                }
                else
                {
                    throw new ApplicationException("Company not found");
                }
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Initializes the navigation links.
        /// </summary>
        private void InitializeNavigationLinks()
        {
            lnkCompanyInventory.HRef = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
            hyperLinkMyBooking.NavigateUrl = string.Format("~/Inventory/MyBookings.aspx?CompanyId={0}", this.CompanyId);

            if (Support.IsCompanyAdministrator(this.CompanyId) || Support.IsCompanyInventoryStaffMember(this.CompanyId))
            {
                spnInventorySharing.Visible = true;
                hyperLinkInventorySharing.NavigateUrl = ResolveUrl(string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));
            }
            else
            {
                spnInventorySharing.Visible = false;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.ClearLinks();
            bc.AddLink("Company Inventory", string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
            bc.AddLink("My Bookings", null);
            bc.LoadControl();
            bc.UpdateBreadCrumb();
        }

        #endregion Private Methods
    }
}