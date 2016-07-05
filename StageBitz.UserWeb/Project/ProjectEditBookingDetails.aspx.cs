using StageBitz.Common.Constants;
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
    public partial class ProjectEditBookingDetails : PageBase
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
        /// Gets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeId
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

        /// <summary>
        /// Gets a value indicating whether this instance is to date change.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is to date change; otherwise, <c>false</c>.
        /// </value>
        public bool IsToDateChange
        {
            get
            {
                if (ViewState["IsToDateChange"] == null)
                {
                    bool isToDateChange = true;

                    if (Request["IsToDateChange"] != null)
                    {
                        bool.TryParse(Request["IsToDateChange"], out isToDateChange);
                    }

                    ViewState["IsToDateChange"] = isToDateChange;
                }

                return (bool)ViewState["IsToDateChange"];
            }
        }

        #endregion Properties

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">project not found</exception>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for this project
        /// or
        /// Permission denied to this page
        /// </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            //IsLargeContentArea = true;
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
                    if (GetBL<ProjectBL>().IsProjectClosed(ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "Permission denied for this project."));
                    }

                    throw new ApplicationException("Permission denied for this project ");
                }

                if (Support.IsReadOnlyRightsForProject(ProjectId, UserID, false))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "Permission denied for this page."));
                    }

                    throw new ApplicationException("Permission denied to this page ");
                }

                Data.Booking booking = this.GetBL<InventoryBL>().GetBooking(ProjectId, GlobalConstants.RelatedTables.Bookings.Project);
                editBookingDetails.ItemTypeId = ItemTypeId;
                editBookingDetails.IsToDateChange = IsToDateChange;
                editBookingDetails.BookingId = booking.BookingId;
                editBookingDetails.IsInventoryManager = false;
                editBookingDetails.CallBackURL = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);

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
            bc.AddLink("Bookings", string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", project.ProjectId));
            bc.AddLink("Change dates", null);
            bc.LoadControl();
        }
    }
}