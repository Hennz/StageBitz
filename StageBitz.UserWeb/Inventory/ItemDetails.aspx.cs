using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// The item detail page.
    /// </summary>
    public partial class ItemDetails : PageBase
    {
        #region Constants

        /// <summary>
        /// The parameter delimiter
        /// </summary>
        private const char ParameterDelimiter = '|';

        #endregion Constants

        #region Properties

        /// <summary>
        /// Gets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public int ItemId
        {
            get
            {
                if (ViewState["ItemId"] == null)
                {
                    int itemId = 0;

                    if (Request["ItemId"] != null)
                    {
                        int.TryParse(Request["ItemId"], out itemId);
                    }

                    ViewState["ItemId"] = itemId;
                }

                return (int)ViewState["ItemId"];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }

            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        /// <summary>
        /// Gets the sort parameter.
        /// </summary>
        /// <value>
        /// The sort parameter.
        /// </value>
        private string SortParam
        {
            get
            {
                if (ViewState["SortParam"] == null)
                {
                    string sortParam = string.Empty;

                    if (!string.IsNullOrEmpty(Request["Sort"]))
                    {
                        sortParam = Request["Sort"];
                    }

                    ViewState["SortParam"] = sortParam;
                }

                return (string)ViewState["SortParam"];
            }
        }

        /// <summary>
        /// Gets the booking parameter.
        /// </summary>
        /// <value>
        /// The booking parameter.
        /// </value>
        private string BookingParam
        {
            get
            {
                if (ViewState["BookingParam"] == null)
                {
                    string bookingParam = string.Empty;

                    if (!string.IsNullOrEmpty(Request["Booking"]))
                    {
                        bookingParam = Request["Booking"];
                    }

                    ViewState["BookingParam"] = bookingParam;
                }

                return (string)ViewState["BookingParam"];
            }
        }

        /// <summary>
        /// Gets the inventory parameter.
        /// </summary>
        /// <value>
        /// The inventory parameter.
        /// </value>
        private string InventoryParam
        {
            get
            {
                if (ViewState["InventoryParam"] == null)
                {
                    string inventoryParam = string.Empty;

                    if (!string.IsNullOrEmpty(Request["Inventory"]))
                    {
                        inventoryParam = Request["Inventory"];
                    }

                    ViewState["InventoryParam"] = inventoryParam;
                }

                return (string)ViewState["InventoryParam"];
            }
        }

        /// <summary>
        /// Gets the company id.
        /// </summary>
        /// <value>
        /// The company id.
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
        /// Gets or sets from date.
        /// </summary>
        /// <value>
        /// From date.
        /// </value>
        public DateTime? InventoryParamFromDate
        {
            get
            {
                if (ViewState["InventoryParamFromDate"] == null)
                {
                    return null;
                }
                return (DateTime)ViewState["InventoryParamFromDate"];
            }
            set
            {
                ViewState["InventoryParamFromDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets to date.
        /// </summary>
        /// <value>
        /// To date.
        /// </value>
        public DateTime? InventoryParamToDate
        {
            get
            {
                if (ViewState["InventoryParamToDate"] == null)
                {
                    return null;
                }
                return (DateTime)ViewState["InventoryParamToDate"];
            }
            set
            {
                ViewState["InventoryParamToDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is from shared company.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is from shared company; otherwise, <c>false</c>.
        /// </value>
        public bool IsFromSharedCompany
        {
            get
            {
                if (ViewState["IsFromSharedCompany"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsFromSharedCompany"];
                }
            }

            set
            {
                ViewState["IsFromSharedCompany"] = value;
            }
        }

        /// <summary>
        /// Gets the company inventory navigate URL.
        /// </summary>
        /// <value>
        /// The company inventory navigate URL.
        /// </value>
        public string CompanyInventoryNavigateUrl
        {
            get
            {
                if (Support.CanAccessInventory(this.CompanyId))
                {
                    string url = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}&Sort={1}&Booking={2}&Inventory={3}&ItemId={4}",
                                this.CompanyId, Server.UrlEncode(this.SortParam), Server.UrlEncode(GetBookingParam()), Server.UrlEncode(this.InventoryParam), this.ItemId));

                    System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "navigateUrl", "navigateUrl = '" + url + "';", true);
                    return url;
                }

                return string.Empty;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBookingPanelParams();
                LoadSortParams();
                if (!LoadData())
                {
                    completeItem.Visible = false;
                    completeItemHeader.Visible = false;
                    inventoryProjectPanel.Visible = false;
                    btnDone.Visible = false;
                    return;
                }

                inventoryProjectPanel.CompanyId = CompanyId;
                sbCompanyWarningDisplay.CompanyID = this.CompanyId;
                sbCompanyWarningDisplay.LoadData();

                var item = DataContext.Items.Where(i => i.ItemId == ItemId).FirstOrDefault();
                inventoryProjectPanel.SharedInventoryCompanyId = Convert.ToInt32(item.CompanyId);
                inventoryProjectPanel.DisplayModule = UserWeb.Controls.Inventory.InventoryBookingPanel.ViewMode.ItemDetail;
                InitializeBookingTab();
                LoadItemDetails();

                projectWarningPopup.Mode = UserWeb.Controls.Project.ProjectWarningMode.Inventory;
                projectWarningPopup.CompanyId = CompanyId;

                popupItemDeletedWarning.ItemId = this.ItemId;
                popupItemDeletedWarning.CompanyId = item.CompanyId.Value;

                #region SET LINKS

                // Inside Load data, ProjectId is being set
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

                #endregion SET LINKS
            }

            completeItemHeader.ItemDocumentList = completeItem.ItemDocumentList;

            completeItem.CompleteItemDocumentListDocumentChanged += delegate
            {
                completeItemHeader.LoadControl();
            };

            inventoryProjectPanel.InformInventoryToUpdate += delegate(bool shouldResetItemId, bool shouldResetPageDirty)
            {
                if (LoadData())
                {
                    LoadItemDetails();
                    InitializeBookingTab();
                    UpdateItemDetailUpdatePanels();
                    if (shouldResetPageDirty)
                    {
                        IsPageDirty = false;
                    }
                }
            };

            inventoryProjectPanel.InformInventoryToUpdateFilterationChange += delegate()
            {
                System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "DisplayHeaderText", "DisplayHeaderText(false,null,null);", true);
            };

            inventoryProjectPanel.OnInformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode)
            {
                projectWarningPopup.ShowErrorPopup(errorCode);
            };
        }

        /// <summary>
        /// Handles the OnFileUploaded event of the uploadItemMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void uploadItemMedia_OnFileUploaded(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                // load complete item header and body
                completeItem.LoadImageList();
                completeItemHeader.DisplayItemThumbnail();
                UpdateItemDetailUpdatePanels();
            }
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_ExcelExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsActionValid())
                {
                    ExportReport(ReportTypes.Excel);
                }
            }
        }

        /// <summary>
        /// Handles the PDFExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_PDFExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsActionValid())
                {
                    ExportReport(ReportTypes.Pdf);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteItem_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!IsPageDirty)
                {
                    ShowDeleteItemPopup();
                }
                else
                {
                    popupConfirmItemDetailSave.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmDelteItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmDelteItem_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsActionValid())
                {
                    this.GetBL<InventoryBL>().DeleteItem(ItemId, UserID);
                    Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}&Sort={1}&Booking={2}&Inventory={3}",
                                    this.CompanyId, Server.UrlEncode(this.SortParam), Server.UrlEncode(GetBookingParam()), Server.UrlEncode(this.InventoryParam)));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationDeleteFutureBookings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationDeleteFutureBookings_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsActionValid())
                {
                    this.GetBL<InventoryBL>().DeleteItem(ItemId, UserID, true);
                    Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}&Sort={1}&Booking={2}&Inventory={3}",
                                    this.CompanyId, Server.UrlEncode(this.SortParam), Server.UrlEncode(GetBookingParam()), Server.UrlEncode(this.InventoryParam)));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDoneItemIsPinnedPopup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDoneItemIsPinnedPopup_Click(object sender, EventArgs e)
        {
            if (Support.CanAccessInventory(this.CompanyId))
            {
                Response.Redirect(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}&Sort={2}&Booking={3}&Inventory={4}",
                                this.ItemId, this.CompanyId, Server.UrlEncode(this.SortParam), Server.UrlEncode(GetBookingParam()), Server.UrlEncode(this.InventoryParam)));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmSave_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupConfirmItemDetailSave.HidePopup();
                if (IsActionValid())
                {
                    ShowDeleteItemPopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelSave_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupConfirmItemDetailSave.HidePopup();
                if (IsActionValid())
                {
                    ShowDeleteItemPopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkAddToWatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkAddToWatchList_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Data.Item item = DataContext.Items.Where(i => i.ItemId == ItemId).FirstOrDefault();
                if (item.CompanyId.Value != CompanyId && this.GetBL<InventoryBL>().IsCompanyInventorySharingRemoved(item.CompanyId.Value, CompanyId))
                {
                    popupInventorySharingRemovedWarning.ShowInventorySharingRemoved(CompanyId, item.CompanyId.Value);
                    return;
                }
                else
                {
                    Support.AddToWatchList(ItemId, CompanyId);
                    LoadData();
                }
            }
        }

        /// <summary>
        /// Handles the SortCommand event of the gvBookingList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvBookingList_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvBookingList.MasterTableView.SortExpressions.Clear();
                gvBookingList.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvBookingList.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvBookingList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvBookingList_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            gvBookingList.DataSource = GetBookingTabData();
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the sort parameters.
        /// </summary>
        private void LoadSortParams()
        {
            string[] sortParamArray = SortParam.Split(ParameterDelimiter);
            if (sortParamArray.Length == 7)
            {
                InventoryParamFromDate = sortParamArray[5] == string.Empty ? (DateTime?)null : Utils.GetDatetime(sortParamArray[5], false).Value;
                InventoryParamToDate = sortParamArray[6] == string.Empty ? (DateTime?)null : Utils.GetDatetime(sortParamArray[6], false).Value;
            }
        }

        /// <summary>
        /// Loads the booking panel parameters.
        /// </summary>
        private void LoadBookingPanelParams()
        {
            string[] bookingParams = this.BookingParam.Split(ParameterDelimiter);
            if (bookingParams.Length == 5)
            {
                BookingTypes bookingTypes;
                int bookingId = inventoryProjectPanel.GetBookingId(bookingParams[0], out bookingTypes);
                inventoryProjectPanel.RelatedId = bookingId;
                inventoryProjectPanel.RelatedTable = bookingTypes;

                int itemTypeId;
                if (int.TryParse(bookingParams[1], out itemTypeId))
                {
                    inventoryProjectPanel.ItemTypeId = itemTypeId;
                }

                DateTime fromDate;
                if (DateTime.TryParse(bookingParams[2], out fromDate))
                {
                    inventoryProjectPanel.FromDate = fromDate;
                }

                DateTime toDate;
                if (DateTime.TryParse(bookingParams[3], out toDate))
                {
                    inventoryProjectPanel.ToDate = toDate;
                }

                int bookedQty;
                if (int.TryParse(bookingParams[4], out bookedQty))
                {
                    System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "bookedQty", string.Format("bookedQty = {0};", bookedQty), true);
                    inventoryProjectPanel.BookedQuantity = bookedQty;
                }
            }

            inventoryProjectPanel.divInventoryPanelHeight = 560;
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="company">The company.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Item item)
        {
            var company = DataContext.Companies.Where(c => c.CompanyId == this.CompanyId).FirstOrDefault();
            if (item != null && company != null)
            {
                BreadCrumbs bc = GetBreadCrumbsControl();
                bc.ClearLinks();
                string companyUrl = null;
                if (Support.IsCompanyAdministrator(this.CompanyId))
                {
                    companyUrl = string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", this.CompanyId);
                }

                bc.AddLink(company.CompanyName, companyUrl);
                bc.AddLink("Company Inventory", this.CompanyInventoryNavigateUrl, "InventoryLink");
                bc.AddLink(DisplayTitle, null);
                bc.LoadControl();
                bc.UpdateBreadCrumb();
            }
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <returns>Is data loaded.</returns>
        private bool LoadData()
        {
            Data.Item item = this.GetBL<InventoryBL>().CheckPermissionsForItemDetailsPage(UserID, ItemId, CompanyId);

            #region Validation

            if (item == null)
            {
                DisplayTitle = "Item not found.";
                plcItemNotAvailable.Visible = true;
                grpItem.Visible = false;
                plcHeaderLinks.Visible = false;
                upnlBottom.Visible = false;
                plhScripts.Visible = false;
                return false;
            }
            else
            {
                DisplayTitle = string.Concat("Item Details - ", item.Name);
                Page.Title = string.Concat("'", item.Name, "'");
                LoadBreadCrumbs(item);
                SetItemDetailsWatchListViewSettings();
            }

            if (!Support.CanAccessInventory(this.CompanyId))
            {
                btnDone.Visible = false;
            }

            #endregion Validation

            this.IsFromSharedCompany = GetBL<InventoryBL>().IsCompanyInSharedInventory(this.CompanyId) && (this.CompanyId != item.CompanyId.Value);

            ItemStatusInformationForUser itemStatusInformationForUser = this.GetBL<InventoryBL>().GetItemStatusInformationForUser(item, CompanyId, UserID);

            bool isCompanyAdmin = itemStatusInformationForUser.IsCompanyAdmin;
            bool isInventoryManager = itemStatusInformationForUser.IsInventoryManager;
            bool isItemInUse = itemStatusInformationForUser.IsItemInUse;

            bool hasPaymentSetuped = itemStatusInformationForUser.HasPaymentSetuped;
            bool hasSuspended = itemStatusInformationForUser.HasSuspended;
            IsReadOnly = itemStatusInformationForUser.IsReadOnly;

            #region DeleteItemButton

            if ((isCompanyAdmin || isInventoryManager) && item.CompanyId == CompanyId && !this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) && hasPaymentSetuped && !hasSuspended)
            {
                btnDeleteItem.Visible = btnDone.Enabled = true;
                if (!isItemInUse)
                {
                    btnDeleteItem.Enabled = true;
                }
                else
                {
                    btnDeleteItem.Enabled = false;
                    btnDeleteItem.ToolTip = "This Item is being used on a Project so cannot be deleted";
                }
            }
            else
            {
                btnDeleteItem.Visible = btnDone.Enabled = false;
            }

            #endregion DeleteItemButton

            return true;
        }

        /// <summary>
        /// Navigates to company inventory.
        /// </summary>
        private void NavigateToCompanyInventory()
        {
            Response.Redirect(CompanyInventoryNavigateUrl);
        }

        /// <summary>
        /// Gets the booking parameter.
        /// </summary>
        /// <returns>The booking parameter.</returns>
        private string GetBookingParam()
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", inventoryProjectPanel.BookingCode, ParameterDelimiter, inventoryProjectPanel.ItemTypeId, ParameterDelimiter, inventoryProjectPanel.FromDate, ParameterDelimiter, inventoryProjectPanel.ToDate, ParameterDelimiter, inventoryProjectPanel.BookedQuantity);
        }

        /// <summary>
        /// Initializes the booking tab.
        /// </summary>
        private void InitializeBookingTab()
        {
            List<ItemBookingData> bookings = GetBookingTabData();

            int count = bookings.Count();
            if (count > 0)
            {
                gvBookingList.DataSource = bookings;
                gvBookingList.DataBind();
                divEmptyBookings.Visible = false;
                gvBookingList.Visible = true;
                divExportData.Visible = true;
            }
            else
            {
                gvBookingList.Visible = false;
                divEmptyBookings.Visible = true;
                divExportData.Visible = false;
            }

            if (this.IsFromSharedCompany)
            {
                foreach (GridColumn col in gvBookingList.MasterTableView.RenderColumns)
                {
                    if (col.UniqueName == "Project" || col.UniqueName == "ItemBrief" || col.UniqueName == "BookedBy")
                    {
                        col.Visible = false;
                    }
                }

                divExportData.Visible = false;
            }

            itemDetailsTabs.Tabs[1].Text = string.Format("Bookings ({0})", count);
        }

        /// <summary>
        /// Gets the booking tab data.
        /// </summary>
        /// <returns></returns>
        private List<ItemBookingData> GetBookingTabData()
        {
            return this.GetBL<InventoryBL>().GetBookingTabData(this.ItemId);
        }

        /// <summary>
        /// Loads the item details.
        /// </summary>
        private void LoadItemDetails()
        {
            completeItem.ItemId = this.ItemId;
            completeItem.IsReadOnly = this.IsReadOnly;
            completeItem.ValidationGroup = btnDone.ValidationGroup;
            completeItem.LoadControl();

            completeItemHeader.ItemId = this.ItemId;
            completeItemHeader.IsReadOnly = this.IsReadOnly;
            completeItemHeader.ItemDocumentList = completeItem.ItemDocumentList;
            completeItemHeader.ValidationGroup = btnDone.ValidationGroup;
            completeItemHeader.FunctionPrefix = completeItem.ClientID;
            completeItemHeader.LoadControl();

            uploadItemMedia.IsReadOnly = this.IsReadOnly;
            uploadItemMedia.RelatedTableName = "Item";
            uploadItemMedia.RelatedId = this.ItemId;
            uploadItemMedia.LoadUI();
        }

        /// <summary>
        /// Shows the delete item popup.
        /// </summary>
        private void ShowDeleteItemPopup()
        {
            if (!IsActionValid())
            {
                return;
            }
            else if (this.GetBL<InventoryBL>().IsItemInUse(this.ItemId))
            {
                PinnedItemData pinnedItemData = this.GetBL<InventoryBL>().GetPinnedItemData(this.ItemId);
                ltrProjectName.Text = pinnedItemData.PinnedToProjectName;
                ltrUserName.Text = pinnedItemData.PinnedByUserName;
                lnkItemPinnedUserEmail.InnerText = pinnedItemData.PinnedByUserEmail;
                lnkItemPinnedUserEmail.HRef = "mailto:" + pinnedItemData.PinnedByUserEmail;
                popupItemIsPinned.ShowPopup();
                return;
            }
            else
            {
                //check if there are any future bookings
                if (GetBL<InventoryBL>().HasFutureBookingsForItem(ItemId))
                {
                    popUpConfirmationDeleteFutureBookings.ShowPopup();
                }
                else
                {
                    popupConfirmDeleteItem.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Updates the item detail update panels.
        /// </summary>
        private void UpdateItemDetailUpdatePanels()
        {
            upTabs.Update();
            upnlWatchList.Update();
            upnlFileUpload.Update();
            upnlBooking.Update();
        }

        /// <summary>
        /// Determines whether [is action valid].
        /// </summary>
        /// <returns></returns>
        private bool IsActionValid()
        {
            Data.Item item = null;
            if (this.GetBL<InventoryBL>().IsItemDeleted(this.ItemId))
            {
                popupItemDeletedWarning.ShowItemDeleteMessagePopup(this.ItemId, CompanyId);
                return false;
            }
            else
            {
                item = GetBL<InventoryBL>().GetItem(this.ItemId);
            }

            if (!(Utils.IsCompanyInventoryAdmin(this.CompanyId, UserID) || Utils.IsCompanyInventoryStaffMember(CompanyId, this.UserID, item.LocationId, DataContext)))
            {
                bool canAccessInventory = Support.CanAccessInventory(this.CompanyId);
                projectWarningPopup.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !canAccessInventory);
                return false;
            }
            else if (this.GetBL<InventoryBL>().CheckPermissionsForItemDetailsPage(UserID, ItemId, CompanyId, false) == null)
            {
                projectWarningPopup.ShowErrorPopup(ErrorCodes.ItemNotVisible);
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Sets the item details watch ListView settings.
        /// </summary>
        public void SetItemDetailsWatchListViewSettings()
        {
            bool isIteminWatchList = Support.IsItemInWatchList(ItemId, CompanyId);
            lnkAddToWatchList.Visible = !isIteminWatchList;
            ltrlAddedToWatchList.Visible = isIteminWatchList;
        }

        #endregion Public Methods

        #region Report export

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            if (!this.IsFromSharedCompany)
            {
                Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);
                if (item != null)
                {
                    ItemBookingListReportParameters parameters = new ItemBookingListReportParameters
                    {
                        ItemId = this.ItemId,
                        SortExpression = gvBookingList.MasterTableView.SortExpressions.GetSortString(),
                        UserId = this.UserID
                    };

                    string fileName = item.Name + "_Bookings";
                    string fileNameExtension;
                    string encoding;
                    string mimeType;

                    byte[] reportBytes = UserWebReportHandler.GenerateItemBookingListReport(parameters, exportType,
                            out fileNameExtension, out encoding, out mimeType);
                    Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
                }
            }
        }

        #endregion Report export
    }
}