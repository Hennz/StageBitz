using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// Booking details web page.
    /// </summary>
    public partial class BookingDetails : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the booking identifier.
        /// </summary>
        /// <value>
        /// The booking identifier.
        /// </value>
        public int BookingId
        {
            get
            {
                if (ViewState["BookingId"] == null)
                {
                    int bookingId = 0;

                    if (Request["BookingId"] != null)
                    {
                        int.TryParse(Request["BookingId"], out bookingId);
                    }

                    ViewState["BookingId"] = bookingId;
                }

                return (int)ViewState["BookingId"];
            }
        }

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

            private set
            {
                ViewState["CompanyId"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Permission denied for this Company.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bookingDetails.BookingId = BookingId;
                bookingDetails.CompanyId = CompanyId;
                if (!(Support.IsCompanyAdministrator(this.CompanyId) || Support.IsCompanyInventoryStaffMember(this.CompanyId)))
                {
                    throw new ApplicationException("Permission denied for this Company.");
                }

                hyperLinkInventorySharing.NavigateUrl = ResolveUrl(string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));

                StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == this.CompanyId).FirstOrDefault();
                DisplayTitle = string.Concat(Support.TruncateString(company.CompanyName, 30), "'s Inventory");
                lnkCompanyInventory.HRef = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
                hyperLinkMyBooking.NavigateUrl = string.Format("~/Inventory/MyBookings.aspx?CompanyId={0}", this.CompanyId);
                LoadBreadCrumbs(company);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="company">The company.</param>
        private void LoadBreadCrumbs(Data.Company company)
        {
            if (this.CompanyId > 0)
            {
                bool isCompanyAdmin = Support.IsCompanyAdministrator(this.CompanyId);
                BreadCrumbs bc = GetBreadCrumbsControl();
                bc.ClearLinks();
                bc.AddLink(company.CompanyName, isCompanyAdmin ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", this.CompanyId) : null);
                bc.AddLink("Company Inventory", string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
                bc.AddLink("Manage Inventory", string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));
                bc.AddLink("Booking Details", null);
                bc.LoadControl();
                bc.UpdateBreadCrumb();
            }
        }

        #endregion Private Methods
    }
}