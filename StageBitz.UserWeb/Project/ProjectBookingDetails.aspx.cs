using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web Page
    /// </summary>
    public partial class ProjectBookingDetails : PageBase
    {
        #region Properties

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
                if (ViewState["ProjectId"] == null)
                {
                    int projectId = 0;

                    if (Request["ProjectId"] != null)
                    {
                        int.TryParse(Request["ProjectId"], out projectId);
                    }

                    ViewState["ProjectId"] = projectId;
                }

                return (int)ViewState["ProjectId"];
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
        /// Gets a value indicating whether [only show my bookings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [only show my bookings]; otherwise, <c>false</c>.
        /// </value>
        public bool OnlyShowMyBookings
        {
            get
            {
                if (ViewState["OnlyShowMyBookings"] == null)
                {
                    bool onlyShowMyBookings = false;

                    if (Request["OnlyShowMyBookings"] != null)
                    {
                        bool.TryParse(Request["OnlyShowMyBookings"], out onlyShowMyBookings);
                    }

                    ViewState["OnlyShowMyBookings"] = onlyShowMyBookings;
                }

                return (bool)ViewState["OnlyShowMyBookings"];
            }
        }

        #endregion Properties

        #region Event Handles

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
                StageBitz.Data.Project project = (from p in DataContext.Projects
                                                  where p.ProjectId == ProjectId && p.IsActive == true
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
                bookingDetails.CanEditBookingDates = !GetBL<ProjectBL>().IsReadOnlyRightsForProject(ProjectId, UserID, false);
                //bookingDetails.RelatedId = ProjectId;
                //bookingDetails.RelatedTableName = "Project";
                bookingDetails.BookingId = GetBL<InventoryBL>().GetBooking(ProjectId, "Project").BookingId;
                bookingDetails.DisplayMode = UserWeb.Controls.Inventory.BookingDetails.ViewMode.Project;
                bookingDetails.OnlyShowMyBookings = this.OnlyShowMyBookings;
                DisplayTitle = "Bookings";
                LoadBreadCrumbs(project);
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", project.CompanyId, (int)BookingTypes.Project, ProjectId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                linkTaskManager.HRef = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId);
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();
            }
        }

        #endregion Event Handles

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="project">The project.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Project project)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            string companyUrl = Support.IsCompanyAdministrator(project.Company.CompanyId) ?
                string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", project.Company.CompanyId) : null;
            bc.AddLink(project.Company.CompanyName, companyUrl);

            bc.AddLink(project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}