using StageBitz.Common;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// Delegate for raise event when a invitation accepted
    /// </summary>
    public delegate void InvitationAccepted();

    /// <summary>
    /// User control for Project List.
    /// </summary>
    public partial class ProjectList : UserControlBase
    {
        #region Events

        /// <summary>
        /// Raise when a invitation accepted
        /// </summary>
        public InvitationAccepted InvitationAccepted;

        #endregion Events

        #region Fields

        /// <summary>
        /// The primary admin code identifier var
        /// </summary>
        private int PrimaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");

        /// <summary>
        /// The secondary admin code identifier var
        /// </summary>
        private int SecondaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");

        #endregion Fields

        #region Enums

        /// <summary>
        /// Enum for view mode
        /// </summary>
        public enum ViewMode { UserDashboard, CompanyDashboard }

        #endregion Enums

        #region Properties

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
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public ViewMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = ViewMode.UserDashboard;
                }

                return (ViewMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user is company admin.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user is company admin; otherwise, <c>false</c>.
        /// </value>
        private bool IsCompanyAdmin
        {
            get
            {
                if (ViewState["IsCompanyAdmin"] == null)
                {
                    ViewState["IsCompanyAdmin"] = Support.IsCompanyAdministrator(CompanyId);
                }

                return (bool)ViewState["IsCompanyAdmin"];
            }
            set
            {
                ViewState["IsCompanyAdmin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the active project count.
        /// </summary>
        /// <value>
        /// The active project count.
        /// </value>
        public int ActiveProjectCount
        {
            get
            {
                if (ViewState["ActiveProjectCount"] == null)
                {
                    ViewState["ActiveProjectCount"] = 0;
                }

                return (int)ViewState["ActiveProjectCount"];
            }
            set
            {
                ViewState["ActiveProjectCount"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            divNotification.Visible = false;

            if (!IsPostBack)
            {
                switch (DisplayMode)
                {
                    case ViewMode.UserDashboard:
                        displaySettings.Module = ListViewDisplaySettings.ViewSettingModule.UserDashboardProjectList;
                        displaySettings.LoadControl();
                        break;

                    case ViewMode.CompanyDashboard:
                        displaySettings.Module = ListViewDisplaySettings.ViewSettingModule.CompanyDashBoardProjectList;
                        displaySettings.LoadControl();
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvProjects_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ProjectListInfo project = e.Item.DataItem as ProjectListInfo;
                ProjectBL projectBL = GetBL<ProjectBL>();

                //INITIALIZATION
                string projectName = (project.ProjectName != null) ? Support.TruncateString(project.ProjectName, 20) : string.Empty;
                string companyName = (project.CompanyName != null) ? Support.TruncateString(project.CompanyName, 20) : string.Empty;

                PlaceHolder plcProjectMemberView = (PlaceHolder)e.Item.FindControl("plcProjectMemberView");
                PlaceHolder plcInvitationView = (PlaceHolder)e.Item.FindControl("plcInvitationView");
                PlaceHolder plcClosedProjectsView = (PlaceHolder)e.Item.FindControl("plcClosedProjectsView");
                if (projectBL.IsProjectClosed(project.ProjectId))
                {
                    plcProjectMemberView.Visible = false;
                    plcInvitationView.Visible = false;
                    plcClosedProjectsView.Visible = true;

                    HyperLink lnkProjectClosed = (HyperLink)e.Item.FindControl("lnkProjectClosed");
                    Label lblProjectClosed = (Label)e.Item.FindControl("lblProjectClosed");
                    Label lblClosedDate = (Label)e.Item.FindControl("lblClosedDate");
                    Label lblClosedBy = (Label)e.Item.FindControl("lblClosedBy");

                    if (lnkProjectClosed != null && lblProjectClosed != null)
                    {
                        lnkProjectClosed.Text = lblProjectClosed.Text = projectName;
                        int projectId = project.ProjectId;
                        if (project.IsCompanyAdmin || Support.CanAccessProject(projectId))
                        {
                            lnkProjectClosed.NavigateUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                            lblProjectClosed.Visible = false;
                            lnkProjectClosed.Visible = true;
                        }
                        else
                        {
                            lblProjectClosed.Visible = true;
                            lnkProjectClosed.Visible = false;
                        }
                    }

                    if (lblClosedDate != null)
                    {
                        lblClosedDate.Text = project.ClosedOn.HasValue ? Utils.FormatDatetime(project.ClosedOn.Value, false) : string.Empty;
                    }

                    if (lblClosedBy != null)
                    {
                        lblClosedBy.Text = Support.TruncateString(project.ClosedByName, 10);
                        lblClosedBy.ToolTip = project.ClosedByName;
                    }
                }
                else
                {
                    plcClosedProjectsView.Visible = false;

                    if (project.InvitationId == 0)
                    {
                        #region Project that this user is a member of

                        plcProjectMemberView.Visible = true;
                        plcInvitationView.Visible = false;

                        HyperLink lnkProject = (HyperLink)e.Item.FindControl("lnkBtnProject");
                        HyperLink lnkCompany = (HyperLink)e.Item.FindControl("lnkBtnCompany");
                        Label lblProject = (Label)e.Item.FindControl("lblProject");

                        //Set Links
                        if (lnkProject != null && lblProject != null)
                        {
                            lnkProject.Text = lblProject.Text = projectName;
                            int projectId = project.ProjectId;
                            if (project.IsCompanyAdmin || Support.CanAccessProject(projectId))
                            {
                                lnkProject.NavigateUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                                lblProject.Visible = false;
                                lnkProject.Visible = true;
                            }
                            else
                            {
                                lblProject.Visible = true;
                                lnkProject.Visible = false;
                            }
                        }

                        #region Company link

                        if (lnkCompany != null)
                        {
                            //If it is in Userdashboard and if not a Company Admin, show the Literal.
                            if (DisplayMode == ViewMode.UserDashboard)
                            {
                                HtmlGenericControl spnCompany = (HtmlGenericControl)e.Item.FindControl("spnCompany");
                                //check if it is a company admin.
                                if (project.IsCompanyAdmin)
                                {
                                    lnkCompany.Text = string.Format("({0})", companyName);
                                    lnkCompany.NavigateUrl = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                                    spnCompany.Visible = false;
                                }
                                else
                                {
                                    spnCompany.InnerText = string.Format("({0})", companyName);
                                    lnkCompany.Visible = false;
                                }
                            }
                            else if (DisplayMode == ViewMode.CompanyDashboard)
                            {
                                lnkCompany.Text = string.Format("({0})", companyName);
                                lnkCompany.NavigateUrl = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                            }
                        }

                        #endregion Company link

                        #region Item counts

                        Literal litItems = (Literal)e.Item.FindControl("litItems");
                        Literal litCompleted = (Literal)e.Item.FindControl("litCompleted");
                        Literal litInProgress = (Literal)e.Item.FindControl("litInProgress");
                        Literal litNotstarted = (Literal)e.Item.FindControl("litNotstarted");

                        //Validate data before assigning
                        int itemCount = (project.ItemCount != null) ? project.ItemCount : 0;
                        int completeItemCount = (project.CompletedItemCount != null) ? project.CompletedItemCount : 0;
                        int inProgressItemCount = (project.InProgressItemCount != null) ? project.InProgressItemCount : 0;
                        int notStartedItemCount = (project.NotStartedItemCount != null) ? project.NotStartedItemCount : 0;

                        //Set literals
                        if (itemCount == 1)
                            litItems.Text = string.Format("{0} Item Brief", itemCount);
                        else
                            litItems.Text = string.Format("{0} Item Briefs", itemCount);

                        litCompleted.Text = (litCompleted != null) ? string.Format("{0} Completed", completeItemCount) : string.Empty;
                        litInProgress.Text = (litInProgress != null) ? string.Format("{0} In Progress", inProgressItemCount) : string.Empty;
                        litNotstarted.Text = (litNotstarted != null) ? string.Format("{0} Not Started", notStartedItemCount) : string.Empty;

                        #endregion Item counts

                        #region Notifications

                        HtmlGenericControl divNotificationArea = (HtmlGenericControl)e.Item.FindControl("divNotificationArea");
                        HtmlAnchor lnkNotificationCount = (HtmlAnchor)e.Item.FindControl("lnkNotificationCount");

                        if (project.NotificationCount > 0 && DisplayMode == ViewMode.UserDashboard)
                        {
                            lnkNotificationCount.Visible = true;
                            lnkNotificationCount.InnerText = project.NotificationCount.ToString();
                            lnkNotificationCount.HRef = string.Format("~/Project/ProjectNotifications.aspx?projectid={0}", project.ProjectId);
                        }
                        else
                        {
                            lnkNotificationCount.Visible = false;
                        }

                        #endregion Notifications

                        #region Warning Icon

                        if (project.IsCompanyAdmin)
                        {
                            HtmlGenericControl divProjectWarning = (HtmlGenericControl)e.Item.FindControl("divProjectWarning");
                            ProjectStatusHandler.ProjectWarningInfo warningInfo = ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == Support.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);
                            if (warningInfo.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.NoWarning)
                            {
                                divProjectWarning.Visible = false;
                            }
                            else
                            {
                                divProjectWarning.Visible = true;
                            }
                        }

                        #endregion Warning Icon

                        #region Project Suspention Icon

                        HtmlGenericControl divProjectSuspended = (HtmlGenericControl)e.Item.FindControl("divProjectSuspended");
                        divProjectSuspended.Visible = (project.ProjectStatusCodeId == Support.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId);

                        #endregion Project Suspention Icon

                        #endregion Project that this user is a member of
                    }
                    else
                    {
                        //This only occurs in user dashboard for invited projects

                        #region Invited project

                        plcProjectMemberView.Visible = false;
                        plcInvitationView.Visible = true;

                        HyperLink lnkProject = (HyperLink)e.Item.FindControl("lnkBtnProjectInvt");
                        HyperLink lnkCompany = (HyperLink)e.Item.FindControl("lnkBtnCompanyInvt");
                        Literal litProjectName = (Literal)e.Item.FindControl("litProjectName");
                        Literal litCompanyName = (Literal)e.Item.FindControl("litCompanyName");

                        #region Project/Company links

                        //check if it is a company admin.
                        if (project.IsCompanyAdmin)
                        {
                            lnkProject.Text = string.Format("({0})", projectName);
                            lnkProject.NavigateUrl = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                            litProjectName.Visible = false;
                        }
                        else //If not a Company Admin, show the Literal.
                        {
                            litProjectName.Visible = true;
                            litProjectName.Text = projectName;
                            lnkProject.Visible = false;
                        }

                        if (project.IsCompanyAdmin)
                        {
                            lnkCompany.Text = string.Format("({0})", companyName);
                            lnkCompany.NavigateUrl = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                            litCompanyName.Visible = false;
                        }
                        else
                        {
                            litCompanyName.Visible = true;
                            litCompanyName.Text = companyName;
                            lnkCompany.Visible = false;
                        }

                        #endregion Project/Company links

                        LinkButton lnkbtnViewInvite = (LinkButton)e.Item.FindControl("lnkbtnViewInvite");
                        lnkbtnViewInvite.CommandArgument = project.InvitationId.ToString();

                        #endregion Invited project
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvProjects_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (e.CommandName == "ViewInvite")
                {
                    int invitationId = int.Parse(e.CommandArgument.ToString());

                    invitationViewer.ShowInvitation(invitationId);
                }
            }
        }

        /// <summary>
        /// Handles the InvitationStatusChanged event of the invitationViewer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InvitationStatusChangedEventArgs"/> instance containing the event data.</param>
        protected void invitationViewer_InvitationStatusChanged(object sender, InvitationStatusChangedEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                divNotification.Visible = true;

                if (e.Accepted)
                {
                    divNotification.InnerText = "You have accepted the project invitation.";
                }
                else
                {
                    divNotification.InnerText = "You have declined the project invitation.";
                }

                LoadData();

                if (InvitationAccepted != null) //inform it to dashboard
                {
                    InvitationAccepted();
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvProjectInvitations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvProjectInvitations_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (e.CommandName == "ViewInvite")
                {
                    int invitationId = int.Parse(e.CommandArgument.ToString());

                    invitationViewer.ShowInvitation(invitationId);
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_ItemDataBound(object sender, GridItemEventArgs e)
        {
            //Set company name, set project name, and set those links
            if (e.Item is GridDataItem)
            {
                ProjectListInfo project = e.Item.DataItem as ProjectListInfo;

                HtmlAnchor lnkProject = (HtmlAnchor)e.Item.FindControl("lnkProject");
                HtmlAnchor lnkCompany = (HtmlAnchor)e.Item.FindControl("lnkCompany");
                Literal litCompany = (Literal)e.Item.FindControl("litCompany");
                Label lblProject = (Label)e.Item.FindControl("lblProject");
                //INITIALIZATION
                string projectName = (project.ProjectName != null) ? Support.TruncateString(project.ProjectName, 20) : string.Empty;
                string companyName = (project.CompanyName != null) ? Support.TruncateString(project.CompanyName, 20) : string.Empty;

                if (lnkProject != null)
                {
                    lnkProject.HRef = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                    lnkProject.InnerText = lblProject.Text = projectName;

                    if (project.IsCompanyAdmin || Support.CanAccessProject(project.ProjectId))
                    {
                        lblProject.Visible = false;
                        lnkProject.Visible = true;
                    }
                    else
                    {
                        lblProject.Visible = true;
                        lnkProject.Visible = false;
                    }
                }

                if (lnkCompany != null && litCompany != null)
                {
                    lnkCompany.HRef = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                    lnkCompany.InnerText = companyName;
                    litCompany.Text = companyName;
                }

                if (!project.IsCompanyAdmin)
                {
                    lnkCompany.Visible = false;
                    litCompany.Visible = true;
                }

                #region Notifications

                HtmlAnchor lnkNotificationCount = (HtmlAnchor)e.Item.FindControl("lnkNotificationCount");

                if (project.NotificationCount > 0 && DisplayMode == ViewMode.UserDashboard)
                {
                    lnkNotificationCount.Visible = true;
                    lnkNotificationCount.InnerText = project.NotificationCount.ToString();
                    lnkNotificationCount.HRef = string.Format("~/Project/ProjectNotifications.aspx?projectid={0}", project.ProjectId);
                }
                else
                {
                    lnkNotificationCount.Visible = false;
                }

                #endregion Notifications

                #region Warning Icon

                if (DisplayMode == ViewMode.CompanyDashboard && project.IsCompanyAdmin)
                {
                    HtmlGenericControl divProjectWarning = (HtmlGenericControl)e.Item.FindControl("divProjectWarning");
                    ProjectStatusHandler.ProjectWarningInfo warningInfo = ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == Support.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);
                    if (warningInfo.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.NoWarning)
                    {
                        divProjectWarning.Visible = false;
                    }
                    else
                    {
                        divProjectWarning.Visible = true;
                    }
                }

                #endregion Warning Icon

                #region Project Suspention Icon

                HtmlGenericControl divProjectSuspended = (HtmlGenericControl)e.Item.FindControl("divProjectSuspended");
                divProjectSuspended.Visible = (project.ProjectStatusCodeId == Support.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId);

                #endregion Project Suspention Icon
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvClosedProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvClosedProjects_ItemDataBound(object sender, GridItemEventArgs e)
        {
            //Set company name, set project name, and set those links
            if (e.Item is GridDataItem)
            {
                ProjectListInfo project = e.Item.DataItem as ProjectListInfo;

                HtmlAnchor lnkProject = (HtmlAnchor)e.Item.FindControl("lnkProject");
                HtmlAnchor lnkCompany = (HtmlAnchor)e.Item.FindControl("lnkCompany");
                Literal litCompany = (Literal)e.Item.FindControl("litCompany");
                Label lblProject = (Label)e.Item.FindControl("lblProject");
                //INITIALIZATION
                string projectName = (project.ProjectName != null) ? Support.TruncateString(project.ProjectName, 20) : string.Empty;
                string companyName = (project.CompanyName != null) ? Support.TruncateString(project.CompanyName, 20) : string.Empty;

                if (lnkProject != null)
                {
                    lnkProject.HRef = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                    lnkProject.InnerText = lblProject.Text = projectName;

                    if (project.IsCompanyAdmin || Support.CanAccessProject(project.ProjectId))
                    {
                        lblProject.Visible = false;
                        lnkProject.Visible = true;
                    }
                    else
                    {
                        lblProject.Visible = true;
                        lnkProject.Visible = false;
                    }
                }

                if (lnkCompany != null && litCompany != null)
                {
                    lnkCompany.HRef = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                    lnkCompany.InnerText = companyName;
                    litCompany.Text = companyName;
                }

                if (!project.IsCompanyAdmin)
                {
                    lnkCompany.Visible = false;
                    litCompany.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvProjectInvitations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvProjectInvitations_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                ProjectListInfo project = e.Item.DataItem as ProjectListInfo;

                HtmlAnchor lnkProject = (HtmlAnchor)e.Item.FindControl("lnkProject");
                HtmlAnchor lnkCompany = (HtmlAnchor)e.Item.FindControl("lnkCompany");
                LinkButton lnkbtnViewInvite = (LinkButton)e.Item.FindControl("lnkbtnViewInvite");
                Literal litCompany = (Literal)e.Item.FindControl("litCompany");
                Literal litProject = (Literal)e.Item.FindControl("litProject");
                //INITIALIZATION
                string projectName = (project.ProjectName != null) ? Support.TruncateString(project.ProjectName, 20) : string.Empty;
                string companyName = (project.CompanyName != null) ? Support.TruncateString(project.CompanyName, 20) : string.Empty;

                lnkbtnViewInvite.CommandArgument = project.InvitationId.ToString();

                if (lnkProject != null)
                {
                    lnkProject.HRef = string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId);
                    lnkProject.InnerText = projectName;
                    litProject.Text = projectName;
                }

                if (lnkCompany != null)
                {
                    lnkCompany.HRef = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", project.CompanyId);
                    lnkCompany.InnerText = companyName;
                    litCompany.Text = companyName;
                }

                if (!Support.IsCompanyAdministrator(project.CompanyId))
                {
                    lnkCompany.Visible = false;
                    litCompany.Visible = true;
                    lnkProject.Visible = false;
                    litProject.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the DisplayModeChanged event of the displaySettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void displaySettings_DisplayModeChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadData();
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            int completedItemStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            int projectInvitationTypeCodeId = Support.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");
            int pendingInvitationStatusCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");

            ProjectBL projectBL = GetBL<ProjectBL>();

            if (DisplayMode == ViewMode.UserDashboard)
            {
                #region Load Project List for User Dashboard

                divGridViewClosedProjects.Visible = false;

                var projects = projectBL.GetProjectListUserDashboardActiveProjectList(UserID);
                var invitedProjects = projectBL.GetProjectListUserDashboardInvitedProjectList(UserID);

                this.ActiveProjectCount = projects.Count;

                if (projects.Count() == 1)
                    lblLeftHeader.Text = string.Format("You have {0} Project", projects.Count());
                else
                    lblLeftHeader.Text = string.Format("You have {0} Projects", projects.Count());

                //Populate projects section
                //Sort order: display invitations first, then sort by project name

                switch (displaySettings.DisplayMode)
                {
                    case ListViewDisplaySettings.ViewSettingValue.ThumbnailView:

                        //Hide the grid and show list
                        divGridViewProjects.Visible = false;
                        divProjectInvitations.Visible = false;
                        divListViewProjects.Visible = true;

                        lvProjects.DataSource = projects.Union(invitedProjects).OrderBy(p => p.InvitationId > 0 ? 0 : 1).ThenBy(p => p.ProjectName);
                        lvProjects.DataBind();

                        break;

                    case ListViewDisplaySettings.ViewSettingValue.ListView:

                        //Hide the list and show table grid
                        divGridViewProjects.Visible = true;
                        divListViewProjects.Visible = false;

                        gvProjects.DataSource = projects.OrderBy(p => p.ProjectName);
                        gvProjects.DataBind();

                        //If invitedprojects not null show it, otherwise hide it
                        if (invitedProjects.Count() != 0)
                        {
                            divProjectInvitations.Visible = true;
                            gvProjectInvitations.DataSource = invitedProjects;//invitedProjects
                            gvProjectInvitations.DataBind();
                            h4ActiveProjects.Visible = true;
                        }
                        else
                        {
                            divProjectInvitations.Visible = false;
                            h4ActiveProjects.Visible = false;
                        }

                        break;

                    default:
                        break;
                }

                if (projects.Count() + invitedProjects.Count() > 0)
                {
                    //1 or more projects
                    divNoProjectsUserDashBoard.Visible = false;
                    this.Visible = true;
                }
                else
                {
                    //0 projects
                    this.Visible = false;
                    divNoProjectsUserDashBoard.Visible = true;
                    divGridViewProjects.Visible = false;
                    divListViewProjects.Visible = false;
                }

                #endregion Load Project List for User Dashboard
            }
            else if (DisplayMode == ViewMode.CompanyDashboard)
            {
                #region Load Project List for Company Dashboard

                int projectClosedCodeId = Support.GetCodeIdByCodeValue("ProjectStatus", "CLOSED");

                gvClosedProjects.Columns[1].Visible = false;
                gvProjectInvitations.Columns[1].Visible = false;
                gvProjects.Columns[1].Visible = false;

                HelpTip1.Visible = false;
                hLinkAddNewProject.Visible = spanCreateNewProject.Visible = !this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId)
                        && Support.IsCompanyAdministrator(CompanyId) && !this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);
                hLinkAddNewProject.CompanyId = CompanyId;
                hLinkAddNewProject.LoadData();

                var projects = projectBL.GetProjectListCompanyDashboardActiveProjectList(UserID, CompanyId);
                this.ActiveProjectCount = projects.Count;

                var closedProjects = projectBL.GetProjectListCompanyDashboardClosedProjectList(UserID, CompanyId);

                string companyName = string.Empty;

                var company = GetBL<CompanyBL>().GetCompany(CompanyId);

                if (company != null)
                {
                    if (projects.Count() == 1)
                        lblLeftHeader.Text = string.Format("{0} has {1} active Project", Support.TruncateString(company.CompanyName, 20), projects.Count());
                    else
                        lblLeftHeader.Text = string.Format("{0} has {1} active Projects", Support.TruncateString(company.CompanyName, 20), projects.Count());
                }

                switch (displaySettings.DisplayMode)
                {
                    case ListViewDisplaySettings.ViewSettingValue.ThumbnailView:

                        //Hide the grid and show list
                        divGridViewProjects.Visible = false;
                        divProjectInvitations.Visible = false;
                        divListViewProjects.Visible = true;
                        divGridViewClosedProjects.Visible = false;

                        var allProjects = projects.Union(closedProjects).OrderBy(p => p.ClosedByUserId > 0 ? 1 : 0).ThenBy(p => p.ProjectName);

                        lvProjects.DataSource = allProjects;
                        lvProjects.DataBind();

                        if (projects.Count() > 0)
                        {
                            //1 or more projects
                            divNoProjects.Visible = false;
                        }
                        else
                        {
                            //0 projects
                            divNoProjects.Visible = true;
                            divGridViewProjects.Visible = false;
                            divProjectInvitations.Visible = false;

                            if (allProjects.Count() == 0)
                            {
                                divListViewProjects.Visible = false;
                            }
                        }

                        break;

                    case ListViewDisplaySettings.ViewSettingValue.ListView:

                        //Hide the grid and show list
                        divGridViewProjects.Visible = true;
                        divProjectInvitations.Visible = false;
                        divListViewProjects.Visible = false;

                        gvProjects.DataSource = projects.OrderBy(p => p.ProjectName);
                        gvProjects.DataBind();

                        if (closedProjects.Count > 0)
                        {
                            divGridViewClosedProjects.Visible = true;
                            gvClosedProjects.DataSource = closedProjects.OrderBy(p => p.ProjectName);
                            gvClosedProjects.DataBind();
                            h4ActiveProjects.Visible = true;
                        }
                        else
                        {
                            divGridViewClosedProjects.Visible = false;
                            h4ActiveProjects.Visible = false;
                        }

                        if (projects.Count() > 0)
                        {
                            //1 or more projects
                            divNoProjects.Visible = false;
                        }
                        else
                        {
                            //0 projects
                            divNoProjects.Visible = true;
                            divGridViewProjects.Visible = false;
                            divProjectInvitations.Visible = false;
                            divListViewProjects.Visible = false;
                        }

                        break;

                    default:
                        break;
                }

                #endregion Load Project List for Company Dashboard
            }

            if (this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId))
            {
                lnkCreateNewProject.Visible = false;
            }
            else
            {
                lnkCreateNewProject.Visible = true;
                lnkCreateNewProject.CompanyId = CompanyId;
                lnkCreateNewProject.LoadData();
            }

            upnlProjectList.Update();
        }

        /// <summary>
        /// Initializes the welcome tool tips.
        /// </summary>
        /// <param name="freeTrialOption">The free trial option.</param>
        public void InitializeWelcomeToolTips(WelcomeMessage.FreeTrialOption freeTrialOption)
        {
            switch (freeTrialOption)
            {
                case WelcomeMessage.FreeTrialOption.None:
                case WelcomeMessage.FreeTrialOption.ExpectingInvitation:
                case WelcomeMessage.FreeTrialOption.CreateInventory:
                    return;

                case WelcomeMessage.FreeTrialOption.CreateNewProject:
                case WelcomeMessage.FreeTrialOption.CreateProjectAndInventory:
                    helptipProjectLink.Visible = true;
                    break;
            }
        }

        #endregion Public Methods
    }
}