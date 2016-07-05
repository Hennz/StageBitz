using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// Enum for project warning mode.
    /// </summary>
    public enum ProjectWarningMode
    {
        Project = 0,
        Inventory = 1
    }

    /// <summary>
    /// User control for project warning popup.
    /// </summary>
    public partial class ProjectWarningPopup : UserControlBase
    {
        #region Fields

        /// <summary>
        /// The project suspend code identifier var
        /// </summary>
        private int ProjectSuspendCodeId = Support.GetCodeIdByCodeValue("ProjectStatus", "SUSPENDED");

        #endregion Fields

        #region properties

        /// <summary>
        /// Gets or sets the project identifier.
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
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the company identifier.
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
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public ProjectWarningMode Mode
        {
            get
            {
                if (ViewState["Mode"] == null)
                {
                    ViewState["Mode"] = ProjectWarningMode.Project;
                }

                return (ProjectWarningMode)ViewState["Mode"];
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this project is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this project is closed; otherwise, <c>false</c>.
        /// </value>
        private bool IsProjectClosed
        {
            get
            {
                if (ViewState["IsProjectClosed"] == null)
                {
                    ViewState["IsProjectClosed"] = false;
                }

                return (bool)ViewState["IsProjectClosed"];
            }
            set
            {
                ViewState["IsProjectClosed"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this project is suspended.
        /// </summary>
        /// <value>
        /// <c>true</c> if this project is suspended; otherwise, <c>false</c>.
        /// </value>
        private bool IsProjectSuspended
        {
            get
            {
                if (ViewState["IsProjectSuspended"] == null)
                {
                    ViewState["IsProjectSuspended"] = false;
                }

                return (bool)ViewState["IsProjectSuspended"];
            }
            set
            {
                ViewState["IsProjectSuspended"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company is suspended.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company is suspended; otherwise, <c>false</c>.
        /// </value>
        private bool IsCompanySuspended
        {
            get
            {
                if (ViewState["IsCompanySuspended"] == null)
                {
                    ViewState["IsCompanySuspended"] = false;
                }

                return (bool)ViewState["IsCompanySuspended"];
            }
            set
            {
                ViewState["IsCompanySuspended"] = value;
            }
        }

        #endregion properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                if (this.ProjectId > 0)
                {
                    var project = GetBL<ProjectBL>().GetProject(ProjectId);
                    if (project != null && Mode == ProjectWarningMode.Project)
                    {
                        CompanyId = project.CompanyId;
                        IsProjectClosed = GetBL<ProjectBL>().IsProjectClosed(ProjectId);
                        IsProjectSuspended = project.ProjectStatusCodeId == ProjectSuspendCodeId;
                    }
                }
                else if (this.CompanyId > 0)
                {
                    IsCompanySuspended = GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || GetBL<CompanyBL>().IsCompanySuspended(CompanyId);
                }

                InitializeErrorPopups();
            }

            CheckForProjectStatus();
        }

        /// <summary>
        /// Handles the Click event of the btnGotoMyDashboard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGotoMyDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        /// <summary>
        /// Handles the Click event of the btnReload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReload_Click(object sender, EventArgs e)
        {
            Page.Response.Redirect(Page.Request.Url.ToString(), true);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Checks for project status.
        /// </summary>
        private void CheckForProjectStatus()
        {
            int zIndex = 1002;
            if (IsPostBack)
            {
                if (Mode == ProjectWarningMode.Project && ProjectId > 0)
                {
                    var project = GetBL<ProjectBL>().GetProject(ProjectId);
                    if (project != null)
                    {
                        if (!IsProjectClosed && GetBL<ProjectBL>().IsProjectClosed(ProjectId))
                        {
                            if (Support.IsCompanyAdministrator(CompanyId))
                            {
                                Page.Response.Redirect(Page.Request.Url.ToString(), true);
                            }
                            else
                            {
                                InitializePopup();
                                popupProjectCloseWarning.ShowPopup(zIndex);
                                PageBase.StopProcessing = true;
                                PageBase.IsPageDirty = false;
                            }
                        }
                        else if (!IsProjectSuspended && project.ProjectStatusCodeId == ProjectSuspendCodeId)
                        {
                            InitializePopup();
                            popupProjectSuspendedWarning.ShowPopup(zIndex);
                            PageBase.StopProcessing = true;
                            PageBase.IsPageDirty = false;
                        }
                    }
                }
                else if (Mode == ProjectWarningMode.Inventory && CompanyId > 0)
                {
                    if (!Support.CanAccessInventory(CompanyId))
                    {
                        InitializePopup();
                        ShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                        PageBase.StopProcessing = true;
                        PageBase.IsPageDirty = false;
                    }
                    else if (!IsCompanySuspended && (GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || GetBL<CompanyBL>().IsCompanySuspended(CompanyId)))
                    {
                        InitializePopup();
                        popupProjectSuspendedWarning.Title = "This Company is currently Suspended";
                        popupProjectSuspendedWarning.ShowPopup(zIndex);
                        PageBase.StopProcessing = true;
                        PageBase.IsPageDirty = false;
                        IsProjectSuspended = true;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the popup.
        /// </summary>
        private void InitializePopup()
        {
            ProjectArchive projectArchive = null;
            if (ProjectId > 0)
            {
                projectArchive = GetBL<ProjectBL>().GetProjectArchive(ProjectId);
            }
            else
            {
                projectArchive = GetBL<ProjectBL>().GetLatestProjectArchiveByCompanyId(CompanyId, UserID);
            }

            if (projectArchive != null)
            {
                User user = GetBL<PersonalBL>().GetUser(projectArchive.ProjectClosedBy);
                if (user != null)
                {
                    lblClosedBy.Text = string.Concat(user.FirstName, " ", user.LastName);
                    lblClosedByInventory.Text = string.Concat(user.FirstName, " ", user.LastName);
                }
            }

            if (CompanyId > 0)
            {
                Data.Company company = GetBL<CompanyBL>().GetCompany(CompanyId);
                if (company != null)
                {
                    lblCompanyName.Text = company.CompanyName;
                    lblCompanyNameInventory1.Text = company.CompanyName;
                    lblCompanyNameInventory2.Text = company.CompanyName;
                }

                User primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyId);
                if (primaryAdmin != null)
                {
                    string primaryAdminLink = string.Concat("<a href='mailto:", primaryAdmin.Email1, "'>", primaryAdmin.FirstName, " ", primaryAdmin.LastName, "</a>");
                    ltrlPrimaryAdmin.Text = primaryAdminLink;
                    ltrlPrimaryAdminInventory.Text = primaryAdminLink;
                    ltrlPrimaryAdminProjectSuspended.Text = primaryAdminLink;
                }
            }
        }

        /// <summary>
        /// Initializes the error popups.
        /// </summary>
        private void InitializeErrorPopups()
        {
            #region popupItemNotVisible

            User inventoryAdmin = this.GetBL<InventoryBL>().GetInventoryAdmin(this.CompanyId);
            if (inventoryAdmin != null)
            {
                lnkContactInventoryAdmin.Text = string.Format("{0} {1}", inventoryAdmin.FirstName, inventoryAdmin.LastName);
                lnkContactInventoryAdmin.NavigateUrl = string.Format("mailto:{0}", inventoryAdmin.Email1);
            }

            lnkOnVisibility.NavigateUrl = string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId);

            #endregion popupItemNotVisible
        }

        #endregion Methods

        #region Public Methods

        /// <summary>
        /// Shows the error popup.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="shouldNavigateToHome">if set to <c>true</c> [should navigate to home].</param>
        public void ShowErrorPopup(ErrorCodes errorCode, bool shouldNavigateToHome = false)
        {
            switch (errorCode)
            {
                case ErrorCodes.NoEditPermissionForInventory:
                    btnOk.Visible = !shouldNavigateToHome;
                    lnkOk.Visible = shouldNavigateToHome;
                    popupNoEditPermission.ShowPopup(1001);
                    break;

                case ErrorCodes.ItemNotVisible:
                    popupItemNotVisible.ShowPopup(1001);
                    break;
            }
        }

        #endregion Public Methods
    }
}