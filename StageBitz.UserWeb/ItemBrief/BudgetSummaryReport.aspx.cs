using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Project;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Data;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for budget summary report.
    /// </summary>
    public partial class BudgetSummaryReport : PageBase
    {
        #region Private Constants

        private const string colItemBriefID = "ItemBriefID";
        private const string colItemType = "ItemType";
        private const string colItemName = "ItemName";
        private const string colBudget = "Budget";
        private const string colExpended = "Expended";
        private const string colRemaining = "Remaining";
        private const string colBalance = "Balance";
        private const string colIsEstimatedCostNull = "IsEstimatedCostNull";

        #endregion Private Constants

        #region PROPERTIES

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
                    int projectId = 0;

                    if (Request["projectid"] != null)
                    {
                        int.TryParse(Request["projectid"], out projectId);
                    }

                    ViewState["projectid"] = projectId;
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

        #endregion PROPERTIES

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for this project.
        /// or
        /// Permission denied for Budget Summary Page.
        /// </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.ExportData1.PDFExportClick += new EventHandler(ExportData1_PDFExportClick);
            this.ExportData1.ExcelExportClick += new EventHandler(ExportData1_ExcelExportClick);
            projectItemTypes.ProjectID = ProjectId;
            projectItemTypes.ShouldShowAllItemTypeText = true;
            if (!IsPostBack)
            {
                StageBitz.Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);
                this.CompanyId = project.CompanyId;

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project.");
                }

                if (!Support.CanSeeBudgetSummary(UserID, ProjectId))
                {
                    throw new ApplicationException("Permission denied for Budget Summary Page.");
                }

                if (project != null)
                {
                    CultureName = Support.GetCultureName(project.Country.CountryCode);
                }

                warningDisplay.ProjectID = ProjectId;
                warningDisplay.LoadData();

                //Set links
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}",
                    project.CompanyId, (int)BookingTypes.Project, ProjectId, projectItemTypes.SelectedItemTypeId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                hyperLinkTaskManager.NavigateUrl = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId);
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                reportList.ApplyReportLinkStyle("BudgetSummaryReport");
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();

                LoadData();

                projectWarningPopup.ProjectId = ProjectId;
                budgetList.ProjectID = ProjectId;
                budgetList.ItemTypeID = projectItemTypes.SelectedItemTypeId;
                budgetList.BudgetListViewMode = UserWeb.Controls.ItemBrief.BudgetList.ViewMode.BudgetSummary;
                LoadBreadCrumbs();

                if (projectItemTypes.SelectedItemTypeId > 0)
                    DisplayTitle = "Budget Summary Report - " + Utils.GetItemTypeById(projectItemTypes.SelectedItemTypeId).Name;
                else
                    DisplayTitle = "Budget Summary Report";
            }
            projectItemTypes.InformParentToReload += delegate()
            {
                //Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
                ReloadByItemType();
            };

            bool IsItemTypeColVisible = true;
            foreach (GridColumn col in gvItems.MasterTableView.RenderColumns)
            {
                if (col.UniqueName == "ItemType" && projectItemTypes.SelectedItemTypeId != -1)
                {
                    col.Visible = false;
                    IsItemTypeColVisible = false;
                }
                else if (IsItemTypeColVisible)
                {
                    switch (col.UniqueName)
                    {
                        case "Budget":
                            {
                                col.HeaderStyle.Width = 110;
                                break;
                            }
                        case "Expended":
                            {
                                col.HeaderStyle.Width = 110;
                                break;
                            }
                        case "Remaining":
                            {
                                col.HeaderStyle.Width = 110;
                                break;
                            }
                        case "Balance":
                            {
                                col.HeaderStyle.Width = 110;
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SortCommand event of the gvItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvItems_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;
                gvItems.MasterTableView.SortExpressions.AddSortExpression(sortExpr);
                e.Canceled = true;
                gvItems.Rebind();
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

        /// <summary>
        /// Handles the ItemDataBound event of the gvItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvItems_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = (GridDataItem)e.Item;
                DataRowView ItemRow = (DataRowView)item.DataItem;

                HyperLink lnkItemBriefDetails = (HyperLink)item.FindControl("lnkItemBriefDetails");
                HtmlImage imgNoEstimatedCost = (HtmlImage)item.FindControl("imgNoEstimatedCost");
                imgNoEstimatedCost.Visible = this.GetBL<ItemBriefBL>().HasEmptyEstimateCostInItemBrief(int.Parse(ItemRow[colItemBriefID].ToString()));

                //Item link
                lnkItemBriefDetails.Text = Support.TruncateString(ItemRow[colItemName].ToString(), 40);
                lnkItemBriefDetails.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}", ItemRow[colItemBriefID]));
                if (ItemRow[colItemName].ToString().Length > 40)
                {
                    lnkItemBriefDetails.ToolTip = ItemRow[colItemName].ToString();
                }

                item[colBudget].Text = Support.FormatCurrency(ItemRow[colBudget], CultureName);
                item[colExpended].Text = Support.FormatCurrency(ItemRow[colExpended], CultureName);
                item[colBalance].Text = Support.FormatCurrency(ItemRow[colBalance], CultureName);
                //item[colRemaining].Text = Support.FormatCurrency(ItemRow[colRemaining], CultureInfo);
            }
        }

        protected void gvItems_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            gvItems.DataSource = GetBL<ItemBriefBL>().GetBudgetDetails(projectItemTypes.SelectedItemTypeId, this.ProjectId);
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ExportData1_ExcelExportClick(object sender, EventArgs e)
        {
            //Bind data and Export excel report
            if (!StopProcessing)
            {
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

        #endregion Event Handlers

        #region Private Methods

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
            //popupEditTask.Visible = false;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            gvItems.DataSource = GetBL<ItemBriefBL>().GetBudgetDetails(projectItemTypes.SelectedItemTypeId, ProjectId);
            gvItems.DataBind();
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            StageBitz.Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);

            int companyId = project.Company.CompanyId;
            string companyName = project.Company.CompanyName;
            string projectName = project.ProjectName;
            if (Support.IsCompanyAdministrator(companyId))
            {
                bc.AddLink(companyName, string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", companyId));
            }
            else
            {
                bc.AddLink(companyName, "");
            }

            bc.AddLink(projectName, string.Format("~/Project/ProjectDashboard.aspx?ProjectId={0}", ProjectId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();

            //Build Company and Project name labels
            lblCompanyName.Text = Support.TruncateString(companyName, 100);
            lblProjectName.Text = Support.TruncateString(projectName, 100);
        }

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);
            if (project != null)
            {
                string sortExpression = gvItems.MasterTableView.SortExpressions.GetSortString();
                int itemTypeId = projectItemTypes.SelectedItemTypeId;
                budgetList.ItemTypeID = itemTypeId;

                BudgetSummaryReportParameters parameters = new BudgetSummaryReportParameters();
                parameters.SortExpression = sortExpression;
                parameters.ItemTypeId = projectItemTypes.SelectedItemTypeId;
                parameters.UserId = this.UserID;
                parameters.CultureName = CultureName;
                parameters.ProjectId = this.ProjectId;

                string fileName = string.Format("{0}_BudgetSummaryReport", project.ProjectName);

                string fileNameExtension;
                string encoding;
                string mimeType;

                byte[] reportBytes = UserWebReportHandler.GenerateBudgetSummaryReport(parameters, exportType,
                        out fileNameExtension, out encoding, out mimeType);
                Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
            }
        }

        #endregion Private Methods
    }
}