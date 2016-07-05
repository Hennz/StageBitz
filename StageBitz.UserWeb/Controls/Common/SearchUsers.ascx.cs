using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Location;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for search users in stagebitz.
    /// </summary>
    public partial class SearchUsers : UserControlBase
    {
        #region Events

        /// <summary>
        /// Delegate for inform company inventory to show error popup.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="shouldNavigateToHome">if set to <c>true</c> [should navigate to home].</param>
        public delegate void InformCompanyInventoryToShowErrorPopup(ErrorCodes errorCode, bool shouldNavigateToHome);

        /// <summary>
        /// Occurs when [invitation sent].
        /// </summary>
        public event EventHandler InvitationSent;

        /// <summary>
        /// The inform company inventory to show error popup
        /// </summary>
        public InformCompanyInventoryToShowErrorPopup OnInformCompanyInventoryToShowErrorPopup;

        #endregion Events

        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            ProjectTeam,
            CompanyAdministrators,
            InventoryTeam
        }

        #endregion Enums

        private string StageBitzUrl = Support.GetSystemUrl();

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
        /// Gets or sets the selected user identifier.
        /// </summary>
        /// <value>
        /// The selected user identifier.
        /// </value>
        public int SelectedUserId
        {
            get
            {
                if (ViewState["SelectedUserId"] == null)
                {
                    ViewState["SelectedUserId"] = 0;
                }

                return (int)ViewState["SelectedUserId"];
            }
            set
            {
                ViewState["SelectedUserId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected user email.
        /// </summary>
        /// <value>
        /// The selected user email.
        /// </value>
        public string SelectedUserEmail
        {
            get
            {
                if (ViewState["SelectedUserEmail"] == null)
                {
                    ViewState["SelectedUserEmail"] = string.Empty;
                }

                return ViewState["SelectedUserEmail"].ToString();
            }
            set
            {
                ViewState["SelectedUserEmail"] = value;
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
                    ViewState["DisplayMode"] = default(ViewMode);
                }

                return (ViewMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
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
            //Hide this allways. This will appear if needed when inviting people.
            hyperLinkAdjustLimitPopup.Visible = false;
            if (!IsPostBack)
            {
                string validationGroup = string.Concat(ucUserInvitationPopup.ClientID, "ValidationGroup");
                btnInviteProjectMember.ValidationGroup = validationGroup;
                ucUserInvitationPopup.InitializeValidationGroup(validationGroup);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (Page.IsValid && !PageBase.StopProcessing)
            {
                HideNotifications();
                divSearchResults.Visible = false;

                //If an email address has been typed, it will take priority.
                if (txtEmail.Text.Trim().Length > 0)
                {
                    SearchByEmailAddress(txtEmail.Text.Trim());
                }

                upnlSearchResults.Update();
                btnSearch.Focus();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvSearchResults_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            dynamic userData = (dynamic)e.Item.DataItem;

            ImageDisplay userThumbDisplay = (ImageDisplay)e.Item.FindControl("userThumbDisplay");
            userThumbDisplay.DocumentMediaId = (int)userData.DocumentMediaId;

            LinkButton lnkbtnUser = (LinkButton)e.Item.FindControl("lnkbtnUser");
            Literal ltrlUsername = (Literal)e.Item.FindControl("ltrlUsername");

            string userFullName = (userData.User.FirstName + " " + userData.User.LastName).Trim();
            ltrlUsername.Text = Support.TruncateString(userFullName, 15);
            if (userFullName.Length > 15)
            {
                lnkbtnUser.ToolTip = userFullName;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvSearchResults_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                HideNotifications();

                if (e.CommandName == "InviteUser")
                {
                    #region Check existing memberships and invitations

                    SelectedUserId = (int)lvSearchResults.DataKeys[e.Item.DataItemIndex]["UserId"];
                    SelectedUserEmail = string.Empty;

                    User user = this.GetBL<PersonalBL>().GetUser(SelectedUserId);
                    string userFullName = (user.FirstName + " " + user.LastName).Trim();

                    if (IsUserTeamMember(user.UserId))
                    {
                        ltrlPopupNotification.Text = string.Format("{0} ({1}) is already {2}.",
                                                    userFullName,
                                                    user.LoginName,
                                                    (DisplayMode == ViewMode.CompanyAdministrators) ? "a Team Member of this Company" :
                                                        (DisplayMode == ViewMode.InventoryTeam) ? "a Team Member of this Inventory" : "a member of this Project");
                        popupNotification.ShowPopup();
                        return;
                    }

                    Invitation pendingInvitation = GetPendingInvitationForUser(user);

                    if (pendingInvitation != null)
                    {
                        ltrlPopupNotification.Text = string.Format("{0} ({1}) has already been invited.", userFullName, user.LoginName);
                        popupNotification.ShowPopup();
                        return;
                    }

                    #endregion Check existing memberships and invitations

                    sbPackageLimitsValidation.InvitedUserEmail = user.LoginName;
                    if (DisplayMode == ViewMode.CompanyAdministrators)
                    {
                        tblCompInviteName.Visible = false;
                        ltrlCompanyAdminInvitationText.Text = string.Format("Invite '{0}' to the Company?", userFullName);
                        popupInviteCompanyAdmin.ShowPopup();
                    }
                    else if (DisplayMode == ViewMode.ProjectTeam)
                    {
                        sbPackageLimitsValidation.ProjectId = ProjectId;
                        if (sbPackageLimitsValidation.Validate())
                        {
                            ucUserInvitationPopup.CreateInviteExistingUserLayout(userFullName, user.UserId, ProjectId);
                            popupInviteProjectMember.ShowPopup();
                        }
                    }
                    else if (DisplayMode == ViewMode.InventoryTeam)
                    {
                        tblInventoryUserName.Visible = false;
                        ltrInviteInventoryUsersText.Text = string.Format("Invite '{0}' to the Company Inventory?", userFullName);
                        ResetInventoryInvitePopup();
                        int locations = sbInventoryLocationRoles.LoadData(null);
                        popupInviteInventoryUsers.ShowPopup();
                        btnInviteInventoryUsers.Enabled = locations > 0;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnInviteInventoryUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInviteInventoryUsers_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (!Page.IsValid)
                {
                    return;
                }

                InviteInventoryUsers();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnInviteProjectMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInviteProjectMember_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (!Page.IsValid || !sbPackageLimitsValidation.Validate())
                {
                    return;
                }

                HideNotifications();

                Invitation pendingInvitation = null;
                User user = null;
                if (SelectedUserId > 0)
                {
                    user = this.GetBL<PersonalBL>().GetUser(SelectedUserId);
                    //Check whether this user already has a pending invitation.
                    pendingInvitation = GetPendingInvitationForUser(user);
                }
                else
                {
                    pendingInvitation = GetPendingInvitationForEmail(SelectedUserEmail);
                }

                if (pendingInvitation != null)
                {
                    //This scenario can only occur if the user accidentaly double clicks the send button.
                    //So the popup is silently closed without doing anything.
                    divSearchResults.Visible = false;
                    popupInviteProjectMember.HidePopup();
                }
                else
                {
                    string fromUserFullName = (Support.UserFirstName + " " + Support.UserLastName).Trim();
                    string fromUserEmail = (from u in DataContext.Users where u.UserId == UserID select u.Email1).FirstOrDefault();
                    StageBitz.Data.Project project = (from p in DataContext.Projects where p.ProjectId == ProjectId select p).FirstOrDefault();
                    string projectName = project.ProjectName;
                    string companyName = (from c in DataContext.Companies where c.CompanyId == project.CompanyId select c.CompanyName).FirstOrDefault();
                    string projectRole = ucUserInvitationPopup.ProjectRole;// txtProjectRole.Text.Trim();

                    #region Create and save Invitation object

                    Invitation invitation = new Invitation();
                    DataContext.Invitations.AddObject(invitation);

                    invitation.FromUserId = UserID;

                    if (SelectedUserId > 0)
                    {
                        invitation.ToUserId = SelectedUserId;
                        invitation.ToEmail = user.Email1;
                    }
                    else
                    {
                        invitation.ToName = ucUserInvitationPopup.ProjectInvitePersonName; //txtProjInvitePersonName.Text.Trim();
                        invitation.ToEmail = SelectedUserEmail;
                    }

                    invitation.InvitationUserRoles.Add(
                        new InvitationUserRole
                        {
                            CreatedByUserId = UserID,
                            CreatedDate = Now,
                            IsActive = true,
                            LastUpdatedByUserId = UserID,
                            LastUpdatedDate = Now,
                            UserTypeCodeId = Support.GetCodeIdByCodeValue("ProjectUserTypeCode", ucUserInvitationPopup.IsProjectObserverRadioChecked ? "OBSERVER" : "STAFF")
                        }
                    );

                    invitation.InvitationTypeCodeId = GetInvitationTypeCodeId(ViewMode.ProjectTeam);
                    invitation.InvitationStatusCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");

                    invitation.ProjectRole = projectRole;
                    invitation.RelatedTable = StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Projects;
                    invitation.RelatedId = ProjectId;
                    invitation.CreatedByUserId = invitation.LastUpdatedByUserId = UserID;
                    invitation.CreatedDate = invitation.LastUpdatedDate = Now;
                    invitation.CanSeeProjectBudgetSummary = ucUserInvitationPopup.IsBudgetSummaryChecked;
                    DataContext.SaveChanges();

                    #endregion Create and save Invitation object

                    string toUserFullName = string.Empty;

                    if (SelectedUserId > 0)
                    {
                        toUserFullName = (user.FirstName + " " + user.LastName).Trim();
                        string toEmail = user.Email1;
                        string dashboardUrl = StageBitzUrl + "/" + "Default.aspx";

                        if (!user.IsActive)
                        {
                            dashboardUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString())));
                        }

                        //Send invitation email for existing user
                        EmailSender.StageBitzUrl = Support.GetSystemUrl();
                        EmailSender.InviteProjectTeamExistingUser(
                            (ucUserInvitationPopup.IsProjectObserverRadioChecked ? EmailSender.ProjectTeamUserType.Observer : EmailSender.ProjectTeamUserType.Staff),
                            toEmail, toUserFullName, fromUserFullName, fromUserEmail, companyName, projectName, projectRole, dashboardUrl);

                        divInviteSent.InnerText = string.Format("{0} ({1}) has been invited to the project.", Support.TruncateString(toUserFullName, 50), Support.TruncateString(toEmail, 50));
                    }
                    else
                    {
                        toUserFullName = ucUserInvitationPopup.ProjectInvitePersonName;//txtProjInvitePersonName.Text.Trim();
                        string toEmail = SelectedUserEmail;
                        string invitationUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString())));

                        //Send invitation email for new user
                        EmailSender.StageBitzUrl = Support.GetSystemUrl();
                        EmailSender.InviteProjectTeamNewUser(
                            (ucUserInvitationPopup.IsProjectObserverRadioChecked ? EmailSender.ProjectTeamUserType.Observer : EmailSender.ProjectTeamUserType.Staff),
                            toEmail, toUserFullName, fromUserFullName, fromUserEmail, companyName, projectName, projectRole, invitationUrl);
                        divInviteSent.InnerText = string.Format("'{0}' has been invited to the project.", Support.TruncateString(SelectedUserEmail, 50));
                    }

                    #region Notifications

                    Notification nf = new Notification();
                    nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJTEAM");
                    nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "ADD");
                    nf.RelatedId = ProjectId;
                    nf.ProjectId = ProjectId;
                    nf.Message = string.Format("{0} invited {1} to the Project.", Support.UserFullName, toUserFullName);
                    nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                    nf.CreatedDate = nf.LastUpdatedDate = Now;
                    DataContext.Notifications.AddObject(nf);
                    DataContext.SaveChanges();

                    #endregion Notifications

                    divInviteSent.Visible = true;
                    divSearchResults.Visible = false;
                    popupInviteProjectMember.HidePopup();
                }

                if (InvitationSent != null)
                {
                    InvitationSent(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnInviteCompanyAdmin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInviteCompanyAdmin_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (!Page.IsValid || !sbPackageLimitsValidation.Validate())
                {
                    return;
                }

                HideNotifications();

                Invitation pendingInvitation = null;
                User user = null;
                if (SelectedUserId > 0)
                {
                    user = this.GetBL<PersonalBL>().GetUser(SelectedUserId);
                    //Check whether this user already has a pending invitation.
                    pendingInvitation = GetPendingInvitationForUser(user);
                }
                else
                {
                    pendingInvitation = GetPendingInvitationForEmail(SelectedUserEmail);
                }

                if (pendingInvitation != null)
                {
                    //This scenario can only occur if the user accidentaly double clicks the send button.
                    //So the popup is silently closed without doing anything.
                    divSearchResults.Visible = false;
                    //ucUserInvitationPopup.HideInivitePopup();
                    popupInviteProjectMember.HidePopup();
                }
                else
                {
                    string toUserFullName = txtCompInvitePersonName.Text.Trim();
                    string toEmail = SelectedUserEmail;
                    string fromUserFullName = (Support.UserFirstName + " " + Support.UserLastName).Trim();
                    string fromUserEmail = (from u in DataContext.Users where u.UserId == UserID select u.Email1).FirstOrDefault();
                    string companyName = (from c in DataContext.Companies where c.CompanyId == CompanyId select c.CompanyName).FirstOrDefault();

                    #region Create and save Invitation object

                    Invitation invitation = new Invitation();
                    DataContext.Invitations.AddObject(invitation);

                    invitation.FromUserId = UserID;

                    if (SelectedUserId > 0)
                    {
                        invitation.ToUserId = SelectedUserId;
                        invitation.ToEmail = user.Email1;
                    }
                    else
                    {
                        invitation.ToName = txtCompInvitePersonName.Text.Trim();
                        invitation.ToEmail = SelectedUserEmail;
                    }

                    invitation.InvitationTypeCodeId = GetInvitationTypeCodeId(ViewMode.CompanyAdministrators);
                    invitation.InvitationStatusCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
                    invitation.RelatedTable = StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies;
                    invitation.RelatedId = CompanyId;

                    invitation.CreatedByUserId = invitation.LastUpdatedByUserId = UserID;
                    invitation.CreatedDate = invitation.LastUpdatedDate = Now;

                    invitation.InvitationUserRoles.Add(
                                new InvitationUserRole
                                {
                                    CreatedByUserId = UserID,
                                    CreatedDate = Now,
                                    IsActive = true,
                                    LastUpdatedByUserId = UserID,
                                    LastUpdatedDate = Now,
                                    UserTypeCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN")
                                }
                            );

                    DataContext.SaveChanges();

                    #endregion Create and save Invitation object

                    if (SelectedUserId > 0)
                    {
                        //For existing users, we need to get it from db
                        toUserFullName = (user.FirstName + " " + user.LastName).Trim();
                        toEmail = (from u in DataContext.Users where u.UserId == SelectedUserId select u.Email1).FirstOrDefault();
                        string dashboardUrl = string.Format("{0}/Default.aspx", StageBitzUrl);
                        //For Non registered users First need to activate the account
                        if (!user.IsActive)
                        {
                            dashboardUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString())));
                        }

                        EmailSender.StageBitzUrl = StageBitzUrl;
                        EmailSender.InviteCompanyAdminExistingUser(toEmail, toUserFullName, Support.UserFirstName, fromUserFullName, fromUserEmail,
                                companyName, dashboardUrl);
                        divInviteSent.InnerText = string.Format("{0} ({1}) has been invited to the company.", Support.TruncateString(toUserFullName, 50), toEmail);
                    }
                    else
                    {
                        string invitationCode = HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString()));
                        string invitationUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, invitationCode);
                        StageBitz.Common.EmailSender.StageBitzUrl = StageBitzUrl;
                        EmailSender.InviteCompanyAdminNewUser(toEmail, toUserFullName, Support.UserFirstName, fromUserFullName, fromUserEmail, companyName,
                                invitationUrl);
                        divInviteSent.InnerText = string.Format("{0} ({1}) has been invited to the company.", Support.TruncateString(toUserFullName, 50), toEmail);
                    }

                    divInviteSent.Visible = true;
                    divSearchResults.Visible = false;
                    popupInviteCompanyAdmin.HidePopup();
                }
                if (InvitationSent != null)
                {
                    InvitationSent(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkbtnInviteToStageBitz control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkbtnInviteToStageBitz_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //Check whether the email already has a pending invitation.
                Invitation pendingInvitation = GetPendingInvitationForEmail(SelectedUserEmail);
                if (pendingInvitation != null)
                {
                    ltrlPopupNotification.Text = string.Format("'{0}' has already been invited.", Support.TruncateString(SelectedUserEmail, 30));
                    popupNotification.ShowPopup();
                }
                else
                {
                    sbPackageLimitsValidation.InvitedUserEmail = SelectedUserEmail;
                    if (DisplayMode == ViewMode.CompanyAdministrators)
                    {
                        tblCompInviteName.Visible = true;
                        ltrlCompanyAdminInvitationText.Text = string.Format("Invite '{0}' to the Company?", Support.TruncateString(SelectedUserEmail, 30));
                        txtCompInvitePersonName.Text = string.Empty;
                        txtCompInvitePersonName.Focus();
                        popupInviteCompanyAdmin.ShowPopup();
                    }
                    else if (DisplayMode == ViewMode.ProjectTeam)
                    {
                        sbPackageLimitsValidation.ProjectId = ProjectId;
                        if (sbPackageLimitsValidation.Validate())
                        {
                            ucUserInvitationPopup.CreateInviteNewUserLayout(string.Format("Invite '{0}' to the Project.", Support.TruncateString(SelectedUserEmail, 30)));
                            popupInviteProjectMember.ShowPopup();
                        }
                    }
                    else if (DisplayMode == ViewMode.InventoryTeam)
                    {
                        tblInventoryUserName.Visible = true;
                        ltrInviteInventoryUsersText.Text = string.Format("Invite '{0}' to the Company Inventory?", Support.TruncateString(SelectedUserEmail, 30));
                        ResetInventoryInvitePopup();
                        int locations = sbInventoryLocationRoles.LoadData(null);
                        popupInviteInventoryUsers.ShowPopup();
                        btnInviteInventoryUsers.Enabled = locations > 0;
                    }
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.UserLimit;
            if (DisplayMode == ViewMode.ProjectTeam)
            {
                plcProjectTeamTitle.Visible = true;
                plcCompanyAdminTitle.Visible = false;
                plcInventoryTeamTitle.Visible = false;

                int companyId = Support.GetCompanyByProjectId(ProjectId).CompanyId;
                sbPackageLimitsValidation.CompanyId = companyId;
                sbPackageLimitsValidation.LoadData();
            }
            else if (DisplayMode == ViewMode.CompanyAdministrators)
            {
                plcProjectTeamTitle.Visible = false;
                plcCompanyAdminTitle.Visible = true;
                plcInventoryTeamTitle.Visible = false;

                sbPackageLimitsValidation.CompanyId = this.CompanyId;
                sbPackageLimitsValidation.LoadData();
            }
            else if (DisplayMode == ViewMode.InventoryTeam)
            {
                sbInventoryLocationRoles.CompanyId = this.CompanyId;
                sbInventoryLocationRoles.RadioButtonGroupName = "InviteInventoryUsers";
                sbPackageLimitsValidation.Visible = false;
                plcProjectTeamTitle.Visible = false;
                plcCompanyAdminTitle.Visible = false;
                plcInventoryTeamTitle.Visible = true;
            }
        }

        /// <summary>
        /// Hides the notifications.
        /// </summary>
        public void HideNotifications()
        {
            divEmailNotFound.Visible = false;
            divNotification.Visible = false;
            divInviteSent.Visible = false;

            upnlSearchResults.Update();
        }

        /// <summary>
        /// Invites the inventory users.
        /// </summary>
        public void InviteInventoryUsers()
        {
            bool isInventoryAdmin = Utils.IsCompanyInventoryAdmin(this.CompanyId, UserID);
            CompanyUserRole highestRole = GetBL<InventoryBL>().GetHighestInventoryRole(this.CompanyId, UserID);
            bool hasStaffMemberPermission = isInventoryAdmin || (highestRole != null && highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").SortOrder);

            if (isInventoryAdmin || GetBL<InventoryBL>().IsCompanyLocationManagerAnyLocation(this.CompanyId, UserID))
            {
                HideNotifications();

                Invitation pendingInvitation = null;
                User user = null;
                if (SelectedUserId > 0)
                {
                    user = this.GetBL<PersonalBL>().GetUser(SelectedUserId);
                    //Check whether this user already has a pending invitation.
                    pendingInvitation = GetPendingInvitationForUser(user);
                }
                else
                {
                    pendingInvitation = GetPendingInvitationForEmail(SelectedUserEmail);
                }

                if (pendingInvitation != null)
                {
                    //This scenario can only occur if the user accidentaly double clicks the send button.
                    //So the popup is silently closed without doing anything.
                    divSearchResults.Visible = false;
                    //ucUserInvitationPopup.HideInivitePopup();
                    popupInviteProjectMember.HidePopup();
                }
                else
                {
                    string toUserFullName = txtInventoryUserName.Text.Trim();
                    string toEmail = SelectedUserEmail;
                    string fromUserFullName = (Support.UserFirstName + " " + Support.UserLastName).Trim();
                    string fromUserEmail = GetBL<PersonalBL>().GetUser(this.UserID).Email1;
                    string companyName = GetBL<CompanyBL>().GetCompany(this.CompanyId).CompanyName;

                    #region Create and save Invitation object

                    Invitation invitation = new Invitation();
                    DataContext.Invitations.AddObject(invitation);

                    invitation.FromUserId = UserID;

                    if (SelectedUserId > 0)
                    {
                        invitation.ToUserId = SelectedUserId;
                        invitation.ToEmail = user.Email1;
                    }
                    else
                    {
                        invitation.ToName = toUserFullName;
                        invitation.ToEmail = SelectedUserEmail;
                    }

                    invitation.InvitationTypeCodeId = GetInvitationTypeCodeId(ViewMode.InventoryTeam);
                    invitation.InvitationStatusCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
                    invitation.RelatedTable = StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies;
                    invitation.RelatedId = CompanyId;

                    invitation.CreatedByUserId = invitation.LastUpdatedByUserId = UserID;
                    invitation.CreatedDate = invitation.LastUpdatedDate = Now;
                    StringBuilder sbInviteInventoryStaffEmail = new StringBuilder();
                    StringBuilder sbInviteInventoryObserverEmail = new StringBuilder();
                    string staffHtml = string.Empty;
                    string observerHtml = string.Empty;
                    string allNoAcessHtml = string.Empty;

                    Dictionary<int, int> locationRoles = sbInventoryLocationRoles.LocationPermissions;
                    if (locationRoles.Count == 0)
                    {
                        popupInviteInventoryUsers.HidePopup();
                        if (isInventoryAdmin)
                        {
                            PageBase.ShowErrorPopup(ErrorCodes.InventoryLocationDeleted);
                        }
                        else
                        {
                            if (OnInformCompanyInventoryToShowErrorPopup != null)
                            {
                                OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, true);
                            }
                        }

                        return;
                    }

                    foreach (int locationId in locationRoles.Keys)
                    {
                        if (Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, locationId))
                        {
                            int userTypeCodeId = locationRoles[locationId];
                            invitation.InvitationUserRoles.Add(
                                        new InvitationUserRole
                                        {
                                            CreatedByUserId = UserID,
                                            CreatedDate = Now,
                                            IsActive = true,
                                            LastUpdatedByUserId = UserID,
                                            LastUpdatedDate = Now,
                                            UserTypeCodeId = userTypeCodeId,
                                            LocationId = locationId
                                        }
                                    );

                            Data.Location location = GetBL<LocationBL>().GetLocation(locationId);
                            Code inventoryStaffCode = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF");
                            Code inventoryObserverCode = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER");

                            if (userTypeCodeId == inventoryStaffCode.CodeId)
                            {
                                sbInviteInventoryStaffEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                            }
                            else if (userTypeCodeId == inventoryObserverCode.CodeId)
                            {
                                sbInviteInventoryObserverEmail.Append(string.Format("<li>{0}</li>", location.LocationName));
                            }
                        }
                        else
                        {
                            if (OnInformCompanyInventoryToShowErrorPopup != null)
                            {
                                popupInviteInventoryUsers.HidePopup();
                                OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !hasStaffMemberPermission);
                            }

                            return;
                        }
                    }

                    if (sbInviteInventoryStaffEmail.Length > 0)
                    {
                        staffHtml = string.Format(@"<p>You have been invited as Inventory Staff for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that for this location:</p>
                                                    <ul>
                                                     <li>You can now view all the bookings for your Inventory</li>
                                                     <li>You can create, edit and delete inventory Items</li>
                                                     <li>And you can still make your own bookings.</li>
                                                    </ul></div>", sbInviteInventoryStaffEmail.ToString());
                    }

                    if (sbInviteInventoryObserverEmail.Length > 0)
                    {
                        observerHtml = string.Format(@"<p>You have been invited as Inventory Observer for the following location(s):</p>
                                                    <ul>
                                                     {0}
                                                    </ul>
                                                    <div style='margin-left:25px;'><p>This means that for this location:</p>
                                                    <ul>
                                                     <li>You’ll be able to browse their Inventory</li>
                                                     <li>You can request Items from it for your own bookings.</li>
                                                    </ul></div>", sbInviteInventoryObserverEmail.ToString());
                    }

                    if (sbInviteInventoryObserverEmail.Length == 0 && sbInviteInventoryStaffEmail.Length == 0)
                    {
                        allNoAcessHtml = @"<p>You'll have Inventory Observer access to Items listed against the default Inventory location for
                                            @CompanyName. This means that you'll be able to browse these Items and also request these Items for your own bookings</p>";
                    }

                    DataContext.SaveChanges();

                    #endregion Create and save Invitation object

                    if (SelectedUserId > 0)
                    {
                        toEmail = user.Email1;
                        string dashboardUrl = string.Format("{0}/Default.aspx", StageBitzUrl);
                        //For Non registered users First need to activate the account
                        if (!user.IsActive)
                        {
                            dashboardUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString())));
                        }

                        EmailSender.StageBitzUrl = StageBitzUrl;
                        EmailSender.InviteInventoryUserExistingUser(toEmail, user.FirstName, fromUserFullName, fromUserEmail,
                                companyName, dashboardUrl, staffHtml, observerHtml, allNoAcessHtml);
                        divInviteSent.InnerText = string.Format("{0} ({1}) has been invited to the Company Inventory.", Support.TruncateString(toUserFullName, 50), toEmail);
                    }
                    else
                    {
                        string invitationCode = HttpServerUtility.UrlTokenEncode(Utils.EncryptStringAES(invitation.InvitationId.ToString()));
                        string invitationUrl = string.Format("{0}/Public/Invitation.aspx?invitationCode={1}", StageBitzUrl, invitationCode);
                        StageBitz.Common.EmailSender.StageBitzUrl = StageBitzUrl;

                        EmailSender.InviteInventoryUserNewUser(toEmail, toUserFullName, fromUserFullName, fromUserEmail,
                                companyName, invitationUrl, staffHtml, observerHtml, allNoAcessHtml);

                        divInviteSent.InnerText = string.Format("{0} ({1}) has been invited to the Company Inventory.", Support.TruncateString(toUserFullName, 50), toEmail);
                    }

                    divInviteSent.Visible = true;
                    divSearchResults.Visible = false;
                    popupInviteInventoryUsers.HidePopup();
                }

                if (InvitationSent != null)
                {
                    InvitationSent(this, EventArgs.Empty);
                }
            }
            else
            {
                if (OnInformCompanyInventoryToShowErrorPopup != null)
                {
                    popupInviteInventoryUsers.HidePopup();
                    OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory, !hasStaffMemberPermission);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Searches and displays users by the provided email address.
        /// </summary>
        private void SearchByEmailAddress(string emailAddress)
        {
            var userData = (from u in DataContext.Users
                            from mediaId in
                                (from m in DataContext.DocumentMedias
                                 where m.RelatedTableName == "User" && m.RelatedId == u.UserId && m.SortOrder == 1
                                 select m.DocumentMediaId).DefaultIfEmpty() //get user's profile picture

                            where u.LoginName.Equals(emailAddress, StringComparison.InvariantCultureIgnoreCase)
                                && u.IsActive == true
                            select new { User = u, UserId = u.UserId, DocumentMediaId = mediaId }).FirstOrDefault();

            if (userData == null)
            {
                //No stagebitz users found with the specified email address.

                //Check whether the email already has a pending invitation.
                Invitation pendingInvitation = GetPendingInvitationForEmail(emailAddress);
                if (pendingInvitation != null)
                {
                    divNotification.InnerText = string.Format("'{0}' has already been invited.", Support.TruncateString(emailAddress, 50));
                    divNotification.Visible = true;
                }
                else
                {
                    divEmailNotFound.Visible = true;
                    lnkbtnInviteToStageBitz.Text = string.Format("Invite '{0}'", Support.TruncateString(emailAddress, 50));
                    SelectedUserEmail = emailAddress;
                    User user = this.GetBL<PersonalBL>().GetUserByLogInName(emailAddress);
                    SelectedUserId = (user == null) ? 0 : user.UserId;
                }
            }
            else
            {
                List<dynamic> users = new List<dynamic>();
                users.Add(userData);
                DisplayResults(users);
            }
        }

        /// <summary>
        /// Displays the specified users in a grid.
        /// </summary>
        private void DisplayResults(List<dynamic> users)
        {
            divSearchResults.Visible = true;
            lvSearchResults.DataSource = users;
            lvSearchResults.DataBind();

            ltrlMatchCount.Text = string.Format("{0} match{1} found", users.Count, users.Count == 1 ? string.Empty : "es");
        }

        /// <summary>
        /// Determines whether [is user team member] [the specified user identifier].
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        private bool IsUserTeamMember(int userId)
        {
            bool userIsMember = false;
            if (DisplayMode == ViewMode.CompanyAdministrators)
            {
                userIsMember = Support.IsCompanyAdministrator(CompanyId, userId);
            }
            else if (DisplayMode == ViewMode.ProjectTeam)
            {
                userIsMember = this.GetBL<ProjectBL>().IsProjectUser(ProjectId, userId);
            }
            else if (DisplayMode == ViewMode.InventoryTeam)
            {
                userIsMember = Utils.IsCompanyInventoryTeamMember(CompanyId, userId);
            }

            return userIsMember;
        }

        /// <summary>
        /// Gets the pending invitation for user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private Invitation GetPendingInvitationForUser(User user)
        {
            int currentInvitationTypeCodeId = GetInvitationTypeCodeId(DisplayMode);
            int relatedId = DisplayMode == ViewMode.ProjectTeam ? ProjectId : CompanyId;
            return this.GetBL<PersonalBL>().GetPendingInvitationForUser(user, currentInvitationTypeCodeId, relatedId);
        }

        /// <summary>
        /// Gets the pending invitation for email.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        private Invitation GetPendingInvitationForEmail(string emailAddress)
        {
            int currentInvitationTypeCodeId = GetInvitationTypeCodeId(DisplayMode);
            int relatedId = DisplayMode == ViewMode.ProjectTeam ? ProjectId : CompanyId;

            return this.GetBL<PersonalBL>().GetPendingInvitationForEmail(emailAddress, currentInvitationTypeCodeId, relatedId);
        }

        /// <summary>
        /// Gets the invitation type code identifier.
        /// </summary>
        /// <param name="viewMode">The view mode.</param>
        /// <returns></returns>
        private int GetInvitationTypeCodeId(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.CompanyAdministrators:
                    return Support.GetCodeIdByCodeValue("InvitationType", "COMPANYADMIN");

                case ViewMode.ProjectTeam:
                    return Support.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");

                case ViewMode.InventoryTeam:
                    return Support.GetCodeIdByCodeValue("InvitationType", "INVENTORYTEAM");

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Resets the inventory invite popup.
        /// </summary>
        private void ResetInventoryInvitePopup()
        {
            //rbtnInventoryObserver.Checked = false;
            //rbtnInventoryStaff.Checked = false;
            txtInventoryUserName.Text = string.Empty;
        }

        #endregion Private Methods
    }
}