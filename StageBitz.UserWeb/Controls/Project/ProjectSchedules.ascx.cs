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
    /// User control for Project Schedule.
    /// </summary>
    public partial class ProjectSchedule : UserControlBase
    {
        #region Public Properties

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

        #endregion Public Properties

        private int left, middle, right = 0;

        #region Private Methods

        /// <summary>
        /// Configures the grids.
        /// </summary>
        /// <param name="projectEventList">The project event list.</param>
        private void ConfigureGrids(List<ProjectEvent> projectEventList)
        {
            switch (projectEventList.Count)
            {
                case 1:
                case 2:
                    left = projectEventList.Count;
                    break;

                case 3:
                    left = 2;
                    middle = 1;
                    break;

                case 4:
                    left = 2;
                    middle = 2;
                    break;

                case 5:
                    left = 2;
                    middle = 2;
                    right = 1;
                    break;

                case 6:
                    left = 2;
                    middle = 2;
                    right = 2;
                    break;

                case 7:
                    left = 3;
                    middle = 2;
                    right = 2;
                    break;

                case 8:
                    left = 3;
                    middle = 3;
                    right = 2;
                    break;

                default:
                    left = 3;
                    middle = 3;
                    right = 3;
                    break;
            }
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
                if (ProjectID > 0)
                {
                    //Load the project
                    StageBitz.Data.Project project = (from p in DataContext.Projects
                                                      where p.ProjectId == ProjectID && p.IsActive == true
                                                      select p).FirstOrDefault();

                    if (project != null)
                    {
                        if (project.StartDate == DateTime.MinValue || project.StartDate == null)
                        {
                            startDate.Text = "TBC";
                        }
                        else
                        {
                            startDate.Text = Support.FormatDate(project.StartDate);
                        }

                        if (project.EndDate == DateTime.MinValue || project.EndDate == null)
                        {
                            endDate.Text = "TBC";
                        }
                        else
                        {
                            endDate.Text = Support.FormatDate(project.EndDate);
                        }
                    }

                    //Load the top 10 events that are not being expired.
                    var eventList = (from pe in DataContext.ProjectEvents
                                     join proj in DataContext.Projects on pe.ProjectId equals proj.ProjectId
                                     where pe.ProjectId == ProjectID && pe.EventDate > Now && proj.IsActive == true
                                     orderby pe.EventDate
                                     select pe).Take(9).ToList<ProjectEvent>();

                    ConfigureGrids(eventList);

                    int eventListCount = eventList.Count();
                    if (eventListCount > 0)
                    {
                        if (left > 0)
                        {
                            gvEventsLeft.DataSource = eventList.Take(left);
                            gvEventsLeft.DataBind();
                        }

                        if (middle > 0)
                        {
                            gvMiddle.DataSource = eventList.Skip(left).Take(middle);
                            gvMiddle.DataBind();
                            divLeft.Style["border-right"] = "1px Solid Black";
                        }

                        if (right > 0)
                        {
                            gvRight.DataSource = eventList.Skip(left + middle).Take(right);
                            gvRight.DataBind();
                            divMiddle.Style["border-right"] = "1px Solid Black";
                        }

                        ////Assign top 5 to left grid
                        //gvEventsLeft.DataSource = eventList.Take(3);
                        //gvEventsLeft.DataBind();

                        //if (eventList.Skip(3).Count() > 0)
                        //{
                        //    //Assign next 5 to right grid
                        //    gvMiddle.DataSource = eventList.Skip(3).Take(3);
                        //    gvMiddle.DataBind();
                        //}
                        //else
                        //{
                        //    divMiddle.Visible = false;
                        //}

                        //if (eventList.Skip(6).Count() > 0)
                        //{
                        //    gvRight.DataSource = eventList.Skip(6).Take(3);
                        //    gvRight.DataBind();
                        //}
                        //else
                        //{
                        //    divRight.Visible = false;
                        //}
                        ltlEventFeedback.Visible = true;
                    }
                    else
                    {
                        divGridArea.Visible = false;
                    }

                    ltlEventFeedback.Visible = (eventListCount > 0);
                    ltlNoEventFeedback.Visible = !(eventListCount > 0);

                    lnkSchedule.HRef = ResolveUrl(string.Format("~/Project/ProjectSchedule.aspx?projectid={0}", ProjectID));
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvEventsRight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvEventsRight_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                ProjectEvent projectEvent = (ProjectEvent)dataItem.DataItem;

                //Name
                dataItem["EventName"].Text = Support.TruncateString(projectEvent.EventName, 10);
                if (projectEvent.EventName.Length > 10)
                {
                    dataItem["EventName"].ToolTip = projectEvent.EventName;
                }
            }
        }

        /// <summary>
        /// Handles the OnRowDataBound event of the gvEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gvEvents_OnRowDataBound(Object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                dynamic projectEvent = e.Row.DataItem as dynamic;

                Label lblEventName = (Label)e.Row.FindControl("lblEventName");
                Literal litDate = (Literal)e.Row.FindControl("litDate");

                litDate.Text = Support.FormatDate(projectEvent.EventDate);

                lblEventName.Text = Support.TruncateString(projectEvent.EventName, 22);
                if (projectEvent.EventName.Length > 22)
                {
                    lblEventName.ToolTip = projectEvent.EventName;
                }
            }
        }

        #endregion Event Handlers
    }
}