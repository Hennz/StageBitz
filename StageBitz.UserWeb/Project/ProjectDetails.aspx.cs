using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// web page for project details page.
    /// </summary>
    public partial class ProjectDetails : PageBase
    {
        #region Private Fields

        //ProjectBL _projectBL = new ProjectBL();
        //NotificationBL _notificationBL = new NotificationBL();
        //ItemBriefBL _itemBriefBL = new ItemBriefBL();

        #endregion Private Fields

        #region Private Properties

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
            private set
            {
                ViewState["CompanyID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        private string ProjectName
        {
            get
            {
                if (ViewState["ProjectName"] == null)
                {
                    ViewState["ProjectName"] = string.Empty;
                }

                return (string)ViewState["ProjectName"];
            }
            set
            {
                ViewState["ProjectName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is project closed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is project closed; otherwise, <c>false</c>.
        /// </value>
        private bool IsProjectClosed
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return GetBL<ProjectBL>().IsProjectClosed(ProjectID);
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        /// <summary>
        /// Gets the last updated date of the original record which was used to load data on the page.
        /// Used for concurrency handling.
        /// </summary>
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

        #endregion Private Properties

        #region Events Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">project not found</exception>
        /// <exception cref="System.ApplicationException">Permission denied for this project.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["projectid"] != null)
                {
                    ProjectID = Convert.ToInt32(Request.QueryString["projectid"].ToString());
                    ucLocation.ProjectID = ProjectID;
                    attachments.RelatedId = ProjectID;
                    attachments.ProjectId = ProjectID;
                    attachments.Mode = ItemAttachments.DisplayMode.Project;
                    attachments.IsReadOnly = Support.IsReadOnlyRightsForProject(ProjectID);
                }

                string tabId = Request.QueryString["tabId"];
                if (tabId != null)
                {
                    int tabIndex = Convert.ToInt16(tabId);
                    projectDetailsTabs.SelectedIndex = tabIndex;
                    projectDetailsPages.SelectedIndex = tabIndex;
                }

                projectWarningPopup.ProjectId = ProjectID;

                var project = GetBL<ProjectBL>().GetProject(ProjectID);

                if (project == null)
                {
                    throw new Exception("project not found");
                }

                CompanyID = project.CompanyId;
                ProjectName = project.ProjectName;
                DisplayTitle = string.Format("{0}'s Details", Support.TruncateString(project.ProjectName, 32));
                txtName.Text = project.ProjectName;

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project.");
                }

                warningDisplay.ProjectID = ProjectID;
                warningDisplay.LoadData();

                bool isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);

                //Set last updated date for concurrency handling
                OriginalLastUpdatedDate = project.LastUpdatedDate.Value;
                btnCancel.PostBackUrl = String.Format("~/Project/ProjectDashboard.aspx?projectid={0}&companyid={1}", ProjectID, CompanyID);
                btnCancel.Visible = !isReadOnlyRightsForProject;
                txtName.ReadOnly = isReadOnlyRightsForProject;

                #region SET LINKS

                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", CompanyID, (int)BookingTypes.Project, ProjectID);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectID);
                linkTaskManager.HRef = ResolveUrl(string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectID));
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectID), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectID), ProjectID);
                projectUpdatesLink.ProjectID = ProjectID;
                projectUpdatesLink.LoadData();

                #endregion SET LINKS

                LoadBreadCrumbs(project);
                LoadProjectWarpupTab();
            }

            ucLocation.SetLocationsGridLength(880);//to fix the grid issue in chrome set the lengh of the grid manually.

            ucLocation.UpdateLocationTabCount += delegate(int count)
            {
                UpdateLocationsTabCount(count);
            };
            attachments.UpdateAttachmentsCount += delegate(int count)
            {
                UpdateAttachmentsTabCount(count);
            };
        }

        /// <summary>
        /// Saves the details.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveDetails(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                SaveProjectDetails();
                Response.Redirect(string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", ProjectID));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnProjectClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnProjectClose_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!IsPageDirty)
                {
                    ProjectBL projectBL = GetBL<ProjectBL>();

                    if (projectBL.GetFreeTrialProjectsNotInClosedStatus(CompanyID, Utils.Today).Count() == 1)
                    {
                        //if this is the last free trial project which is going to be closed we should give the warning.
                        //usually a company will have one free trial project, but when clearing finance a company can have more free trial projects,
                        //so closing one free trial project doesn't end the free trial of that company.
                        popupCloseFreeTrailProject.ShowPopup();
                    }
                    else if (projectBL.IsProjectInGracePeriod(ProjectID))
                    {
                        popupPaymentFailed.ShowPopup();
                    }
                    else if (this.GetBL<ProjectBL>().HasHiddenItemsForProject(ProjectID))
                    {
                        ShowProjectCloseInventoryLimitReachedPopUp();
                    }
                    else
                    {
                        popupConfirmCloseProject.ShowPopup();
                    }
                }
                else
                {
                    popupConfirmSaveProjectDetails.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmSaveProjectDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmSaveProjectDetails_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                SaveProjectDetails();
                IsPageDirty = false;
                popupConfirmSaveProjectDetails.HidePopup();
                popupConfirmCloseProject.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmSaveProjectDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmSaveProjectDetails_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupConfirmSaveProjectDetails.HidePopup();
                popupConfirmCloseProject.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmCloseProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmCloseProject_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupConfirmCloseProject.HidePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmCloseProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmCloseProject_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                string action = string.Empty;
                Button button = sender as Button;
                if (button != null)
                {
                    action = button.CommandName;
                }

                if ((action.Equals("FreeTrailEnd") || action.Equals("ConfirmClose")) && this.GetBL<ProjectBL>().HasHiddenItemsForProject(ProjectID))
                {
                    if (action.Equals("FreeTrailEnd"))
                    {
                        popupCloseFreeTrailProject.HidePopup();
                    }
                    else if (action.Equals("ConfirmClose"))
                    {
                        popupConfirmCloseProject.HidePopup();
                    }

                    ShowProjectCloseInventoryLimitReachedPopUp();
                    return;
                }

                CloseProject(action);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnGotoCompanyBilling control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGotoCompanyBilling_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyID));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpgradeProjectCloseInventoryLimitReached control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpgradeProjectCloseInventoryLimitReached_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect(string.Format("~/Company/CompanyPricingPlans.aspx?companyid={0}", CompanyID));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendEmailProjectCloseInventoryLimitReached control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendEmailProjectCloseInventoryLimitReached_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                SendEmailToCompanyAdminCloseProjectInventoryLimitReached();
                popupProjectCloseInventoryLimitReached.HidePopup();
                ShowEmailSentToAdminConfirmation();
            }
        }

        #endregion Events Handlers

        #region Privete Methods

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

        /// <summary>
        /// Loads the project warpup tab.
        /// </summary>
        private void LoadProjectWarpupTab()
        {
            var projectBL = GetBL<ProjectBL>();

            if (projectBL.IsProjectClosed(ProjectID))
            {
                divAfterCloseProject.Visible = true;
                divBeforeCloseProject.Visible = false;
                //divBeforeCloseProjectBlank.Visible = false;

                itemBriefTypeSummaryAfterClose.ProjectID = this.ProjectID;
                itemBriefTypeSummaryAfterClose.LoadControl();
                btnProjectClose.Visible = false;
                btnProjectClose.ToolTip = "This Project has been closed.";
            }
            else
            {
                btnProjectClose.Visible = true;
                btnProjectClose.ToolTip = string.Empty;
                itemBriefTypeSummaryBeforeClose.ProjectID = this.ProjectID;
                itemBriefTypeSummaryBeforeClose.LoadControl();

                string feedbackEmail = Support.GetSystemValue("FeedbackEmail");
                lnkFeedBackEmail.NavigateUrl = string.Concat("mailto:", feedbackEmail);
                lnkFeedBackEmail.Text = feedbackEmail;

                divAfterCloseProject.Visible = false;
                divBeforeCloseProject.Visible = true;
            }

            bool isProjectAdmin = Support.IsProjectAdministrator(ProjectID);
            bool isCompanyAdmin = Support.IsCompanyAdministrator(CompanyID);

            if (IsAllowedToCloseProject())
            {
                btnProjectClose.Enabled = true;
                btnProjectClose.ToolTip = string.Empty;
            }
            else if (!(isProjectAdmin || isCompanyAdmin))
            {
                btnProjectClose.Enabled = false;
                btnProjectClose.ToolTip = "Only Administrators can close a Project";
            }

            if (!isCompanyAdmin && isProjectAdmin)
            {
                string primaryAdminEmail = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID).Email1;
                ltrlContactPrimaryAdmin.Text = string.Format("<li>You can contact Primary Company Administrator <a href='mailto:{0}'>here</a>.</li>", primaryAdminEmail);
            }

            btnGotoCompanyBilling.Visible = isCompanyAdmin;
        }

        /// <summary>
        /// Updates the locations tab count.
        /// </summary>
        /// <param name="count">The count.</param>
        private void UpdateLocationsTabCount(int count)
        {
            projectDetailsTabs.Tabs[0].Text = string.Format("Locations ({0})", count);
            upnlProjectDetailsTabs.Update();
        }

        /// <summary>
        /// Updates the attachments tab count.
        /// </summary>
        /// <param name="count">The count.</param>
        private void UpdateAttachmentsTabCount(int count)
        {
            projectDetailsTabs.Tabs[1].Text = string.Format("Attachments ({0})", count);
            upnlProjectDetailsTabs.Update();
        }

        /// <summary>
        /// Sends the emails project is closed.
        /// </summary>
        private void SendEmailsProjectIsClosed()
        {
            List<User> projectUsers = this.GetBL<ProjectBL>().GetProjectUsers(ProjectID);
            List<User> companyAdministrators = this.GetBL<ProjectBL>().GetCompanyAdministrators(CompanyID);
            //List<User> usersToInformProjectClosed = projectUsers.Concat(companyAdministrators).ToList<User>();
            List<User> usersToInformProjectClosed = projectUsers.Union(companyAdministrators).ToList<User>();
            Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectID).FirstOrDefault();
            User projectClosedBy = this.GetBL<ProjectBL>().GetProjectClosedBy(ProjectID);

            foreach (User user in usersToInformProjectClosed)
            {
                EmailSender.InformUsersProjectIsClosed(user.Email1, ProjectID, user.FirstName, (projectClosedBy.FirstName + " " + projectClosedBy.LastName), Support.GetCompanyNameById(CompanyID), project.ProjectName);
            }
        }

        /// <summary>
        /// Sends the email to company admin close project inventory limit reached.
        /// </summary>
        private void SendEmailToCompanyAdminCloseProjectInventoryLimitReached()
        {
            Data.User primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID);
            Data.User currentUser = this.GetBL<PersonalBL>().GetUser(UserID);
            string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
            string pricePlanUrl = string.Format("{0}/Company/CompanyPricingPlans.aspx?companyId={1}", userWebUrl, CompanyID);
            int noOfInventoryItems = this.GetBL<FinanceBL>().GetCompanyCurrentItemCount(CompanyID);
            int noOfNewItems = this.GetBL<ProjectBL>().GetHiddenItemCountForProject(ProjectID);
            string projectname = this.GetBL<ProjectBL>().GetProject(ProjectID).ProjectName;
            EmailSender.SendInventoryLimitUpgradeRequestCloseProject(primaryAdmin.Email1, primaryAdmin.FirstName, projectname, pricePlanUrl, noOfInventoryItems.ToString(),
                noOfNewItems.ToString(), (noOfInventoryItems + noOfNewItems).ToString(), currentUser.Email1, (currentUser.FirstName + " " + currentUser.LastName));
        }

        /// <summary>
        /// Determines whether [is valid to save].
        /// </summary>
        /// <returns></returns>
        private bool IsValidToSave()
        {
            bool status = false;
            var existingprojects = GetBL<ProjectBL>().GetProjectsByName(txtName.Text.Trim(), CompanyID, true);

            if ((existingprojects.Count() == 1 && existingprojects[0].ProjectId == ProjectID) || existingprojects.Count() == 0)
            {
                status = true;
            }

            return status;
        }

        /// <summary>
        /// Saves the project details.
        /// </summary>
        private void SaveProjectDetails()
        {
            bool isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(ProjectID);
            if (txtName.Text.Trim() != string.Empty && ProjectName != txtName.Text.Trim() && !isReadOnlyRightsForProject)
            {
                StageBitz.Data.Project project = GetBL<ProjectBL>().GetProjectByLastUpdatedDateTime(ProjectID, OriginalLastUpdatedDate);

                if (project == null)
                {
                    StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ProjectDetails, ProjectID));
                }

                ////Check whether the same project name exist in the company.
                if (!IsValidToSave())
                {
                    errormsg.InnerText = "Project Name cannot be duplicated within the company.";
                    return;
                }

                bool shouldChangeCreatedForTag = false;
                string oldProjectName = project.ProjectName;
                string newProjectName = txtName.Text.Trim();

                // Create notifications, if the project name is being changed.
                // Change Item's CreatedFor field.(if item is created from this project)
                if (project.ProjectName != newProjectName)
                {
                    Notification nf = new Notification();
                    nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJECT");
                    nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "EDIT");
                    nf.RelatedId = project.ProjectId;
                    nf.ProjectId = project.ProjectId;
                    nf.Message = string.Format("{0} edited Project Name.", Support.UserFullName);
                    nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                    nf.CreatedDate = nf.LastUpdatedDate = Now;

                    GetBL<NotificationBL>().AddNotification(nf);

                    #region Initialize Update CreatedFor field

                    shouldChangeCreatedForTag = true;

                    #endregion Initialize Update CreatedFor field
                }

                project.ProjectName = newProjectName;
                project.LastUpdatedByUserId = UserID;
                project.LastUpdatedDate = Now;
                GetBL<ProjectBL>().SaveChanges();

                #region Update CreatedFor field

                if (shouldChangeCreatedForTag)
                {
                    DataContext.UpdateCreatedForField(project.ProjectId, newProjectName, oldProjectName);
                }

                #endregion Update CreatedFor field
            }
        }

        /// <summary>
        /// Shows the project close inventory limit reached pop up.
        /// </summary>
        private void ShowProjectCloseInventoryLimitReachedPopUp()
        {
            if (Support.IsCompanyAdministrator(CompanyID, UserID))
            {
                divCompanyAdminInventoryReached.Visible = true;
                divProjectAdminInventoryReached.Visible = false;
                btnSendEmailProjectCloseInventoryLimitReached.Visible = false;
                btnUpgradeProjectCloseInventoryLimitReached.Visible = true;
            }
            else
            {
                Data.User primaryCompanyAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID);
                ltrCompanyPrimaryAdminName.Text = primaryCompanyAdmin.FirstName + " " + primaryCompanyAdmin.LastName;
                divProjectAdminInventoryReached.Visible = true;
                divCompanyAdminInventoryReached.Visible = false;
                btnSendEmailProjectCloseInventoryLimitReached.Visible = true;
                btnUpgradeProjectCloseInventoryLimitReached.Visible = false;
            }
            popupProjectCloseInventoryLimitReached.ShowPopup();
        }

        /// <summary>
        /// Shows the email sent to admin confirmation.
        /// </summary>
        private void ShowEmailSentToAdminConfirmation()
        {
            Data.User primaryCompanyAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID);
            ltrPrimaryAdminName.Text = primaryCompanyAdmin.FirstName + " " + primaryCompanyAdmin.LastName;
            popupConfirmEmailSentToAdminToUpgrade.ShowPopup();
        }

        /// <summary>
        /// Determines whether [is allowed to close project].
        /// </summary>
        /// <returns></returns>
        private bool IsAllowedToCloseProject()
        {
            bool isProjectAdmin = Support.IsProjectAdministrator(ProjectID);
            bool isCompanyAdmin = Support.IsCompanyAdministrator(CompanyID);

            return (isProjectAdmin || isCompanyAdmin) && !Support.IsReadOnlyProjectByProjectStatus(ProjectID);
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        /// <param name="condition">The condition.</param>
        private void CloseProject(string condition)
        {
            if (IsAllowedToCloseProject())
            {
                // get current price plan before close project.
                var currentPricingPlan = GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyID);

                popupConfirmCloseProject.HidePopup();

                if (condition.Equals("FreeTrailEnd"))
                {
                    ProjectUsageHandler.UpdatePricingPlanAndPaymentSummaryClosingFreeTrial(CompanyID, UserID, DataContext);
                    popupCloseFreeTrailProject.HidePopup();
                }

                if (condition.Equals("InventoryLimitReached"))
                {
                    popupProjectCloseInventoryLimitReached.HidePopup();
                }

                // Close Project
                int status = GetBL<ProjectBL>().CloseProject(ProjectID, UserID);

                if (status > 0)
                {
                    //Set the Page Read only

                    //Send Emails
                    SendEmailsProjectIsClosed();
                }

                LoadProjectWarpupTab();

                if (Support.IsCompanyAdministrator(CompanyID))
                {
                    warningDisplay.LoadData();
                    btnCancel.Visible = false;
                    txtName.ReadOnly = true;
                    IsPageDirty = false;
                    ucLocation.LoadData();
                }
                else
                {
                    Response.Redirect("~/Default.aspx");
                }
            }
            else
            {
                Response.Redirect(Request.Url.ToString(), true);
            }
        }

        #endregion Privete Methods
    }
}