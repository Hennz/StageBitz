using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for Project Events
    /// </summary>
    public partial class ProjectEvents : UserControlBase
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="ProjectEvents"/> is isvalid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if isvalid; otherwise, <c>false</c>.
        /// </value>
        public bool Isvalid
        {
            get
            {
                return Page.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the event list.
        /// </summary>
        /// <value>
        /// The event list.
        /// </value>
        public List<ProjectEvent> EventList
        {
            get
            {
                if (ViewState["EventList"] == null)
                {
                    ViewState["EventList"] = new List<ProjectEvent>();
                }
                return (List<ProjectEvent>)ViewState["EventList"];
            }
            set
            {
                ViewState["EventList"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }
                return (int)ViewState["ProjectID"];
            }
            set
            {
                ViewState["ProjectID"] = value;
            }
        }

        /// <summary>
        /// Gets the project start date.
        /// </summary>
        /// <value>
        /// The project start date.
        /// </value>
        public DateTime ProjectStartDate
        {
            get
            {
                if (StartDate.SelectedDate != null)
                {
                    return (DateTime)StartDate.SelectedDate;
                }
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the project end date.
        /// </summary>
        /// <value>
        /// The project end date.
        /// </value>
        public DateTime ProjectEndDate
        {
            get
            {
                if (EndDate.SelectedDate != null)
                {
                    return (DateTime)EndDate.SelectedDate;
                }
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets or sets the height of the events grid.
        /// </summary>
        /// <value>
        /// The height of the events grid.
        /// </value>
        public int EventsGridHeight
        {
            get
            {
                if (ViewState["EventsGridHeight"] == null)
                {
                    ViewState["EventsGridHeight"] = 120;
                }
                return (int)ViewState["EventsGridHeight"];
            }
            set
            {
                ViewState["EventsGridHeight"] = value;
            }
        }

        #endregion Public Properties

        #region Public methods

        /// <summary>
        /// Adds the events to context.
        /// </summary>
        public void AddEventsToContext()
        {
            foreach (ProjectEvent projectEvent in EventList)
            {
                DataContext.ProjectEvents.AddObject(projectEvent);
            }
        }

        /// <summary>
        /// Displays the event date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public string DisplayEventDate(string date)
        {
            if (date == string.Empty)
            {
                return "TBC";
            }
            return date;
        }

        /// <summary>
        /// Sets the length of the events grid.
        /// </summary>
        /// <param name="width">The width.</param>
        public void SetEventsGridLength(int width)
        {
            gvEvents.Width = width;
            gvEvents.MasterTableView.Width = width;
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            bool isReadOnlyRightsForProject;

            if (ProjectID > 0)
            {
                //Load the all the events
                var eventList = from pe in DataContext.ProjectEvents
                                join proj in DataContext.Projects on pe.ProjectId equals proj.ProjectId
                                where pe.ProjectId == ProjectID && proj.IsActive == true
                                select pe;

                gvEvents.DataSource = eventList;

                isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);
            }
            else
            {
                gvEvents.DataSource = EventList;//Assign the ViewState ProjectEvents list.
                isReadOnlyRightsForProject = false;
            }

            #region Set ReadOnly Moode

            gvEvents.Columns[2].Visible = !isReadOnlyRightsForProject;//Edit
            gvEvents.Columns[3].Visible = !isReadOnlyRightsForProject;//Delete
            txtEvent.ReadOnly = isReadOnlyRightsForProject;
            btnAddKeyDate.Enabled = !isReadOnlyRightsForProject;
            StartDate.Enabled = !isReadOnlyRightsForProject;
            EndDate.Enabled = !isReadOnlyRightsForProject;
            EventDate.Enabled = !isReadOnlyRightsForProject;

            #endregion Set ReadOnly Moode
        }

        /// <summary>
        /// Creates the notification.
        /// </summary>
        /// <param name="operationTypeCodeId">The operation type code identifier.</param>
        /// <param name="notification">The notification.</param>
        /// <returns></returns>
        private Notification CreateNotification(int operationTypeCodeId, string notification)
        {
            Notification nf = new Notification();
            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "SCHEDULE");
            nf.OperationTypeCodeId = operationTypeCodeId;
            nf.RelatedId = ProjectID;
            nf.ProjectId = ProjectID;
            nf.Message = notification;
            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
            nf.CreatedDate = nf.LastUpdatedDate = Now;
            return nf;
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
            EventDate.DateInput.Enabled = true;
            if (!IsPostBack)
            {
                gvEvents.ClientSettings.Scrolling.ScrollHeight = EventsGridHeight;
                //Load project details
                var project = (from p in DataContext.Projects
                               where p.ProjectId == ProjectID && p.IsActive == true
                               select p).FirstOrDefault<StageBitz.Data.Project>();

                if (project != null)
                {
                    if (project.StartDate.HasValue)
                    {
                        StartDate.SelectedDate = project.StartDate;
                    }

                    //Drop down has the value "Select Date".
                    if (project.EndDate.HasValue)
                    {
                        EndDate.SelectedDate = project.EndDate;
                    }
                    divEditScheduleMsg.Visible = true;
                }

                btnAddKeyDate.Attributes["disabled"] = "disabled";
                LoadData();
            }
        }

        /// <summary>
        /// Handles the ItemDeleted event of the gvEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvEvents_ItemDeleted(object sender, GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //Get the GridDataItem of the RadGrid
                GridDataItem item = (GridDataItem)e.Item;
                //ProjectID = 0 means, there is no project being created.So we only deal with the EventList item list in viewstate.
                if (ProjectID == 0)
                {
                    EventList.RemoveAt(item.ItemIndex);
                }
                else
                {
                    //Get the primary key value using the DataKeyValue.
                    int projectEventId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["ProjectEventId"];
                    DataContext.DeleteObject(DataContext.ProjectEvents.First(pe => pe.ProjectEventId == projectEventId));

                    #region Project Notification

                    DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "DELETE"), string.Format("{0} deleted a Project Schedule.", Support.UserFullName)));

                    #endregion Project Notification

                    DataContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Handles the UpdateCommand event of the gvEvents control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvEvents_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (Page.IsValid)
                {
                    //Get the GridEditableItem of the RadGrid
                    GridEditableItem editedItem = e.Item as GridEditableItem;
                    TextBox tbEventName = (TextBox)editedItem.FindControl("tbEventName");
                    RadDatePicker tbEventDate = (RadDatePicker)editedItem.FindControl("tbEventDate");

                    if (ProjectID == 0) //Update viewState
                    {
                        ProjectEvent projectEvent = EventList[editedItem.ItemIndex];
                        projectEvent.EventName = tbEventName.Text.Trim();
                        projectEvent.LastUpdatedByUserId = UserID;
                        projectEvent.LastUpdatedDate = Now;
                        projectEvent.EventDate = tbEventDate.SelectedDate;
                    }
                    else
                    {
                        //Get the primary key value using the DataKeyValue.
                        int projectEventId = (int)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["ProjectEventId"];
                        DateTime originalLastUpdatedDate = (DateTime)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["LastUpdatedDate"];

                        ProjectEvent projectEvent = (from pe in DataContext.ProjectEvents
                                                     where pe.ProjectEventId == projectEventId && pe.LastUpdatedDate == originalLastUpdatedDate
                                                     select pe).FirstOrDefault();

                        if (projectEvent == null)
                        {
                            StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ProjectDetails, ProjectID));
                        }

                        //Create Notification for edit project events

                        #region Project Notification

                        if (projectEvent.EventName != tbEventName.Text || projectEvent.EventDate != tbEventDate.SelectedDate)
                        {
                            #region Project Notification

                            DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} edited a Project Schedule.", Support.UserFullName)));

                            #endregion Project Notification
                        }

                        #endregion Project Notification

                        projectEvent.EventName = tbEventName.Text;
                        projectEvent.EventDate = tbEventDate.SelectedDate;
                        projectEvent.LastUpdatedByUserId = UserID;
                        projectEvent.LastUpdatedDate = Now;

                        DataContext.SaveChanges();
                    }
                    gvEvents.EditIndexes.Clear();
                    gvEvents.MasterTableView.IsItemInserted = false;
                    gvEvents.Rebind();
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvEvents control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvEvents_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            //No need to bind the data set to grid.Only set the data source to grid.
            //OnNeedDataSource will autometically rebind the grid
            LoadData();
        }

        /// <summary>
        /// Adds to event grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddToEventGrid(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (Page.IsValid)
                {
                    ProjectEvent projectEvent = new ProjectEvent();
                    projectEvent.EventName = txtEvent.Text.Trim();

                    if (EventDate.SelectedDate.HasValue)
                    {
                        projectEvent.EventDate = EventDate.SelectedDate;
                    }

                    projectEvent.CreatedByUserId = UserID;
                    projectEvent.CreatedDate = Now;
                    projectEvent.LastUpdatedByUserId = UserID;
                    projectEvent.LastUpdatedDate = Now;

                    if (ProjectID != 0)
                    {
                        #region Project Notification

                        DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "ADD"), string.Format("{0} added a Project Schedule.", Support.UserFullName)));

                        #endregion Project Notification

                        projectEvent.ProjectId = ProjectID;
                        DataContext.ProjectEvents.AddObject(projectEvent);
                        DataContext.SaveChanges();
                    }
                    else
                    {
                        //Will be used from AddNewProject screen.So Saving will not be done.Instead add to the viewState
                        EventList.Add(projectEvent);
                    }
                    LoadData();
                    btnAddKeyDate.Attributes["disabled"] = "disabled";
                    txtEvent.Text = string.Empty;
                    EventDate.Clear();
                    EventDate.DateInput.Clear();
                    gvEvents.DataBind();
                    upnl.Update();
                }

                txtEvent.Focus();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvEvents_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ProjectEvent projectEvent = (ProjectEvent)dataItem.DataItem;
                TextBox txtEventName = (TextBox)dataItem.FindControl("tbEventName");
                txtEventName.Text = projectEvent.EventName;
            }
            else if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ProjectEvent projectEvent = (ProjectEvent)dataItem.DataItem;

                //EventName
                if (ProjectID == 0)
                {
                    dataItem["EventName"].Text = Support.TruncateString(projectEvent.EventName, 20);
                    if (projectEvent.EventName.Length > 20)
                    {
                        dataItem["EventName"].ToolTip = projectEvent.EventName;
                    }
                }
                else
                {
                    dataItem["EventName"].Text = Support.TruncateString(projectEvent.EventName, 80);
                    if (projectEvent.EventName != null && projectEvent.EventName.Length > 80)
                    {
                        dataItem["EventName"].ToolTip = projectEvent.EventName;
                    }
                }
            }
        }

        #endregion Event Handlers
    }
}