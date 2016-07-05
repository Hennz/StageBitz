using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web page for Project Dashboard.
    /// </summary>
    public partial class ProjectDashboard : PageBase
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

        #endregion Properties

        #region Events

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
                    projectWarningPopup.ProjectId = ProjectID;

                    StageBitz.Data.Project project = (from p in DataContext.Projects
                                                      where p.ProjectId == ProjectID && p.IsActive == true
                                                      select p).FirstOrDefault();
                    if (project == null)
                    {
                        throw new Exception("project not found");
                    }

                    this.CompanyId = project.CompanyId;
                    DisplayTitle = string.Format("{0}'s Dashboard", Support.TruncateString(project.ProjectName, 32));

                    if (!Support.CanAccessProject(project))
                    {
                        if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                        {
                            StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                        }

                        throw new ApplicationException("Permission denied for this project ");
                    }

                    warningDisplay.ProjectID = ProjectID;
                    warningDisplay.LoadData();

                    ucprojectSchedules.ProjectID = ProjectID;
                    ucprojectItemBrief.ProjectID = ProjectID;
                    ucProjectAdministration.ProjectID = ProjectID;

                    #region SET LINKS

                    linkTaskManager.HRef = string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectID);
                    lnkProjectDetails.HRef = string.Format("~/Project/ProjectDetails.aspx?ProjectID={0}", ProjectID);
                    lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", project.CompanyId, (int)BookingTypes.Project, ProjectID);
                    lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectID);
                    reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectID), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectID), ProjectID);
                    projectUpdatesLink.ProjectID = ProjectID;
                    projectUpdatesLink.LoadData();

                    #endregion SET LINKS

                    LoadBreadCrumbs(project.Company);
                }
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="company">The company.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Company company)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            string companyUrl = Support.IsCompanyAdministrator(company.CompanyId) ?
                string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", company.CompanyId) : null;
            bc.AddLink(company.CompanyName, companyUrl);
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}