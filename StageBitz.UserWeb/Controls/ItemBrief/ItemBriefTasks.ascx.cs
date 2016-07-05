using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.ItemBrief
{
    /// <summary>
    /// Delegate for inform item brief detail
    /// </summary>
    /// <param name="itembriefStatusCodeID">The itembrief status code identifier.</param>
    /// <param name="swtichToCompletedTab">if set to <c>true</c> [swtich to completed tab].</param>
    public delegate void InformItemBriefDetail(int itembriefStatusCodeID, bool swtichToCompletedTab);

    /// <summary>
    /// Delegate for inform item brief detail to show budget
    /// </summary>
    public delegate void InformItemBriefDetailToShowBudget();

    /// <summary>
    /// Delegate for update task count in Item brief details page.
    /// </summary>
    /// <param name="count">The count.</param>
    public delegate void UpdateTaskCount(int count);

    /// <summary>
    /// User control for item brief tasks
    /// </summary>
    public partial class ItemBriefTasks : UserControlBase
    {
        #region Private variables

        /// <summary>
        /// The code complete var
        /// </summary>
        private Code codeComplete = Support.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED");

        /// <summary>
        /// The code inprogress var
        /// </summary>
        private Code codeInprogress = Support.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS");

        #endregion Private variables

        #region Enums

        /// <summary>
        /// Enum for view mode
        /// </summary>
        public enum ViewMode { ItemBriefTaskListView, TaskListView }

        #endregion Enums

        #region Events

        /// <summary>
        /// The inform item brief detail
        /// </summary>
        public InformItemBriefDetail InformItemBriefDetail;

        /// <summary>
        /// The inform item brief detail to show budget
        /// </summary>
        public InformItemBriefDetailToShowBudget InformItemBriefDetailToShowBudget;

        /// <summary>
        /// The update task count in Item brief details page.
        /// </summary>
        public UpdateTaskCount UpdateTaskCount;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the last update date for update task.
        /// </summary>
        /// <value>
        /// The last update date for update task.
        /// </value>
        private DateTime LastUpdateDateForUpdateTask
        {
            get
            {
                if (ViewState["LastUpdateDateForUpdateTask"] == null)
                {
                    ViewState["LastUpdateDateForUpdateTask"] = Now;
                }

                return (DateTime)ViewState["LastUpdateDateForUpdateTask"];
            }
            set
            {
                ViewState["LastUpdateDateForUpdateTask"] = value;
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
                    ViewState["DisplayMode"] = ViewMode.ItemBriefTaskListView; //Default view
                }

                return (ViewMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item brief identifier.
        /// </summary>
        /// <value>
        /// The item brief identifier.
        /// </value>
        public int ItemBriefID
        {
            get
            {
                if (ViewState["ItemBriefID"] == null)
                {
                    ViewState["ItemBriefID"] = 0;
                }
                return (int)ViewState["ItemBriefID"];
            }
            set
            {
                ViewState["ItemBriefID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the task list identifier.
        /// </summary>
        /// <value>
        /// The task list identifier.
        /// </value>
        public int TaskListId
        {
            get
            {
                if (ViewState["TaskListId"] == null)
                {
                    ViewState["TaskListId"] = 0;
                }

                return (int)ViewState["TaskListId"];
            }
            set
            {
                ViewState["TaskListId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only rights for project.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only rights for project; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnlyRightsForProject
        {
            get
            {
                if (ViewState["IsReadOnlyRightsForProject"] == null)
                {
                    ViewState["IsReadOnlyRightsForProject"] = false;
                }

                return (bool)ViewState["IsReadOnlyRightsForProject"];
            }
            set
            {
                ViewState["IsReadOnlyRightsForProject"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control has empty data.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control has empty data; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmptyData
        {
            get
            {
                if (ViewState["IsEmptyData"] == null)
                {
                    ViewState["IsEmptyData"] = false;
                }

                return (bool)ViewState["IsEmptyData"];
            }
            set
            {
                ViewState["IsEmptyData"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the task list sort expression.
        /// </summary>
        /// <value>
        /// The task list sort expression.
        /// </value>
        public string TaskListSortExpression
        {
            get
            {
                if (ViewState["TaskListSortExpression"] == null)
                {
                    ViewState["TaskListSortExpression"] = "Status ASC";
                }

                return ViewState["TaskListSortExpression"].ToString();
            }
            set
            {
                ViewState["TaskListSortExpression"] = value;
            }
        }

        #endregion Properties

        #region Private methods

        /// <summary>
        /// Loads the tasks.
        /// </summary>
        private void LoadTasks()
        {
            switch (DisplayMode)
            {
                case ViewMode.ItemBriefTaskListView:
                    List<ItemBriefTaskDetails> itemBriefTasks = this.GetBL<ItemBriefBL>().GetItemBriefTasks(ItemBriefID);
                    gvTasks.DataSource = itemBriefTasks;
                    if (UpdateTaskCount != null)
                    {
                        //Inform parent to show the count of the task list
                        UpdateTaskCount(this.GetBL<ItemBriefBL>().GetItemBriefIncomplteTasksCount(itemBriefTasks));
                    }
                    break;

                case ViewMode.TaskListView:
                    List<ItemBriefTaskDetails> tasksList = this.GetBL<ProjectBL>().GetTaskList(TaskListId);
                    gvTasks.DataSource = tasksList;
                    break;

                default:
                    //NO DATA BINDING
                    break;
            }
        }

        /// <summary>
        /// Gets the item brief task.
        /// </summary>
        /// <param name="itemBriefTaskId">The item brief task identifier.</param>
        /// <param name="originalLastUpdatedDate">The original last updated date.</param>
        /// <returns></returns>
        private ItemBriefTask GetItemBriefTask(int itemBriefTaskId, DateTime originalLastUpdatedDate)
        {
            ItemBriefTask itemBriefTask = this.GetBL<ItemBriefBL>().GetItemBriefTask(itemBriefTaskId, originalLastUpdatedDate);
            if (itemBriefTask == null)
            {
                StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ItemBriefTasks, ProjectId));
            }
            return itemBriefTask;
        }

        /// <summary>
        /// Gets the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        private StageBitz.Data.ItemBrief GetItemBrief(int itemBriefId)
        {
            return this.GetBL<ItemBriefBL>().GetItemBrief(itemBriefId);
        }

        /// <summary>
        /// Sets the tooltip for status.
        /// </summary>
        /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
        /// <returns></returns>
        private string SetTooltipForStatus(bool isChecked)
        {
            if (isChecked)
                return codeComplete.Description;
            else
                return codeInprogress.Description;
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
                if (DisplayMode == ViewMode.ItemBriefTaskListView && ItemBriefID == 0)
                {
                    return;
                }

                //Page level security
                Data.Project project = this.GetBL<ProjectBL>().GetProject(ProjectId);
                CultureName = Support.GetCultureName(project.Country.CountryCode);

                var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name == CultureName).FirstOrDefault();
                txtEstimatedCost.Culture = txtNetCost.Culture = txtTax.Culture = txtTotalCost.Culture = cultureInfo;

                LoadTasks();
                IsReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectId);
                btnAddTask.Enabled = !IsReadOnlyRightsForProject;
                txtDescription.ReadOnly = IsReadOnlyRightsForProject;
                txtEstimatedCost.ReadOnly = IsReadOnlyRightsForProject;

                //If Itembrief status is Complete, hide the btnCompleteItemBrief button
                switch (DisplayMode)
                {
                    case ViewMode.ItemBriefTaskListView:

                        //Delete button tool tip
                        ((GridButtonColumn)gvTasks.MasterTableView.GetColumnSafe("DeleteColumn")).ConfirmText = "Are you sure you want to delete this task?";

                        break;

                    case ViewMode.TaskListView:

                        pnlAddTask.Visible = false;
                        //popupEditTask.Visible = false;
                        foreach (GridColumn col in gvTasks.MasterTableView.RenderColumns)
                        {
                            if (col.UniqueName == "ItemName")
                            {
                                col.Visible = true;
                            }
                        }
                        ((GridButtonColumn)gvTasks.MasterTableView.GetColumnSafe("DeleteColumn")).ConfirmText = "Are you sure you want to remove this task from the task list?";
                        break;

                    default:
                        break;
                }

                //Security - Hide editing deleting for Observers
                if (IsReadOnlyRightsForProject)
                {
                    foreach (GridColumn col in gvTasks.MasterTableView.RenderColumns)
                    {
                        if (col.UniqueName == "EditCommandColumn")
                        {
                            col.Visible = false;
                        }
                        else if (col.UniqueName == "DeleteColumn")
                        {
                            col.Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvTasks control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvTasks_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadTasks();
            //Get the sorting expression
            if (gvTasks.MasterTableView.SortExpressions.GetSortString() != string.Empty)
            {
                TaskListSortExpression = gvTasks.MasterTableView.SortExpressions.GetSortString();
            }
        }

        /// <summary>
        /// Handles the ItemDeleted event of the gvTasks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvTasks_ItemDeleted(object sender, GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //Get the GridDataItem of the RadGrid
                GridDataItem item = (GridDataItem)e.Item;
                //Get the primary key value using the DataKeyValue.
                int itemBriefTaskId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["ItemBriefTask.ItemBriefTaskId"];

                switch (DisplayMode)
                {
                    case ViewMode.ItemBriefTaskListView:
                        ItemBriefTask itemBriefTask = this.GetBL<ItemBriefBL>().GetItemBriefTask(itemBriefTaskId);
                        if (itemBriefTask != null)
                        {
                            this.GetBL<ItemBriefBL>().DeleteItemBriefTask(itemBriefTask);

                            #region Generate Task Notification

                            Notification nf = new Notification();
                            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "TASK");
                            nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "DELETE");
                            nf.RelatedId = itemBriefTask.ItemBriefId;
                            nf.ProjectId = ProjectId;
                            nf.Message = string.Format("{0} removed the task - '{1}'.", Support.UserFullName, itemBriefTask.Description);
                            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                            nf.CreatedDate = nf.LastUpdatedDate = Now;

                            this.GetBL<NotificationBL>().AddNotification(nf);

                            #endregion Generate Task Notification

                            if (InformItemBriefDetailToShowBudget != null)
                            {
                                InformItemBriefDetailToShowBudget();
                            }
                        }
                        break;

                    case ViewMode.TaskListView:
                        this.GetBL<ProjectBL>().RemoveTaskFromList(itemBriefTaskId, TaskListId);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the UpdateCommand event of the gvTasks control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvTasks_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (Page.IsValid && !PageBase.StopProcessing)
            {
                //Get the GridEditableItem of the RadGrid
                GridEditableItem editedItem = e.Item as GridEditableItem;

                //Get the primary key value using the DataKeyValue.
                int itemBriefTaskId = (int)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["ItemBriefTask.ItemBriefTaskId"];
                DateTime originalLastUpdatedDate = (DateTime)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["ItemBriefTask.LastUpdatedDate"];

                //Access the textbox from the edit form template and store the values in string variables.
                TextBox description = (TextBox)editedItem.FindControl("tbDescription");
                TextBox vendor = (TextBox)editedItem.FindControl("tbVendor");
                RadNumericTextBox estimatedCost = (RadNumericTextBox)editedItem.FindControl("tbEstimatedCost");
                RadNumericTextBox netCost = (RadNumericTextBox)editedItem.FindControl("tbNetCost");
                RadNumericTextBox tax = (RadNumericTextBox)editedItem.FindControl("tbTax");
                RadNumericTextBox total = (RadNumericTextBox)editedItem.FindControl("tbTotal");

                ItemBriefTask itemBriefTask = GetItemBriefTask(itemBriefTaskId, originalLastUpdatedDate);

                itemBriefTask.Description = description.Text.Trim();
                itemBriefTask.Vendor = vendor.Text.Trim();

                if (estimatedCost.Value != null)
                {
                    itemBriefTask.EstimatedCost = (decimal)estimatedCost.Value;
                }
                else
                    itemBriefTask.EstimatedCost = null;

                if (netCost.Value != null)
                {
                    itemBriefTask.NetCost = (decimal)netCost.Value;
                }
                else
                {
                    itemBriefTask.NetCost = 0;
                }

                if (tax.Value != null)
                {
                    itemBriefTask.Tax = (decimal)tax.Value;
                }
                else
                {
                    itemBriefTask.Tax = 0;
                }

                if (total.Value != null)
                {
                    itemBriefTask.TotalCost = (decimal)total.Value;
                }
                else
                {
                    itemBriefTask.TotalCost = 0;
                }

                itemBriefTask.LastUpdatedDate = Now;
                itemBriefTask.LastUpdatedByUserId = UserID;
                DataContext.SaveChanges();
                up.Update();
                gvTasks.EditIndexes.Clear();
                gvTasks.MasterTableView.IsItemInserted = false;
                gvTasks.Rebind();
                if (InformItemBriefDetailToShowBudget != null)
                {
                    InformItemBriefDetailToShowBudget();
                }
            }
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddTask(object sender, EventArgs e)
        {
            if (Page.IsValid && !PageBase.StopProcessing)
            {
                #region Create new task object

                ItemBriefTask itemBriefTask = new ItemBriefTask();
                itemBriefTask.Description = txtDescription.Text.Trim();
                if (txtEstimatedCost.Value.HasValue)
                    itemBriefTask.EstimatedCost = (decimal)txtEstimatedCost.Value;
                else
                    itemBriefTask.EstimatedCost = null;
                itemBriefTask.Tax = 0;
                itemBriefTask.NetCost = 0;
                itemBriefTask.TotalCost = 0;
                itemBriefTask.ItemBriefId = ItemBriefID;
                itemBriefTask.ItemBriefTaskStatusCodeId = codeInprogress.CodeId;
                itemBriefTask.CreatedByUserId = UserID;
                itemBriefTask.CreatedDate = Now;
                itemBriefTask.LastUpdatedByUserId = UserID;
                itemBriefTask.LastUpdatedDate = Now;
                DataContext.ItemBriefTasks.AddObject(itemBriefTask);

                #endregion Create new task object

                #region Generate Task Add Notification

                Notification nfTask = new Notification();
                nfTask.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "TASK");
                nfTask.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "ADD");
                nfTask.RelatedId = itemBriefTask.ItemBriefId;
                nfTask.ProjectId = ProjectId;
                nfTask.Message = string.Format("{0} added a task - '{1}'.", Support.UserFullName, itemBriefTask.Description);
                nfTask.CreatedByUserId = nfTask.LastUpdatedByUserId = UserID;
                nfTask.CreatedDate = nfTask.LastUpdatedDate = Now;

                DataContext.Notifications.AddObject(nfTask);

                #endregion Generate Task Add Notification

                #region Update Item Brief status

                StageBitz.Data.ItemBrief itemBrief = GetItemBrief(ItemBriefID);

                //Mark the item brief as 'In Progress' if it is not in progress.
                int ibNotStartedCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");
                int ibInProgressCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");

                if (!GetBL<ItemBriefBL>().IsItemBriefComplete(itemBrief))
                {
                    if (itemBrief.ItemBriefStatusCodeId != ibInProgressCodeId)
                    {
                        string oldStatus = (itemBrief.ItemBriefStatusCodeId == ibNotStartedCodeId) ? "Not Started" : "Completed";

                        itemBrief.ItemBriefStatusCodeId = ibInProgressCodeId;
                        itemBrief.LastUpdatedByUserId = UserID;
                        //itemBrief.LastUpdatedDate = Now;
                        DataContext.SaveChanges();

                        #region Generate Item Brief Notification

                        Notification nfItemBrief = new Notification();
                        nfItemBrief.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
                        nfItemBrief.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "EDIT");
                        nfItemBrief.RelatedId = itemBrief.ItemBriefId;
                        nfItemBrief.ProjectId = ProjectId;
                        nfItemBrief.Message = string.Format("{0} changed the Item Brief status from '{1}' to 'In Progress'.", Support.UserFullName, oldStatus);
                        nfItemBrief.CreatedByUserId = nfItemBrief.LastUpdatedByUserId = UserID;
                        nfItemBrief.CreatedDate = nfItemBrief.LastUpdatedDate = Now;

                        DataContext.Notifications.AddObject(nfItemBrief);

                        #endregion Generate Item Brief Notification

                        if (InformItemBriefDetail != null)
                        {
                            //Inform parent to show the description as inprogress.
                            InformItemBriefDetail(ibInProgressCodeId, false);
                        }
                    }
                }

                #endregion Update Item Brief status

                DataContext.SaveChanges();

                LoadTasks();
                gvTasks.DataBind();
                txtDescription.Text = string.Empty;
                txtEstimatedCost.Text = string.Empty;
                txtDescription.Focus();
                if (InformItemBriefDetailToShowBudget != null)
                {
                    InformItemBriefDetailToShowBudget();
                }
            }
        }

        /// <summary>
        /// Handles the SortCommand event of the gvTasks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvTasks_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;
                gvTasks.MasterTableView.SortExpressions.AddSortExpression(sortExpr);
                e.Canceled = true;
                gvTasks.Rebind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvTasks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvTasks_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem || e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ItemBriefTask itemBriefTask = ((dynamic)(dataItem.DataItem)).ItemBriefTask;

                CheckBox chkStatus = (CheckBox)dataItem.FindControl("chkStatus");
                HyperLink hyperLinkItem = (HyperLink)dataItem.FindControl("hyperLinkItem");

                if (hyperLinkItem != null)
                {
                    hyperLinkItem.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}", itemBriefTask.ItemBriefId));
                    hyperLinkItem.Text = Support.TruncateString(itemBriefTask.ItemBrief.Name, 15);
                }

                if (IsReadOnlyRightsForProject)
                {
                    chkStatus.Enabled = false;
                }

                if (chkStatus != null)
                {
                    if (itemBriefTask.ItemBriefTaskStatusCodeId == codeComplete.CodeId)
                    {
                        chkStatus.Checked = true;
                        chkStatus.Enabled = false;
                    }
                    chkStatus.ToolTip = SetTooltipForStatus(chkStatus.Checked);
                }

                if (e.Item.ItemType == GridItemType.EditItem)
                {
                    TextBox txtDescription = (TextBox)dataItem.FindControl("tbDescription");
                    TextBox txtVendor = (TextBox)dataItem.FindControl("tbVendor");
                    RadNumericTextBox tbNetCost = (RadNumericTextBox)dataItem.FindControl("tbNetCost");

                    RadNumericTextBox tbTax = (RadNumericTextBox)dataItem.FindControl("tbTax");
                    RadNumericTextBox tbTotal = (RadNumericTextBox)dataItem.FindControl("tbTotal");
                    RadNumericTextBox tbEstimatedCost = (RadNumericTextBox)dataItem.FindControl("tbEstimatedCost");

                    tbNetCost.Culture = tbTax.Culture = tbTotal.Culture = tbEstimatedCost.Culture = new System.Globalization.CultureInfo(CultureName);

                    txtDescription.Text = itemBriefTask.Description;
                    txtVendor.Text = itemBriefTask.Vendor;
                    if (chkStatus != null)
                    {
                        chkStatus.Enabled = false;

                        if (itemBriefTask.ItemBriefTaskStatusCodeId == codeInprogress.CodeId)
                        {
                            tbNetCost.Enabled = false;
                            tbTax.Enabled = false;
                            tbTotal.Enabled = false;
                        }
                    }
                }
                else if (e.Item is GridDataItem)
                {
                    HtmlImage imgNoEstimatedCost = (HtmlImage)dataItem.FindControl("imgNoEstimatedCost");
                    imgNoEstimatedCost.Visible = (itemBriefTask.EstimatedCost == null && itemBriefTask.ItemBriefTaskStatusCodeId == Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId);

                    //Description
                    dataItem["Description"].Text = Support.TruncateString(itemBriefTask.Description, 20);
                    if (itemBriefTask.Description != null && itemBriefTask.Description.Length > 20)
                    {
                        dataItem["Description"].ToolTip = itemBriefTask.Description;
                    }

                    //Vendor
                    dataItem["Vendor"].Text = Support.TruncateString(itemBriefTask.Vendor, 10);

                    if (itemBriefTask.Vendor != null && itemBriefTask.Vendor.Length > 10)
                    {
                        dataItem["Vendor"].ToolTip = itemBriefTask.Vendor;
                    }
                }

                ImageButton DeleteBtn = (ImageButton)dataItem["DeleteColumn"].Controls[0];
                if (DisplayMode == ViewMode.TaskListView)
                {
                    DeleteBtn.ToolTip = "Remove task from task list";
                }
                else
                {
                    DeleteBtn.ToolTip = "Remove task";
                }
            }
        }

        /// <summary>
        /// Changes the task status.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ChangeTaskStatus(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                CheckBox checkBoxStatus = sender as CheckBox;
                GridDataItem row = checkBoxStatus.NamingContainer as GridDataItem;
                int itemBriefTaskId = (int)row.GetDataKeyValue("ItemBriefTask.ItemBriefTaskId");
                DateTime originalLastUpdatedDate = (DateTime)row.GetDataKeyValue("ItemBriefTask.LastUpdatedDate");
                LastUpdateDateForUpdateTask = originalLastUpdatedDate;
                ItemBriefTask itemBriefTask = GetItemBriefTask(itemBriefTaskId, originalLastUpdatedDate);

                if (checkBoxStatus.Checked)
                {
                    if (itemBriefTask != null)
                    {
                        txtVendor.Text = itemBriefTask.Vendor;
                        txtNetCost.Value = (double)itemBriefTask.NetCost;
                        txtTax.Value = (double)itemBriefTask.Tax;
                        txtTotalCost.Value = (double)itemBriefTask.TotalCost;
                        checkBoxStatus.Checked = false;
                        btnSave.CommandArgument = itemBriefTaskId.ToString();
                    }
                    popupEditTask.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Saves the popup.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SavePopup(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int itemBriefTaskId = int.Parse(btnSave.CommandArgument);
                ItemBriefTask itemBriefTask = GetItemBriefTask(itemBriefTaskId, LastUpdateDateForUpdateTask);

                if (itemBriefTask != null)
                {
                    itemBriefTask.Vendor = txtVendor.Text.Trim();

                    itemBriefTask.NetCost = (txtNetCost.Value != null) ? (decimal)txtNetCost.Value : 0;
                    itemBriefTask.Tax = (txtTax.Value != null) ? (decimal)txtTax.Value : 0;
                    itemBriefTask.TotalCost = (txtTotalCost.Value != null) ? (decimal)txtTotalCost.Value : 0;
                    itemBriefTask.ItemBriefTaskStatusCodeId = codeComplete.CodeId;
                    itemBriefTask.LastUpdatedByUserId = UserID;
                    itemBriefTask.LastUpdatedDate = LastUpdateDateForUpdateTask = Now;
                    itemBriefTask.CompletedDate = Now;

                    #region Generate Task Notification

                    Notification nf = new Notification();
                    nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "TASK");
                    nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "EDIT");
                    nf.RelatedId = itemBriefTask.ItemBriefId;
                    nf.ProjectId = ProjectId;
                    nf.Message = string.Format("{0} completed the task '{1}'.", Support.UserFullName, itemBriefTask.Description);
                    nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                    nf.CreatedDate = nf.LastUpdatedDate = Now;

                    this.GetBL<NotificationBL>().AddNotification(nf);

                    #endregion Generate Task Notification

                    LoadTasks();
                    gvTasks.DataBind();
                    if (InformItemBriefDetailToShowBudget != null)
                    {
                        InformItemBriefDetailToShowBudget();
                    }
                }
                popupEditTask.HidePopup();
            }
        }

        #endregion Event Handlers

        #region public methods

        /// <summary>
        /// Sets the width of the tasks grid.
        /// </summary>
        /// <param name="width">The width.</param>
        public void SetTasksGridWidth(int width)
        {
            gvTasks.Width = width;
            gvTasks.MasterTableView.Width = width;
        }

        #endregion public methods
    }
}