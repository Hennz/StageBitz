using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Project
{
    public partial class AddNewProject : PageBase
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

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">Permission denied for this company</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["companyid"] != null)
                {
                    CompanyID = Convert.ToInt32(Request.QueryString["companyid"].ToString());
                    bool hasCompanySuspended = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyID) || this.GetBL<CompanyBL>().IsCompanySuspended(CompanyID);
                    if (!Support.IsCompanyAdministrator(CompanyID) || hasCompanySuspended)
                    {
                        throw new ApplicationException("Permission denied for this company");
                    }

                    btnCancel.PostBackUrl = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", CompanyID);

                    ucPackageLimitsValidation.CompanyId = CompanyID;
                    ucPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.CreateProjects;
                    ucPackageLimitsValidation.LoadData();
                }
                LoadBreadCrumbs();
            }
            ucLocation.SetLocationsGridLength(450);//to fix the lcoations grid issue in chrome set the lengh of the grid manually.
            ucProjectEvents.SetEventsGridLength(400);//to fix the events grid issue in chrome set the lengh of the grid manually.
        }

        /// <summary>
        /// Creates the next project.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void CreateNextProject(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                CreateProject();
            }
        }

        /// <summary>
        /// Adds the project.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddProject(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                CreateProject();
            }
        }

        /// <summary>
        /// Popups the closed on cancel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void PopupClosedOnCancel(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                //popupConfirmPaymentDetails.HidePopup();
            }
        }

        #endregion Event Handlers

        #region Private Methods

        #endregion Private Methods

        /// <summary>
        /// Creates the project.
        /// </summary>
        /// <exception cref="System.ApplicationException">Permission denied for this company</exception>
        private void CreateProject()
        {
            //popupConfirmPaymentDetails.HidePopup();
            bool hasCompanySuspended = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyID) || this.GetBL<CompanyBL>().IsCompanySuspended(CompanyID);

            if (!StopProcessing && Page.IsValid && ucProjectEvents.Isvalid && !hasCompanySuspended && ucPackageLimitsValidation.Validate())
            {
                if (CompanyID == 0)
                {
                    throw new ApplicationException("Permission denied for this company");
                }

                //Check whether the same project name exist in the company.
                var existingprojects = from p in DataContext.Projects
                                       where p.ProjectName == txtProjectName.Text.Trim() && p.CompanyId == CompanyID && p.IsActive == true
                                       select p;

                if (existingprojects.Count<StageBitz.Data.Project>() > 0)
                {
                    errormsg.InnerText = "Project Name cannot be duplicated within the company.";
                    return;
                }

                int activeStatusCodeId = Support.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;

                #region Project

                int normalProjectTypeCodeId = Support.GetCodeByValue("ProjectType", "NORMAL").CodeId;
                StageBitz.Data.Project project = new Data.Project();
                project.ProjectName = txtProjectName.Text.Trim();

                if (ucProjectEvents.ProjectStartDate != DateTime.MinValue)
                    project.StartDate = ucProjectEvents.ProjectStartDate;

                if (ucProjectEvents.ProjectEndDate != DateTime.MinValue)
                    project.EndDate = ucProjectEvents.ProjectEndDate;

                project.IsActive = true;
                project.ProjectStatusCodeId = activeStatusCodeId;
                project.ProjectTypeCodeId = normalProjectTypeCodeId;
                project.CompanyId = CompanyID;
                project.CreatedByUserId = UserID;
                project.CreatedDate = Now;
                project.LastUpdatedByUserId = UserID;
                project.LastUpdatedDate = Now;
                project.CountryId = DataContext.Companies.Where(c => c.CompanyId == CompanyID).FirstOrDefault().CountryId;
                DataContext.Projects.AddObject(project);

                #endregion Project

                #region Project User

                ProjectUser projectUser = new ProjectUser();
                projectUser.ProjectUserTypeCodeId = Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");
                projectUser.CreatedDate = Now;
                projectUser.UserId = UserID;
                projectUser.Role = "N/A";
                projectUser.CanSeeBudgetSummary = true;
                projectUser.IsActive = true;
                projectUser.CreatedByUserId = UserID;
                projectUser.LastUpdatedByUserId = UserID;
                projectUser.LastUpdatedDate = Now;
                DataContext.ProjectUsers.AddObject(projectUser);

                #endregion Project User

                ucProjectEvents.AddEventsToContext();
                ucLocation.AddLocationsToContext();

                //Create Notification for New Project

                #region Project Notification

                Notification nf = new Notification();
                nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJECT");
                nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "ADD");
                nf.RelatedId = project.ProjectId;
                nf.ProjectId = project.ProjectId;
                nf.Message = string.Format("{0} created the Project - '{1}'.", Support.UserFullName, project.ProjectName);
                nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                nf.CreatedDate = nf.LastUpdatedDate = Now;
                DataContext.Notifications.AddObject(nf);

                #endregion Project Notification

                //Update Project Daily Usage Summary
                //ProjectUsageHandler.UpdateProjectUsage(project, UserID, UserID, false, Today, DataContext);

                DataContext.SaveChanges();

                Response.Redirect(string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId));
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == CompanyID).FirstOrDefault();
            string companyUrl = Support.IsCompanyAdministrator(company.CompanyId) ?
                string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", company.CompanyId) : null;
            bc.AddLink(company.CompanyName, companyUrl);

            bc.AddLink("Create New Project", null);
            bc.LoadControl();
        }
    }
}