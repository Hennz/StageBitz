using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Common.Enum;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for booking details.
    /// </summary>
    public partial class BookingDetails : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            Project,
            NonProject,
            Admin
        }

        #endregion Enums

        #region Fileds

        /// <summary>
        /// The rejected status var
        /// </summary>
        public string rejectedStatus = Utils.GetCodeByValue("ItemBookingStatusCode", "REJECTED").Description;

        /// <summary>
        /// The not approved status var
        /// </summary>
        public string notApprovedStatus = Utils.GetCodeByValue("ItemBookingStatusCode", "NOTAPPROVED").Description;

        /// <summary>
        /// The approved status var
        /// </summary>
        public string approvedStatus = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").Description;

        /// <summary>
        /// The returned status var
        /// </summary>
        public string returnedStatus = Utils.GetCodeByValue("InventoryStatusCode", "RETURNED").Description;

        #endregion Fileds

        #region Properties

        /// <summary>
        /// Gets or sets the booking identifier.
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
                    ViewState["BookingId"] = 0;
                }

                return (int)ViewState["BookingId"];
            }

            set
            {
                ViewState["BookingId"] = value;
            }
        }

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
        /// Gets or sets the viewing company identifier.
        /// </summary>
        /// <value>
        /// The viewing company identifier.
        /// </value>
        public int ViewingCompanyId
        {
            get
            {
                if (ViewState["ViewingCompanyId"] == null)
                {
                    ViewState["ViewingCompanyId"] = 0;
                }

                return (int)ViewState["ViewingCompanyId"];
            }

            set
            {
                ViewState["ViewingCompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId
        {
            get
            {
                if (ViewState["RelatedId"] == null)
                {
                    ViewState["RelatedId"] = 0;
                }

                return (int)ViewState["RelatedId"];
            }

            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the booking count.
        /// </summary>
        /// <value>
        /// The booking count.
        /// </value>
        public int BookingCount
        {
            get
            {
                if (ViewState["BookingCount"] == null)
                {
                    ViewState["BookingCount"] = 0;
                }

                return (int)ViewState["BookingCount"];
            }

            set
            {
                ViewState["BookingCount"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit booking dates.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can edit booking dates; otherwise, <c>false</c>.
        /// </value>
        public bool CanEditBookingDates
        {
            get
            {
                if (ViewState["CanEditBookingDates"] == null)
                {
                    ViewState["CanEditBookingDates"] = true;
                }

                return (bool)ViewState["CanEditBookingDates"];
            }

            set
            {
                ViewState["CanEditBookingDates"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the related table.
        /// </summary>
        /// <value>
        /// The name of the related table.
        /// </value>
        public string RelatedTableName
        {
            get
            {
                if (ViewState["RelatedTableName"] == null)
                {
                    ViewState["RelatedTableName"] = string.Empty;
                }

                return (string)ViewState["RelatedTableName"];
            }

            set
            {
                ViewState["RelatedTableName"] = value;
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
                    ViewState["DisplayMode"] = ViewMode.Admin;
                }

                return (ViewMode)ViewState["DisplayMode"];
            }

            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [only show my bookings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [only show my bookings]; otherwise, <c>false</c>.
        /// </value>
        public bool OnlyShowMyBookings
        {
            get
            {
                if (ViewState["OnlyShowMyBookings"] == null)
                {
                    ViewState["OnlyShowMyBookings"] = false;
                }

                return (bool)ViewState["OnlyShowMyBookings"];
            }
            set
            {
                ViewState["OnlyShowMyBookings"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the contact booking manager identifier.
        /// </summary>
        /// <value>
        /// The contact booking manager identifier.
        /// </value>
        private int? ContactBookingManagerId
        {
            get
            {
                return (int?)ViewState["ContactBookingManagerId"];
            }

            set
            {
                ViewState["ContactBookingManagerId"] = value;
            }
        }

        #endregion Properties

        #region Private methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            if (DisplayMode == ViewMode.Admin)
            {
                divCompanyInventory.Visible = chkMyBookingsOnly.Visible = false;
                ViewingCompanyId = CompanyId;
            }
            else
            {
                //Load CompanyList only if the mode is Non-Projerct
                var companyList = GetBL<InventoryBL>().GetBookingCompanies(BookingId);
                Data.Company company = companyList.FirstOrDefault();
                ViewingCompanyId = company.CompanyId;
                if (companyList.Count() > 1)
                {
                    ddCompanyInventory.DataSource = companyList;
                    ddCompanyInventory.DataTextField = "CompanyName";
                    ddCompanyInventory.DataValueField = "CompanyId";
                    ddCompanyInventory.DataBind();
                    lblCompany.Visible = false;
                }
                else
                {
                    ddCompanyInventory.Visible = false;
                    lblCompany.Text = company.CompanyName;
                }

                chkMyBookingsOnly.Visible = DisplayMode == ViewMode.Project;
            }

            if (this.OnlyShowMyBookings)
            {
                chkMyBookingsOnly.Checked = true;
            }

            ddItemTypes.DataSource = Utils.GetALLItemTypes();
            ddItemTypes.DataTextField = "Name";
            ddItemTypes.DataValueField = "ItemTypeId";
            ddItemTypes.DataBind();
            hdnUserId.Value = UserID.ToString();

            string bookingName = GetBookingName();

            lblBookingName.Text = Support.TruncateString(bookingName, 70);
            lblBookingName.ToolTip = bookingName.Length > 70 ? bookingName : string.Empty;

            LoadBookingDetails();
            tooltipManager.TargetControls.Clear();
        }

        /// <summary>
        /// Gets the name of the booking.
        /// </summary>
        /// <returns></returns>
        private string GetBookingName()
        {
            Data.Booking booking = GetBL<InventoryBL>().GetBooking(BookingId);
            RelatedTableName = booking.RelatedTable;
            RelatedId = booking.RelatedId;

            if (RelatedTableName == GlobalConstants.RelatedTables.Bookings.Project)
            {
                Data.Project project = GetBL<ProjectBL>().GetProject(RelatedId);
                return project.ProjectName;
            }
            else
            {
                Data.NonProjectBooking nonProjectBooking = GetBL<InventoryBL>().GetNonProjectBooking(BookingId);
                return nonProjectBooking.Name;
            }
        }

        /// <summary>
        /// Registers the script.
        /// </summary>
        private void RegisterScript()
        {
            string script = string.Format("InitializeSettings();");
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "InitializeSettings", script, true);
        }

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            int itemTypeId;
            int.TryParse(ddItemTypes.SelectedValue, out itemTypeId);
            string fileName = string.Empty;
            Data.Company company = GetBL<CompanyBL>().GetCompany(ViewingCompanyId);
            fileName = string.Format("{0}'s_Bookings", company.CompanyName);

            BookingDetailsReportParameters parameters = new BookingDetailsReportParameters
            {
                BookingId = this.BookingId,
                BookingName = GetBookingName(),
                CompanyId = ViewingCompanyId,
                ContactPerson = GetContactedPerson(),
                DisplayMode = DisplayMode.ToString(),
                ItemTypeId = itemTypeId,
                RelatedTable = this.RelatedTableName,
                SortExpression = gvBookingDetails.MasterTableView.SortExpressions.GetSortString(),
                UserId = this.UserID,
                ShowMyBookingsOnly = chkMyBookingsOnly.Checked
            };

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateBookingDetailsReport(parameters, exportType,
                    out fileNameExtension, out encoding, out mimeType, true);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }

        /// <summary>
        /// Loads the booking header.
        /// </summary>
        private void LoadBookingHeader()
        {
            //Only the Contacted Person needs to be shown
            string bookingownerFullName = string.Empty;
            string bookingownerEmail = string.Empty;
            Data.User contactedPerson = GetContactedPerson();
            bookingownerFullName = string.Concat(contactedPerson.FirstName, " ", contactedPerson.LastName);
            bookingownerEmail = contactedPerson.Email1;

            lnkContactPersonEmail.Text = Support.TruncateString(bookingownerFullName, 50);
            lnkContactPersonEmail.ToolTip = bookingownerFullName.Length > 50 ? Support.TruncateString(bookingownerFullName, 50) : string.Empty;
            lnkContactPersonEmail.NavigateUrl = string.Format("mailto:{0}", bookingownerEmail);
        }

        /// <summary>
        /// Gets the contacted person.
        /// </summary>
        /// <returns></returns>
        private Data.User GetContactedPerson()
        {
            if (RelatedTableName == "NonProject" && DisplayMode == ViewMode.Admin)
            {
                //Display the Booking owner
                return GetBL<InventoryBL>().GetNonProjectBooking(BookingId).User;
            }
            else
            {
                if (this.ContactBookingManagerId.HasValue)
                {
                    return GetBL<PersonalBL>().GetUser(this.ContactBookingManagerId.Value);
                }
                else
                {
                    //Get Inventory Administrator name
                    return GetBL<InventoryBL>().GetInventoryAdmin(ViewingCompanyId);
                }
            }
        }

        /// <summary>
        /// Loads the empty bookings grid.
        /// </summary>
        private void LoadEmptyBookingsGrid()
        {
            List<CompanyBookingDetails> cbList = new List<CompanyBookingDetails>();
            gvBookingDetails.DataSource = cbList;
            BookingCount = 0;
            trLastUpdated.Visible = false;
        }

        /// <summary>
        /// Loads the booking details.
        /// </summary>
        private void LoadBookingDetails()
        {
            int itemTypeId;
            int.TryParse(ddItemTypes.SelectedValue, out itemTypeId);
            IEnumerable<CompanyBookingDetails> companyBookingList = GetBL<InventoryBL>().GetBookingDetails(BookingId, DisplayMode == ViewMode.Project ? 0 : ViewingCompanyId, itemTypeId, chkMyBookingsOnly.Checked, UserID);

            Data.CompanyBookingNumber companyBookingNumber = GetBL<InventoryBL>().GetCompanyBookingNumber(BookingId, ViewingCompanyId);
            if (companyBookingNumber != null)
            {
                litBookingNumber.Text = companyBookingNumber.BookingNumber.ToString();
            }

            if (companyBookingList != null)
            {
                BookingCount = companyBookingList.Sum(cbl => cbl.BookingCount);
                CompanyBookingDetails companyBookingDetails = companyBookingList.FirstOrDefault();
                if (companyBookingDetails != null)
                {
                    int maxContactBookingManagerId = companyBookingDetails.BookingDetailList.Max(bd => bd.ContactLocationManagerId);
                    int minContactBookingManagerId = companyBookingDetails.BookingDetailList.Min(bd => bd.ContactLocationManagerId);
                    this.ContactBookingManagerId = maxContactBookingManagerId == minContactBookingManagerId ? (int?)minContactBookingManagerId : null;

                    gvBookingDetails.DataSource = companyBookingDetails.BookingDetailList;
                    litLastUpdatedDate.Text = Support.FormatDate(companyBookingDetails.MaxLastUpdatedDate);
                    trLastUpdated.Visible = true;
                }
                else
                {
                    LoadEmptyBookingsGrid();
                }
            }
            else
            {
                LoadEmptyBookingsGrid();
            }

            litContactedPersonType.Text = (RelatedTableName == "NonProject" && DisplayMode == ViewMode.Admin) ? "Booked By" : "Booking Manager";
            ltrlBookingCount.Text = BookingCount == 1 ? "1 Booking found" : BookingCount + " Bookings found";
            ScriptManager.RegisterStartupScript(this, GetType(), "isNonProjectBooking", "isNonProjectBooking = " + (RelatedTableName == "NonProject").ToString().ToLower() + ";", true);
            upnel.Update();

            LoadBookingHeader();
        }

        #endregion Private methods

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
                LoadData();
            }
            RegisterScript();
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_ExcelExportClick(object sender, EventArgs e)
        {
            ExportReport(ReportTypes.Excel);
        }

        /// <summary>
        /// Handles the PDFExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_PDFExportClick(object sender, EventArgs e)
        {
            ExportReport(ReportTypes.Pdf);
        }

        /// <summary>
        /// Handles the SortCommand event of the gvBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvBookingDetails_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExprByCompany = new GridSortExpression();
                sortExprByCompany.FieldName = "ItemTypeId";
                sortExprByCompany.SortOrder = GridSortOrder.Ascending;

                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvBookingDetails.MasterTableView.SortExpressions.Clear();
                gvBookingDetails.MasterTableView.SortExpressions.AddSortExpression(sortExprByCompany);
                gvBookingDetails.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvBookingDetails.Rebind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvBookingDetails_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int itemId = ((dynamic)dataItem.DataItem).ItemId;
                HtmlAnchor lnkItemDetails = (HtmlAnchor)e.Item.FindControl("lnkItemDetails");
                tooltipManager.TargetControls.Add(lnkItemDetails.ClientID, itemId.ToString(), true);
            }
        }

        /// <summary>
        /// Shows the thumbnail image of the item as a tooltip.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ToolTipUpdateEventArgs" /> instance containing the event data.</param>
        protected void tooltipManager_AjaxUpdate(object sender, ToolTipUpdateEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int itemId = 0;
                if (int.TryParse(e.Value, out itemId))
                {
                    var item = GetBL<InventoryBL>().GetItem(itemId);
                    if (item != null && item.Code.SortOrder >= GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(item.CompanyId.Value, this.UserID, item.LocationId, false).SortOrder)
                    {
                        InventoryBusinessCard businessCard = (InventoryBusinessCard)LoadControl("~/Controls/Inventory/InventoryBusinessCard.ascx");
                        businessCard.ItemId = itemId;
                        businessCard.RelatedTable = "Item";
                        businessCard.LoadData();
                        e.UpdatePanel.ContentTemplateContainer.Controls.Add(businessCard);
                    }
                    else
                    {
                        Literal ltrlMessage = new Literal();
                        Data.User locationManager = GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                        ltrlMessage.Text = string.Format("<div style='padding:20px 10px;'>The visibility settings for this Item have just been changed." +
                            "You will not have access to this Item. Please contact your Booking Manager, {0} {1}, if you have any questions.<div>", locationManager.FirstName, locationManager.LastName);
                        e.UpdatePanel.ContentTemplateContainer.Controls.Add(ltrlMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the PreRender event of the pagerBooking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pagerBooking_PreRender(object sender, EventArgs e)
        {
            RadDataPagerFieldItem radDataPagerFieldItem = null;

            foreach (Control c in pagerBooking.Controls)
            {
                RadDataPagerFieldItem a = c as RadDataPagerFieldItem;
                if (a != null && a.Field is RadDataPagerTemplatePageField)
                {
                    radDataPagerFieldItem = c as RadDataPagerFieldItem;
                }
            }

            if (radDataPagerFieldItem != null)
            {
                Label lblItemText = radDataPagerFieldItem.FindControl("lblItemText") as Label;
                Label lblPagesText = radDataPagerFieldItem.FindControl("lblPagesText") as Label;
                lblItemText.Text = pagerBooking.TotalRowCount == 1 ? "Item in" : "Items in";
                lblPagesText.Text = pagerBooking.PageCount == 1 ? "page" : "pages";

                Label CurrentPageLabel = radDataPagerFieldItem.FindControl("CurrentPageLabel") as Label;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddItemTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddItemTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //LoadBookingDetails();
            gvBookingDetails.Rebind();
        }

        /// <summary>
        /// Handles the PreRender event of the gvBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gvBookingDetails_PreRender(object sender, EventArgs e)
        {
            gvBookingDetails.MasterTableView.GetColumn("ItemBriefName").Visible = DisplayMode == ViewMode.Project;
            //gvBookingDetails.MasterTableView.GetColumn("ItemName").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 120 : 160;
            gvBookingDetails.MasterTableView.GetColumn("Quantity").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 45 : 70;
            gvBookingDetails.MasterTableView.GetColumn("CurrentStatus").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 80 : 100;
            gvBookingDetails.MasterTableView.GetColumn("BookedBy").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 100 : 140;
            gvBookingDetails.MasterTableView.GetColumn("IsReturned").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 75 : 80;
            gvBookingDetails.MasterTableView.GetColumn("IsActive").HeaderStyle.Width = DisplayMode == ViewMode.Project ? 80 : 100;
            gvBookingDetails.MasterTableView.GetColumn("IsActive").Visible = gvBookingDetails.MasterTableView.GetColumn("BookedBy").Visible = (RelatedTableName == GlobalConstants.RelatedTables.Bookings.Project);
            gvBookingDetails.MasterTableView.GetColumn("Approval").Visible = DisplayMode == ViewMode.Admin && RelatedTableName == GlobalConstants.RelatedTables.Bookings.NonProject;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddCompanyInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddCompanyInventory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int companyId;
            int.TryParse(ddCompanyInventory.SelectedValue, out companyId);
            ViewingCompanyId = companyId;
            LoadBookingHeader();
            gvBookingDetails.Rebind();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkMyBookingsOnly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkMyBookingsOnly_CheckedChanged(object sender, EventArgs e)
        {
            gvBookingDetails.Rebind();
        }

        /// <summary>
        /// Handles the Click event of the btnReloadBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReloadBookingDetails_Click(object sender, EventArgs e)
        {
            gvBookingDetails.Rebind();
            popUpError.HidePopup();
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvBookingDetails_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            LoadBookingDetails();
        }

        #endregion Event Handlers
    }
}