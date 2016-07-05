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
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for task manager.
    /// </summary>
    public partial class TaskManager : PageBase
    {
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

        /// <summary>
        /// Gets or sets a value indicating whether this page is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this page is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
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
            projectItemTypes.ProjectID = ProjectId;
            if (!IsPostBack)
            {
                StageBitz.Data.Project project = this.GetBL<ProjectBL>().GetProject(ProjectId);

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project.");
                }

                this.CompanyId = project.CompanyId;
                projectWarningPopup.ProjectId = ProjectId;
                warningDisplay.ProjectID = ProjectId;
                warningDisplay.LoadData();

                if (Support.IsReadOnlyRightsForProject(ProjectId) || projectItemTypes.SelectedItemTypeId == 0)
                {
                    IsReadOnly = true;
                    //Can not create lists, can not add tasks to lists, can view lists
                    //Set visibility and enabling
                    txtListName.Enabled = false;
                    btnAddList.Enabled = false;
                    gridviewTaskList.Enabled = false;
                }

                #region SET Links

                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}",
                    project.CompanyId, (int)BookingTypes.Project, ProjectId, projectItemTypes.SelectedItemTypeId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                hyperLinkTaskManager.NavigateUrl = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId);
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();

                #endregion SET Links

                LoadData();
                if (projectItemTypes.SelectedItemTypeId != 0)
                {
                    string itemTypename = Utils.GetItemTypeById(projectItemTypes.SelectedItemTypeId).Name;
                    litTitleAT.Text = string.Concat("All Active " + itemTypename + " Tasks");
                    litItemtype1.Text = litItemtype2.Text = itemTypename;
                    DisplayTitle = "Task Manager - " + Utils.GetItemTypeById(projectItemTypes.SelectedItemTypeId).Name;
                }
                else
                {
                    litTitleAT.Text = "All Active Tasks";
                    DisplayTitle = "Task Manager";
                }
            }

            projectItemTypes.InformParentToReload += delegate()
            {
                //Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
                ReloadByItemType();
            };
        }

        #region Activetasklist events

        /// <summary>
        /// Handles the RowDataBound event of the gridviewTaskList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gridviewTaskList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                /*The dynamic type enables the operations in which it occurs to bypass compile-time type checking.
                 Instead, these operations are resolved at run time. */
                dynamic task = e.Row.DataItem as dynamic;

                Label lblItemDescription = (Label)e.Row.FindControl("lblItemDescription");
                Label lblTaskDescription = (Label)e.Row.FindControl("lblTaskDescription");

                RadToolTip toolTip1 = (RadToolTip)e.Row.FindControl("radToolTipLists1");
                RadToolTip toolTip2 = (RadToolTip)e.Row.FindControl("radToolTipLists2");

                ListView listViewTaskListToolTip1 = (ListView)e.Row.FindControl("listViewTaskListToolTip1");
                ListView listViewTaskListToolTip2 = (ListView)e.Row.FindControl("listViewTaskListToolTip2");

                lblItemDescription.Text = Support.TruncateString(task.itemBriefName, 22);
                lblTaskDescription.Text = Support.TruncateString(task.taskDescription, 50);

                int taskId = 0;
                taskId = (int)task.ItemBriefTaskId;
                //Style the text if task is already added to a list
                if (task.taskListId > 0)
                {
                    lblItemDescription.CssClass = "grayText";
                    lblTaskDescription.CssClass = "grayText"; //.Style.Add("color", "#C0C3C6");

                    var taskLists = this.GetBL<ItemBriefBL>().GetTaskList(taskId);

                    listViewTaskListToolTip1.DataSource = taskLists;
                    listViewTaskListToolTip1.DataBind();

                    listViewTaskListToolTip2.DataSource = taskLists;
                    listViewTaskListToolTip2.DataBind();
                }
                else
                {
                    toolTip1.Visible = false;
                    toolTip2.Visible = false;
                }
                HtmlImage imgNoEstimatedCost = (HtmlImage)e.Row.FindControl("imgNoEstimatedCost");
                imgNoEstimatedCost.Visible = (task.EstimatedCost == null);
            }
        }

        #endregion Activetasklist events

        #region TaskLists management events

        /// <summary>
        /// Handles the Click event of the btnAddList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddList_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                string trimmedlistName = txtListName.Text.Trim();

                var listName = (from tl in DataContext.TaskLists
                                where tl.Name.ToLower() == trimmedlistName.ToLower() && tl.ProjectId == ProjectId
                                select tl).FirstOrDefault();

                if (listName == null)
                {
                    //Add list to task lists table.
                    StageBitz.Data.TaskList taskList = new Data.TaskList();

                    taskList.Name = trimmedlistName;
                    taskList.ProjectId = ProjectId;
                    taskList.ItemTypeId = projectItemTypes.SelectedItemTypeId;
                    taskList.CreatedByUserId = UserID;
                    taskList.CreatedDate = Now;
                    taskList.LastUpdatedDate = Now;
                    taskList.LastUpdatedByUserId = UserID;
                    DataContext.TaskLists.AddObject(taskList);

                    DataContext.SaveChanges();

                    txtListName.Text = "";
                    SetFocus(txtListName);
                    LoadData();
                    upnlTaskList.Update();
                }
                else
                {
                    //Warning message - there is a list with the same name
                    lblError.Visible = true;
                    lblError.Text = string.Format("Task list name should be unique under a project.");
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the listViewTaskLists control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void listViewTaskLists_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.CommandName == "AddTasks")
                {
                    ListViewItem itemClicked = e.Item;

                    //Read TaskListId ,
                    //read selected tasks from the grid
                    //populate TaskListsItemBriefTasks table
                    int taskListId = Convert.ToInt32(e.CommandArgument.ToString());
                    // Find Controls/Retrieve values from the item  here

                    foreach (GridViewRow gvr in gridviewTaskList.Rows)
                    {
                        CheckBox chkBx = (CheckBox)gvr.FindControl("chkBoxSelect");
                        if (chkBx != null && chkBx.Checked)
                        {
                            //Read the itembrieftaskId from the gridview
                            int itemBriefTaskid = (int)gridviewTaskList.DataKeys[gvr.RowIndex].Value;

                            //Check for duplicate tasks
                            var taskListItemTask = this.GetBL<ItemBriefBL>().GetTaskListsItemBriefTask(taskListId, itemBriefTaskid);

                            if (taskListItemTask == null)
                            {
                                //Add to list
                                StageBitz.Data.TaskListsItemBriefTask taskListTasks = new Data.TaskListsItemBriefTask();

                                taskListTasks.TaskListId = taskListId;
                                taskListTasks.ItemBriefTaskId = itemBriefTaskid;
                                taskListTasks.CreatedByUserId = UserID;
                                taskListTasks.CreatedDate = Now;
                                taskListTasks.LastUpdatedDate = Now;
                                taskListTasks.LastUpdatedByUserId = UserID;
                                DataContext.TaskListsItemBriefTasks.AddObject(taskListTasks);
                            }
                        }
                    }

                    DataContext.SaveChanges();
                    LoadData();
                    upnlTaskList.Update();
                    upnlTaskManager.Update();
                }
            }
        }

        /// <summary>
        /// Shopping lists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void listViewTaskLists_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                /*The dynamic type enables the operations in which it occurs to bypass compile-time type checking.
                 Instead, these operations are resolved at run time. */
                dynamic taskList = e.Item.DataItem as dynamic;

                HtmlAnchor linkTaskList = (HtmlAnchor)e.Item.FindControl("linkTaskList");
                Literal litTasksCount = (Literal)e.Item.FindControl("litTasks");
                Literal litActiveTasksCount = (Literal)e.Item.FindControl("litActiveTasks");
                Literal litCompletedTasksCount = (Literal)e.Item.FindControl("litCompletedTasks");
                Literal litEmpty = (Literal)e.Item.FindControl("litEmpty");
                Button btnAddSelectedTask = (Button)e.Item.FindControl("btnAddSelectedTask");
                //Set linkTaskList href - Modifylist page
                linkTaskList.HRef = string.Format("~/ItemBrief/ShoppingList.aspx?projectId={0}&taskListId={1}&itemTypeId={2}",
                        ProjectId, taskList.TaskListId, projectItemTypes.SelectedItemTypeId);

                if (IsReadOnly)
                {
                    btnAddSelectedTask.Enabled = false;
                }

                linkTaskList.InnerText = Support.TruncateString(taskList.Name, 40);

                if (taskList.TaskCount > 0)
                {
                    //Tasks found
                    litTasksCount.Visible = true;
                    litActiveTasksCount.Visible = true;
                    litCompletedTasksCount.Visible = true;
                    litEmpty.Visible = false;

                    litTasksCount.Text = string.Format("{0} Task{1}", taskList.TaskCount, taskList.TaskCount == 1 ? string.Empty : "s");
                    litCompletedTasksCount.Text = string.Format("{0} Completed", taskList.CompletedItemCount);
                    litActiveTasksCount.Text = string.Format("{0} Active", taskList.InprogressItemCount);
                }
                else
                {
                    litTasksCount.Visible = false;
                    litActiveTasksCount.Visible = false;
                    litCompletedTasksCount.Visible = false;
                    litEmpty.Visible = true;
                }
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

        #endregion TaskLists management events

        /// <summary>
        /// Handles the ExcelExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ExportData1_ExcelExportClick(object sender, EventArgs e)
        {
            //Bind data and Export excel report
            ExportReport(ReportTypes.Excel);
        }

        /// <summary>
        /// Handles the PDFExportClick event of the ExportData1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ExportData1_PDFExportClick(object sender, EventArgs e)
        {
            ExportReport(ReportTypes.Pdf);
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
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            lblError.Visible = false;
            LoadTaskList();
            LoadActiveTasks();
            if (ProjectId > 0)
                LoadBreadCrumbs();
        }

        /// <summary>
        /// Load shopping lists
        /// </summary>
        private void LoadTaskList()
        {
            int ItemBriefTaskInprogressStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");
            int ItemBriefTaskCompletedTaskStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");

            var taskLists = this.GetBL<ItemBriefBL>().LoadCompleteAndInprogressTaskList(ProjectId, projectItemTypes.SelectedItemTypeId);

            listViewTaskLists.DataSource = taskLists;
            listViewTaskLists.DataBind();
        }

        /// <summary>
        /// Loads the active tasks.
        /// </summary>
        private void LoadActiveTasks()
        {
            int ItemBriefTaskStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");

            var tasks = this.GetBL<ItemBriefBL>().LoadActiveTasks(ProjectId, projectItemTypes.SelectedItemTypeId);

            gridviewTaskList.DataSource = tasks;
            gridviewTaskList.DataBind();

            if (!(tasks.Count() > 0))
            {
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

            //bc.AddLink(string.Concat(itemTypeName, " List"), string.Format("~/ItemBrief/ItemBriefList.aspx?ProjectId={0}&ItemTypeId={1}", ProjectId, projectItemTypes.ItemTypeId));
            bc.AddLink("Task Manager", null);
            bc.LoadControl();
        }

        #region Export Report

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            string cultureName = string.Empty;

            Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);
            if (project != null)
            {
                cultureName = Support.GetCultureName(project.Country.CountryCode);

                string fileNameExtension;
                string encoding;
                string mimeType;
                string fileName = string.Format("{0}_Active_TaskList", project.ProjectName);

                ActiveTaskListReportParameters parameters = new ActiveTaskListReportParameters
                {
                    CultureName = cultureName,
                    ItemTypeId = projectItemTypes.SelectedItemTypeId,
                    ProjectId = ProjectId,
                    UserId = this.UserID
                };

                byte[] reportBytes = UserWebReportHandler.GenerateActiveTaskListReport(parameters, exportType,
                        out fileNameExtension, out encoding, out mimeType);
                Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
            }
        }

        #endregion Export Report

        #endregion Private Methods
    }
}