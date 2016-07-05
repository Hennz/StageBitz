using StageBitz.Common;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Location;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Inventory
{
    /// <summary>
    /// ASP web page for Manage inventory page.
    /// </summary>
    public partial class InventorySharing : PageBase
    {
        #region Enums

        /// <summary>
        /// Enum for views of the manage grid.
        /// </summary>
        public enum Views
        {
            Yes = 0,
            No = 1,
            Pending = 2
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is company read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is company read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompanyReadOnly
        {
            get
            {
                if (ViewState["IsCompanyReadOnly"] != null)
                {
                    return (bool)ViewState["IsCompanyReadOnly"];
                }
                else
                {
                    return true;
                }
            }
            set
            {
                ViewState["IsCompanyReadOnly"] = value;
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
        /// Gets or sets the find by location.
        /// </summary>
        /// <value>
        /// The find by location.
        /// </value>
        private string FindByLocation
        {
            get
            {
                if (ViewState["FindByLocation"] == null)
                {
                    ViewState["FindByLocation"] = string.Empty;
                }

                return ViewState["FindByLocation"].ToString();
            }

            set
            {
                ViewState["FindByLocation"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has inventory admin rights.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has inventory admin rights; otherwise, <c>false</c>.
        /// </value>
        public bool HasInventoryAdminRights
        {
            get
            {
                if (ViewState["HasInventoryAdminRights"] != null)
                {
                    return (bool)ViewState["HasInventoryAdminRights"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["HasInventoryAdminRights"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has location manager rights any location.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has location manager rights any location; otherwise, <c>false</c>.
        /// </value>
        private bool HasLocationManagerRightsAnyLocation
        {
            get
            {
                if (ViewState["HasLocationManagerRightsAnyLocation"] != null)
                {
                    return (bool)ViewState["HasLocationManagerRightsAnyLocation"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["HasLocationManagerRightsAnyLocation"] = value;
            }
        }

        #endregion Properties

        #region Private Fields

        private int InventoryStaffCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
        private int InventoryAdminCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").CodeId;
        private int InventoryObserverCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
        private int LocationManagerCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
        private int NoAccessCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "NOINVENTORYACCESS").CodeId;

        #endregion Private Fields

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Permission denied for this page.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool isCompanyAdmin = Support.IsCompanyAdministrator(this.CompanyId);
                if (!(isCompanyAdmin || Support.IsCompanyInventoryStaffMember(this.CompanyId)))
                {
                    throw new ApplicationException("Permission denied for this page.");
                }

                hyperLinkInventorySharing.NavigateUrl = ResolveUrl(string.Format("~/Inventory/InventorySharing.aspx?CompanyId={0}", this.CompanyId));
                lnkCompanyInventory.HRef = ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
                hyperLinkMyBooking.NavigateUrl = string.Format("~/Inventory/MyBookings.aspx?CompanyId={0}", this.CompanyId);

                Data.Company company = DataContext.Companies.Where(c => c.CompanyId == this.CompanyId).FirstOrDefault();
                DisplayTitle = string.Concat("Manage ", Support.TruncateString(company.CompanyName, 30), "'s Inventory");
                hdnPendingSharingStatusCode.Value = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "PENDING").ToString();
                hdnActiveSharingStatusCode.Value = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "ACTIVE").ToString();

                InitializeUI();
                InitializeUserData();
                InitializeAdminUI();
                LoadBreadCrumbs(company, isCompanyAdmin);

                sbSearchUsers.CompanyId = CompanyId;
                sbSearchUsers.LoadControl();

                sbManageBookings.CompanyId = this.CompanyId;
                sbManageBookings.LoadData();

                string tabId = Request.QueryString["TabId"];
                if (tabId != null)
                {
                    int tabIndex = Convert.ToInt16(tabId);
                    inventorySharingTabs.SelectedIndex = tabIndex;
                    sharingPages.SelectedIndex = tabIndex;
                }

                sbCompanyWarningDisplay.CompanyID = this.CompanyId;
                sbCompanyWarningDisplay.LoadData();

                sbMoveInventoryLocations.CompanyId = this.CompanyId;
                sbInventoryLocationRoles.CompanyId = this.CompanyId;

                projectWarningPopupInventory.Mode = UserWeb.Controls.Project.ProjectWarningMode.Inventory;
                projectWarningPopupInventory.CompanyId = CompanyId;

                rbtnInventoryAdmin.Attributes.Add("onClick", "SelectInventoryAdmin()");
            }

            RegisterInitializeScript();

            sbSearchUsers.OnInformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode, bool shouldNavigateToHome)
            {
                projectWarningPopupInventory.ShowErrorPopup(errorCode, shouldNavigateToHome);
            };

            sbManageBookings.OnInformCompanyInventoryToShowErrorPopup += delegate(ErrorCodes errorCode)
            {
                projectWarningPopupInventory.ShowErrorPopup(errorCode, true);
            };
        }

        /// <summary>
        /// Handles the ItemsRequested event of the cboSearchCompanyName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs"/> instance containing the event data.</param>
        protected void cboSearchCompanyName_ItemsRequested(object sender, Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs e)
        {
            string keyword = e.Text.Trim().ToLower();

            if (keyword == string.Empty)
            {
                return;
            }

            try
            {
                Data.Company[] companyNames = this.GetBL<InventoryBL>().GetCompanySearchNamesForInventoryShare(this.CompanyId, keyword);

                int resultCount = companyNames.Length;

                for (int i = 0; i < resultCount; i++)
                {
                    //Search beginning of words.
                    string matchPattern = string.Format(@"\b{0}", Regex.Escape(keyword));
                    Match keywordMatch = Regex.Match(companyNames[i].CompanyName, matchPattern, RegexOptions.IgnoreCase);
                    StringBuilder formattedItemText = new StringBuilder(companyNames[i].CompanyName);

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

                        item.Text = companyNames[i].CompanyName;

                        cboSearchCompanyName.Items.Add(item);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShare control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShare_Click(object sender, EventArgs e)
        {
            if (!this.StopProcessing)
            {
                InitializeUserData();
                if (this.HasInventoryAdminRights)
                {
                    string errorMsg = string.Empty;
                    int selectedCompanyId = int.Parse(hdnSelectedCompanyId.Value);

                    int viewingCompanyId = 0;
                    int comppanyIdToView = 0;
                    string successMsgOption1 = "Notifying them that they can now browse your Inventory and send you requests.";
                    string successMsgOption2 = "Requesting you are given access to their Inventory.";

                    StringBuilder msgBuilder = new StringBuilder("<h2>The Inventory Administrator of the selected Company has been sent an email...</h2>");
                    msgBuilder.Append("<ul><li>");
                    if (selectedCompanyId > 0)
                    {
                        Data.Company company = GetBL<CompanyBL>().GetCompany(selectedCompanyId);
                        if (company != null)
                        {
                            if (company.IsCompanyVisibleForSearchInInventory)
                            {
                                if (rdViewMyCompany.Checked)
                                {
                                    viewingCompanyId = selectedCompanyId;
                                    comppanyIdToView = CompanyId;
                                    msgBuilder.Append(successMsgOption1);
                                }
                                else if (rdViewOtherCompany.Checked)
                                {
                                    viewingCompanyId = CompanyId;
                                    comppanyIdToView = selectedCompanyId;
                                    msgBuilder.Append(successMsgOption2);
                                }
                                else
                                {
                                    viewingCompanyId = CompanyId;
                                    comppanyIdToView = selectedCompanyId;
                                    msgBuilder.Append(successMsgOption1);
                                    msgBuilder.Append("</li><li>");
                                    msgBuilder.Append(successMsgOption2);
                                }
                                msgBuilder.Append("</li><ul>");

                                errorMsg = this.GetBL<InventoryBL>().ShareInventory(viewingCompanyId, comppanyIdToView, rdBoth.Checked, UserID, Support.GetSystemUrl(), CompanyId);
                            }
                            else
                            {
                                errorMsg = "This Company hasn't share their Inventory.";
                            }

                            if (errorMsg.Length > 0)
                            {
                                popupSharingDetails.Title = "Request Failed";
                                litMsg.Text = errorMsg;
                            }
                            else
                            {
                                popupSharingDetails.Title = "An email has been sent";
                                litMsg.Text = msgBuilder.ToString();
                            }

                            lvCompanyList.Rebind();
                            RegisterInitializeScript();
                            InitializeAdminUI();
                        }
                        else
                        {
                            litMsg.Text = "Please select a Company to share the Inventory with";
                        }
                    }
                    else
                    {
                        litMsg.Text = "Please select a Company to share the Inventory with";
                    }

                    popupSharingDetails.ShowPopup(1001);
                }
                else
                {
                    projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the lvCompanyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.RadListViewNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void lvCompanyList_NeedDataSource(object sender, Telerik.Web.UI.RadListViewNeedDataSourceEventArgs e)
        {
            List<CompanySearchData> companyList = LoadSearch();
            lvCompanyList.DataSource = companyList;
            int noOfCompanies = companyList.Count();
            ltrNoOfCompaniesFound.Text = noOfCompanies == 1 ? noOfCompanies.ToString() + " Company" : noOfCompanies.ToString() + " Companies";
            divSharingSection.Visible = (noOfCompanies > 0);
        }

        /// <summary>
        /// Handles the Click event of the btnFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFind_Click(object sender, EventArgs e)
        {
            if (!this.StopProcessing)
            {
                lvCompanyList.Rebind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvCompanyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvCompanyList_ItemDataBound(object sender, RadListViewItemEventArgs e)
        {
            int thumbnailMediaId = ((dynamic)(e.Item)).DataItem.ThumbnailMediaId;
            Data.Company company = ((dynamic)(e.Item)).DataItem.Company;
            int accessToMyCompanyStatusCodeId = ((dynamic)(e.Item)).DataItem.AccessToMyCompanyStatusCodeId;
            int accessToSelectedCompanyStatusCodeId = ((dynamic)(e.Item)).DataItem.AccessToSelectedCompanyStatusCodeId;

            ImageDisplay itemThumbDisplay = (ImageDisplay)e.Item.FindControl("itemThumbDisplay");
            RadioButton rbtnCompany = (RadioButton)e.Item.FindControl("rbtnCompany");
            Label lblCompanyName = (Label)e.Item.FindControl("lblCompanyName");
            Literal ltrCompanyCity = (Literal)e.Item.FindControl("ltrCompanyCity");
            Literal ltrCompanyCountry = (Literal)e.Item.FindControl("ltrCompanyCountry");
            Image imgAccessToMyCompany = (Image)e.Item.FindControl("imgAccessToMyCompany");
            Image imgAccessToSelectedCompany = (Image)e.Item.FindControl("imgAccessToSelectedCompany");
            HtmlGenericControl divAccessToMyCompany = (HtmlGenericControl)e.Item.FindControl("divAccessToMyCompany");
            HtmlGenericControl divAccessToSelectedCompany = (HtmlGenericControl)e.Item.FindControl("divAccessToSelectedCompany");
            HtmlGenericControl divToolTip = (HtmlGenericControl)e.Item.FindControl("divToolTip");

            int activeSharingStatusCodeId = int.Parse(hdnActiveSharingStatusCode.Value);
            int pendingSharingStatusCodeId = int.Parse(hdnPendingSharingStatusCode.Value);
            string selectedCompanyName = company.CompanyName;
            string toolTip = string.Empty;
            if (accessToMyCompanyStatusCodeId > 0 && accessToSelectedCompanyStatusCodeId == 0)
            {
                imgAccessToMyCompany.ImageUrl = accessToMyCompanyStatusCodeId == activeSharingStatusCodeId ? "~/Common/Images/shareleftactive.png" : "~/Common/Images/shareleftpending.png";
                toolTip = accessToMyCompanyStatusCodeId == activeSharingStatusCodeId ? selectedCompanyName + " can already see your Inventory." : selectedCompanyName + " has already invited you to share your Inventory with them.";
                divToolTip.Attributes.Add("title", toolTip);
                divAccessToMyCompany.Visible = true;
                divAccessToSelectedCompany.Visible = false;
                divAccessToMyCompany.Style["top"] = "0px";
            }
            else if (accessToMyCompanyStatusCodeId == 0 && accessToSelectedCompanyStatusCodeId > 0)
            {
                imgAccessToSelectedCompany.ImageUrl = accessToSelectedCompanyStatusCodeId == activeSharingStatusCodeId ? "~/Common/Images/sharerightactive.png" : "~/Common/Images/sharerightpending.png";
                toolTip = accessToSelectedCompanyStatusCodeId == activeSharingStatusCodeId ? "You can already see " + selectedCompanyName + "'s Inventory. Why not share yours with them?" : "You have already invited " + selectedCompanyName + " to share their Inventory with you.";
                divToolTip.Attributes.Add("title", toolTip);
                divAccessToSelectedCompany.Visible = true;
                divAccessToMyCompany.Visible = false;
                divAccessToSelectedCompany.Style["top"] = "0px";
            }
            else if (accessToMyCompanyStatusCodeId > 0 && accessToSelectedCompanyStatusCodeId > 0)
            {
                imgAccessToMyCompany.ImageUrl = accessToMyCompanyStatusCodeId == activeSharingStatusCodeId ? "~/Common/Images/shareleftactive.png" : "~/Common/Images/shareleftpending.png";
                imgAccessToSelectedCompany.ImageUrl = accessToSelectedCompanyStatusCodeId == activeSharingStatusCodeId ? "~/Common/Images/sharerightactive.png" : "~/Common/Images/sharerightpending.png";
                if (accessToMyCompanyStatusCodeId == activeSharingStatusCodeId && accessToSelectedCompanyStatusCodeId == pendingSharingStatusCodeId)
                {
                    toolTip = selectedCompanyName + " can already see your Inventory and you have invited them to share their Inventory with you.";
                }
                else if (accessToMyCompanyStatusCodeId == pendingSharingStatusCodeId && accessToSelectedCompanyStatusCodeId == activeSharingStatusCodeId)
                {
                    toolTip = "You can already see " + selectedCompanyName + "'s Inventory and they have invited you to share your Inventory with them.";
                }
                else if (accessToMyCompanyStatusCodeId == activeSharingStatusCodeId && accessToSelectedCompanyStatusCodeId == activeSharingStatusCodeId)
                {
                    toolTip = "Your Company and " + selectedCompanyName + " can already see each other's Inventories.";
                }
                else
                {
                    toolTip = "Your Company and " + selectedCompanyName + " already have pending invitations to see each other's Inventories.";
                }
                divToolTip.Attributes.Add("title", toolTip);
                rbtnCompany.Enabled = false;
                divAccessToSelectedCompany.Visible = true;
                divAccessToMyCompany.Visible = true;
                divAccessToMyCompany.Style["top"] = "0px";
                divAccessToSelectedCompany.Style["top"] = "-8px";
            }
            else
            {
                divAccessToSelectedCompany.Visible = false;
                divAccessToMyCompany.Visible = false;
            }

            itemThumbDisplay.DocumentMediaId = thumbnailMediaId;
            lblCompanyName.Text = Support.TruncateString(selectedCompanyName, 15);

            lblCompanyName.ToolTip = GetBL<CompanyBL>().GetCompanyAddress(company);
            ltrCompanyCity.Text = (company.City != null) ? Support.TruncateString(company.City, 15) : string.Empty;
            ltrCompanyCountry.Text = (company.Country != null) ? Support.TruncateString(company.Country.CountryName, 15) : string.Empty;
        }

        /// <summary>
        /// Handles the PreRender event of the pagerCompanyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pagerCompanyList_PreRender(object sender, EventArgs e)
        {
            RadDataPagerFieldItem radDataPagerFieldItem = null;
            foreach (Control c in pagerCompanyList.Controls)
            {
                RadDataPagerFieldItem a = c as RadDataPagerFieldItem;
                if (a != null && a.Field is RadDataPagerTemplatePageField)
                {
                    radDataPagerFieldItem = c as RadDataPagerFieldItem;
                }
            }

            if (radDataPagerFieldItem != null)
            {
                Label lblCompanyText = radDataPagerFieldItem.FindControl("lblCompanyText") as Label;
                Label lblPagesText = radDataPagerFieldItem.FindControl("lblPagesText") as Label;
                lblCompanyText.Text = pagerCompanyList.TotalRowCount == 1 ? "Company in" : "Companies in";
                lblPagesText.Text = pagerCompanyList.PageCount == 1 ? "page" : "pages";
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvManageSharings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvManageSharings_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                CompanySearchData companySearchData = (CompanySearchData)dataItem.DataItem;
                string companyidValue = companySearchData.Company.CompanyId.ToString(CultureInfo.InvariantCulture);
                bool isCompanySuspended = GetBL<CompanyBL>().IsCompanySuspended(companySearchData.Company.CompanyId, true);

                HtmlGenericControl spanProjectSuspended = dataItem.FindControl("spanProjectSuspended") as HtmlGenericControl;
                if (spanProjectSuspended != null)
                {
                    spanProjectSuspended.Visible = isCompanySuspended;
                }

                HtmlAnchor anchorForCompany = dataItem.FindControl("anchorForCompany") as HtmlAnchor;
                if (anchorForCompany != null)
                {
                    anchorForCompany.Attributes.Add("name", companyidValue);
                }

                HiddenField hdnCompanyId = dataItem.FindControl("hdnCompanyId") as HiddenField;
                if (hdnCompanyId != null)
                {
                    hdnCompanyId.Value = companyidValue;
                }

                Label lblCompanyName = dataItem.FindControl("lblCompanyName") as Label;
                if (lblCompanyName != null)
                {
                    lblCompanyName.Text = Support.TruncateString(companySearchData.Company.CompanyName, 35);
                    lblCompanyName.ToolTip = GetBL<CompanyBL>().GetCompanyAddress(companySearchData.Company);
                }

                MultiView multiViewCanTheySeeOurs = dataItem.FindControl("multiViewCanTheySeeOurs") as MultiView;
                if (multiViewCanTheySeeOurs != null)
                {
                    // always pass false. because current company is always active. (if current company suspended links will be disabled.)
                    SetMutiViewActiveView(multiViewCanTheySeeOurs, companySearchData.AccessToMyCompanyStatusCodeId, false);
                }

                MultiView multiViewCanWeSeeTheirs = dataItem.FindControl("multiViewCanWeSeeTheirs") as MultiView;
                if (multiViewCanWeSeeTheirs != null)
                {
                    SetMutiViewActiveView(multiViewCanWeSeeTheirs, companySearchData.AccessToSelectedCompanyStatusCodeId, isCompanySuspended);
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvManageSharings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvManageSharings_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!this.StopProcessing)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                HiddenField hdnCompanyId = dataItem.FindControl("hdnCompanyId") as HiddenField;
                if (hdnCompanyId != null)
                {
                    CompanySearchData companySearchData = (CompanySearchData)dataItem.DataItem;
                    btnPopupConfirmAccept.CommandName = e.CommandName;
                    btnPopupConfirmAccept.CommandArgument = hdnCompanyId.Value;
                    if (e.CommandName.Equals("Remove_CanTheySeeOurs") || e.CommandName.Equals("Remove_CanWeSeeTheirs"))
                    {
                        ltrConfirmationMsg.Visible = true;
                        ltrConfirmationMsg.Text = "This will remove sharing access to the Inventory!";
                        divPopupConfirm.Style["width"] = "400px;";
                    }
                    else
                    {
                        ltrConfirmationMsg.Visible = false;
                        divPopupConfirm.Style["width"] = "200px;";
                    }

                    popupConfirm.ShowPopup(1001);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPopupConfirmAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPopupConfirmAccept_Click(object sender, EventArgs e)
        {
            if (!this.StopProcessing)
            {
                if (GetBL<CompanyBL>().IsCompanySuspended(this.CompanyId, true))
                {
                    lblErrorMsg.Text = "Company has been suspended.";
                    popupConfirm.HidePopup();
                    popupError.ShowPopup(1001);
                    return;
                }

                InitializeUserData();
                if (this.HasInventoryAdminRights)
                {
                    Button btn = sender as Button;
                    int companyIdToView = 0;
                    if (btn != null && int.TryParse(btn.CommandArgument, out companyIdToView))
                    {
                        int pendingShareCodeId = Utils.GetCodeByValue("CompanyInventorySharingStatus", "PENDING").CodeId;
                        int activeShareCodeId = Utils.GetCodeByValue("CompanyInventorySharingStatus", "ACTIVE").CodeId;
                        int declinedShareCodeId = Utils.GetCodeByValue("CompanyInventorySharingStatus", "DECLINED").CodeId;
                        Data.CompanyInventorySharingDetail inventorySharingDetail = null;
                        switch (btn.CommandName)
                        {
                            case "Remove_CanTheySeeOurs":
                                inventorySharingDetail = GetBL<InventoryBL>().GetCompanyInventorySharingDetail(companyIdToView, this.CompanyId);
                                if (inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == activeShareCodeId)
                                {
                                    inventorySharingDetail.IsActive = false;
                                    inventorySharingDetail.LastUpdatedDate = Utils.Now;
                                    inventorySharingDetail.LastUpdatedByUserId = UserID;
                                    DataContext.RemoveInventorySharing(CompanyId, companyIdToView);
                                    SendEmailStopSharing(CompanyId, companyIdToView);
                                }
                                break;

                            case "Approve_CanTheySeeOurs":
                                inventorySharingDetail = GetBL<InventoryBL>().GetCompanyInventorySharingDetail(companyIdToView, this.CompanyId);
                                if (inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == pendingShareCodeId)
                                {
                                    inventorySharingDetail.CompanySharingStatusCodeId = activeShareCodeId;
                                    inventorySharingDetail.LastUpdatedDate = Utils.Now;
                                    inventorySharingDetail.LastUpdatedByUserId = UserID;
                                }
                                else
                                {
                                    string requestStatus = inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == activeShareCodeId ? "accepted" : "declined";
                                    lblErrorMsg.Text = string.Format("Already {0} the pending request.", requestStatus);
                                    popupError.ShowPopup(1001);
                                }
                                break;

                            case "Deny_CanTheySeeOurs":
                                inventorySharingDetail = GetBL<InventoryBL>().GetCompanyInventorySharingDetail(companyIdToView, this.CompanyId);
                                if (inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == pendingShareCodeId)
                                {
                                    inventorySharingDetail.CompanySharingStatusCodeId = declinedShareCodeId;
                                    inventorySharingDetail.IsActive = false;
                                    inventorySharingDetail.LastUpdatedDate = Utils.Now;
                                    inventorySharingDetail.LastUpdatedByUserId = UserID;
                                }
                                else
                                {
                                    string requestStatus = inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == activeShareCodeId ? "accepted" : "declined";
                                    lblErrorMsg.Text = string.Format("Already {0} the pending request.", requestStatus);
                                    popupError.ShowPopup(1001);
                                }
                                break;

                            case "Remove_CanWeSeeTheirs":
                                inventorySharingDetail = GetBL<InventoryBL>().GetCompanyInventorySharingDetail(this.CompanyId, companyIdToView);
                                if (inventorySharingDetail != null && inventorySharingDetail.CompanySharingStatusCodeId == activeShareCodeId)
                                {
                                    inventorySharingDetail.IsActive = false;
                                    inventorySharingDetail.LastUpdatedDate = Utils.Now;
                                    inventorySharingDetail.LastUpdatedByUserId = UserID;
                                    DataContext.RemoveInventorySharing(companyIdToView, CompanyId);
                                }
                                break;
                        }

                        GetBL<InventoryBL>().SaveChanges();
                        popupConfirm.HidePopup();
                        InitializeAdminUI();
                        RegisterInitializeScript();

                        lvCompanyList.DataSource = LoadSearch();
                        lvCompanyList.Rebind();
                        upnlInventorySharing.Update();
                    }
                }
                else
                {
                    projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveShowInSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveShowInSearchResults_Click(object sender, EventArgs e)
        {
            InitializeUserData();
            if (this.HasInventoryAdminRights)
            {
                Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);
                if (company != null && !GetBL<CompanyBL>().IsCompanySuspended(this.CompanyId, true))
                {
                    company.IsCompanyVisibleForSearchInInventory = rbtnYes.Checked;
                    GetBL<CompanyBL>().SaveChanges();
                    IsPageDirty = false;
                    ShowNotification("showInSearchResultsSavedNotice");
                }
            }
            else
            {
                IsPageDirty = false;
                projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMoveLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMoveLocation_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                MoveLocation();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMove_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                InitializeUserData();
                lblMoveLocationError.Text = string.Empty;

                int selectedLocationId = 0;
                if (int.TryParse(tvLocation.SelectedValue, out selectedLocationId))
                {
                    if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, selectedLocationId))
                    {
                        List<int> excludeList = new List<int>();
                        excludeList.Add(selectedLocationId);
                        sbMoveInventoryLocations.LoadData(excludeList);
                        popupMoveLocation.ShowPopup();
                    }
                    else
                    {
                        projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddLocation_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                InitializeUserData();
                //Need to assign since it is set from client side. Ineach postback the value gets cleared
                spanAddLocation.InnerText = hdnLocationBreadCrumb.Value;

                if (txtLocationName.Text.Trim().Length > 0)
                {
                    int selectedLocationId = 0;
                    Data.Location location = null;
                    //Root node does not have a LocationId
                    if (htnSelectedLocationId.Value.Length > 0)
                    {
                        selectedLocationId = int.Parse(htnSelectedLocationId.Value);
                        location = GetBL<LocationBL>().GetLocation(selectedLocationId);
                    }

                    if ((location != null && Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, selectedLocationId)) ||
                                (location == null && Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID)))
                    {
                        spanAddLocation.Attributes.Add("Title", hdnLocationBreadCrumb.Value);
                        if (location != null || selectedLocationId == 0)
                        {
                            //Check if the Location is already available in the same level
                            if (!GetBL<LocationBL>().HasDuplcateLocations(CompanyId, selectedLocationId == 0 ? (int?)null : selectedLocationId, txtLocationName.Text.Trim()))
                            {
                                //Commit the changes
                                GetBL<LocationBL>().SaveLocation(selectedLocationId == 0 ? (int?)null : selectedLocationId, txtLocationName.Text.Trim(), UserID, CompanyId);
                                popupAddLocation.HidePopup();
                                LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, tvLocation.SelectedValue);
                                return;//After registering the script, it should get exit.
                            }
                            else
                            {
                                //Display the duplicate error message
                                lblErrorAddLocation.InnerText = "You already have a location with this name at this level in the Inventory. Please choose a different name.";
                            }
                        }
                        else
                        {
                            // Location has been deleted
                            lblErrorAddLocation.InnerText = "Location has already been deleted.";
                            htnSelectedLocationId.Value = string.Empty;
                        }
                    }
                    else
                    {
                        projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                    }
                }

                LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, tvLocation.SelectedValue);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLocationNameEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLocationNameEdit_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                InitializeUserData();

                if (htnSelectedLocationId.Value.Trim().Length > 0)
                {
                    int selectedLocationId = int.Parse(htnSelectedLocationId.Value);
                    if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, selectedLocationId))
                    {
                        Data.Location location = GetBL<LocationBL>().GetLocation(selectedLocationId);
                        if (location != null)
                        {
                            if (!GetBL<LocationBL>().HasDuplcateLocations(CompanyId, location.ParentLocationId, txtLocationNameEdit.Text.Trim(), selectedLocationId))
                            {
                                //Commit the changes
                                GetBL<LocationBL>().SaveLocation(selectedLocationId, txtLocationNameEdit.Text.Trim(), UserID, CompanyId, false);
                                popupEditLocation.HidePopup();
                                LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, tvLocation.SelectedValue);
                                return;//After registering the script, it should get exit.
                            }
                            else
                            {
                                //Display the duplicate error message
                                lblErrorEditLocation.InnerText = "You already have a location with this name at this level in the Inventory. Please choose a different name";
                            }
                        }
                        else
                        {
                            // Location has been deleted.
                            lblErrorEditLocation.InnerText = "Location has already been deleted.";
                            htnSelectedLocationId.Value = string.Empty;
                        }
                    }
                    else
                    {
                        projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                    }
                }

                LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, tvLocation.SelectedValue);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                DeleteLocation(false);//Decides which UI to to display when deleting the location
                popupDeleteLocation.ShowPopup();//Display the generic Delete confirmation popup
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            //Just delete the location
            DeleteLocation(true);
        }

        /// <summary>
        /// Handles the InvitationSent event of the sbSearchUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSearchUsers_InvitationSent(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadInventoryTeam();
                gvInventoryTeam.DataBind();
                upnlInventoryTeam.Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdatePermission control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatePermission_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                UpdateLocationRoles();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmDowngradeExistingLocationManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmDowngradeExistingLocationManager_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!Page.IsValid)
                {
                    return;
                }

                UpdateLocationRoles(true);
                popupLocationManagerAlreadyExist.HidePopup();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvInventoryTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvInventoryTeam_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                InventoryTeamInfo user = (InventoryTeamInfo)dataItem.DataItem;
                bool isUserInventoryAdmin = user.CompanyUserTypeCodeId == InventoryAdminCodeId;
                int invitationId = user.InvitationId;
                int userID = user.UserId;
                bool isCurrentUserCompanyAdmin = GetBL<CompanyBL>().IsCompanyAdministrator(this.CompanyId, this.UserID);

                ImageButton Deletebtn = dataItem["DeleteColumn"].Controls[0] as ImageButton;

                if (Deletebtn != null && (IsCompanyReadOnly || isUserInventoryAdmin || (userID == this.UserID && !isCurrentUserCompanyAdmin)))
                {
                    Deletebtn.Visible = false;
                }

                #region Name truncation

                HyperLink lnkUserName = (HyperLink)dataItem.FindControl("lnkUserName");
                Label lblUserName = (Label)dataItem.FindControl("lblUserName");

                if (lnkUserName != null && lblUserName != null)
                {
                    bool canViewContact = userID != this.UserID && invitationId == 0;
                    lnkUserName.Visible = canViewContact;
                    lblUserName.Visible = !canViewContact;

                    lnkUserName.NavigateUrl = string.Format("~/Personal/UserDetails.aspx?userId={0}", userID);
                    lnkUserName.Text = lblUserName.Text = Support.TruncateString(user.Name, 45);
                    if (user.Name.Length > 45)
                    {
                        lnkUserName.ToolTip = lblUserName.ToolTip = user.Name;
                    }
                }

                #endregion Name truncation

                #region Position truncation

                if (user.Position != null)
                {
                    dataItem["Position"].Text = Support.TruncateString(user.Position, 25);
                    if (user.Position.Length > 25)
                    {
                        dataItem["Position"].ToolTip = user.Position;
                    }
                }

                #endregion Position truncation

                ImageButton ibtnEditPermision = (ImageButton)dataItem.FindControl("ibtnEditPermision");
                if (ibtnEditPermision != null)
                {
                    ibtnEditPermision.Visible = !IsCompanyReadOnly && HasLocationManagerRightsAnyLocation
                        && !isUserInventoryAdmin && invitationId == 0 && (userID != this.UserID || isCurrentUserCompanyAdmin);
                }

                LinkButton lbtnPermission = (LinkButton)dataItem.FindControl("lbtnPermission");
                Label lblPermission = (Label)dataItem.FindControl("lblPermission");
                if (lbtnPermission != null && lblPermission != null)
                {
                    lbtnPermission.Text = lblPermission.Text = user.UserPermission;
                    lbtnPermission.Visible = !IsCompanyReadOnly && !isUserInventoryAdmin;
                    lblPermission.Visible = !lbtnPermission.Visible;
                }

                Image imgCompAdmin = (Image)dataItem.FindControl("imgCompAdmin");
                if (imgCompAdmin != null)
                {
                    bool isCompanyAdmin = this.GetBL<CompanyBL>().IsCompanyAdministrator(CompanyId, user.UserId);
                    if (isCompanyAdmin)
                    {
                        imgCompAdmin.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvInventoryTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvInventoryTeam_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                InitializeUserData();

                GridDataItem dataItem = (GridDataItem)e.Item;
                int companyUserId;
                string[] argArray = e.CommandArgument.ToString().Split('|');
                if (int.TryParse(argArray[0], out companyUserId))
                {
                    Data.User selectedUser = null;
                    InitializeChangePermissionPopup(companyUserId);
                    Dictionary<int, int> locationRoles = new Dictionary<int, int>();
                    bool isInventoryAdmin = false;

                    if (companyUserId > 0)
                    {
                        Data.CompanyUser companyUser = GetBL<InventoryBL>().GetCompanyUser(companyUserId);
                        selectedUser = companyUser.User;

                        //Check whether this user is already in contacts before saving the new contact
                        List<Data.CompanyUserRole> userRoles = DataContext.CompanyUserRoles.Where(ucr =>
                            ucr.CompanyUserId == companyUserId &&
                            ucr.IsActive && (ucr.LocationId.HasValue || ucr.CompanyUserTypeCodeId == InventoryAdminCodeId)).ToList<Data.CompanyUserRole>();
                        isInventoryAdmin = userRoles.Where(ur => ur.CompanyUserTypeCodeId == InventoryAdminCodeId).FirstOrDefault() != null;

                        foreach (Data.CompanyUserRole companyUserRole in userRoles)
                        {
                            if (companyUserRole.LocationId.HasValue && !locationRoles.Keys.Contains(companyUserRole.LocationId.Value))
                            {
                                locationRoles.Add(companyUserRole.LocationId.Value, companyUserRole.CompanyUserTypeCodeId);
                            }
                        }
                    }
                    else
                    {
                        int invitationId;
                        if (argArray.Length > 1 && int.TryParse(argArray[1], out invitationId) && invitationId > 0)
                        {
                            Data.Invitation invitation = GetBL<UtilityBL>().GetInvitation(invitationId);
                            selectedUser = invitation.ToUserId.HasValue ? GetBL<PersonalBL>().GetUser(invitation.ToUserId.Value) : null;

                            List<Data.InvitationUserRole> userRoles = DataContext.InvitationUserRoles.Where(iur =>
                                iur.InvitationId == invitationId && iur.IsActive && iur.LocationId.HasValue).ToList();

                            foreach (Data.InvitationUserRole invitationUserRole in userRoles)
                            {
                                if (invitationUserRole.LocationId.HasValue && !locationRoles.Keys.Contains(invitationUserRole.LocationId.Value))
                                {
                                    locationRoles.Add(invitationUserRole.LocationId.Value, invitationUserRole.UserTypeCodeId);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (selectedUser != null)
                    {
                        popupChangePremission.Title = string.Format("{0}'s Inventory Roles", Utils.GetFullName(selectedUser));
                    }

                    if (e.CommandName == "EditPermission")
                    {
                        if (this.HasLocationManagerRightsAnyLocation)
                        {
                            divInventoryAdminChangePremission.Visible = Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID);
                            divInventoryAdminChangePremissionEmail.Visible = true;
                            btnUpdatePermission.Visible = true;
                            popupChangePremission.ShowPopup();

                            if (isInventoryAdmin)
                            {
                                rbtnInventoryAdmin.Checked = true;
                            }
                            else
                            {
                                sbInventoryLocationRoles.LoadData(locationRoles);
                            }
                        }
                        else
                        {
                            projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                        }
                    }
                    else if (e.CommandName == "ViewPermission")
                    {
                        divInventoryAdminChangePremission.Visible = false;
                        divInventoryAdminChangePremissionEmail.Visible = false;
                        btnUpdatePermission.Visible = false;
                        popupChangePremission.ShowPopup();
                        sbInventoryLocationRoles.LoadData(locationRoles, true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmMoveLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmMoveLocation_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                MoveLocation(true);
            }
        }

        /// <summary>
        /// Handles the DeleteCommand event of the gvInventoryTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvInventoryTeam_DeleteCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                InitializeUserData();
                if (this.HasInventoryAdminRights)
                {
                    GridDataItem item = (GridDataItem)e.Item;
                    int companyUserId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["CompanyUserId"];
                    int invitationId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["InvitationId"];
                    if (companyUserId == 0) //This is an invitation
                    {
                        var invitation = DataContext.Invitations.FirstOrDefault(i => i.InvitationId == invitationId);
                        if (invitation != null)
                        {
                            DataContext.DeleteInvitation(invitation.InvitationId);
                        }
                    }
                    else
                    {
                        DataContext.DeleteInventoryUser(companyUserId);
                    }

                    DataContext.SaveChanges();
                    sbSearchUsers.HideNotifications();
                }
                else
                {
                    projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvInventoryTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvInventoryTeam_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            LoadInventoryTeam();
        }

        /// <summary>
        /// Handles the NodeClick event of the tvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadTreeNodeEventArgs"/> instance containing the event data.</param>
        protected void tvLocation_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            LoadLocationUserRoles(e.Node);
        }

        #endregion Event Handlers

        #region PrivateMethods

        /// <summary>
        /// Display the relavent error popup and Deletes the selected location.
        /// </summary>
        /// <param name="shouldDeleteLocation">Decides whether to delete the Location to display the confirmation popup.</param>
        private void DeleteLocation(bool shouldDeleteLocation)
        {
            if (!StopProcessing)
            {
                InitializeUserData();

                int selectedLocationId = 0;
                Data.Location location = null;
                //Root node does not have a LocationId
                if (htnSelectedLocationId.Value.Length > 0)
                {
                    selectedLocationId = int.Parse(htnSelectedLocationId.Value);
                    location = GetBL<LocationBL>().GetLocation(selectedLocationId);
                    if (location != null)
                    {
                        if ((location.ParentLocationId.HasValue && Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, selectedLocationId)) ||
                                (!location.ParentLocationId.HasValue && Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID)))
                        {
                            bool hasItems = GetBL<LocationBL>().GetAllItemsInLocation(selectedLocationId, CompanyId, this.UserID).Count() > 0;
                            if (hasItems)
                            {
                                //Display can not perform delete.
                                lblDeletLocationeMsg.Text = "Hold up! You still have Items assigned to this location in your Inventory. You will need to move them to another location first.";
                                string url = ResolveClientUrl(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}&Inventory={1}&IsManagerMode={2}",
                                    this.CompanyId, Server.UrlEncode(Support.GetInventoryNavigateURLParams(string.Empty, string.Empty, 0, htnSelectedLocationId.Value, string.Empty)), true));
                                lnkMoveItems.HRef = url;
                                lnkMoveItems.Target = "_blank";
                            }
                            else
                            {
                                if (shouldDeleteLocation)
                                {
                                    GetBL<LocationBL>().DeleteLocaton(location, UserID);
                                    //hide the popup and reload the tree
                                    popupDeleteLocation.HidePopup();
                                }
                                else
                                {
                                    //Display "Can perform delete" message
                                    lblDeletLocationeMsg.Text = "Are you sure you want to delete this location?";
                                }
                            }
                            lnkMoveItems.Visible = hasItems;
                            btnConfirmDelete.Visible = !hasItems;
                            if (!hasItems)
                                btnConfirmDelete.Focus();
                            else
                                lnkMoveItems.Focus();
                        }
                        else
                        {
                            projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                        }
                    }
                    else
                    {
                        lblDeletLocationeMsg.Text = "Location is already being deleted.";
                        lnkMoveItems.Visible = false;
                        btnConfirmDelete.Visible = false;
                    }

                    if (shouldDeleteLocation)
                    {
                        string locationToSelect = location != null && location.ParentLocationId.HasValue ?
                                location.ParentLocationId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, locationToSelect);
                    }
                }
            }
        }

        /// <summary>
        /// Registers the initialize script.
        /// </summary>
        private void RegisterInitializeScript()
        {
            string script = string.Format("InitializeRadioButtons();InitializeCheckBoxes('{0}');DisableOptions('{1}');addEventFired = false;", IsCompanyReadOnly, IsCompanyReadOnly);
            ScriptManager.RegisterStartupScript(this, GetType(), "RegisterInitializeScript", script, true);
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="company">The company.</param>
        private void LoadBreadCrumbs(Data.Company company, bool isCompanyAdmin)
        {
            if (this.CompanyId > 0)
            {
                BreadCrumbs bc = GetBreadCrumbsControl();
                bc.ClearLinks();
                bc.AddLink(company.CompanyName, isCompanyAdmin ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", this.CompanyId) : null);
                bc.AddLink("Company Inventory", string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
                bc.AddLink("Manage Inventory", null);
                bc.LoadControl();
                bc.UpdateBreadCrumb();
            }
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        private void InitializeUI()
        {
            IsCompanyReadOnly = GetBL<CompanyBL>().IsCompanySuspended(CompanyId, true);
            string companyName = GetBL<CompanyBL>().GetCompany(CompanyId).CompanyName;
            rdViewMyCompany.Text = String.Format("Give their staff access to {0} Inventory", companyName);
            rdViewOtherCompany.Text = String.Format("Request access for {0} staff to browse their Inventory", companyName);
            btnMove.Enabled = btnEdit.Enabled = btnAddLocation.Enabled = btnAddSubLoation.Enabled = btnShare.Enabled = rdBoth.Enabled = rdViewMyCompany.Enabled = rdViewOtherCompany.Enabled = !IsCompanyReadOnly;
            btnEdit.ToolTip = btnMove.ToolTip = btnAddLocation.ToolTip = btnAddSubLoation.ToolTip = btnShare.ToolTip = IsCompanyReadOnly ? "Company is suspended" : string.Empty;
        }

        /// <summary>
        /// Initializes the user data.
        /// </summary>
        private void InitializeUserData()
        {
            bool isInventoryAdmin = Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID);
            bool isLocationManager = isInventoryAdmin || GetBL<InventoryBL>().IsCompanyLocationManagerAnyLocation(this.CompanyId, this.UserID);

            this.HasInventoryAdminRights = isInventoryAdmin;
            this.HasLocationManagerRightsAnyLocation = isLocationManager;

            inventorySharingTabs.FindTabByValue("Search").Visible = isInventoryAdmin;
            inventorySharingTabs.FindTabByValue("Administration").Visible = isInventoryAdmin;
            inventorySharingTabs.FindTabByValue("Locations").Visible = isLocationManager;
            sbSearchUsers.Visible = !IsCompanyReadOnly && (isInventoryAdmin || isLocationManager);
            upnlTabs.Update();
        }

        /// <summary>
        /// Loads the search.
        /// </summary>
        /// <returns></returns>
        private List<CompanySearchData> LoadSearch()
        {
            FindByName = cboSearchCompanyName.Text.Trim().ToLower();
            FindByLocation = txtSearchCompanyLocation.Text.Trim().ToLower();
            return this.GetBL<InventoryBL>().GetCompaniesByNameAndLocation(this.CompanyId, FindByName, FindByLocation);
        }

        /// <summary>
        /// Sets the muti view active view.
        /// </summary>
        /// <param name="multiViewControl">The multi view control.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="isCompanySuspended">if set to <c>true</c> [is company suspended].</param>
        private void SetMutiViewActiveView(MultiView multiViewControl, int statusId, bool isCompanySuspended)
        {
            int pendingShare = Utils.GetCodeByValue("CompanyInventorySharingStatus", "PENDING").CodeId;
            int acceptShare = Utils.GetCodeByValue("CompanyInventorySharingStatus", "ACTIVE").CodeId;
            if (statusId == pendingShare)
            {
                multiViewControl.ActiveViewIndex = (int)Views.Pending;
            }
            else if (statusId == acceptShare)
            {
                multiViewControl.ActiveViewIndex = (int)Views.Yes;
            }
            else
            {
                multiViewControl.ActiveViewIndex = (int)Views.No;
            }

            // disable action links and show lables
            if (IsCompanyReadOnly)
            {
                foreach (Control control in multiViewControl.GetActiveView().Controls)
                {
                    if (control is LinkButton)
                    {
                        LinkButton lnk = control as LinkButton;
                        if (lnk.CssClass == "ActionLink")
                        {
                            lnk.Visible = false;
                        }
                    }
                    else if (control is Label)
                    {
                        Label lbl = control as Label;
                        if (lbl.CssClass == "ActionLink")
                        {
                            lbl.Visible = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the admin UI.
        /// </summary>
        private void InitializeAdminUI()
        {
            Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);
            if (company != null)
            {
                rbtnNo.Enabled = !IsCompanyReadOnly;
                rbtnYes.Enabled = !IsCompanyReadOnly;
                btnSaveShowInSearchResults.Enabled = !IsCompanyReadOnly;
                btnSaveShowInSearchResults.ToolTip = IsCompanyReadOnly ? "Company is suspended" : string.Empty;

                rbtnYes.Checked = company.IsCompanyVisibleForSearchInInventory;
                rbtnNo.Checked = !company.IsCompanyVisibleForSearchInInventory;

                gvManageSharings.DataSource = GetBL<InventoryBL>().GetSharedCompanyDetails(this.CompanyId);
                gvManageSharings.DataBind();
                upnlAdminTab.Update();

                //Initialize Location Tree view
                LoadLocations(company.CompanyName, string.Empty);
            }
        }

        /// <summary>
        /// Loads the locations.
        /// </summary>
        /// <param name="rootName">Name of the root.</param>
        /// <param name="nodeToSelect">The node to select.</param>
        private void LoadLocations(string rootName, string nodeToSelect)
        {
            var locations = GetBL<LocationBL>().GetLocations(CompanyId, UserID).AsEnumerable();
            //var locations = GetBL<LocationBL>().GetLocations(CompanyId, UserID).AsEnumerable().Where(l => Utils.HasLocationManagerPermission(CompanyId, UserID, l.LocationId));
            tvLocation.DataSource = locations;
            tvLocation.DataBind();

            RadTreeNode node = new RadTreeNode(rootName, string.Empty);
            node.Expanded = true;
            //node.Enabled = Utils.IsCompanyInventoryAdmin(CompanyId, UserID);
            node.Nodes.AddRange(tvLocation.Nodes.Cast<RadTreeNode>());
            tvLocation.Nodes.Add(node);
            if (node != null) // && node.Enabled)
            {
                node.Selected = true;
            }

            foreach (RadTreeNode tier2Node in node.Nodes)
            {
                tier2Node.CssClass = "Tier2";
            }

            RadTreeNode selectednode = tvLocation.FindNodeByValue(nodeToSelect);
            if (selectednode != null)// && selectednode.Enabled)
            {
                selectednode.Selected = true;
                selectednode.Expanded = true;
                selectednode.ExpandParentNodes();
                LoadLocationUserRoles(selectednode);
            }
        }

        /// <summary>
        /// Sends the email stop sharing.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="viewingCompanyId">The viewing company identifier.</param>
        private void SendEmailStopSharing(int companyId, int viewingCompanyId)
        {
            string companyName = this.GetBL<CompanyBL>().GetCompany(companyId).CompanyName;
            string viewingCompanyName = this.GetBL<CompanyBL>().GetCompany(viewingCompanyId).CompanyName;
            int emailTempleTypeCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "STOPINVENTORYSHARING");
            string feedbackAndTechSupport = Utils.GetSystemValue("FeedBackAndTechSupportURL");
            //Get the Inventory Admin
            Data.User inventoryAdmin = this.GetBL<InventoryBL>().GetInventoryAdmin(viewingCompanyId);
            EmailSender.StopShareInventory(inventoryAdmin.Email1, inventoryAdmin.FirstName, companyName, viewingCompanyName, feedbackAndTechSupport, emailTempleTypeCodeId);
        }

        /// <summary>
        /// Updates the location roles.
        /// </summary>
        /// <param name="skipExistingLocationManagerValidation">if set to <c>true</c> [skip existing location manager validation].</param>
        private void UpdateLocationRoles(bool skipExistingLocationManagerValidation = false)
        {
            InitializeUserData();
            if (this.HasLocationManagerRightsAnyLocation)
            {
                int companyUserID;
                if (int.TryParse(hdnCompanyUserId.Value, out companyUserID))
                {
                    Data.CompanyUser inventoryUser = GetBL<InventoryBL>().GetCompanyUser(companyUserID);
                    List<Data.CompanyUserRole> inventoryRoles = GetBL<InventoryBL>().GetInventoryRoles(this.CompanyId, companyUserID);

                    Data.User user = this.GetBL<PersonalBL>().GetUser(inventoryUser.UserId);
                    string companyName = inventoryUser.Company.CompanyName;
                    string fromUserFullName = (Support.UserFirstName + " " + Support.UserLastName).Trim();

                    Data.CompanyUserRole highestRole = GetBL<InventoryBL>().GetHighestInventoryRole(this.CompanyId, UserID);
                    bool isStaffMember = Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID) || (highestRole != null && highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").SortOrder);

                    string locationManagerHtmlUpgrade = string.Empty;
                    string staffHtmlUpgrade = string.Empty;
                    string observerHtmlUpgrade = string.Empty;

                    string staffHtmlDowngrade = string.Empty;
                    string observerHtmlDowngrade = string.Empty;
                    string noAccessHtmlDowngrade = string.Empty;

                    string noChange = string.Empty;

                    if (rbtnInventoryAdmin.Checked)
                    {
                        Data.CompanyUserRole allInventoryAdminRole = GetBL<InventoryBL>().GetAllCompanyInventoryAdminRole(this.CompanyId);
                        if (allInventoryAdminRole != null)
                        {
                            Data.User adminUser = allInventoryAdminRole.CompanyUser.User;
                            StringBuilder sbInviteInventoryStaffEmail = new StringBuilder();
                            string staffHtml = string.Empty;

                            allInventoryAdminRole.IsActive = false;

                            var locations = GetBL<LocationBL>().GetTier2Locations(this.CompanyId, adminUser.UserId, true);
                            foreach (var location in locations)
                            {
                                allInventoryAdminRole.CompanyUser.CompanyUserRoles.Add(
                                    new Data.CompanyUserRole
                                                {
                                                    CreatedByUserId = UserID,
                                                    CreatedDate = Now,
                                                    IsActive = true,
                                                    LastUpdatedByUserId = UserID,
                                                    LastUpdatedDate = Now,
                                                    CompanyUserTypeCodeId = InventoryStaffCodeId,
                                                    LocationId = location.Location.LocationId
                                                }
                                    );

                                sbInviteInventoryStaffEmail.Append(string.Format("<li>{0}</li>", location.Location.LocationName));
                            }

                            EmailSender.DowngradeInventoryAdmin(adminUser.Email1, adminUser.FirstName, fromUserFullName, Support.UserPrimaryEmail, companyName, sbInviteInventoryStaffEmail.ToString());
                        }

                        if (Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID))
                        {
                            foreach (Data.CompanyUserRole inventoryRole in inventoryRoles)
                            {
                                inventoryRole.IsActive = false;
                            }

                            inventoryUser.CompanyUserRoles.Add(
                                                new Data.CompanyUserRole
                                                {
                                                    CreatedByUserId = UserID,
                                                    CreatedDate = Now,
                                                    IsActive = true,
                                                    LastUpdatedByUserId = UserID,
                                                    LastUpdatedDate = Now,
                                                    CompanyUserTypeCodeId = InventoryAdminCodeId,
                                                    LocationId = null
                                                }
                                            );

                            EmailSender.UpgradeToInventoryAdmin(user.Email1, user.FirstName, fromUserFullName, Support.UserPrimaryEmail,
                                 companyName);
                        }
                        else
                        {
                            projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !isStaffMember);
                            return;
                        }
                    }
                    else
                    {
                        StringBuilder sbInventoryStaffUpgradeEmail = new StringBuilder();
                        StringBuilder sbLocationManagerUpgradeEmail = new StringBuilder();
                        StringBuilder sbInventoryObserverUpgradeEmail = new StringBuilder();

                        StringBuilder sbInventoryStaffDowngradeEmail = new StringBuilder();
                        StringBuilder sbInventoryObserverDowngradeEmail = new StringBuilder();
                        StringBuilder sbNoAccessDowngradeEmail = new StringBuilder();

                        StringBuilder sbNoChangeEmail = new StringBuilder();

                        // If all location roles been removed
                        Dictionary<int, int> locationRoles = sbInventoryLocationRoles.LocationPermissions;
                        if (locationRoles.Count == 0)
                        {
                            bool isInventoryAdmin = highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").SortOrder;
                            if (isInventoryAdmin)
                            {
                                this.ShowErrorPopup(ErrorCodes.InventoryLocationDeleted);
                            }
                            else
                            {
                                projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !isStaffMember);
                            }

                            return;
                        }

                        // Check for existing location managers
                        var locationManagerRoles = locationRoles.Where(lr => lr.Value == LocationManagerCodeId);
                        bool hasLocationManager = false;
                        string existingLocationManagers = string.Empty;

                        foreach (KeyValuePair<int, int> locationManagerRole in locationManagerRoles)
                        {
                            int locationId = locationManagerRole.Key;
                            Data.Location location = GetBL<LocationBL>().GetLocation(locationId);
                            if (location != null)
                            {
                                Data.User locationManager = GetBL<InventoryBL>().GetLocationManager(this.CompanyId, locationId);
                                if (locationManager != null)
                                {

                                    string locationManagerName = locationManager != null ? Utils.GetFullName(locationManager) : string.Empty;
                                    if (!skipExistingLocationManagerValidation)
                                    {
                                        if (locationManager != null && user.UserId != locationManager.UserId)
                                        {
                                            existingLocationManagers = existingLocationManagers + string.Format("{0} is already the Location Manager for {1}.<br />", locationManagerName, location.LocationName);
                                            hasLocationManager = hasLocationManager || true;
                                        }
                                    }
                                    else
                                    {
                                        Data.CompanyUserRole userRole = GetBL<InventoryBL>().GetLocationManagerRole(this.CompanyId, locationId);
                                        if (userRole != null)
                                        {
                                            userRole.CompanyUserTypeCodeId = InventoryStaffCodeId;
                                            EmailSender.DowngradeLocationManager(locationManager.Email1, locationManagerName, fromUserFullName,
                                                Support.UserPrimaryEmail, companyName, location.LocationName);
                                        }
                                    }
                                }
                            }
                        }

                        if (hasLocationManager)
                        {
                            ltrExistingLocationManagers.Text = existingLocationManagers;
                            popupLocationManagerAlreadyExist.ShowPopup(1001);
                            return;
                        }

                        var nonLocationBasedInventoryRoles = inventoryRoles.Where(ir => !ir.LocationId.HasValue);
                        foreach (Data.CompanyUserRole userRole in nonLocationBasedInventoryRoles)
                        {
                            userRole.IsActive = false;
                        }

                        foreach (int locationId in locationRoles.Keys)
                        {
                            if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, locationId))
                            {
                                int newRoleCodeId = locationRoles[locationId];
                                Data.Code newRoleCode = Utils.GetCodeByCodeId(newRoleCodeId);
                                Data.Location location = GetBL<LocationBL>().GetLocation(locationId);

                                Data.CompanyUserRole inventoryRole = inventoryRoles.Where(ir => ir.LocationId == locationId).FirstOrDefault();
                                if (inventoryRole != null)
                                {
                                    if (inventoryRole.Code.SortOrder > newRoleCode.SortOrder)
                                    {
                                        // upgrade
                                        if (newRoleCodeId == LocationManagerCodeId)
                                        {
                                            sbLocationManagerUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == InventoryStaffCodeId)
                                        {
                                            sbInventoryStaffUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == InventoryObserverCodeId)
                                        {
                                            sbInventoryObserverUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                    }
                                    else if (inventoryRole.Code.SortOrder < newRoleCode.SortOrder)
                                    {
                                        // downgrade
                                        if (newRoleCodeId == InventoryStaffCodeId)
                                        {
                                            sbInventoryStaffDowngradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == InventoryObserverCodeId)
                                        {
                                            sbInventoryObserverDowngradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == NoAccessCodeId)
                                        {
                                            sbNoAccessDowngradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                    }
                                    else
                                    {
                                        sbNoChangeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                    }

                                    inventoryRole.IsActive = true;
                                    inventoryRole.CompanyUserTypeCodeId = newRoleCodeId;
                                }
                                else
                                {
                                    // Add new one here.
                                    inventoryUser.CompanyUserRoles.Add(
                                                new Data.CompanyUserRole
                                                {
                                                    CreatedByUserId = UserID,
                                                    CreatedDate = Now,
                                                    IsActive = true,
                                                    LastUpdatedByUserId = UserID,
                                                    LastUpdatedDate = Now,
                                                    CompanyUserTypeCodeId = newRoleCodeId,
                                                    LocationId = locationId
                                                }
                                            );

                                    if (newRoleCodeId != Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "NOINVENTORYACCESS"))
                                    {
                                        // upgrade
                                        if (newRoleCodeId == LocationManagerCodeId)
                                        {
                                            sbLocationManagerUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == InventoryStaffCodeId)
                                        {
                                            sbInventoryStaffUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                        else if (newRoleCodeId == InventoryObserverCodeId)
                                        {
                                            sbInventoryObserverUpgradeEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !isStaffMember);
                                return;
                            }
                        }

                        if (sbLocationManagerUpgradeEmail.Length > 0)
                        {
                            locationManagerHtmlUpgrade = string.Format(@"<p>You have been upgraded to Location Manager for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'>
                                                    <p>This means that for this location:</p>
                                                    <ul>
                                                        <li>You can now invite other team members to join.</li>
                                                        <li>You can approve and deny bookings.</li>
                                                        <li>You’ll receive any correspondence regarding bookings.</li>
                                                        <li>And you have full administrative rights over all other elements of the Inventory in this location.</li>
                                                    </ul></div>", sbLocationManagerUpgradeEmail.ToString());
                        }

                        if (sbInventoryStaffUpgradeEmail.Length > 0)
                        {
                            staffHtmlUpgrade = string.Format(@"<p>You have been upgraded to Inventory Staff for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <p>This means that for this location:</p>
                                                    <div style='margin-left:25px;'><ul>
                                                     <li>You can now view all the bookings for your Inventory.</li>
                                                     <li>You can create, edit and delete Inventory Items.</li>
                                                     <li>And you can still make your own bookings.</li>
                                                    </ul></div>", sbInventoryStaffUpgradeEmail.ToString());
                        }

                        if (sbInventoryObserverUpgradeEmail.Length > 0)
                        {
                            observerHtmlUpgrade = string.Format(@"<p>You have been upgraded to Inventory Observer for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that for this location:</p>
                                                    <ul>
                                                     <li>You’ll be able to browse their Inventory</li>
                                                     <li>You can request Items from it for your own bookings.</li>
                                                    </ul></div>", sbInventoryObserverUpgradeEmail.ToString());
                        }

                        if (sbInventoryStaffDowngradeEmail.Length > 0)
                        {
                            staffHtmlDowngrade = string.Format(@"<p>Your role has been changed to Inventory Staff for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that for this location:</p>
                                                    <ul>
                                                     <li>You can still view all the bookings for your Inventory, but you can’t approve or deny requests.</li>
                                                     <li>You can still create, edit and delete Inventory Items.</li>
                                                     <li>You can still make your own bookings.</li>
                                                     <li>You will no longer be able to invite other Inventory team members to join.</li>
                                                    </ul></div>", sbInventoryStaffDowngradeEmail.ToString());
                        }

                        if (sbInventoryObserverDowngradeEmail.Length > 0)
                        {
                            observerHtmlDowngrade = string.Format(@"<p>Your role has been changed to Inventory Observer for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that for this location:</p>
                                                    <ul>
                                                     <li>You won’t have access to the Manage Inventory section any more.</li>
                                                     <li>You can still browse Items, but you won’t be able to add or edit them.</li>
                                                     <li>You can still make and view your own bookings.</li>
                                                    </ul></div>", sbInventoryObserverDowngradeEmail.ToString());
                        }

                        if (sbNoAccessDowngradeEmail.Length > 0)
                        {
                            noAccessHtmlDowngrade = string.Format(@"<p>Your role has been changed to No Access for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that you will no longer have any access to Items stored here, including browsing or booking access.</p>
                                                    </div>", sbNoAccessDowngradeEmail.ToString());
                        }

                        if (sbNoChangeEmail.Length > 0)
                        {
                            noChange = string.Format(@"<p>Your Inventory Role has not changed in the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>", sbNoChangeEmail.ToString());
                        }

                        EmailSender.ChangePermissionInventoryUser(user.Email1, user.FirstName, fromUserFullName, Support.UserPrimaryEmail,
                                companyName, noChange, locationManagerHtmlUpgrade, staffHtmlUpgrade,
                                observerHtmlUpgrade, staffHtmlDowngrade, observerHtmlDowngrade, noAccessHtmlDowngrade);
                    }

                    DataContext.SaveChanges();

                    InitializeUserData();
                    LoadInventoryTeam();
                    gvInventoryTeam.DataBind();
                    upnlInventoryTeam.Update();
                    popupChangePremission.HidePopup();
                }
            }
            else
            {
                bool isStaffMember = Support.IsCompanyInventoryStaffMember(this.CompanyId);
                projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !isStaffMember);
                popupChangePremission.HidePopup();
            }
        }

        /// <summary>
        /// Moves the location.
        /// </summary>
        private void MoveLocation(bool skipMoveConfirmation = false)
        {
            InitializeUserData();
            int selectedLocationId = 0;
            if (int.TryParse(tvLocation.SelectedValue, out selectedLocationId))
            {
                if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, selectedLocationId))
                {
                    Data.Location selectedLocation = GetBL<LocationBL>().GetLocation(selectedLocationId);
                    Data.Location selectedParentLocation = null;
                    if (sbMoveInventoryLocations.SelectedLocationId.HasValue)
                    {
                        selectedParentLocation = GetBL<LocationBL>().GetLocation(sbMoveInventoryLocations.SelectedLocationId.Value);
                        if (selectedParentLocation == null)
                        {
                            // Location has been deleted
                            lblMoveLocationError.Text = "Selected parent location is already being deleted.";
                            LoadLocations(selectedLocation.Company.CompanyName, string.Empty);
                            return;
                        }
                    }
                    else
                    {
                        if (!Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID))
                        {
                            lblMoveLocationError.Text = "Locations created directly under the Company Name become Managed Locations, " +
                                "and only the Inventory Administrator is allowed to create those.";
                            return;
                        }
                    }

                    if (selectedLocation != null)
                    {
                        //Check if the Location is already available in the same level
                        if (!GetBL<LocationBL>().HasDuplcateLocations(CompanyId, sbMoveInventoryLocations.SelectedLocationId, selectedLocation.LocationName))
                        {
                            bool shouldMoveUsers = !sbMoveInventoryLocations.SelectedLocationId.HasValue && Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID);

                            if (skipMoveConfirmation)
                            {
                                List<Data.CompanyUserRole> currentUserRoles = GetBL<InventoryBL>().GetInventoryRolesByLocationId(selectedLocationId);
                                if (shouldMoveUsers)
                                {
                                    foreach (Data.CompanyUserRole role in currentUserRoles)
                                    {
                                        DataContext.CompanyUserRoles.AddObject(new Data.CompanyUserRole
                                        {
                                            CompanyUserId = role.CompanyUserId,
                                            CompanyUserTypeCodeId = role.CompanyUserTypeCodeId,
                                            IsActive = true,
                                            LocationId = selectedLocationId,
                                            CreatedByUserId = UserID,
                                            LastUpdatedByUserId = UserID,
                                            CreatedDate = Now,
                                            LastUpdatedDate = Now
                                        });
                                    }
                                }
                                else if (!selectedLocation.ParentLocationId.HasValue)
                                {
                                    foreach (Data.CompanyUserRole role in currentUserRoles)
                                    {
                                        role.IsActive = false;
                                        role.LastUpdatedByUserId = this.UserID;
                                        role.LastUpdatedDate = Now;
                                    }
                                }

                                popupMoveLocationConfirmUserRoles.HidePopup();
                            }
                            else
                            {
                                Data.Location fromTier2Loc = GetBL<LocationBL>().GetTier2Location(selectedLocationId);
                                Data.Location toTier2Loc = sbMoveInventoryLocations.SelectedLocationId.HasValue ? GetBL<LocationBL>().GetTier2Location(sbMoveInventoryLocations.SelectedLocationId.Value) : null;
                                bool isMovingToNewTier2Loc = sbMoveInventoryLocations.SelectedLocationId.HasValue && fromTier2Loc.LocationId != toTier2Loc.LocationId;

                                // asign popup text
                                if (shouldMoveUsers)
                                {
                                    ltrlConfirmPopupHeaderStrip.Text = "By moving this location to directly under the Company Name, you're making it a Managed Location.";

                                    ltrlConfirmPopupContentStrip.Text = "This means this location will now be able to have its own Location Manager, Inventory Staff and Observers."
                                        + " For now, everyone who had access to it when it was positioned inside '" + Utils.Ellipsize(fromTier2Loc.LocationName, 15)
                                        + "' will have the same access to it. They'll show at the right after the move is complete.";
                                }
                                else if (isMovingToNewTier2Loc)
                                {
                                    ltrlConfirmPopupHeaderStrip.Text = "You're moving this location from one Managed Location (" + Utils.Ellipsize(fromTier2Loc.LocationName, 15)
                                        + ") to a new one (" + Utils.Ellipsize(toTier2Loc.LocationName, 15) + ").";

                                    ltrlConfirmPopupContentStrip.Text = "If you continue with this move, only team members assigned to the location '" + Utils.Ellipsize(toTier2Loc.LocationName, 15)
                                        + "' will have access to the Items in '" + Utils.Ellipsize(selectedLocation.LocationName, 15)
                                        + "'. To check if everyone has access that needs it, click on '" + Utils.Ellipsize(toTier2Loc.LocationName, 15)
                                        + "' to see the full team listed, or look up individual team members on the Team tab.";
                                }

                                popupMoveLocationConfirmUserRoles.ShowPopup();
                                return;
                            }

                            selectedLocation.ParentLocationId = sbMoveInventoryLocations.SelectedLocationId;
                            GetBL<LocationBL>().SaveChanges();
                            popupMoveLocation.HidePopup();
                            LoadLocations(selectedLocation.Company.CompanyName, tvLocation.SelectedValue);
                            return;
                        }
                        else
                        {
                            //Display the duplicate error message
                            lblMoveLocationError.Text = "You already have a location with this name at this level in the Inventory.";
                        }
                    }
                    else
                    {
                        // Location has been deleted
                        lblMoveLocationError.Text = "Location is already being deleted.";
                        htnSelectedLocationId.Value = string.Empty;
                    }

                    LoadLocations(GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName, tvLocation.SelectedValue);
                }
                else
                {
                    projectWarningPopupInventory.ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                }
            }
            else
            {
                lblMoveLocationError.Text = "Cannot move Company Inventory Location.";
            }
        }

        private void InitializeChangePermissionPopup(int companyUserId)
        {
            rbtnInventoryAdmin.Checked = false;
            btnUpdatePermission.Enabled = false;
            hdnCompanyUserId.Value = companyUserId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Loads the location user roles.
        /// </summary>
        /// <param name="node">The node.</param>
        private void LoadLocationUserRoles(RadTreeNode node)
        {
            if (node.Level == 1)
            {
                int inventoryStaffCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVSTAFF");
                int inventoryObserverCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVOBSERVER");

                int locationId = 0;
                if (int.TryParse(node.Value, out locationId))
                {
                    divLocationRoles.Visible = true;
                    Data.User locationManager = GetBL<InventoryBL>().GetContactBookingManager(this.CompanyId, locationId);
                    lblLocationManager.Text = Utils.GetFullName(locationManager);

                    List<Data.User> inventoryStaffUsers = GetBL<InventoryBL>().GetInventoryRolesByLocationIdAndUserType(locationId, inventoryStaffCodeId);
                    List<Data.User> inventoryObserverUsers = GetBL<InventoryBL>().GetInventoryRolesByLocationIdAndUserType(locationId, inventoryObserverCodeId);

                    lbxLocationStaff.DataSource = inventoryStaffUsers.Select(isu => new { FullName = Utils.GetFullName(isu) });
                    lbxLocationStaff.DataTextField = "FullName";
                    lbxLocationStaff.DataBind();

                    lbxLocationObservers.DataSource = inventoryObserverUsers.Select(iou => new { FullName = Utils.GetFullName(iou) });
                    lbxLocationObservers.DataTextField = "FullName";
                    lbxLocationObservers.DataBind();
                }
            }
            else
            {
                divLocationRoles.Visible = false;
            }
        }

        /// <summary>
        /// Loads the inventory team.
        /// </summary>
        private void LoadInventoryTeam()
        {
            #region Load Codevalues

            int inventoryTeamInvitationTypeCodeId = Support.GetCodeByValue("InvitationType", "INVENTORYTEAM").CodeId;

            List<Data.Code> invitationStatusList = Support.GetCodesByCodeHeader("InvitationStatus");
            int invitationStatusPendingCodeID = invitationStatusList.Where(c => c.Value == "PENDING").FirstOrDefault().CodeId;
            int invitationStatusRejectedCodeID = invitationStatusList.Where(c => c.Value == "REJECTED").FirstOrDefault().CodeId;

            Data.Code invitationStatusAcceptedCode = invitationStatusList.Where(c => c.Value == "ACCEPTED").FirstOrDefault();

            #endregion Load Codevalues

            #region Current Administrators

            var inventoryTeamList = from cu in DataContext.CompanyUsers
                                    from cur in
                                        (
                                            from curTemp in DataContext.CompanyUserRoles
                                            join cut in DataContext.Codes on curTemp.CompanyUserTypeCodeId equals cut.CodeId
                                            where cu.CompanyUserId == curTemp.CompanyUserId && curTemp.IsActive
                                                 && (curTemp.CompanyUserTypeCodeId == InventoryStaffCodeId ||
                                                        curTemp.CompanyUserTypeCodeId == InventoryAdminCodeId ||
                                                        curTemp.CompanyUserTypeCodeId == InventoryObserverCodeId ||
                                                        curTemp.CompanyUserTypeCodeId == LocationManagerCodeId ||
                                                        curTemp.CompanyUserTypeCodeId == NoAccessCodeId)
                                            orderby cut.SortOrder // get highest role
                                            select curTemp).Take(1)
                                    join u in DataContext.Users on cu.UserId equals u.UserId
                                    join cut in DataContext.Codes on cur.CompanyUserTypeCodeId equals cut.CodeId
                                    where cu.CompanyId == CompanyId && cu.IsActive == true

                                    select new InventoryTeamInfo
                                    {
                                        UserId = u.UserId,
                                        CompanyUserId = cu.CompanyUserId,
                                        InvitationId = 0,
                                        Name = (u.FirstName + " " + u.LastName),
                                        Position = u.Position,
                                        Status = invitationStatusAcceptedCode.Description,
                                        StatusSortOrder = invitationStatusAcceptedCode.SortOrder,
                                        CompanyUserTypeStatusSortOrder = cut.SortOrder,
                                        CompanyUserTypeCodeId = cur.CompanyUserTypeCodeId,
                                        CreatedDate = cu.CreatedDate,
                                        UserPermission = cut.Description
                                    };

            #endregion Current Administrators

            #region Pending invitations

            var inventoryTeamInvitationList = from i in DataContext.Invitations
                                              from iur in
                                                  (
                                                      from iurTemp in DataContext.InvitationUserRoles
                                                      join iut in DataContext.Codes on iurTemp.UserTypeCodeId equals iut.CodeId
                                                      where i.InvitationId == iurTemp.InvitationId && iurTemp.IsActive
                                                           && (iurTemp.UserTypeCodeId == InventoryStaffCodeId ||
                                                                iurTemp.UserTypeCodeId == InventoryAdminCodeId ||
                                                                iurTemp.UserTypeCodeId == InventoryObserverCodeId ||
                                                                iurTemp.UserTypeCodeId == LocationManagerCodeId ||
                                                                iurTemp.UserTypeCodeId == NoAccessCodeId)
                                                      orderby iut.SortOrder
                                                      select iurTemp).Take(1)
                                              from u in DataContext.Users.Where(user => user.UserId == i.ToUserId).Take(1).DefaultIfEmpty()
                                              join iut in DataContext.Codes on iur.UserTypeCodeId equals iut.CodeId
                                              join invSta in DataContext.Codes on i.InvitationStatusCodeId equals invSta.CodeId
                                              where i.RelatedTable == StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies && i.RelatedId == CompanyId
                                              && (i.InvitationStatusCodeId == invitationStatusPendingCodeID || i.InvitationStatusCodeId == invitationStatusRejectedCodeID)
                                              && i.InvitationTypeCodeId == inventoryTeamInvitationTypeCodeId
                                              orderby i.CreatedDate
                                              select new InventoryTeamInfo
                                              {
                                                  UserId = 0,
                                                  CompanyUserId = 0,
                                                  InvitationId = i.InvitationId,
                                                  Name = ((u == null ? i.ToName : (u.FirstName + " " + u.LastName).Trim()) + " (" + i.ToEmail + ")"),
                                                  Position = string.Empty,
                                                  Status = invSta.Description,
                                                  StatusSortOrder = invSta.SortOrder,
                                                  CompanyUserTypeStatusSortOrder = 0,
                                                  CompanyUserTypeCodeId = iur.UserTypeCodeId,
                                                  CreatedDate = i.CreatedDate,
                                                  UserPermission = iut.Description
                                              };

            #endregion Pending invitations

            var allUsers = inventoryTeamInvitationList.Union(inventoryTeamList).OrderBy(u1 => u1.StatusSortOrder).ThenBy(u2 => u2.CompanyUserTypeStatusSortOrder);
            gvInventoryTeam.DataSource = allUsers;

            if (!Utils.IsCompanyInventoryAdmin(this.CompanyId, this.UserID))
            {
                foreach (GridColumn col in gvInventoryTeam.MasterTableView.RenderColumns)
                {
                    if (col.UniqueName == "DeleteColumn")
                    {
                        col.Visible = false;
                    }
                }
            }
        }

        #endregion PrivateMethods
    }

    /// <summary>
    /// Inventory Team info class.
    /// </summary>
    public class InventoryTeamInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the company user identifier.
        /// </summary>
        /// <value>
        /// The company user identifier.
        /// </value>
        public int CompanyUserId { get; set; }

        /// <summary>
        /// Gets or sets the invitation identifier.
        /// </summary>
        /// <value>
        /// The invitation identifier.
        /// </value>
        public int InvitationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public string Position { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status sort order.
        /// </summary>
        /// <value>
        /// The status sort order.
        /// </value>
        public int StatusSortOrder { get; set; }

        /// <summary>
        /// Gets or sets the company user type status sort order.
        /// </summary>
        /// <value>
        /// The company user type status sort order.
        /// </value>
        public int CompanyUserTypeStatusSortOrder { get; set; }

        /// <summary>
        /// Gets or sets the company user type code identifier.
        /// </summary>
        /// <value>
        /// The company user type code identifier.
        /// </value>
        public int CompanyUserTypeCodeId { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>
        /// The created date.
        /// </value>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the user permission.
        /// </summary>
        /// <value>
        /// The user permission.
        /// </value>
        public string UserPermission { get; set; }
    }
}