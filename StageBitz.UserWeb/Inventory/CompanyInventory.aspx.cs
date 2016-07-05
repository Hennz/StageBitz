using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Location;
using StageBitz.Logic.Business.Project;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using StageBitz.UserWeb.Controls.Inventory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// Company Inventory Page.
    /// </summary>
    public partial class CompanyInventory : PageBase
    {
        #region Constants

        /// <summary>
        /// The select booking period message.
        /// </summary>
        private const string SelectBookingPeriodMsg = "Please select a booking period to see the availability of Items before selecting an Item to book.";

        /// <summary>
        /// The visibility filter value delimiter
        /// </summary>
        private const char VisibilityFilterValueDelimiter = '-';

        #endregion Constants

        #region Enums

        /// <summary>
        /// Enum for diplay mode
        /// </summary>
        private enum InventoryDisplayMode
        {
            SearchResults = 1,
            WatchList = 2
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is manager mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is manager mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsManagerMode
        {
            get
            {
                if (ViewState["IsManagerMode"] == null)
                {
                    bool isManagerMode = false;

                    if (Request["IsManagerMode"] != null)
                    {
                        bool.TryParse(Request["IsManagerMode"], out isManagerMode);
                    }

                    ViewState["IsManagerMode"] = isManagerMode;
                }

                return (bool)ViewState["IsManagerMode"];
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
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        private int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    int projectid = 0;

                    if (Request["ProjectId"] != null)
                    {
                        int.TryParse(Request["ProjectId"], out projectid);
                    }

                    ViewState["ProjectId"] = projectid;
                }

                return (int)ViewState["ProjectId"];
            }
        }

        /// <summary>
        /// Gets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        private int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    int projectid = 0;

                    if (Request["ItemTypeId"] != null)
                    {
                        int.TryParse(Request["ItemTypeId"], out projectid);
                    }

                    ViewState["ItemTypeId"] = projectid;
                }

                return (int)ViewState["ItemTypeId"];
            }
        }

        /// <summary>
        /// Gets or sets the booked qty.
        /// </summary>
        /// <value>
        /// The booked qty.
        /// </value>
        private int BookedQty
        {
            get
            {
                if (ViewState["BookedQty"] == null)
                {
                    return 1;
                }

                return (int)ViewState["BookedQty"];
            }
            set
            {
                ViewState["BookedQty"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        private string FindByName
        {
            get
            {
                if (ViewState["FindByName"] == null)
                {
                    ViewState["FindByName"] = string.Empty;
                }

                return ViewState["FindByName"].ToString();
            }

            set
            {
                ViewState["FindByName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        private DateTime? FindFromDate
        {
            get
            {
                if (ViewState["FindFromDate"] != null)
                {
                    return (DateTime)ViewState["FindFromDate"];
                }

                return null;
            }

            set
            {
                ViewState["FindFromDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        private DateTime? FindToDate
        {
            get
            {
                if (ViewState["FindToDate"] != null)
                {
                    return (DateTime)ViewState["FindToDate"];
                }

                return null;
            }

            set
            {
                ViewState["FindToDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the find by item type id.
        /// </summary>
        /// <value>
        /// The find by item type id.
        /// </value>
        private int FindByItemTypeId
        {
            get
            {
                if (ViewState["FindByItemTypeId"] == null)
                {
                    ViewState["FindByItemTypeId"] = 0;
                }

                return (int)ViewState["FindByItemTypeId"];
            }

            set
            {
                ViewState["FindByItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is allowed to add items.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is allowed to add items; otherwise, <c>false</c>.
        /// </value>
        private bool IsAllowedToAddItems
        {
            get
            {
                if (ViewState["CanAddItems"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["CanAddItems"];
                }
            }

            set
            {
                ViewState["CanAddItems"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company in Shared Inventory
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is allowed to add items; otherwise, <c>false</c>.
        /// </value>
        private bool IsCompanyInSharedInventory
        {
            get
            {
                if (ViewState["IsCompanyInSharedInventory"] == null)
                {
                    ViewState["IsCompanyInSharedInventory"] = this.GetBL<InventoryBL>().IsCompanyInSharedInventory(CompanyId);
                    return (bool)ViewState["IsCompanyInSharedInventory"];
                }
                else
                {
                    return (bool)ViewState["IsCompanyInSharedInventory"];
                }
            }
            set
            {
                ViewState["IsCompanyInSharedInventory"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        private int SortOrder
        {
            get
            {
                int order = 1;
                if (ViewState["SortOrder"] != null)
                {
                    int.TryParse(ViewState["SortOrder"].ToString(), out order);
                }

                return order;
            }

            set
            {
                ViewState["SortOrder"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort by.
        /// </summary>
        /// <value>
        /// The sort by.
        /// </value>
        private string SortBy
        {
            get
            {
                string sortBy = string.Empty;
                if (ViewState["SortBy"] != null)
                {
                    sortBy = ViewState["SortBy"].ToString();
                }

                return sortBy;
            }

            set
            {
                ViewState["SortBy"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        private int PageIndex
        {
            get
            {
                int pageIndex = 0;
                if (ViewState["PageIndex"] != null)
                {
                    int.TryParse(ViewState["PageIndex"].ToString(), out pageIndex);
                }

                return pageIndex;
            }

            set
            {
                ViewState["PageIndex"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        private int PageSize
        {
            get
            {
                int pageSize = 0;
                if (ViewState["PageSize"] != null)
                {
                    int.TryParse(ViewState["PageSize"].ToString(), out pageSize);
                }

                return pageSize;
            }

            set
            {
                ViewState["PageSize"] = value;
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
        /// Gets or sets the selected location identifier.
        /// </summary>
        /// <value>
        /// The selected location identifier.
        /// </value>
        public int SelectedLocationId
        {
            get
            {
                if (ViewState["SelectedLocationId"] == null)
                {
                    return -1;
                }

                return (int)ViewState["SelectedLocationId"];
            }
            set
            {
                ViewState["SelectedLocationId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        private InventoryDisplayMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(InventoryDisplayMode);
                }

                return (InventoryDisplayMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the shared inventory company identifier.
        /// </summary>
        /// <value>
        /// The shared inventory company identifier.
        /// </value>
        private int SharedInventoryCompanyId
        {
            get
            {
                int companyId = 0;
                if (ViewState["SharedInventoryCompanyId"] != null)
                {
                    int.TryParse(ViewState["SharedInventoryCompanyId"].ToString(), out companyId);
                }

                return companyId;
            }

            set
            {
                ViewState["SharedInventoryCompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets the visibility filter selected value.
        /// </summary>
        /// <value>
        /// The visibility filter selected value.
        /// </value>
        private int? VisibilityFilterSelectedValue
        {
            get
            {
                int? itemVisibilityCode = null;
                int visibility = 0;
                string value = string.IsNullOrEmpty(ddlVisibilityFilter.SelectedValue) ? string.Empty : ddlVisibilityFilter.SelectedValue.Split(VisibilityFilterValueDelimiter)[1];
                if (tbtnManagerMode.Checked && int.TryParse(value, out visibility))
                {
                    itemVisibilityCode = visibility;
                }

                return itemVisibilityCode;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.Exception">Company not found</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ImportbulkItemsControl.CompanyID = this.CompanyId;
                SharedInventoryCompanyId = CompanyId;
                PopulateItemTypeDropdowns();
                PopulateVisibilityDropdown();
                LoadInventorySortingAndPaging();
                inventoryProjectPanel.CompanyId = CompanyId;
                inventoryProjectPanel.DisplayModule = UserWeb.Controls.Inventory.InventoryBookingPanel.ViewMode.CompanyInventory;
                sbInventoryLocations.CompanyId = CompanyId;

                sbInventoryLocations.LoadData();
                LoadBookingParams();
                LoadInventoryParams();
                LoadSharedInventories();
                litSearchResult.Value = Convert.ToString(InventoryDisplayMode.SearchResults);
                litWatchList.Value = Convert.ToString(InventoryDisplayMode.WatchList);
                divWatchList.Visible = false;
                DisplayMode = InventoryDisplayMode.SearchResults;

                inventoryProjectPanel.SharedInventoryCompanyId = SharedInventoryCompanyId;
                sbCompanyWarningDisplay.CompanyID = this.CompanyId;
                sbCompanyWarningDisplay.LoadData();

                sbPackageLimitsValidation.CompanyId = CompanyId;
                sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.InventoryLimit;
                sbPackageLimitsValidation.LoadData();

                searchInvLocation.CompanyId = SharedInventoryCompanyId;
                searchInvLocation.LoadData();
                searchInvLocation.SelectedLocationId = SelectedLocationId;
                StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == this.CompanyId).FirstOrDefault();

                #region Check access security

                Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectId).FirstOrDefault();

                if (ProjectId > 0 && (project == null || !Support.CanAccessProject(project)))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Project not found");
                }

                if (!Support.CanAccessInventory(this.CompanyId))
                {
                    throw new ApplicationException("Company not found");
                }

                #endregion Check access security

                // Set the page read only status if the company is suspended or user is project "Observer
                var canAddItems = this.GetBL<CompanyBL>().HasEditPermissionForInventoryStaff(this.CompanyId, UserID, null);
                this.IsAllowedToAddItems = canAddItems;
                pnlAddItem.Visible = canAddItems;
                ImportbulkItemsControl.IsReadOnly = !canAddItems;
                divManagerMode.Visible = canAddItems && this.SharedInventoryCompanyId == this.CompanyId;
                tbtnManagerMode.Checked = canAddItems && this.SharedInventoryCompanyId == this.CompanyId && IsManagerMode;

                displaySettings.Module = ListViewDisplaySettings.ViewSettingModule.ProjectItemBriefList;
                displaySettings.LoadControl();

                txtName.Focus();

                #region SET LINKS

                hyperLinkCompanyInventory.NavigateUrl = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
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

                DisplayTitle = string.Concat(Support.TruncateString(company.CompanyName, 30), "'s Inventory");

                LoadBreadCrumbs(company);

                projectWarningPopupInventory.Mode = UserWeb.Controls.Project.ProjectWarningMode.Inventory;
                projectWarningPopupInventory.CompanyId = CompanyId;
                contactInventoryManager.FindFromDate = inventoryProjectPanel.FromDate;
                contactInventoryManager.FindToDate = inventoryProjectPanel.ToDate;
            }

            contactInventoryManager.InformInventoryToReLoad += delegate
            {
                LoadWatchListData();
            };

            // InformInventoryToUpdate
            inventoryProjectPanel.InformInventoryToUpdate += delegate(bool shouldResetItemId, bool shouldResetPageDirty)
            {
                if (shouldResetItemId)
                    ClearSelectedItemId();

                this.FindFromDate = contactInventoryManager.FindFromDate = inventoryProjectPanel.FromDate;
                this.FindToDate = contactInventoryManager.FindToDate = inventoryProjectPanel.ToDate;
                LoadAllData();
            };

            inventoryProjectPanel.InformInventoryToUpdateFilterationChange += delegate()
            {
                FindFromDate = inventoryProjectPanel.FromDate;
                FindToDate = inventoryProjectPanel.ToDate;

                ltrlItemListTitleForDateFilteration.Text = BuildTitleText();
                upnlDisplaySettings.Update();
            };

            inventoryProjectPanel.OnInformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode)
            {
                projectWarningPopupInventory.ShowErrorPopup(errorCode);
            };

            searchInvLocation.LocationChanged += delegate()
            {
                if (!StopProcessing)
                {
                    SelectedLocationId = searchInvLocation.SelectedLocationId.HasValue ? searchInvLocation.SelectedLocationId.Value : -1;
                    LoadSearch();
                }
            };

            ImportbulkItemsControl.InformItemListToLoad += delegate
            {
                LoadAllData();
            };

            ImportbulkItemsControl.InformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode)
            {
                projectWarningPopupInventory.ShowErrorPopup(errorCode);
            };

            inventoryBulkUpdatePanel.InformCompanyInventoryToReloadBulkUpdate += delegate
            {
                ddlVisibilityFilter.SelectedValue = lblVisibilityText.Text = string.Empty;
                LoadData();
            };

            inventoryBulkUpdatePanel.OnInformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode)
            {
                projectWarningPopupInventory.ShowErrorPopup(errorCode);
            };

            // register checkbox initialize script
            RegisterInitializeScript();
            if (!IsCompanyInSharedInventory)
            {
                RemoveItemGroupingByCompany();
            }

            // Change default timeout value to 60 secs.
            this.DataContext.CommandTimeout = 60;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tbtnManagerMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tbtnManagerMode_CheckedChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadData(false);
                if (!tbtnManagerMode.Checked)
                {
                    ddlVisibilityFilter.SelectedValue = lblVisibilityText.Text = string.Empty;
                    LoadSharedInventories();
                }
                else
                {
                    inventoryBulkUpdatePanel.LoadLoations();
                }
                    

            }
        }

        #region Grid View events

        /// <summary>
        /// Handles the SortCommand event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs" /> instance containing the event data.</param>
        protected void gvItemList_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvItemList.MasterTableView.SortExpressions.Clear();
                gvItemList.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvItemList.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs" /> instance containing the event data.</param>
        protected void gvItemList_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            ClearSelectedItemId();

            if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
            {
                int count = 0;
                gvItemList.DataSource = GetItemList(gvItemList.MasterTableView.PageSize, gvItemList.MasterTableView.CurrentPageIndex,
                    gvItemList.MasterTableView.SortExpressions.GetSortString(), out count);
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs" /> instance containing the event data.</param>
        protected void gvItemList_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                Data.Item item = itemData.Item;

                HtmlAnchor lnkItemDetails = (HtmlAnchor)e.Item.FindControl("lnkItemDetails");

                RadioButton rbtnItem = (RadioButton)e.Item.FindControl("rbtnItem");

                LinkButton lnkbtnAddToWatchList = (LinkButton)e.Item.FindControl("lnkAddToWatchList");
                Literal ltrlAddedToWatchList = (Literal)e.Item.FindControl("ltrlAddedToWatchList");

                RadNumericTextBox txtQtyBooked = (RadNumericTextBox)e.Item.FindControl("txtQtyBooked");

                if (!IsPostBack && this.ItemId == item.ItemId && !HasNoDateConfigured())
                {
                    rbtnItem.Checked = true;
                    txtQtyBooked.Display = true;
                    lnkItemDetails.HRef = GetItemDetailPageUrl(item.ItemId, BookedQty);
                    int availableQtyforProjectpanelDuration = itemData.AvailableQty;
                    //Set the Max value taken from Project panel filteration
                    if (inventoryProjectPanel.FromDate.HasValue && inventoryProjectPanel.ToDate.HasValue)
                    {
                        availableQtyforProjectpanelDuration = GetBL<InventoryBL>().GetAvailableItemQuantity(this.ItemId, inventoryProjectPanel.FromDate, inventoryProjectPanel.ToDate);
                        txtQtyBooked.MinValue = availableQtyforProjectpanelDuration == 0 ? 0 : 1;
                        txtQtyBooked.MaxValue = availableQtyforProjectpanelDuration;
                    }
                    else
                    {
                        txtQtyBooked.MinValue = availableQtyforProjectpanelDuration == 0 ? 0 : 1;
                        txtQtyBooked.MaxValue = availableQtyforProjectpanelDuration;
                    }

                    double itemQtyToDisplay = availableQtyforProjectpanelDuration < BookedQty ? availableQtyforProjectpanelDuration : BookedQty;
                    txtQtyBooked.Value = txtQtyBooked.MinValue > itemQtyToDisplay ? txtQtyBooked.MinValue : itemQtyToDisplay;
                    txtQtyBooked.DisplayText = itemQtyToDisplay.ToString();
                }
                else
                {
                    lnkItemDetails.HRef = GetItemDetailPageUrl(item.ItemId, 1);
                    txtQtyBooked.MaxValue = itemData.AvailableQty;
                }

                rbtnItem.Enabled = !HasNoDateConfigured();
                rbtnItem.ToolTip = HasNoDateConfigured() ? SelectBookingPeriodMsg : string.Empty;

                // Item link
                lnkItemDetails.InnerText = Support.TruncateString(item.Name, 20);

                tooltipManager.TargetControls.Add(lnkItemDetails.ClientID, item.ItemId.ToString(), true);

                dataItem["AvailableQty"].Text = itemData.AvailableQty.ToString();

                // Description
                dataItem["Description"].Text = Support.TruncateString(item.Description, 40);

                if (!string.IsNullOrEmpty(item.Description) && item.Description.Length > 40)
                {
                    dataItem["Description"].ToolTip = item.Description;
                }

                // Item Type Name
                dataItem["ItemTypeName"].Text = Support.TruncateString(itemData.ItemTypeName, 14);

                if (!string.IsNullOrEmpty(itemData.ItemTypeName) && itemData.ItemTypeName.Length > 14)
                {
                    dataItem["ItemTypeName"].ToolTip = itemData.ItemTypeName;
                }

                if (item.Quantity != null)
                {
                    dataItem["Quantity"].Text = item.Quantity.ToString();

                    if (item.Quantity.ToString().Length > 6)
                        dataItem["Quantity"].ToolTip = item.Quantity.ToString();
                }

                //bool isIteminWatchList = Support.IsItemInWatchList(item.ItemId, CompanyId);
                lnkbtnAddToWatchList.Visible = !itemData.IsWatchListItem;
                lnkbtnAddToWatchList.CommandArgument = item.ItemId.ToString();
                lnkbtnAddToWatchList.CommandName = "AddToWatchList";
                ltrlAddedToWatchList.Visible = itemData.IsWatchListItem;
            }
        }

        protected void gvWatchList_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExprByCompany = new GridSortExpression();
                sortExprByCompany.FieldName = "CompanyId";
                sortExprByCompany.SortOrder = GridSortOrder.Ascending;

                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvWatchList.MasterTableView.SortExpressions.Clear();
                gvWatchList.MasterTableView.SortExpressions.AddSortExpression(sortExprByCompany);
                gvWatchList.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvWatchList.Rebind();
            }
        }

        protected void gvWatchList_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            gvWatchList.DataSource = GetWatchListItems();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvWatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs" /> instance containing the event data.</param>
        protected void gvWatchList_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                int thumbnailMediaId = itemData.ThumbnailMediaId;
                int watchListHeaderId = itemData.WatchListHeaderId;

                RadioButton rbtnItem = (RadioButton)e.Item.FindControl("rbtnItem");
                ImageDisplay itemThumbDisplay = (ImageDisplay)e.Item.FindControl("itemThumbDisplay");

                ImageButton imgbtnEmailSent = (ImageButton)e.Item.FindControl("imgbtnEmailSent");
                ImageButton imgbtnSendEmail = (ImageButton)e.Item.FindControl("imgbtnSendEmail");
                HiddenField hdnCompanyId = (HiddenField)e.Item.FindControl("hdnCompanyId");
                if (hdnCompanyId != null)
                {
                    hdnCompanyId.Value = itemData.CompanyId.ToString(CultureInfo.InvariantCulture);
                }

                int watchListCompanyId = 0;
                int.TryParse(hdnCompanyId.Value, out watchListCompanyId);
                bool isEmailSent = Support.IsEmailSentToInventoryManager(watchListCompanyId);
                imgbtnEmailSent.Visible = isEmailSent;
                imgbtnSendEmail.Visible = !isEmailSent;

                dataItem["Name"].Text = Support.TruncateString(itemData.Name, 20);
                if (!string.IsNullOrEmpty(itemData.Name) && itemData.Name.Length > 20)
                {
                    dataItem["Name"].ToolTip = itemData.Name;
                }

                itemThumbDisplay.DocumentMediaId = thumbnailMediaId;

                //// Description
                dataItem["Description"].Text = Support.TruncateString(itemData.Description, 20);
                if (!string.IsNullOrEmpty(itemData.Description) && itemData.Description.Length > 20)
                {
                    dataItem["Description"].ToolTip = itemData.Description;
                }

                RadNumericTextBox txtQtyBooked = (RadNumericTextBox)e.Item.FindControl("txtQtyBooked");

                txtQtyBooked.MinValue = itemData.AvailableQuantity == 0 ? 0 : 1;
                if (inventoryProjectPanel.FromDate.HasValue && inventoryProjectPanel.ToDate.HasValue)
                {
                    int availableQtyforProjectpanelDuration = GetBL<InventoryBL>().GetAvailableItemQuantity(itemData.ItemId, inventoryProjectPanel.FromDate, inventoryProjectPanel.ToDate);
                    txtQtyBooked.MaxValue = availableQtyforProjectpanelDuration;
                    txtQtyBooked.MinValue = availableQtyforProjectpanelDuration == 0 ? 0 : 1;
                }
                else
                    txtQtyBooked.MaxValue = itemData.AvailableQuantity;

                if (itemData.Quantity != null)
                {
                    dataItem["Quantity"].Text = itemData.Quantity.ToString();
                    if (itemData.Quantity.ToString().Length > 6)
                    {
                        dataItem["Quantity"].ToolTip = itemData.Quantity.ToString();
                    }
                }

                rbtnItem.Enabled = !HasNoDateConfigured();
                rbtnItem.ToolTip = HasNoDateConfigured() ? SelectBookingPeriodMsg : string.Empty;

                dataItem["AvailableQty"].Text = itemData.AvailableQuantity.ToString();
            }
        }

        protected void gvWatchList_OnItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.Item is GridDataItem)
                {
                    //Pay now for this project
                    GridDataItem dataItem = (GridDataItem)e.Item;
                    int watchListHeaderId = (int)dataItem.GetDataKeyValue("WatchListHeaderId");
                    int companyId = (int)dataItem.GetDataKeyValue("CompanyId");

                    switch (e.CommandName)
                    {
                        case "SendEmail":
                            contactInventoryManager.WatchListHeaderId = watchListHeaderId;
                            contactInventoryManager.CompanyId = companyId;
                            contactInventoryManager.UserCompanyId = CompanyId;
                            contactInventoryManager.SortExpressions = gvWatchList.MasterTableView.SortExpressions;
                            contactInventoryManager.ShowContactPoppup();
                            break;
                    }
                }
            }
        }

        protected void gvWatchList_PreRender(object sender, EventArgs e)
        {
            if (this.GetBL<InventoryBL>().IsCompanyInSharedInventory(CompanyId))
            {
                for (int rowIndex = gvWatchList.Items.Count - 2; rowIndex >= 0; rowIndex--)
                {
                    GridDataItem row = gvWatchList.Items[rowIndex];
                    GridDataItem nextRow = gvWatchList.Items[rowIndex + 1];
                    HiddenField hdnCompanyIdrow = (HiddenField)row.FindControl("hdnCompanyId");
                    HiddenField hdnCompanyIdnextRow = (HiddenField)nextRow.FindControl("hdnCompanyId");

                    if (hdnCompanyIdnextRow != null && hdnCompanyIdrow != null)
                    {
                        if (hdnCompanyIdrow.Value == hdnCompanyIdnextRow.Value)
                        {
                            row.Cells[3].RowSpan = nextRow.Cells[3].RowSpan < 2 ? 2 : nextRow.Cells[3].RowSpan + 1;
                            row.Cells[3].VerticalAlign = VerticalAlign.Top;
                            nextRow.Cells[3].Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvItemBulkEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvItemBulkEdit_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                Data.Item item = itemData.Item;

                HtmlAnchor lnkItemDetails = (HtmlAnchor)e.Item.FindControl("lnkItemDetails");

                RadNumericTextBox txtQtyBooked = (RadNumericTextBox)e.Item.FindControl("txtQtyBooked");
                lnkItemDetails.HRef = GetBaseItemDetailPageUrl(item.ItemId);

                // Item link
                lnkItemDetails.InnerText = Support.TruncateString(item.Name, 20);
                lnkItemDetails.Target = "_blank";

                tooltipManager.TargetControls.Add(lnkItemDetails.ClientID, item.ItemId.ToString(), true);

                // Description
                dataItem["Description"].Text = Support.TruncateString(item.Description, 30);

                if (!string.IsNullOrEmpty(item.Description) && item.Description.Length > 30)
                {
                    dataItem["Description"].ToolTip = item.Description;
                }

                // Location
                string path = itemData.LocationPath;
                dataItem["Location"].Text = Utils.ReverseEllipsize(path, 15);

                if (!string.IsNullOrEmpty(path) && path.Length > 15)
                {
                    dataItem["Location"].ToolTip = path;
                }

                // Item Type Name
                dataItem["ItemTypeName"].Text = Support.TruncateString(itemData.ItemTypeName, 14);

                if (!string.IsNullOrEmpty(itemData.ItemTypeName) && itemData.ItemTypeName.Length > 14)
                {
                    dataItem["ItemTypeName"].ToolTip = itemData.ItemTypeName;
                }

                if (item.Quantity != null)
                {
                    dataItem["Quantity"].Text = item.Quantity.ToString();

                    if (item.Quantity.ToString().Length > 6)
                    {
                        dataItem["Quantity"].ToolTip = item.Quantity.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SortCommand event of the gvItemBulkEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvItemBulkEdit_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvItemBulkEdit.MasterTableView.SortExpressions.Clear();
                gvItemBulkEdit.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvItemBulkEdit.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvItemBulkEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvItemBulkEdit_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            int count = 0;
            gvItemBulkEdit.DataSource = GetItemList(gvItemBulkEdit.MasterTableView.PageSize, gvItemBulkEdit.MasterTableView.CurrentPageIndex,
                gvItemBulkEdit.MasterTableView.SortExpressions.GetSortString(), out count, isManageInventory: true);
        }

        #endregion Grid View events

        #region List view

        /// <summary>
        /// Handles the ItemDataBound event of the lvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadListViewItemEventArgs" /> instance containing the event data.</param>
        protected void lvItemList_ItemDataBound(object sender, RadListViewItemEventArgs e)
        {
            dynamic dataItem = ((dynamic)(e.Item)).DataItem;
            dynamic item = dataItem.Item;
            int thumbnailMediaId = ((dynamic)(e.Item)).DataItem.ThumbnailMediaId;

            HyperLink lnkItem = (HyperLink)e.Item.FindControl("lnkItem");
            ImageDisplay itemThumbDisplay = (ImageDisplay)e.Item.FindControl("itemThumbDisplay");

            RadioButton rbtnItem = (RadioButton)e.Item.FindControl("rbtnItem");
            Literal ltrItemName = (Literal)e.Item.FindControl("ltrItemName");

            LinkButton lnkbtnAddToWatchList = (LinkButton)e.Item.FindControl("lnkAddToWatchList");
            Literal ltrlAddedToWatchList = (Literal)e.Item.FindControl("ltrlAddedToWatchList");

            tooltipManager.TargetControls.Add(lnkItem.ClientID, item.ItemId.ToString(), true);

            if (rbtnItem != null)
            {
                rbtnItem.Visible = true;
                if (ltrItemName != null)
                {
                    ltrItemName.Text = Support.TruncateString(item.Name, 10);
                }
            }

            RadNumericTextBox txtQtyBooked = (RadNumericTextBox)e.Item.FindControl("txtQtyBooked");

            if (!IsPostBack && this.ItemId == item.ItemId && !HasNoDateConfigured())
            {
                rbtnItem.Checked = true;
                txtQtyBooked.Display = true;
                lnkItem.NavigateUrl = GetItemDetailPageUrl(item.ItemId, BookedQty);
                int availableQtyforProjectpanelDuration = dataItem.AvailableQty;
                //Set the Max value taken from Project panel filteration
                if (FindFromDate.HasValue && FindToDate.HasValue)
                {
                    availableQtyforProjectpanelDuration = GetBL<InventoryBL>().GetAvailableItemQuantity(this.ItemId, inventoryProjectPanel.FromDate, inventoryProjectPanel.ToDate);
                    txtQtyBooked.MinValue = availableQtyforProjectpanelDuration == 0 ? 0 : 1;
                    txtQtyBooked.MaxValue = availableQtyforProjectpanelDuration;
                }
                else
                {
                    txtQtyBooked.MaxValue = availableQtyforProjectpanelDuration;
                    txtQtyBooked.MinValue = availableQtyforProjectpanelDuration == 0 ? 0 : 1;
                }

                double itemQtyToDisplay = availableQtyforProjectpanelDuration < BookedQty ? availableQtyforProjectpanelDuration : BookedQty;
                txtQtyBooked.Value = itemQtyToDisplay;
                txtQtyBooked.DisplayText = itemQtyToDisplay.ToString();
            }
            else
            {
                lnkItem.NavigateUrl = GetItemDetailPageUrl(item.ItemId, 1);
                txtQtyBooked.MaxValue = dataItem.AvailableQty;
                txtQtyBooked.MinValue = dataItem.AvailableQty == 0 ? 0 : 1;
            }

            lnkItem.ToolTip = item.Name;

            itemThumbDisplay.DocumentMediaId = thumbnailMediaId;

            bool isIteminWatchList = Support.IsItemInWatchList(item.ItemId, CompanyId);
            lnkbtnAddToWatchList.Visible = !isIteminWatchList;
            lnkbtnAddToWatchList.CommandArgument = item.ItemId.ToString();
            lnkbtnAddToWatchList.CommandName = "AddToWatchList";
            ltrlAddedToWatchList.Visible = isIteminWatchList;

            Literal litAvailableQty = (Literal)e.Item.FindControl("litAvailableQty");
            litAvailableQty.Visible = rbtnItem.Enabled = !HasNoDateConfigured();
            rbtnItem.ToolTip = HasNoDateConfigured() ? SelectBookingPeriodMsg : string.Empty;

            litAvailableQty.Text = dataItem.AvailableQty > 0 ? dataItem.AvailableQty + " available" : "Not available";
        }

        /// <summary>
        /// Handles the NeedDataSource event of the lvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadListViewNeedDataSourceEventArgs" /> instance containing the event data.</param>
        protected void lvItemList_NeedDataSource(object sender, RadListViewNeedDataSourceEventArgs e)
        {
            ClearSelectedItemId();

            if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ThumbnailView)
            {
                int count = 0;
                lvItemList.DataSource = GetItemList(lvItemList.PageSize, lvItemList.CurrentPageIndex,
                    lvItemList.SortExpressions.GetSortString(), out count);
            }
        }

        #endregion List view

        /// <summary>
        /// Handles the DisplayModeChanged event of the displaySettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void displaySettings_DisplayModeChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (this.ItemId == 0)
                {
                    ClearSelectedItemId();
                }
                if (IsPostBack)
                {
                    inventoryProjectPanel.ItemId = 0; // to clear the plus button after coming back to inventory from item detail and changing the project panel drop downs
                }
                inventoryProjectPanel.UpdateProjectPanel();
                FindFromDate = inventoryProjectPanel.FromDate;
                FindToDate = inventoryProjectPanel.ToDate;
                LoadData(false);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (this.GetBL<CompanyBL>().HasEditPermissionForInventoryStaff(this.CompanyId, UserID, sbInventoryLocations.SelectedLocationId))
                {
                    if (sbPackageLimitsValidation.Validate())
                    {
                        int itemTypeId;
                        cboSearch.Text = string.Empty;
                        FindByName = string.Empty;
                        if (!string.IsNullOrEmpty(ddlSearchItemTypes.SelectedValue))
                        {
                            ddlSearchItemTypes.SelectedValue = ddlAddItemTypes.SelectedValue;
                        }

                        int findByItemType;
                        if (int.TryParse(ddlSearchItemTypes.SelectedValue, out findByItemType))
                        {
                            FindByItemTypeId = findByItemType;
                        }

                        upnlFindItems.Update();

                        if (this.IsValid && int.TryParse(ddlAddItemTypes.SelectedValue, out itemTypeId))
                        {
                            string itemName = txtName.Text.Trim();
                            if (!sbInventoryLocations.SelectedLocationId.HasValue || sbInventoryLocations.SelectedLocationId.HasValue && GetBL<LocationBL>().GetLocation(sbInventoryLocations.SelectedLocationId.Value) != null)
                            {
                                Data.Item item = new Data.Item();

                                #region Fill data

                                item.CompanyId = this.CompanyId;
                                item.Name = itemName;
                                item.Description = txtDescription.Text.Trim();
                                item.ItemTypeId = itemTypeId;

                                if (txtQuantity.Value.HasValue)
                                {
                                    item.Quantity = (int)txtQuantity.Value.Value;
                                }
                                else
                                {
                                    item.Quantity = null;
                                }

                                item.LocationId = sbInventoryLocations.SelectedLocationId;
                                item.CreatedByUserId = item.LastUpdatedByUserId = UserID;
                                item.CreatedDate = item.LastUpdatedDate = Now;
                                item.IsActive = true;
                                item.IsManuallyAdded = true;
                                item.VisibilityLevelCodeId = Support.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");

                                #endregion Fill data

                                DataContext.Items.AddObject(item);
                                DataContext.SaveChanges();
                            }
                            else
                            {
                                lblError.Text = "Location has already been deleted.";
                                popupSaveGenericError.ShowPopup();
                                sbInventoryLocations.LoadData();
                                searchInvLocation.LoadData();
                            }

                            LoadData();
                            ClearSelectedItemId();
                            txtName.Focus();
                        }
                    }
                }
                else
                {
                    projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnFind_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadSearch();
            }
        }

        /// <summary>
        /// Handles the Click event of the ibtnClearSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ibtnClearSearch_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                cboSearch.Text = string.Empty;
                FindByName = string.Empty;
                LoadData();
                ClearSelectedItemId();
                inventoryProjectPanel.ItemId = 0; // to clear the plus button after coming back to inventory from item detail and changing the project panel drop downs
            }
        }

        /// <summary>
        /// Shows the thumbnail image of the item as a tooltip.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ToolTipUpdateEventArgs" /> instance containing the event data.</param>
        protected void tooltipManager_AjaxUpdate(object sender, ToolTipUpdateEventArgs e)
        {
            if (!StopProcessing)
            {
                int itemId = 0;
                if (int.TryParse(e.Value, out itemId))
                {
                    var item = GetBL<InventoryBL>().GetItem(itemId);
                    if (item != null && item.Code.SortOrder >= GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(item.CompanyId.Value, this.UserID, item.LocationId, true).SortOrder)
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
        /// Handles the OnSelectedIndexChanged event of the ddlSearchItemTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlSearchItemTypes_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadSearch();
            }
        }

        /// <summary>
        /// Handles the ItemsRequested event of the cboSearch control.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="e">The <see cref="RadComboBoxItemsRequestedEventArgs" /> instance containing the event data.</param>
        protected void cboSearch_ItemsRequested(object o, RadComboBoxItemsRequestedEventArgs e)
        {
            string keyword = e.Text.Trim().ToLower();

            if (keyword == string.Empty)
            {
                return;
            }

            try
            {
                bool isSharedInventory = false;
                int companyId = SharedInventoryCompanyId;
                // Take top 10 matches
                if (SharedInventoryCompanyId != -1)
                {
                    isSharedInventory = false;
                }
                else
                {
                    isSharedInventory = true;
                    companyId = this.CompanyId;
                }

                Data.Item[] itemNames = this.GetBL<InventoryBL>().GetInventorySearchItems(this.UserID, companyId, keyword, FindByItemTypeId, isSharedInventory,
                         FindFromDate, FindToDate, SelectedLocationId == -1 ? (int?)null : SelectedLocationId, this.VisibilityFilterSelectedValue);

                int resultCount = itemNames.Length;

                for (int i = 0; i < resultCount; i++)
                {
                    //Search beginning of words.
                    string matchPattern = string.Format(@"\b{0}", Regex.Escape(keyword));
                    Match keywordMatch = Regex.Match(itemNames[i].Name, matchPattern, RegexOptions.IgnoreCase);
                    StringBuilder formattedItemText = new StringBuilder(itemNames[i].Name);

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

                        item.Text = itemNames[i].Name;

                        cboSearch.Items.Add(item);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the OnSelectedIndexChanged event of the rbtnWatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rbtnWatchList_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                tbtnManagerMode.Checked = false;

                if (groupWatchList.SelectedValue == Convert.ToString(InventoryDisplayMode.SearchResults))
                {
                    //divItemType.Visible = divCompany.Visible = true;
                    LoadSharedInventories();
                    DisplayMode = InventoryDisplayMode.SearchResults;
                    divManagerMode.Visible = this.IsAllowedToAddItems && true;
                    LoadData();
                    inventoryProjectPanel.DisplayModule = InventoryBookingPanel.ViewMode.CompanyInventory;
                    inventoryProjectPanel.SharedInventoryCompanyId = SharedInventoryCompanyId;
                }
                else
                {
                    //divItemType.Visible = divCompany.Visible = false;
                    DisplayMode = InventoryDisplayMode.WatchList;
                    divManagerMode.Visible = false;
                    LoadWatchListData();
                    inventoryProjectPanel.DisplayModule = InventoryBookingPanel.ViewMode.WatchList;
                    inventoryProjectPanel.SharedInventoryCompanyId = CompanyId;
                    ShowRemovedWatchListNotification();
                }

                ClearSelectedItemId();
                inventoryProjectPanel.UpdateProjectPanel();
            }
        }

        /// <summary>
        /// Handles the OnSelectedIndexChanged event of the ddlSPPCompanies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSPPCompanies_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                int sharedInventoryCompanyId = Convert.ToInt32(ddlSPPCompanies.SelectedValue);
                this.SharedInventoryCompanyId = sharedInventoryCompanyId;
                inventoryProjectPanel.SharedInventoryCompanyId = sharedInventoryCompanyId;
                inventoryProjectPanel.UpdateProjectPanel();
                if (sharedInventoryCompanyId != -1)
                {
                    searchInvLocation.Visible = true;
                    searchInvLocation.CompanyId = sharedInventoryCompanyId;
                    searchInvLocation.LoadData(showAll: this.CompanyId != sharedInventoryCompanyId);
                }
                else
                {
                    searchInvLocation.Visible = false;
                }

                SelectedLocationId = -1;
                LoadData();
                ClearSelectedItemId();
                //inventoryProjectPanel.RegisterCheckboxInitializeScript();

                divManagerMode.Visible = this.IsAllowedToAddItems && sharedInventoryCompanyId == this.CompanyId;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvItemList_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.CommandName == "AddToWatchList")
                {
                    int itemId = Convert.ToInt32(e.CommandArgument);
                    Data.Item item = GetBL<InventoryBL>().GetItem(itemId);

                    if (item != null)
                    {
                        if (CompanyId != SharedInventoryCompanyId && this.GetBL<InventoryBL>().IsCompanyInventorySharingRemoved(item.CompanyId.Value, CompanyId))
                        {
                            popupInventorySharingRemovedWarning.ShowInventorySharingRemoved(CompanyId, item.CompanyId.Value);
                        }
                        else if (this.GetBL<InventoryBL>().IsItemDeleted(itemId))
                        {
                            popupItemDeletedWarning.ShowItemDeleteMessagePopup(itemId, CompanyId);
                        }
                        else
                        {
                            Support.AddToWatchList(itemId, CompanyId);
                            LoadData(false);
                            ClearSelectedItemId();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvItemList_ItemCommand(object sender, RadListViewCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.CommandName == "AddToWatchList")
                {
                    int itemId = Convert.ToInt32(e.CommandArgument);
                    if (CompanyId != SharedInventoryCompanyId && this.GetBL<InventoryBL>().IsCompanyInventorySharingRemoved(SharedInventoryCompanyId, CompanyId))
                    {
                        popupInventorySharingRemovedWarning.ShowInventorySharingRemoved(CompanyId, SharedInventoryCompanyId);
                    }
                    else if (this.GetBL<InventoryBL>().IsItemDeleted(itemId))
                    {
                        popupItemDeletedWarning.ShowItemDeleteMessagePopup(itemId, CompanyId);
                    }
                    else
                    {
                        Support.AddToWatchList(itemId, CompanyId);
                        LoadData(false);
                        ClearSelectedItemId();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnClearListConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearListConfirm_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                DataContext.DeleteWatchList(CompanyId, UserID);
                popupClearWatchListConfirmation.HidePopup();
                inventoryProjectPanel.HiddenItemId = 0;
                inventoryProjectPanel.LoadData();
                LoadWatchListData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRespondRemovedWatchListItemNotification control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRespondRemovedWatchListItemNotification_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                this.GetBL<InventoryBL>().SetResponseToWatchListItemsNotifications(CompanyId, UserID);
                popupRemovedWatchListItemsNotification.HidePopup();
            }
        }

        /// <summary>
        /// Handles the DeleteCommand event of the gvWatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvWatchList_DeleteCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int itemId = (int)dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["ItemId"];

                if (!this.GetBL<InventoryBL>().IsItemDeleted(itemId))
                {
                    this.GetBL<InventoryBL>().DeleteWatchListItem(itemId, CompanyId, UserID);
                }
                ClearSelectedItemId();
                LoadWatchListData();
            }
        }

        /// <summary>
        /// Handles the PreRender event of the pagerInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pagerInventory_PreRender(object sender, EventArgs e)
        {
            RadDataPagerFieldItem radDataPagerFieldItem = null;
            foreach (Control c in pagerInventory.Controls)
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
                lblItemText.Text = pagerInventory.TotalRowCount == 1 ? "Item in" : "Items in";
                lblPagesText.Text = pagerInventory.PageCount == 1 ? "page" : "pages";
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
            if (!StopProcessing)
            {
                ExportReport(ReportTypes.Pdf);
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlVisibilityFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlVisibilityFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                lblVisibilityText.Text = ddlVisibilityFilter.SelectedValue.Contains("cannot") ? "Items CANNOT be seen by :" :
                    ddlVisibilityFilter.SelectedValue.Contains("can") ? "Items CAN be seen by :" : string.Empty;
                LoadSearch();
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Removes the item grouping by company.
        /// </summary>
        private void RemoveItemGroupingByCompany()
        {
            gvWatchList.MasterTableView.GetColumn("CompanyName").Visible = false;
            for (int i = 0; i < gvWatchList.MasterTableView.GroupByExpressions.Count; i++)
            {
                if (gvWatchList.MasterTableView.GroupByExpressions[i].GroupByFields[0].FieldName == "CompanyId")
                {
                    gvWatchList.MasterTableView.GroupByExpressions.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Loads the item sorting and paging.
        /// </summary>
        private void LoadInventorySortingAndPaging()
        {
            if (this.Request.QueryString["Sort"] != null && !string.IsNullOrEmpty(this.Request.QueryString["Sort"]))
            {
                string sortParam = this.Request.QueryString["Sort"];
                string[] sortParamArray = sortParam.Split(StageBitz.Common.Constants.GlobalConstants.parameterDelimiter);

                // validate sort parameters
                if (sortParamArray.Length == 7)
                {
                    if (sortParamArray[0] == "StatusSortOrder" || sortParamArray[0] == "ItemTypeName")
                    {
                        this.SortBy = sortParamArray[0];
                        int sortOrder;
                        if (int.TryParse(sortParamArray[1], out sortOrder))
                        {
                            this.SortOrder = sortOrder <= 2 && sortOrder >= 0 ? sortOrder : 1;
                        }
                    }
                    else
                    {
                        string[] sortByPropertyInfo = sortParamArray[0].Split('.');
                        string sortByProperty = sortByPropertyInfo.Length == 2 ? sortByPropertyInfo[1] : string.Empty;
                        if (!string.IsNullOrEmpty(sortByProperty))
                        {
                            Type itemType = typeof(Data.Item);
                            PropertyInfo[] properties = itemType.GetProperties();
                            foreach (PropertyInfo property in properties)
                            {
                                if (property.Name == sortByProperty)
                                {
                                    int sortOrder;
                                    if (int.TryParse(sortParamArray[1], out sortOrder))
                                    {
                                        this.SortOrder = sortOrder <= 2 && sortOrder > 0 ? sortOrder : 1;
                                    }

                                    this.SortBy = sortParamArray[0];
                                    break;
                                }
                            }
                        }
                    }

                    this.FindByName = Support.DecodeBase64String(sortParamArray[2]);

                    int pageIndex;
                    if (int.TryParse(sortParamArray[3], out pageIndex))
                    {
                        this.PageIndex = pageIndex;
                    }

                    int pageSize;
                    if (int.TryParse(sortParamArray[4], out pageSize))
                    {
                        this.PageSize = pageSize;
                    }

                    cboSearch.Text = this.FindByName;

                    this.FindFromDate = inventoryProjectPanel.FromDate;
                    this.FindToDate = inventoryProjectPanel.ToDate;
                }
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="company">The company.</param>
        private void LoadBreadCrumbs(Data.Company company)
        {
            if (company != null)
            {
                BreadCrumbs bc = GetBreadCrumbsControl();

                string companyUrl = null;
                if (Support.IsCompanyAdministrator(this.CompanyId))
                {
                    companyUrl = string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", this.CompanyId);
                }

                bc.AddLink(company.CompanyName, companyUrl);
                bc.AddLink("Company Inventory", null);
                bc.LoadControl();
            }
        }

        /// <summary>
        /// Loads All data. Delegate method for Inventory Project Panel
        /// </summary>
        private void LoadAllData()
        {
            if (DisplayMode == InventoryDisplayMode.SearchResults)
            {
                LoadData();
            }
            else if (DisplayMode == InventoryDisplayMode.WatchList)
            {
                LoadWatchListData();
            }
            ClearSelectedItemId();
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData(bool resetPageIndex = true)
        {
            if (resetPageIndex)
            {
                this.PageIndex = 0;
            }

            ClearInputFields();
            
            tooltipManager.TargetControls.Clear();

            // Not initializing Sort Varaibles. Only loading from previous values.
            List<InventoryItemData> items = null;
            int itemCount = 0;

            ltrlItemListTitleForDateFilteration.Text = BuildTitleText();
            divSearchItems.Visible = true;
            helpTipInventory.Visible = true;
            displaySettings.Visible = true;
            divAddInventoryItem.Visible = true;

            divItemGrid.Visible = true;
            divItemList.Visible = true;
            divWatchList.Visible = false;
            inventoryBulkUpdatePanel.CompanyId = this.CompanyId;
            if (tbtnManagerMode.Checked)
            {
                
                inventoryBulkUpdatePanel.LoadData();

                displaySettings.Visible = false;
                divItemGrid.Visible = false;
                divItemList.Visible = false;
                divCompany.Visible = false;

                divBulkUpdate.Visible = true;
                tbtnManagerMode.ToolTip = "On";

                // Right panel
                divInventoryBookingPanel.Attributes.Add("style", "display:none;");
                divInventoryBulkUpdatePanel.Attributes.Add("style", "display:block;");

                // Add Panel
                pnlAddItem.Attributes.Add("style", "display:none;");

                // Bulk Import Button
                divImportBulkItems.Attributes.Add("style", "display:none;");
                upnlImportBulkItems.Update();

                gvItemBulkEdit.MasterTableView.SortExpressions.Clear();
                GridSortExpression errorSort = new GridSortExpression();
                errorSort.FieldName = "HasError";
                errorSort.SortOrder = GridSortOrder.Descending;

                gvItemBulkEdit.MasterTableView.SortExpressions.AddSortExpression(errorSort);

                if (!string.IsNullOrEmpty(this.SortBy))
                {
                    GridSortExpression sortExpr = new GridSortExpression();
                    sortExpr.FieldName = this.SortBy;
                    sortExpr.SortOrder = (GridSortOrder)this.SortOrder;

                    gvItemBulkEdit.MasterTableView.SortExpressions.AddSortExpression(sortExpr);
                }

                gvItemBulkEdit.CurrentPageIndex = this.PageIndex;
                if (this.PageSize > 0)
                {
                    gvItemBulkEdit.PageSize = this.PageSize;
                }

                items = GetItemList(gvItemBulkEdit.MasterTableView.PageSize, gvItemBulkEdit.MasterTableView.CurrentPageIndex,
                    gvItemBulkEdit.MasterTableView.SortExpressions.GetSortString(), out itemCount, false, isManageInventory: true);
                gvItemBulkEdit.DataSource = items;
                gvItemBulkEdit.VirtualItemCount = itemCount;
                gvItemBulkEdit.DataBind();

                ddlVisibilityFilter.Visible = Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID) || GetBL<InventoryBL>().IsCompanyLocationManagerAnyLocation(this.CompanyId, this.UserID);
            }
            else
            {
                ddlVisibilityFilter.Visible = false;
                divInventoryBookingPanel.Attributes.Add("style", "display:block;");
                divInventoryBulkUpdatePanel.Attributes.Add("style", "display:none;");
                tbtnManagerMode.ToolTip = "Off";

                divBulkUpdate.Visible = false;
                divCompany.Visible = true;
                displaySettings.Visible = true;

                if (CompanyId == SharedInventoryCompanyId)
                {
                    pnlAddItem.Attributes.Add("style", "display:block;");
                    divImportBulkItems.Attributes.Add("style", "display:block;");
                }
                else
                {
                    pnlAddItem.Attributes.Add("style", "display:none;");
                    divImportBulkItems.Attributes.Add("style", "display:none;");
                }

                upnlImportBulkItems.Update();

                if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
                {
                    if (!string.IsNullOrEmpty(this.SortBy))
                    {
                        GridSortExpression sortExpr = new GridSortExpression();
                        sortExpr.FieldName = this.SortBy;
                        sortExpr.SortOrder = (GridSortOrder)this.SortOrder;

                        gvItemList.MasterTableView.SortExpressions.Clear();
                        gvItemList.MasterTableView.SortExpressions.AddSortExpression(sortExpr);
                    }

                    gvItemList.CurrentPageIndex = this.PageIndex;
                    if (this.PageSize > 0)
                    {
                        gvItemList.PageSize = this.PageSize;
                    }

                    items = GetItemList(gvItemList.MasterTableView.PageSize, gvItemList.MasterTableView.CurrentPageIndex,
                        gvItemList.MasterTableView.SortExpressions.GetSortString(), out itemCount, false);
                    gvItemList.VirtualItemCount = itemCount;
                    gvItemList.DataSource = items;
                    gvItemList.DataBind();
                    divItemGrid.Visible = true;
                    divItemList.Visible = false;
                    //Hide Available Quantity when No search date provided
                    gvItemList.MasterTableView.GetColumn("AvailableQty").Display = !HasNoDateConfigured();
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.SortBy))
                    {
                        RadListViewSortExpression sortExpr = new RadListViewSortExpression();
                        sortExpr.FieldName = this.SortBy;
                        sortExpr.SortOrder = (RadListViewSortOrder)this.SortOrder;

                        lvItemList.SortExpressions.Clear();
                        lvItemList.SortExpressions.AddSortExpression(sortExpr);
                    }

                    lvItemList.CurrentPageIndex = this.PageIndex;

                    if (this.PageSize > 0)
                    {
                        lvItemList.PageSize = this.PageSize;
                    }

                    items = GetItemList(lvItemList.PageSize, lvItemList.CurrentPageIndex,
                        lvItemList.SortExpressions.GetSortString(), out itemCount, false);
                    lvItemList.VirtualItemCount = itemCount;
                    lvItemList.DataSource = items;
                    lvItemList.DataBind();

                    divItemList.Visible = true;
                    divItemGrid.Visible = false;
                }
            }

            if (ItemId > 0)
            {
                var selecteditem = items.Where(i => i.Item.ItemId == ItemId).FirstOrDefault();
                if (selecteditem == null)
                    inventoryProjectPanel.HiddenItemId = inventoryProjectPanel.ItemId = 0;
            }

            if (!IsCompanyInSharedInventory)
            {
                ltrlItemListTitle.Text = string.Format(itemCount == 1 ? "1 Item found" : string.Format("{0} Items found", itemCount));
            }
            else
            {
                if (ddlSPPCompanies.SelectedValue.Equals("-1"))
                {
                    ltrlItemListTitle.Text = itemCount == 1 ? "1 Item found in All Shared Inventories" : string.Format("{0} Items found in All Shared Inventories", itemCount);
                }
                else
                {
                    ltrlItemListTitle.Text = itemCount == 1 ? string.Format("{0} Item found in {1}'s Inventory", itemCount, Support.TruncateString(ddlSPPCompanies.SelectedItem.Text, 10))
                        : string.Format("{0} Items found in {1}'s Inventory", itemCount, Support.TruncateString(ddlSPPCompanies.SelectedItem.Text, 10));
                }
            }

            upnlFindItems.Update();
            upnlItemList.Update();
            upnlDisplaySettings.Update();
            upnlAdd.Update();
        }

        /// <summary>
        /// Builds the title text.
        /// </summary>
        /// <returns></returns>
        private string BuildTitleText()
        {
            return HasNoDateConfigured() ? "Showing all Items" : string.Format("Showing Items available from {0} to {1}", Utils.FormatDate(FindFromDate), Utils.FormatDate(FindToDate));
        }

        /// <summary>
        /// Determines whether [has no date configured].
        /// </summary>
        /// <returns></returns>
        private bool HasNoDateConfigured()
        {
            return (FindFromDate == null && FindToDate == null);
        }

        /// <summary>
        /// Loads the watch list view
        /// </summary>
        private void LoadWatchListData()
        {
            //ClearSelectedItemId();
            List<WatchListItemDetails> watchListItems = GetWatchListItems();
            int itemCount = watchListItems != null ? watchListItems.Count() : 0;
            var groupByCompanyList = watchListItems != null ? watchListItems.GroupBy(g => g.CompanyId) : null;
            int companyCount = groupByCompanyList != null ? groupByCompanyList.Count() : 0;
            if (companyCount > 0)
            {
                ltrlItemListTitle.Text = string.Concat(itemCount == 1 ? "1 Item " : string.Format("{0} Items ", itemCount), string.Format(" from {0} {1}", companyCount, companyCount == 1 ? " Company" : " Companies"));
                ltrlItemListTitleForDateFilteration.Text = BuildTitleText();
            }
            else
            {
                ltrlItemListTitle.Text = ltrlItemListTitleForDateFilteration.Text = string.Empty;
            }
            divSearchItems.Visible = false;
            helpTipInventory.Visible = false;
            displaySettings.Visible = false;
            divAddInventoryItem.Visible = false;
            divItemGrid.Visible = false;
            divItemList.Visible = false;
            divInventoryBookingPanel.Attributes.Add("style", "display:block;");
            divInventoryBulkUpdatePanel.Attributes.Add("style", "display:none;");
            divBulkUpdate.Visible = false;

            divCompany.Visible = true;

            divWatchList.Visible = true;

            if (watchListItems != null)
            {
                gvWatchList.DataSource = watchListItems;
                gvWatchList.DataBind();
                divEmptyWatchList.Visible = false;
                divWatchListItems.Visible = true;
                //Hide Available Quantity when No search date provided
                gvWatchList.MasterTableView.GetColumn("AvailableQty").Display = !HasNoDateConfigured();
            }
            else
            {
                divEmptyWatchList.Visible = true;
                divemptyNormalWatchList.Visible = !IsCompanyInSharedInventory;
                divemptySPPWatchList.Visible = IsCompanyInSharedInventory;
                divWatchListItems.Visible = false;
            }
            upnlItemList.Update();
            upnlDisplaySettings.Update();
        }

        /// <summary>
        /// Shows the removed watch list notification.
        /// </summary>
        private void ShowRemovedWatchListNotification()
        {
            string companyNames = this.GetBL<InventoryBL>().GetWatchListItemRemovedCompanyList(CompanyId, UserID);
            if (companyNames != string.Empty)
            {
                ltrWatchListItemsRemovedCompany.Text = companyNames;
                popupRemovedWatchListItemsNotification.ShowPopup();
            }
        }

        /// <summary>
        /// Load the look in drop down in shared inventory
        /// </summary>
        private void LoadSharedInventories()
        {
            List<SharedInventoryCompaniesData> sppCompanies = this.GetBL<InventoryBL>().GetSharedInventoryListForCompanyCanAccess(CompanyId);
            if (sppCompanies.Count() > 1) // company in shared Inventory
            {
                ddlSPPCompanies.Items.Clear();
                ddlSPPCompanies.DataSource = sppCompanies;
                ddlSPPCompanies.DataValueField = "CompanyId";
                ddlSPPCompanies.DataTextField = "CompanyName";
                ddlSPPCompanies.DataBind();

                ListItem item = new ListItem();
                item.Text = "All Shared Inventories";
                item.Value = "-1";
                ddlSPPCompanies.Items.Add(item);

                if (!sppCompanies.Select(c => c.CompanyId).ToList().Contains(SharedInventoryCompanyId))
                    SharedInventoryCompanyId = CompanyId;

                ddlSPPCompanies.SelectedValue = SharedInventoryCompanyId.ToString();

                ddlSPPCompanies.Visible = true;
            }
            else
            {
                SharedInventoryCompanyId = CompanyId;
                IsCompanyInSharedInventory = false;
                ddlSPPCompanies.Visible = false;
            }
        }

        /// <summary>
        /// Gets the item list.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="count">The count.</param>
        /// <param name="isInitializeSortingSettings">if set to <c>true</c> [is initialize sorting settings].</param>
        /// <param name="isManageInventory">if set to <c>true</c> [is manage inventory].</param>
        /// <returns>
        /// the Item list
        /// </returns>
        private List<InventoryItemData> GetItemList(int pageSize, int pageIndex, string sortBy, out int count, bool isInitializeSortingSettings = true, bool isManageInventory = false)
        {
            int itemIndex = pageIndex * pageSize + 1;

            if (isInitializeSortingSettings)
            {
                InitializeSortingSettings();
            }

            string findName = FindByName;
            if (string.IsNullOrEmpty(findName))
            {
                findName = null;
            }

            return this.GetBL<InventoryBL>().GetInventoryItems(this.UserID,
                this.SharedInventoryCompanyId, CompanyId, findName, FindByItemTypeId,
                FindFromDate, FindToDate, SelectedLocationId == -1 ? (int?)null : SelectedLocationId,
                this.VisibilityFilterSelectedValue, pageSize, itemIndex, sortBy, out count,
                errorIds: inventoryBulkUpdatePanel.VisibilityUpdateErrorItemIds, isManageInventory: isManageInventory);
        }

        /// <summary>
        /// Gets Watch List Item list.
        /// </summary>
        /// <returns>the watch list item list</returns>
        private List<WatchListItemDetails> GetWatchListItems()
        {
            return this.GetBL<InventoryBL>().GetWatchListItems(CompanyId, UserID, FindFromDate, FindToDate);
        }

        /// <summary>
        /// Initializes the sorting settings.
        /// </summary>
        private void InitializeSortingSettings()
        {
            if (!tbtnManagerMode.Checked)
            {
                if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
                {
                    if (gvItemList.MasterTableView.SortExpressions.Count != 0)
                    {
                        string name = gvItemList.MasterTableView.SortExpressions[0].FieldName;
                        GridSortOrder order = gvItemList.MasterTableView.SortExpressions[0].SortOrder;
                        this.SortBy = name;
                        this.SortOrder = (int)order;
                    }

                    this.PageIndex = gvItemList.CurrentPageIndex;
                    this.PageSize = gvItemList.PageSize;
                }
                else
                {
                    if (lvItemList.SortExpressions.Count != 0)
                    {
                        string name = lvItemList.SortExpressions[0].FieldName;
                        RadListViewSortOrder order = lvItemList.SortExpressions[0].SortOrder;
                        this.SortBy = name;
                        this.SortOrder = (int)order;
                    }

                    this.PageIndex = lvItemList.CurrentPageIndex;
                    this.PageSize = lvItemList.PageSize;
                }
            }
            else
            {
                if (gvItemBulkEdit.MasterTableView.SortExpressions.Count != 0)
                {
                    string name = gvItemBulkEdit.MasterTableView.SortExpressions[0].FieldName;
                    GridSortOrder order = gvItemBulkEdit.MasterTableView.SortExpressions[0].SortOrder;
                    this.SortBy = name;
                    this.SortOrder = (int)order;
                }

                this.PageIndex = gvItemBulkEdit.CurrentPageIndex;
                this.PageSize = gvItemBulkEdit.PageSize;
            }
        }

        /// <summary>
        /// Clears out all user inputs.
        /// </summary>
        private void ClearInputFields()
        {
            txtName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtQuantity.Value = null;
        }

        /// <summary>
        /// Populates the item type dropdowns.
        /// </summary>
        private void PopulateItemTypeDropdowns()
        {
            var itemTypes = Utils.GetALLItemTypes();
            ddlAddItemTypes.DataSource = itemTypes;
            ddlAddItemTypes.DataValueField = "ItemTypeId";
            ddlAddItemTypes.DataTextField = "Name";
            ddlAddItemTypes.DataBind();

            ddlSearchItemTypes.DataSource = itemTypes;
            ddlSearchItemTypes.DataValueField = "ItemTypeId";
            ddlSearchItemTypes.DataTextField = "Name";
            ddlSearchItemTypes.DataBind();
        }

        /// <summary>
        /// Registers the checkbox initialize script.
        /// </summary>
        private void RegisterInitializeScript()
        {
            string script = string.Format("BindEvents('{0}', {1}, '{2}', '{3}');InitializeCheckBoxes();addEventFired = false;", inventoryProjectPanel.BookingCode,
                inventoryProjectPanel.ItemTypeId, inventoryProjectPanel.FromDate, inventoryProjectPanel.ToDate);
            ScriptManager.RegisterStartupScript(this, GetType(), "CheckboxInitialize", script, true);
        }

        /// <summary>
        /// Loads the search.
        /// </summary>
        private void LoadSearch()
        {
            FindByName = cboSearch.Text.Trim().ToLower();
            FindFromDate = inventoryProjectPanel.FromDate;
            FindToDate = inventoryProjectPanel.ToDate;

            int searchItemTypeId;
            if (int.TryParse(ddlSearchItemTypes.SelectedValue, out searchItemTypeId))
            {
                FindByItemTypeId = searchItemTypeId;
            }
            else
            {
                FindByItemTypeId = 0;
            }

            LoadData();
            ClearSelectedItemId();
            inventoryProjectPanel.ItemId = 0;// to clear the plus button after coming back to inventory from item detail and changing the project panel drop downs
        }

        /// <summary>
        /// Clears the selected item id.
        /// </summary>
        private void ClearSelectedItemId()
        {
            inventoryProjectPanel.HiddenItemId = 0;
            ScriptManager.RegisterStartupScript(this, GetType(), "DisplayModeChanged", "$(document).trigger('onItemClicked', [0, false]);", true);
        }

        /// <summary>
        /// Gets the booking parameters.
        /// </summary>
        /// <returns>The booking parameters.</returns>
        private string GetBookingParam(int bookedQty)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", inventoryProjectPanel.BookingCode, StageBitz.Common.Constants.GlobalConstants.parameterDelimiter,
                inventoryProjectPanel.ItemTypeId, StageBitz.Common.Constants.GlobalConstants.parameterDelimiter,
                inventoryProjectPanel.FromDate, StageBitz.Common.Constants.GlobalConstants.parameterDelimiter, inventoryProjectPanel.ToDate, StageBitz.Common.Constants.GlobalConstants.parameterDelimiter, bookedQty);
        }

        /// <summary>
        /// Gets the item detail page URL.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>The item detail page URL.</returns>
        private string GetItemDetailPageUrl(int itemId, int bookedQty)
        {
            string sorting = GetSortParam();
            string booking = GetBookingParam(bookedQty);
            string inventory = Support.GetInventoryNavigateURLParams(ddlAddItemTypes.SelectedValue, ddlSearchItemTypes.SelectedValue,
                SharedInventoryCompanyId, SelectedLocationId.ToString(), sbInventoryLocations.SelectedLocationId.ToString());

            return ResolveUrl(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}&Sort={2}&Booking={3}&Inventory={4}",
                    itemId, CompanyId, sorting, booking, inventory));
        }

        /// <summary>
        /// Gets the base item detail page URL.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        private string GetBaseItemDetailPageUrl(int itemId)
        {
            return ResolveUrl(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}",
                    itemId, CompanyId));
        }

        /// <summary>
        /// Gets the sort parameter.
        /// </summary>
        /// <returns>The sort parameter.</returns>
        private string GetSortParam()
        {
            string delimiter = StageBitz.Common.Constants.GlobalConstants.parameterDelimiter.ToString();
            string param = string.Concat(this.SortBy, delimiter,
                    this.SortOrder.ToString(CultureInfo.InvariantCulture), delimiter,
                    Support.EncodeToBase64String(this.FindByName), delimiter,
                    this.PageIndex.ToString(CultureInfo.InvariantCulture), delimiter,
                    this.PageSize.ToString(CultureInfo.InvariantCulture), delimiter,
                    Support.FormatDate(this.FindFromDate), delimiter,
                    Support.FormatDate(this.FindToDate));

            return Server.UrlEncode(param);
        }

        /// <summary>
        /// Loads the inventory parameters.
        /// </summary>
        private void LoadInventoryParams()
        {
            string[] inventoryParams = this.InventoryParam.Split(StageBitz.Common.Constants.GlobalConstants.parameterDelimiter);
            if (inventoryParams.Length == 5)
            {
                int addItemTypeId;
                if (int.TryParse(inventoryParams[0], out addItemTypeId))
                {
                    ddlAddItemTypes.SelectedValue = inventoryParams[0];
                }

                int searchItemTypeId;
                if (int.TryParse(inventoryParams[1], out searchItemTypeId))
                {
                    ddlSearchItemTypes.SelectedValue = inventoryParams[1];
                    FindByItemTypeId = searchItemTypeId;
                }

                int sppCompanyId;
                if (int.TryParse(inventoryParams[2], out sppCompanyId))
                {
                    this.SharedInventoryCompanyId = sppCompanyId;
                }

                int searchLocationId;
                if (int.TryParse(inventoryParams[3], out searchLocationId))
                {
                    SelectedLocationId = searchLocationId;
                }
                else
                    SelectedLocationId = -1;

                int addLocationId;
                if (int.TryParse(inventoryParams[4], out addLocationId))
                {
                    sbInventoryLocations.SelectedLocationId = addLocationId;
                }
                else
                    sbInventoryLocations.SelectedLocationId = null;
            }
        }

        /// <summary>
        /// Loads the booking parameters.
        /// </summary>
        private void LoadBookingParams()
        {
            string[] bookingParams = this.BookingParam.Split(StageBitz.Common.Constants.GlobalConstants.parameterDelimiter);
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
                    BookedQty = bookedQty;
                }
            }
        }

        /// <summary>
        /// Populates the visibility dropdown.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void PopulateVisibilityDropdown()
        {
            int aboveSharedInventoryCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");
            int aboveInventoryObserverCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IO");
            int aboveInventoryStaffCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IS");
            int aboveInventoryAdminCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_IA");

            this.ddlVisibilityFilter.Items.Add(new ListItem("Select Visibility", string.Empty));
            this.ddlVisibilityFilter.AddItemGroup("Items CAN be seen by");
            this.ddlVisibilityFilter.Items.Add(new ListItem("Administrators only", string.Format("can{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryAdminCodeId)));
            this.ddlVisibilityFilter.Items.Add(new ListItem("Administrators & Staff only", string.Format("can{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryStaffCodeId)));
            this.ddlVisibilityFilter.Items.Add(new ListItem("Everyone in my Inventory Team", string.Format("can{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryObserverCodeId)));
            this.ddlVisibilityFilter.Items.Add(new ListItem("Visitors from Shared Inventories", string.Format("can{0}{1}", VisibilityFilterValueDelimiter, aboveSharedInventoryCodeId)));
            this.ddlVisibilityFilter.AddItemGroup("Items CANNOT be seen by");
            this.ddlVisibilityFilter.Items.Add(new ListItem("Inventory Staff", string.Format("cannot{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryAdminCodeId)));
            this.ddlVisibilityFilter.Items.Add(new ListItem("Inventory Observers", string.Format("cannot{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryStaffCodeId)));
            this.ddlVisibilityFilter.Items.Add(new ListItem("Visitors from Shared Inventories", string.Format("cannot{0}{1}", VisibilityFilterValueDelimiter, aboveInventoryObserverCodeId)));
        }

        #endregion Private Methods

        #region Web Methods

        /// <summary>
        /// Gets the tool tip to display in project panel.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="selectedProjectId">The selected project identifier.</param>
        /// <param name="selectedItemBriefTypeId">The selected item brief type identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="bookedQty">The booked qty.</param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static dynamic GetToolTipToDisplayInProjectPanel(int itemId, int selectedProjectId, int selectedItemBriefTypeId, string fromDate, string toDate, int bookedQty)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);

                BookingPanelToolTipResultForItem projectPanelToolTipResultForItem = inventoryBL.GetBookingPanelItemBriefToolTipResultForItem(
                        itemId, selectedProjectId, selectedItemBriefTypeId, Support.UserID,
                        string.IsNullOrEmpty(fromDate) ? (DateTime?)null : DateTime.Parse(fromDate),
                        string.IsNullOrEmpty(toDate) ? (DateTime?)null : DateTime.Parse(toDate),
                        bookedQty);

                return projectPanelToolTipResultForItem;
            }
        }

        /// <summary>
        /// Gets the tool tip to display in my booking panel.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="bookingId">The booking identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="bookedQty">The booked qty.</param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static dynamic GetToolTipToDisplayInMyBookingPanel(int itemId, int bookingId, string fromDate, string toDate, int bookedQty)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);

                BookingPanelToolTipResultForItem projectPanelToolTipResultForItem = inventoryBL.GetBookingPanelNonProjectBookingToolTipResultForItem(
                        itemId, Support.UserID, bookingId,
                        string.IsNullOrEmpty(fromDate) ? (DateTime?)null : DateTime.Parse(fromDate),
                        string.IsNullOrEmpty(toDate) ? (DateTime?)null : DateTime.Parse(toDate),
                        bookedQty);

                return projectPanelToolTipResultForItem;
            }
        }

        #endregion Web Methods

        #region Report export

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            InitializeSortingSettings();

            InventoryItemListReportParameters parameters = new InventoryItemListReportParameters
            {
                CompanyId = this.CompanyId,
                FindByItemTypeId = this.FindByItemTypeId,
                FindByName = this.FindByName,
                FindFromDate = this.FindFromDate,
                FindToDate = this.FindToDate,
                LocationId = SelectedLocationId,
                HasNoDateConfigured = HasNoDateConfigured(),
                IsThumbnailMode = displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ThumbnailView,
                SharedInventoryCompanyId = this.SharedInventoryCompanyId,
                SortExpression = gvItemList.MasterTableView.SortExpressions.GetSortString(),
                UserId = this.UserID,
                ItemVisibilityCodeId = this.VisibilityFilterSelectedValue
            };

            Data.Company company = GetBL<CompanyBL>().GetCompany(parameters.CompanyId);
            string fileTailName = string.Format("_{0}'s_Inventory", company.CompanyName);
            string fileName = company.CompanyName + fileTailName;

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateInventoryItemListReport(parameters, exportType,
                    out fileNameExtension, out encoding, out mimeType);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }

        #endregion Report export
    }
}