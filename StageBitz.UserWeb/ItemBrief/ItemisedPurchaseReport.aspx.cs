using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Logic.Business.Project;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for itemised purchase report
    /// </summary>
    public partial class ItemisedPurchaseReport : PageBase
    {
        #region PROPERTIES

        /// <summary>
        /// Gets or sets the name of the culture.
        /// </summary>
        /// <value>
        /// The name of the culture.
        /// </value>
        public String CultureName
        {
            get
            {
                if (ViewState["CultureName"] == null)
                {
                    ViewState["CultureName"] = Now;
                }

                return (string)ViewState["CultureName"];
            }
            set
            {
                ViewState["CultureName"] = value;
            }
        }

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["projectid"] == null)
                {
                    int ProjectId = 0;

                    if (Request["projectid"] != null)
                    {
                        int.TryParse(Request["projectid"], out ProjectId);
                    }

                    ViewState["projectid"] = ProjectId;
                }

                return (int)ViewState["projectid"];
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
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            private set
            {
                ViewState["CompanyId"] = value;
            }
        }

        #endregion PROPERTIES

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for Itemised Purchase Report.
        /// or
        /// You don't have permission to view data in this project.
        /// </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.ExportData1.PDFExportClick += new EventHandler(ExportData1_PDFExportClick);
            this.ExportData1.ExcelExportClick += new EventHandler(ExportData1_ExcelExportClick);
            projectItemTypes.ProjectID = ProjectId;
            projectWarningPopup.ProjectId = ProjectId;
            if (!IsPostBack)
            {
                var project = GetBL<ProjectBL>().GetProject(ProjectId);

                if (Support.CanAccessProject(project))
                {
                    if (!Support.CanSeeBudgetSummary(UserID, ProjectId))
                    {
                        throw new ApplicationException("Permission denied for Itemised Purchase Report.");
                    }

                    this.CompanyId = project.CompanyId;
                    warningDisplay.ProjectID = ProjectId;
                    warningDisplay.LoadData();

                    CultureName = Support.GetCultureName(project.Country.CountryCode);

                    //Set links
                    lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}",
                        project.CompanyId, (int)BookingTypes.Project, ProjectId, projectItemTypes.SelectedItemTypeId);
                    lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                    hyperLinkTaskManager.NavigateUrl = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId);
                    reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                    reportList.ApplyReportLinkStyle("ItemisedPurchaseReport");
                    projectUpdatesLink.ProjectID = ProjectId;
                    projectUpdatesLink.LoadData();

                    LoadBreadCrumbs();
                    LoadData();
                    if (projectItemTypes.SelectedItemTypeId != 0)
                        DisplayTitle = "Itemised Purchase Report - " + Utils.GetItemTypeById(projectItemTypes.SelectedItemTypeId).Name;
                    else
                        DisplayTitle = "Itemised Purchase Report";
                }
                else
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("You don't have permission to view data in this project.");
                }
            }

            projectItemTypes.InformParentToReload += delegate()
            {
                //Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
                ReloadByItemType();
            };
        }

        /// <summary>
        /// Reloads the type of the by item.
        /// </summary>
        private void ReloadByItemType()
        {
            var nameValues = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            nameValues.Set("ItemTypeId", projectItemTypes.SelectedItemTypeId.ToString());

            string url = Request.Url.AbsolutePath;
            string updatedQueryString = "?" + nameValues.ToString();
            Response.Redirect(url + updatedQueryString);
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ExportData1_ExcelExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                //Bind data and Export excel report
                ExportReport(ReportTypes.Excel);
            }
        }

        /// <summary>
        /// Handles the PDFExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ExportData1_PDFExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                ExportReport(ReportTypes.Pdf);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDone_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect(string.Format("~/Project/ProjectDashboard.aspx?ProjectId={0}", ProjectId));
            }
        }

        #endregion Event Handlers

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            var project = GetBL<ProjectBL>().GetProject(this.ProjectId);
            if (project != null)
            {
                ItemisedPurchaseReportParameters parameters = new ItemisedPurchaseReportParameters
                {
                    CultureName = this.CultureName,
                    ItemTypeId = projectItemTypes.SelectedItemTypeId,
                    ProjectId = this.ProjectId,
                    SortExpression = rgvItemisedPurchase.MasterTableView.SortExpressions.GetSortString(),
                    UserId = this.UserID
                };

                string fileName = string.Format("{0}_ItemisedPurchaseReport", project.ProjectName);
                string fileNameExtension;
                string encoding;
                string mimeType;

                byte[] reportBytes = UserWebReportHandler.GenerateItemisedPurchaseReport(parameters, exportType,
                        out fileNameExtension, out encoding, out mimeType);
                Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the rgvItemisedPurchase control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void rgvItemisedPurchase_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Handles the SortCommand event of the rgvItemisedPurchase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void rgvItemisedPurchase_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;
                rgvItemisedPurchase.MasterTableView.SortExpressions.AddSortExpression(sortExpr);
                e.Canceled = true;
                rgvItemisedPurchase.Rebind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rgvItemisedPurchase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void rgvItemisedPurchase_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem || e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ItemBriefTask itemBriefTask = ((dynamic)(dataItem.DataItem)).ItemBriefTask;
                HyperLink hyperLinkItem = (HyperLink)dataItem.FindControl("hyperLinkItem");

                if (hyperLinkItem != null)
                {
                    hyperLinkItem.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}", itemBriefTask.ItemBriefId));
                    hyperLinkItem.Text = Support.TruncateString(itemBriefTask.ItemBrief.Name, 20);
                    if (itemBriefTask.ItemBrief.Name.Length > 20)
                    {
                        hyperLinkItem.ToolTip = itemBriefTask.ItemBrief.Name;
                    }
                }

                //Description
                dataItem["Description"].Text = Support.TruncateString(itemBriefTask.Description, 30);
                if (itemBriefTask.Description != null && itemBriefTask.Description.Length > 30)
                {
                    dataItem["Description"].ToolTip = itemBriefTask.Description;
                }

                //Vendor
                dataItem["Vendor"].Text = Support.TruncateString(itemBriefTask.Vendor, 15);

                if (itemBriefTask.Vendor != null && itemBriefTask.Vendor.Length > 15)
                {
                    dataItem["Vendor"].ToolTip = itemBriefTask.Vendor;
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            int ItemBriefTaskCompletedStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");

            var ItemBriefTask = from ibt in DataContext.ItemBriefTasks
                                join c in DataContext.Codes on ibt.ItemBriefTaskStatusCodeId equals c.CodeId
                                join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                                join ibtyp in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtyp.ItemBriefId
                                orderby ibt.CompletedDate
                                where ib.ProjectId == ProjectId
                                      && ibt.ItemBriefTaskStatusCodeId == ItemBriefTaskCompletedStatusCodeId && ibtyp.ItemTypeId == projectItemTypes.SelectedItemTypeId
                                select new
                                {
                                    ItemBriefTask = ibt,
                                    ItemBriefName = ib.Name,
                                    Total = (ibt.TotalCost != null ? ibt.TotalCost : 0)
                                };
            rgvItemisedPurchase.DataSource = ItemBriefTask;

            if (ItemBriefTask.Count() > 0)
            {
                //rgvItemisedPurchase.Enabled = true;
                ExportData1.Visible = true;
            }
            else
            {
                //rgvItemisedPurchase.Enabled = false;
                ExportData1.Visible = false;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectId).FirstOrDefault();

            int companyId = project.Company.CompanyId;
            if (Support.IsCompanyAdministrator(companyId))
            {
                bc.AddLink(project.Company.CompanyName, string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", companyId));
            }
            else
            {
                bc.AddLink(project.Company.CompanyName, string.Empty);
            }

            bc.AddLink(project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?ProjectId={0}", ProjectId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}