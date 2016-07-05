using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Common.Enum;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Personal;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for manage bookings.
    /// </summary>
    public partial class ManageBookings : UserControlBase
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate to inform company inventory to show error popup
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public delegate void InformCompanyInventoryToShowErrorPopup(ErrorCodes errorCode);

        /// <summary>
        /// The inform company inventory to show error popup
        /// </summary>
        public InformCompanyInventoryToShowErrorPopup OnInformCompanyInventoryToShowErrorPopup;

        #endregion Delegates and Events

        #region Enums

        /// <summary>
        /// ManageBookings controls view modes.
        /// </summary>
        public enum ViewMode
        {
            InventoryManager,
            MyBookings
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets the company id.
        /// </summary>
        /// <value>
        /// The company id.
        /// </value>
        public int? CompanyId
        {
            get
            {
                return (int?)ViewState["CompanyId"];
            }

            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the created by user identifier.
        /// </summary>
        /// <value>
        /// The created by user identifier.
        /// </value>
        public int? CreatedByUserId
        {
            get
            {
                return (int?)ViewState["CreatedByUserId"];
            }

            set
            {
                ViewState["CreatedByUserId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public ViewMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(ViewMode);
                }

                return (ViewMode)ViewState["DisplayMode"];
            }

            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets the booking status.
        /// </summary>
        /// <value>
        /// The booking status.
        /// </value>
        private int? BookingStatus
        {
            get
            {
                int? bookingStatus = null;

                int statusCodeId;
                if (int.TryParse(ddlBookingStatus.SelectedValue, out statusCodeId))
                {
                    if (statusCodeId > 0)
                    {
                        bookingStatus = statusCodeId;
                    }
                }

                return bookingStatus;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show archived].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show archived]; otherwise, <c>false</c>.
        /// </value>
        private bool ShowArchived
        {
            get
            {
                bool showArchived = false;

                int statusCodeId;
                if (int.TryParse(ddlBookingStatus.SelectedValue, out statusCodeId))
                {
                    if (statusCodeId < 0)
                    {
                        showArchived = true;
                    }
                }

                return showArchived;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the ItemsRequested event of the cboSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs"/> instance containing the event data.</param>
        protected void cboSearch_ItemsRequested(object sender, Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs e)
        {
            string keyword = e.Text.Trim().ToLower();

            if (keyword == string.Empty)
            {
                return;
            }

            try
            {
                List<BookingInfo> bookings = new List<BookingInfo>();
                switch (DisplayMode)
                {
                    case ViewMode.InventoryManager:
                        bookings = this.GetBL<InventoryBL>().GetBookingInfo(this.CompanyId, null, keyword, this.BookingStatus, this.ShowArchived, true).ToList();
                        break;

                    case ViewMode.MyBookings:
                        bookings = this.GetBL<InventoryBL>().GetBookingInfo(null, this.CreatedByUserId, keyword, this.BookingStatus, this.ShowArchived, true).ToList();
                        break;
                }

                int resultCount = bookings.Count;
                for (int i = 0; i < resultCount; i++)
                {
                    //Search beginning of words.
                    string matchPattern = string.Format(@"\b{0}", Regex.Escape(keyword));
                    Match keywordMatch = Regex.Match(bookings[i].BookingName, matchPattern, RegexOptions.IgnoreCase);
                    StringBuilder formattedItemText = new StringBuilder(bookings[i].BookingName);

                    // Highlight matching word portion
                    if (keywordMatch != null && keywordMatch.Length > 0)
                    {
                        formattedItemText.Insert(keywordMatch.Index, "<b>");
                        formattedItemText.Insert(3 + keywordMatch.Index + keyword.Length, "</b>");
                    }

                    // Add the matched items to the suggestion list
                    using (RadComboBoxItem item = new RadComboBoxItem())
                    {
                        Literal ltrl = new Literal();
                        item.Controls.Add(ltrl);
                        ltrl.Text = Support.TruncateString(formattedItemText.ToString(), 35);

                        item.Text = bookings[i].BookingName;

                        cboSearch.Items.Add(item);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the Click event of the ibtnClearSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ibtnClearSearch_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                cboSearch.Text = string.Empty;
                LoadBookingData();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvBookings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvBookings_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            string searchText = cboSearch.Text;
            IEnumerable<BookingInfo> bookings = null;
            switch (DisplayMode)
            {
                case ViewMode.InventoryManager:
                    bookings = GetBL<InventoryBL>().GetBookingInfo(this.CompanyId, null, searchText, this.BookingStatus, this.ShowArchived);
                    break;

                case ViewMode.MyBookings:
                    bookings = GetBL<InventoryBL>().GetBookingInfo(null, this.CreatedByUserId, searchText, this.BookingStatus, this.ShowArchived);
                    break;
            }

            gvBookings.DataSource = bookings;

            UpdateBookingHeader(bookings.Count());
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkArchived control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkArchived_CheckedChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (DisplayMode == ViewMode.InventoryManager && this.CompanyId.HasValue &&
                    (!(Utils.IsCompanyInventoryAdmin(this.CompanyId.Value, UserID))
                    && OnInformCompanyInventoryToShowErrorPopup != null))
                {
                    OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                    return;
                }

                CheckBox checkbox = sender as CheckBox;
                Telerik.Web.UI.GridDataItem row = checkbox.NamingContainer as Telerik.Web.UI.GridDataItem;
                if (row != null)
                {
                    BookingInfo bookingInfo = row.DataItem as BookingInfo;
                    if (bookingInfo != null)
                    {
                        Booking booking = GetBL<InventoryBL>().GetBooking(bookingInfo.BookingId);
                        if (booking != null)
                        {
                            booking.IsArchived = checkbox.Checked;
                            GetBL<InventoryBL>().SaveChanges();
                        }
                    }
                }

                LoadBookingData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFindBooking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFindBooking_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadBookingData();
            }
        }

        /// <summary>
        /// Handles the OnSelectedIndexChanged event of the ddlSPPCompanies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSPPCompanies_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadBookingData();
            }
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_ExcelExportClick(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                ExportReport(ReportTypes.Excel);
            }
        }

        /// <summary>
        /// Handles the PDFExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_PDFExportClick(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                ExportReport(ReportTypes.Pdf);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (DisplayMode == ViewMode.InventoryManager)
            {
                gvBookings.MasterTableView.GetColumn("BookingNumber").Visible = true;
            }
            else if (DisplayMode == ViewMode.MyBookings)
            {
                gvBookings.MasterTableView.GetColumn("BookingNumber").Visible = false;
            }

            if (gvBookings.MasterTableView.GetItems(GridItemType.NoRecordsItem).Length > 0)
            {
                GridNoRecordsItem norecordItem = (GridNoRecordsItem)gvBookings.MasterTableView.GetItems(GridItemType.NoRecordsItem)[0];
                Label lblNoData = (Label)norecordItem.FindControl("lblNoData");
                if (lblNoData != null)
                {
                    if (!IsPostBack)
                    {
                        lblNoData.Text = "When you create a basic booking list or book Items to a Project you can access the details from here.";
                    }
                    else
                    {
                        lblNoData.Text = "No Data";
                    }
                }
            }
        }

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            PopulateBookingStatusDropdown();
            LoadBookingData();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Initializes the booking UI.
        /// </summary>
        private void LoadBookingData()
        {
            string searchText = cboSearch.Text;
            IEnumerable<BookingInfo> bookings = null;
            if (DisplayMode == ViewMode.InventoryManager)
            {
                bookings = GetBL<InventoryBL>().GetBookingInfo(this.CompanyId, null, searchText, this.BookingStatus, this.ShowArchived);
            }
            else if (DisplayMode == ViewMode.MyBookings)
            {
                bookings = GetBL<InventoryBL>().GetBookingInfo(null, this.CreatedByUserId, searchText, this.BookingStatus, this.ShowArchived);
            }

            gvBookings.DataSource = bookings;
            gvBookings.DataBind();

            UpdateBookingHeader(bookings.Count());
        }

        /// <summary>
        /// Populates the booking status dropdown.
        /// </summary>
        private void PopulateBookingStatusDropdown()
        {
            List<Code> bookingStatus = Utils.GetCodesByCodeHeader("BookingStatus");
            ddlBookingStatus.DataSource = bookingStatus;
            ddlBookingStatus.DataTextField = "Description";
            ddlBookingStatus.DataValueField = "CodeId";
            ddlBookingStatus.DataBind();

            ddlBookingStatus.AddItemGroup("---------");
            ddlBookingStatus.Items.Add(new ListItem("Archived", "-1"));
        }

        /// <summary>
        /// Updates the booking header.
        /// </summary>
        /// <param name="bookingCount">The booking count.</param>
        private void UpdateBookingHeader(int bookingCount)
        {
            ltrlBookingCount.Text = bookingCount == 1 ? "1 Booking found" : string.Format("{0} Bookings found", bookingCount);
            upnlBookingsHeader.Update();
        }

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            string fileName = string.Empty;
            InventoryManageBookingListReportParameters parameters = new InventoryManageBookingListReportParameters
            {
                BookingStatus = this.BookingStatus,
                CompanyId = this.CompanyId,
                CreatedByUserId = this.CreatedByUserId,
                IsInventoryManagerMode = this.DisplayMode == ViewMode.InventoryManager,
                SearchText = cboSearch.Text,
                ShowArchived = this.ShowArchived,
                SortExpression = gvBookings.MasterTableView.SortExpressions.GetSortString(),
                UserId = this.UserID
            };

            if (this.DisplayMode == ViewMode.InventoryManager)
            {
                Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId.Value);
                if (company != null)
                {
                    fileName = string.Format("{0}'s_Bookings", company.CompanyName);
                }
            }
            else if (this.DisplayMode == ViewMode.MyBookings)
            {
                Data.User user = GetBL<PersonalBL>().GetUser(this.CreatedByUserId.Value);
                if (user != null)
                {
                    fileName = string.Format("{0}'s_Bookings", user.FirstName);
                }
            }

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateInventoryManageBookingListReport(parameters, exportType,
                    out fileNameExtension, out encoding, out mimeType);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        /// Determines whether this instance [can archive booking] the specified booking status code id.
        /// </summary>
        /// <param name="bookingStatusCodeId">The booking status code id.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can archive booking] the specified booking status code id; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanArchiveBooking()
        {
            return !this.CompanyId.HasValue || Utils.IsCompanyInventoryAdmin(this.CompanyId.Value, UserID) ? true : false;
        }

        /// <summary>
        /// Gets the booking URL.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <returns></returns>
        protected string GetBookingUrl(int bookingId, int companyId, string relatedTable)
        {
            string url = string.Empty;
            if (DisplayMode == ViewMode.MyBookings)
            {
                switch (relatedTable)
                {
                    case GlobalConstants.RelatedTables.Bookings.NonProject:
                        url = string.Format("~/Inventory/MyBookingDetails.aspx?CompanyId={0}&BookingId={1}", companyId, bookingId);
                        break;

                    case GlobalConstants.RelatedTables.Bookings.Project:
                        Booking booking = GetBL<InventoryBL>().GetBooking(bookingId);
                        if (booking != null)
                        {
                            url = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}&OnlyShowMyBookings={1}", booking.RelatedId, "true");
                        }
                        break;
                }
            }
            else if (DisplayMode == ViewMode.InventoryManager)
            {
                url = string.Format("~/Inventory/BookingDetails.aspx?CompanyId={0}&BookingId={1}", companyId, bookingId);
            }

            return url;
        }

        #endregion Protected Methods
    }
}