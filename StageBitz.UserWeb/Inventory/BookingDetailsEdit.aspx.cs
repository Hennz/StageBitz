using StageBitz.Logic.Business.Inventory;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// Web page for booking details edit
    /// </summary>
    public partial class BookingDetailsEdit : PageBase
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

        /// <summary>
        /// Gets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    int itemTypeId = 0;

                    if (Request["ItemTypeId"] != null)
                    {
                        int.TryParse(Request["ItemTypeId"], out itemTypeId);
                    }

                    ViewState["ItemTypeId"] = itemTypeId;
                }

                return (int)ViewState["ItemTypeId"];
            }
        }

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
        /// Gets a value indicating whether this request for todate change.
        /// </summary>
        /// <value>
        /// <c>true</c> if this request for todate change; otherwise, <c>false</c>.
        /// </value>
        public bool IsToDateChange
        {
            get
            {
                if (ViewState["IsToDateChange"] == null)
                {
                    bool isToDateChange = true;

                    if (Request["IsToDateChange"] != null)
                    {
                        bool.TryParse(Request["IsToDateChange"], out isToDateChange);
                    }

                    ViewState["IsToDateChange"] = isToDateChange;
                }

                return (bool)ViewState["IsToDateChange"];
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Permission denied for this Booking.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            //IsLargeContentArea = true;
            if (!IsPostBack)
            {
                Data.Booking booking = GetBL<InventoryBL>().GetBooking(BookingId);
                if (booking != null)
                {
                    bool hasAdimnRights = Support.IsCompanyAdministrator(this.CompanyId) || Support.IsCompanyInventoryStaffMember(this.CompanyId);
                    if (hasAdimnRights)
                    {
                        hyperLinkInventorySharing.NavigateUrl = ResolveUrl(string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));

                        editBookingDetails.IsToDateChange = IsToDateChange;
                        editBookingDetails.CompanyId = CompanyId;
                        editBookingDetails.IsInventoryManager = true;
                        editBookingDetails.ViewingCompanyId = CompanyId;
                        editBookingDetails.ItemTypeId = ItemTypeId;
                        editBookingDetails.BookingId = BookingId;
                        editBookingDetails.CallBackURL = string.Format("~/Inventory/BookingDetails.aspx?BookingId={0}&CompanyId={1}", BookingId, CompanyId);

                        StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == this.CompanyId).FirstOrDefault();
                        DisplayTitle = string.Concat(Support.TruncateString(company.CompanyName, 30), "'s Inventory");
                        lnkCompanyInventory.HRef = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
                        hyperLinkMyBooking.NavigateUrl = string.Format("~/Inventory/MyBookings.aspx?CompanyId={0}", this.CompanyId);
                        LoadBreadCrumbs(company);
                    }
                    else
                    {
                        throw new ApplicationException("Permission denied for this Booking.");
                    }
                }
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
            bool isCompanyAdmin = Support.IsCompanyAdministrator(this.CompanyId);
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.ClearLinks();
            bc.AddLink(company.CompanyName, isCompanyAdmin ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", this.CompanyId) : null);
            bc.AddLink("Company Inventory", string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
            bc.AddLink("Manage Inventory", string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));
            bc.AddLink("Booking Details", string.Format("~/Inventory/BookingDetails.aspx?BookingId={0}&CompanyId={1}", BookingId, CompanyId));
            bc.AddLink("Change Booking Dates", null);
            bc.LoadControl();
            bc.UpdateBreadCrumb();
        }

        #endregion Private Methods
    }
}