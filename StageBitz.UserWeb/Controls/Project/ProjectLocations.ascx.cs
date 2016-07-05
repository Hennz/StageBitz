using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// Delegate for inform parent to The update location tab count
    /// </summary>
    /// <param name="count">The count.</param>
    public delegate void UpdateLocationTabCount(int count);

    /// <summary>
    /// Project locations user control.
    /// </summary>
    public partial class ProjectLocations : UserControlBase
    {
        #region Events

        /// <summary>
        /// Inform parent to The update location tab count
        /// </summary>
        public UpdateLocationTabCount UpdateLocationTabCount;

        #endregion Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the location list.
        /// </summary>
        /// <value>
        /// The location list.
        /// </value>
        public List<ProjectLocation> LocationList
        {
            get
            {
                if (ViewState["LocationList"] == null)
                {
                    ViewState["LocationList"] = new List<ProjectLocation>();
                }
                return (List<ProjectLocation>)ViewState["LocationList"];
            }
            set
            {
                ViewState["LocationList"] = value;
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
        /// Gets or sets the height of the location grid.
        /// </summary>
        /// <value>
        /// The height of the location grid.
        /// </value>
        public int LocationGridHeight
        {
            get
            {
                if (ViewState["LocationGridHeight"] == null)
                {
                    ViewState["LocationGridHeight"] = 120;
                }
                return (int)ViewState["LocationGridHeight"];
            }
            set
            {
                ViewState["LocationGridHeight"] = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            bool isReadOnlyRightsForProject;

            if (ProjectID > 0)
            {
                var locationList = from pl in DataContext.ProjectLocations
                                   join proj in DataContext.Projects on pl.ProjectId equals proj.ProjectId
                                   where pl.ProjectId == ProjectID && proj.IsActive == true
                                   orderby pl.Name
                                   select pl;

                gvLocations.DataSource = locationList;

                isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);

                helpTipProjectDetailsLocations.Visible = true;

                if (UpdateLocationTabCount != null)
                {
                    UpdateLocationTabCount(locationList.Count());
                }
            }
            else
            {
                helpTipProjectDetailsLocations.Visible = false;
                gvLocations.DataSource = LocationList;
                isReadOnlyRightsForProject = false;
            }

            gvLocations.Columns[2].Visible = !isReadOnlyRightsForProject;//Edit
            gvLocations.Columns[3].Visible = !isReadOnlyRightsForProject;//Delete
            txtLocation.ReadOnly = isReadOnlyRightsForProject;
            txtName.ReadOnly = isReadOnlyRightsForProject;
            btnAddLocation.Enabled = !isReadOnlyRightsForProject;
        }

        /// <summary>
        /// Adds the locations to context.
        /// </summary>
        public void AddLocationsToContext()
        {
            foreach (ProjectLocation projectLocation in LocationList)
            {
                DataContext.ProjectLocations.AddObject(projectLocation);
            }
        }

        /// <summary>
        /// Sets the length of the locations grid.
        /// </summary>
        /// <param name="width">The width.</param>
        public void SetLocationsGridLength(int width)
        {
            gvLocations.Width = width;
            gvLocations.MasterTableView.Width = width;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Creates the notification.
        /// </summary>
        /// <param name="operationTypeCodeId">The operation type code identifier.</param>
        /// <param name="notification">The notification.</param>
        /// <returns></returns>
        private Notification CreateNotification(int operationTypeCodeId, string notification)
        {
            Notification nf = new Notification();
            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJECT");
            nf.OperationTypeCodeId = operationTypeCodeId;
            nf.RelatedId = ProjectID;
            nf.ProjectId = ProjectID;
            nf.Message = notification;
            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
            nf.CreatedDate = nf.LastUpdatedDate = Now;
            return nf;
        }

        #endregion Private Methods

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
                gvLocations.ClientSettings.Scrolling.ScrollHeight = LocationGridHeight;
                LoadData();
            }
        }

        /// <summary>
        /// Handles the ItemDeleted event of the gvLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvLocations_ItemDeleted(object sender, GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //Get the GridDataItem of the RadGrid
                GridDataItem item = (GridDataItem)e.Item;
                //ProjectID = 0 means, there is no project being created.So we only deal with the EventList item list in viewstate.
                if (ProjectID == 0)
                {
                    LocationList.RemoveAt(item.ItemIndex);
                }
                else
                {
                    //Get the primary key value using the DataKeyValue.
                    int projectLocationId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["ProjectLocationId"];

                    #region Project Notification

                    DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "DELETE"), string.Format("{0} deleted a Project Location.", Support.UserFullName)));

                    #endregion Project Notification

                    DataContext.DeleteObject(DataContext.ProjectLocations.First(pl => pl.ProjectLocationId == projectLocationId));
                    DataContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvLocations_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ProjectLocation projectLocation = (ProjectLocation)dataItem.DataItem;

                TextBox txtName = (TextBox)dataItem.FindControl("tbName");
                TextBox txtLocation = (TextBox)dataItem.FindControl("tbLocation");
                txtName.Text = projectLocation.Name;
                txtLocation.Text = projectLocation.Location;
            }
            else if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ProjectLocation projectLocation = (ProjectLocation)dataItem.DataItem;

                //Name
                if (ProjectID != 0)
                {
                    dataItem["Name"].Text = Support.TruncateString(projectLocation.Name, 30);
                    if (projectLocation.Name.Length > 30)
                    {
                        dataItem["Name"].ToolTip = projectLocation.Name;
                    }
                }
                else
                {
                    dataItem["Name"].Text = Support.TruncateString(projectLocation.Name, 20);
                    if (projectLocation.Name.Length > 20)
                    {
                        dataItem["Name"].ToolTip = projectLocation.Name;
                    }
                }

                //Location
                dataItem["Location"].Text = Support.TruncateString(projectLocation.Location, 30);

                if (projectLocation.Location.Length > 30)
                {
                    dataItem["Location"].ToolTip = projectLocation.Location;
                }
            }
        }

        /// <summary>
        /// Handles the UpdateCommand event of the gvLocations control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvLocations_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing && Page.IsValid)
            {
                //Get the GridEditableItem of the RadGrid
                GridEditableItem editedItem = e.Item as GridEditableItem;

                //string name = (editedItem["Name"].Controls[0] as TextBox).Text;
                //string location = (editedItem["Location"].Controls[0] as TextBox).Text;

                TextBox name = (TextBox)editedItem.FindControl("tbName");
                TextBox location = (TextBox)editedItem.FindControl("tbLocation");

                if (name.Text.Trim() != string.Empty || location.Text.Trim() != string.Empty)
                {
                    if (ProjectID == 0)
                    {
                        ProjectLocation projectLocation = LocationList[editedItem.ItemIndex];
                        projectLocation.Location = location.Text.Trim();
                        projectLocation.Name = name.Text.Trim();
                        projectLocation.LastUpdatedByUserId = UserID;
                        projectLocation.LastUpdatedDate = Now;
                    }
                    else
                    {
                        int projectLocationID = (int)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["ProjectLocationId"];
                        DateTime originalLastUpdatedDate = (DateTime)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["LastUpdatedDate"];

                        ProjectLocation projectLocation = (from pl in DataContext.ProjectLocations
                                                           where pl.ProjectLocationId == projectLocationID && pl.LastUpdatedDate == originalLastUpdatedDate
                                                           select pl).FirstOrDefault();

                        if (projectLocation == null)
                        {
                            StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ProjectDetails, ProjectID));
                        }

                        #region Project Notification

                        if (projectLocation.Location != location.Text.Trim() || projectLocation.Name != name.Text.Trim())
                        {
                            DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} edited a Project Location.", Support.UserFullName)));
                        }

                        #endregion Project Notification

                        projectLocation.Location = location.Text.Trim();
                        projectLocation.Name = name.Text.Trim();
                        projectLocation.LastUpdatedByUserId = UserID;
                        projectLocation.LastUpdatedDate = Now;
                        DataContext.SaveChanges();
                    }
                    gvLocations.EditIndexes.Clear();
                    gvLocations.MasterTableView.IsItemInserted = false;
                    gvLocations.Rebind();
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvLocations control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvLocations_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            //No need to bind the data set to grid.Only set the data source to grid.
            //OnNeedDataSource will autometically rebind the grid
            LoadData();
        }

        /// <summary>
        /// Adds to location grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddToLocationGrid(object sender, EventArgs e)
        {
            if (Page.IsValid && !PageBase.StopProcessing)
            {
                ProjectLocation projectLocation = new ProjectLocation();
                projectLocation.Location = txtLocation.Text.Trim();
                projectLocation.Name = txtName.Text.Trim();

                projectLocation.CreatedByUserId = UserID;
                projectLocation.CreatedDate = Now;
                projectLocation.LastUpdatedByUserId = UserID;
                projectLocation.LastUpdatedDate = Now;

                if (ProjectID != 0)
                {
                    #region Project Notification

                    DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "ADD"), string.Format("{0} added a Project Location.", Support.UserFullName)));

                    #endregion Project Notification

                    projectLocation.ProjectId = ProjectID;
                    DataContext.ProjectLocations.AddObject(projectLocation);
                    DataContext.SaveChanges();
                }
                else
                {
                    //Will be used from AddNewProject screen.So Saving will not be done.Instead add to the viewState
                    LocationList.Add(projectLocation);
                }
                LoadData();
                btnAddLocation.Enabled = false;
                txtLocation.Text = string.Empty;
                txtName.Text = string.Empty;
                gvLocations.DataBind();
                txtName.Focus();
            }
        }

        #endregion Event Handlers
    }
}