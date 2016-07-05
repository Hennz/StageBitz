using StageBitz.Data;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for project Schedule List.
    /// </summary>
    public partial class ScheduleList : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyID
        {
            get
            {
                if (ViewState["CompanyID"] == null)
                {
                    ViewState["CompanyID"] = 0;
                }

                return (int)ViewState["CompanyID"];
            }
            set
            {
                ViewState["CompanyID"] = value;
            }
        }

        #endregion Properties

        #region Fields

        /// <summary>
        /// The left
        /// </summary>
        private int left, right = 0;

        #endregion Fields

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
                case 3:
                    left = projectEventList.Count;
                    break;

                case 4:
                    left = 2;
                    right = 2;
                    break;

                case 5:
                    left = 3;
                    right = 2;
                    break;

                case 6:
                    left = 3;
                    right = 3;
                    break;

                case 7:
                    left = 4;
                    right = 3;
                    break;

                case 8:
                    left = 4;
                    right = 4;
                    break;

                case 9:
                    left = 5;
                    right = 4;
                    break;

                default:
                    left = 5;
                    right = 5;
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
                LoadData();
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

                LinkButton lnkProject = (LinkButton)e.Row.FindControl("lnkProjectName");
                Label lblProject = (Label)e.Row.FindControl("lblProjectName");
                Label lblEventName = (Label)e.Row.FindControl("lblEventName");
                Literal litDate = (Literal)e.Row.FindControl("litDate");

                litDate.Text = Support.FormatDate(projectEvent.EventDate);

                if (lblProject != null && lblProject != null)
                {
                    string truncatedProjectName = Support.TruncateString(projectEvent.Project.ProjectName, 18);
                    if (Support.CanAccessProject(projectEvent.ProjectId))
                    {
                        lnkProject.Text = truncatedProjectName;
                        lnkProject.PostBackUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", projectEvent.ProjectId);
                        lblProject.Visible = false;
                        lnkProject.Visible = true;
                    }
                    else
                    {
                        lblProject.Text = truncatedProjectName;
                        lblProject.Visible = true;
                        lnkProject.Visible = false;
                    }
                }

                lnkProject.Text = Support.TruncateString(projectEvent.Project.ProjectName, 18);
                lnkProject.PostBackUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", projectEvent.ProjectId);
                if (projectEvent.Project.ProjectName.Length > 18)
                {
                    lnkProject.ToolTip = projectEvent.Project.ProjectName;
                }

                lblEventName.Text = Support.TruncateString(projectEvent.EventName, 18);
                if (projectEvent.EventName.Length > 18)
                {
                    lblEventName.ToolTip = projectEvent.EventName;
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            List<ProjectEvent> eventList = new List<ProjectEvent>();
            //Load the top 10 events that are not being expired.
            if (CompanyID > 0)
            {
                eventList = GetBL<ProjectBL>().GetTopTenProjectEventsByCompanyId(CompanyID);
            }
            else
            {
                eventList = GetBL<ProjectBL>().GetTopTenProjectEventsByUserId(UserID);
            }

            ConfigureGrids(eventList);

            int eventListCount = eventList.Count();
            if (eventListCount > 0)
            {
                divGridArea.Visible = true;

                if (left > 0)
                {
                    gvEventsLeft.DataSource = eventList.Take(left);
                    gvEventsLeft.DataBind();
                }

                if (right > 0)
                {
                    gvRight.DataSource = eventList.Skip(left).Take(right);
                    gvRight.DataBind();
                    divLeft.Style["border-right"] = "1px Solid Black";
                }
            }
            else
            {
                divGridArea.Visible = false;
            }

            divNoEvents.Visible = !(eventListCount > 0);
        }

        #endregion Public Methods
    }
}