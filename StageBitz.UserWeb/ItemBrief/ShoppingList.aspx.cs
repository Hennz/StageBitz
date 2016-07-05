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
using System.Linq;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for task list.
    /// </summary>
    public partial class ShoppingList : PageBase
    {
        #region PROPERTIES

        /// <summary>
        /// Gets the task list identifier.
        /// </summary>
        /// <value>
        /// The task list identifier.
        /// </value>
        private int TaskListId
        {
            get
            {
                if (ViewState["taskListId"] == null)
                {
                    int taskListId = 0;

                    if (Request["taskListId"] != null)
                    {
                        int.TryParse(Request["taskListId"], out taskListId);
                    }

                    ViewState["taskListId"] = taskListId;
                }

                return (int)ViewState["taskListId"];
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
                if (ViewState["projectId"] == null)
                {
                    int ProjectId = 0;

                    if (Request["projectId"] != null)
                    {
                        int.TryParse(Request["projectId"], out ProjectId);
                    }

                    ViewState["projectId"] = ProjectId;
                }

                return (int)ViewState["projectId"];
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

        #endregion PROPERTIES

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Permission denied for this project.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.ExportData1.PDFExportClick += new EventHandler(ExportData1_PDFExportClick);
            this.ExportData1.ExcelExportClick += new EventHandler(ExportData1_ExcelExportClick);
            taskList.SetTasksGridWidth(930);
            if (!IsPostBack)
            {
                StageBitz.Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project.");
                }

                this.CompanyId = project.CompanyId;
                warningDisplay.ProjectID = ProjectId;
                warningDisplay.LoadData();
                projectWarningPopup.ProjectId = ProjectId;

                bool isReadOnly = Support.IsReadOnlyRightsForProject(ProjectId);

                LoadList();
                if (ProjectId > 0)
                {
                    LoadBreadCrumbs();
                    //For observers, do not allow delete list
                    if (isReadOnly)
                    {
                        btnDeleteList.Enabled = false;
                    }
                }

                //Set links
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}", project.CompanyId, (int)BookingTypes.Project, ProjectId, ItemTypeId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                hyperLinkTaskManager.NavigateUrl = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}&ItemTypeId={1}", ProjectId, ItemTypeId);
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();
            }
        }

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (taskList.IsEmptyData)
            {
                ExportData1.Visible = false;
                btnDeleteList.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ExportData1_ExcelExportClick(object sender, EventArgs e)
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
                Response.Redirect(string.Format("~/ItemBrief/TaskManager.aspx?ProjectId={0}&ItemTypeId={1}", ProjectId, ItemTypeId));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveConfirm_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                this.GetBL<ItemBriefBL>().RemoveShoppingList(TaskListId);

                Response.Redirect(string.Format("~/ItemBrief/TaskManager.aspx?ProjectId={0}&ItemTypeId={1}", ProjectId, ItemTypeId));
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            Data.Project project = GetBL<ProjectBL>().GetProject(ProjectId);
            string cultureName = Support.GetCultureName(project.Country.CountryCode);

            TaskListReportParameters parameters = new TaskListReportParameters
            {
                CultureName = cultureName,
                ProjectId = this.ProjectId,
                SortExpression = taskList.TaskListSortExpression,
                TaskListId = this.TaskListId,
                UserId = this.UserID
            };

            var shoppingList = GetBL<ItemBriefBL>().GetTaskListByTaskListId(this.TaskListId);
            string fileName = string.Format("{0}_TaskList", shoppingList.Name);

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateTaskListReport(parameters, exportType,
                    out fileNameExtension, out encoding, out mimeType);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }

        /// <summary>
        /// Loads the list.
        /// </summary>
        private void LoadList()
        {
            taskList.TaskListId = TaskListId;
            taskList.ProjectId = ProjectId;

            var taskList1 = (from l in DataContext.TaskLists
                             where l.TaskListId == TaskListId
                             select l).FirstOrDefault();
            //Load list name
            if (TaskListId != 0)
                this.DisplayTitle = Support.TruncateString(taskList1.Name, 50);
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            StageBitz.Data.Project project = this.GetBL<ProjectBL>().GetProject(ProjectId);
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
            bc.AddLink("Task Manager", string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}&ItemTypeId={1}", ProjectId, ItemTypeId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}