using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// Web page for My Booking Details
    /// </summary>
    public partial class MyBookingDetails : PageBase
    {
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
                if (ViewState["BookingId"] == null || (int)ViewState["BookingId"] == 0)
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
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for this Booking.
        /// or
        /// Booking does not exist.
        /// </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Data.Booking booking = GetBL<InventoryBL>().GetBooking(BookingId);
                if (booking != null && (GetBL<InventoryBL>().GetNonProjectBooking(BookingId).CreatedBy != UserID || booking.RelatedTable == "Project"))
                {
                    throw new ApplicationException("Permission denied for this Booking.");
                }

                if (booking == null)
                {
                    throw new ApplicationException("Booking does not exist.");
                }

                bookingDetails.BookingId = BookingId;
                bookingDetails.CompanyId = CompanyId;
                bookingDetails.DisplayMode = UserWeb.Controls.Inventory.BookingDetails.ViewMode.NonProject;

                StageBitz.Data.Company company = this.GetBL<CompanyBL>().GetCompany(CompanyId);
                //Get NonProject BookingName
                if (booking.RelatedTable == "NonProject")
                {
                    DisplayTitle = "Bookings";
                }

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

                LoadBreadCrumbs(company);
            }
        }

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
                bc.AddLink("Company Inventory", isCompanyAdmin ? string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId) : null);
                bc.AddLink("My Bookings", string.Format("~/Inventory/MyBookings.aspx?CompanyId={0}", this.CompanyId));
                bc.AddLink(string.Concat(Support.TruncateString(GetBL<InventoryBL>().GetNonProjectBooking(BookingId).Name, 30)), null);
                bc.LoadControl();
                bc.UpdateBreadCrumb();
            }
        }
    }
}