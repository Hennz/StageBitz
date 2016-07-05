using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;
using System.Web.UI;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web page for project schedule
    /// </summary>
    public partial class ProjectSchedule : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the project identifier.
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
            private set
            {
                ViewState["ProjectID"] = value;
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
        /// Gets or sets the original last updated date.
        /// </summary>
        /// <value>
        /// The original last updated date.
        /// </value>
        private DateTime OriginalLastUpdatedDate
        {
            get
            {
                return (DateTime)ViewState["LastUpdatedDate"];
            }
            set
            {
                ViewState["LastUpdatedDate"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">project not found</exception>
        /// <exception cref="System.ApplicationException">Permission denied for this project </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["projectid"] != null)
                {
                    ProjectID = Convert.ToInt32(Request.QueryString["projectid"].ToString());
                    ucProjectEvents.ProjectID = ProjectID;
                }

                StageBitz.Data.Project project = (from p in DataContext.Projects
                                                  where p.ProjectId == ProjectID
                                                  select p).FirstOrDefault();
                if (project == null)
                {
                    throw new Exception("project not found");
                }

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project ");
                }

                this.CompanyId = project.CompanyId;
                projectWarningPopup.ProjectId = ProjectID;
                warningDisplay.ProjectID = ProjectID;
                warningDisplay.LoadData();

                //Set last updated date for concurrency handling
                OriginalLastUpdatedDate = project.LastUpdatedDate.Value;
                btnCancel.PostBackUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", ProjectID);
                DisplayTitle = "Manage Schedule";//string.Format("{0}'s Dashboard", Support.TruncateString(project.ProjectName, 32));

                bool isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);
                btnCancel.Visible = !isReadOnlyRightsForProject;

                #region SET LINKS

                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", project.CompanyId, (int)BookingTypes.Project, ProjectID);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectID);
                linkTaskManager.HRef = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectID);
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectID), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectID), ProjectID);
                projectUpdatesLink.ProjectID = ProjectID;
                projectUpdatesLink.LoadData();

                #endregion SET LINKS

                LoadBreadCrumbs(project);
            }

            ucProjectEvents.SetEventsGridLength(900);//to fix the events grid issue in chrome set the lengh of the grid manually.
        }

        /// <summary>
        /// Saves the schedule.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveSchedule(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                bool isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);
                if (Page.IsValid && !isReadOnlyRightsForProject)
                {
                    StageBitz.Data.Project project = (from p in DataContext.Projects
                                                      where p.ProjectId == ProjectID && p.LastUpdatedDate == OriginalLastUpdatedDate
                                                      select p).FirstOrDefault();

                    if (project == null)
                    {
                        StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ProjectDetails, ProjectID));
                    }

                    #region Notification

                    string message = string.Empty;
                    //Build the message
                    if ((ucProjectEvents.ProjectStartDate != project.StartDate && ucProjectEvents.ProjectStartDate != DateTime.MinValue || ucProjectEvents.ProjectStartDate == DateTime.MinValue && project.StartDate != null)
                        && (ucProjectEvents.ProjectEndDate != project.EndDate && ucProjectEvents.ProjectEndDate != DateTime.MinValue || ucProjectEvents.ProjectEndDate == DateTime.MinValue && project.EndDate != null))
                    {
                        message = string.Format("{0} changed Project Start date and End date.", Support.UserFullName);
                    }
                    else if (ucProjectEvents.ProjectStartDate != project.StartDate && ucProjectEvents.ProjectStartDate != DateTime.MinValue || ucProjectEvents.ProjectStartDate == DateTime.MinValue && project.StartDate != null)
                    {
                        message = string.Format("{0} changed the Project Start date.", Support.UserFullName);
                    }
                    else if (ucProjectEvents.ProjectEndDate != project.EndDate && ucProjectEvents.ProjectEndDate != DateTime.MinValue || ucProjectEvents.ProjectEndDate == DateTime.MinValue && project.EndDate != null)
                    {
                        message = string.Format("{0} changed the Project End date.", Support.UserFullName);
                    }

                    if (message != string.Empty)
                    {
                        Notification nf = new Notification();
                        nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "SCHEDULE");
                        nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "EDIT");
                        nf.RelatedId = ProjectID;
                        nf.ProjectId = ProjectID;
                        nf.Message = message;
                        nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                        nf.CreatedDate = nf.LastUpdatedDate = Now;
                        DataContext.Notifications.AddObject(nf);
                    }

                    #endregion Notification

                    if (ucProjectEvents.ProjectStartDate != DateTime.MinValue)
                        project.StartDate = ucProjectEvents.ProjectStartDate;
                    else
                        project.StartDate = null;

                    if (ucProjectEvents.ProjectEndDate != DateTime.MinValue)
                        project.EndDate = ucProjectEvents.ProjectEndDate;
                    else
                        project.EndDate = null;

                    project.LastUpdatedByUserId = UserID;
                    project.LastUpdatedDate = Now;
                    DataContext.SaveChanges();
                }
                Response.Redirect(string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", ProjectID));
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="project">The project.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Project project)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            string companyUrl = Support.IsCompanyAdministrator(project.Company.CompanyId) ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", project.Company.CompanyId) : null;
            bc.AddLink(project.Company.CompanyName, companyUrl);

            bc.AddLink(project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}