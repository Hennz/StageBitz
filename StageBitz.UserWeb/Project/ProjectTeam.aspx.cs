using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web page for project teawm.
    /// </summary>
    public partial class ProjectTeam : PageBase
    {
        #region Fields

        private int projAdminTypeCodeId = 0;
        private int PrimaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
        private int SecondaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");
        private int InventoryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");

        #endregion Fields

        #region Properties

        //delegate void DelPageMethod();

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
                    int ProjectId = 0;

                    if (Request["ProjectId"] != null)
                    {
                        int.TryParse(Request["ProjectId"], out ProjectId);
                    }

                    ViewState["ProjectId"] = ProjectId;
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
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    ViewState["IsReadOnly"] = false;
                }

                return (bool)ViewState["IsReadOnly"];
            }
            set
            {
                ViewState["IsReadOnly"] = value;
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
                    ViewState["IsProjectClosed"] = GetBL<ProjectBL>().IsProjectClosed(this.ProjectId);
                }

                return (bool)ViewState["IsProjectClosed"];
            }
            set
            {
                ViewState["IsProjectClosed"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the new admin user identifier.
        /// </summary>
        /// <value>
        /// The new admin user identifier.
        /// </value>
        private int NewAdminUserId
        {
            get
            {
                if (ViewState["NewAdminUserId"] == null)
                {
                    ViewState["NewAdminUserId"] = 0;
                }

                return (int)ViewState["NewAdminUserId"];
            }
            set
            {
                ViewState["NewAdminUserId"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">User does not have permission to access the project team</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectId && p.IsActive == true).SingleOrDefault();
                if (!Support.CanAccessProject(project))
                {
                    if (IsProjectClosed)
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new Exception("User does not have permission to access the project team");
                }

                this.CompanyId = project.CompanyId;
                projectWarningPopup.ProjectId = ProjectId;
                warningDisplay.ProjectID = ProjectId;
                warningDisplay.LoadData();

                sbPackageLimitsValidation.CompanyId = CompanyId;
                sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.UserLimit;
                sbPackageLimitsValidation.LoadData();

                LoadBreadCrumbs();

                SetReadOnlyMode(project);

                if (!IsReadOnly)
                {
                    searchUsers.DisplayMode = SearchUsers.ViewMode.ProjectTeam;
                    searchUsers.ProjectId = ProjectId;
                    searchUsers.LoadControl();
                }
                else
                {
                    searchUsers.Visible = false;
                }

                LoadProjectTeam();

                #region SET LINKS

                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", project.CompanyId, (int)BookingTypes.Project, ProjectId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                linkTaskManager.HRef = ResolveUrl(string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId));
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();

                #endregion SET LINKS

                #region SetValidationGroup

                string validationGroup = string.Concat(ucUserInvitationPopup.ClientID, "ValidationGroup");
                btnApplyUserPermission.ValidationGroup = validationGroup;
                ucUserInvitationPopup.InitializeValidationGroup(validationGroup);

                #endregion SetValidationGroup
            }
        }

        #region Grid Events

        /// <summary>
        /// Handles the ItemDataBound event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic user = (dynamic)dataItem.DataItem;
                int userID = user.UserId;
                bool isProjectAdmin = (user.UserTypeCode.CodeId == Utils.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN").CodeId);
                //bool isEditing = (e.Item.ItemType == GridItemType.EditItem);

                #region Name string truncation

                HyperLink lnkUserName = (HyperLink)dataItem.FindControl("lnkUserName");
                Label lblUserName = (Label)dataItem.FindControl("lblUserName");

                if (lnkUserName != null && lblUserName != null)
                {
                    lnkUserName.Visible = userID != this.UserID;
                    lblUserName.Visible = userID == this.UserID;

                    lnkUserName.NavigateUrl = string.Format("~/Personal/UserDetails.aspx?userId={0}", userID);
                    lnkUserName.Text = lblUserName.Text = Support.TruncateString(user.FullName, 30);
                    if (user.FullName.Length > 30)
                    {
                        lnkUserName.ToolTip = lblUserName.ToolTip = user.FullName;
                    }
                }

                #endregion Name string truncation

                #region Role

                dataItem["Role"].Text = Support.TruncateString(user.Role, 20);
                if (user.Role.Length > 20)
                {
                    dataItem["Role"].ToolTip = user.Role;
                }

                #endregion Role

                #region Permission List

                Literal ltrlPermission = (Literal)dataItem.FindControl("ltrlPermission");

                ltrlPermission.Visible = true;
                ltrlPermission.Text = user.UserTypeCode.Description;

                #endregion Permission List

                #region Company Admin icon

                //Image imgCompAdmin = (Image)dataItem.FindControl("imgCompAdmin");
                //imgCompAdmin.Visible = user.IsCompanyAdmin;

                Image imgCompAdmin = (Image)dataItem.FindControl("imgCompAdmin");
                if (imgCompAdmin != null)
                {
                    int companyId = (from p in DataContext.Projects where p.ProjectId == ProjectId select p.CompanyId).SingleOrDefault();
                    int userId = user.UserId;

                    List<Data.CompanyUserRole> companyUserRoles = this.GetBL<CompanyBL>().GetCompanyUserRoles(userId, companyId);

                    Data.CompanyUserRole companyInventoryManager = companyUserRoles.Where(cur => cur.CompanyUserTypeCodeId == InventoryAdminCodeId && cur.IsActive).FirstOrDefault();
                    Data.CompanyUserRole companyAdmin = companyUserRoles.Where(cur => (cur.CompanyUserTypeCodeId == PrimaryAdminCodeId || cur.CompanyUserTypeCodeId == SecondaryAdminCodeId) && cur.IsActive).FirstOrDefault();

                    if (companyInventoryManager != null && companyAdmin != null)
                    {
                        imgCompAdmin.Visible = true;
                        imgCompAdmin.ToolTip = "This user is also a Company Administrator and a Company Inventory Administrator";
                    }
                    else if (companyInventoryManager != null)
                    {
                        imgCompAdmin.Visible = true;
                        imgCompAdmin.ToolTip = "This user is also a Company Inventory Administrator";
                    }
                    else if (companyAdmin != null)
                    {
                        imgCompAdmin.Visible = true;
                        imgCompAdmin.ToolTip = "This user is also a Company Administrator";
                    }
                }

                #endregion Company Admin icon

                #region Is Acitve

                CheckBox chkActive = (CheckBox)dataItem.FindControl("chkActive");
                if (isProjectAdmin || !user.IsMember)
                {
                    chkActive.Visible = false;
                }
                else
                {
                    chkActive.Visible = true;
                    chkActive.Enabled = !IsProjectClosed;
                    chkActive.Checked = user.IsActive;
                }

                #endregion Is Acitve

                #region Edit/Delete buttons

                if (!IsReadOnly)
                {
                    dataItem["EditPermission"].FindControl("imgbtnEditPermission").Visible = user.IsMember;
                    //dataItem["EditColumn"].Controls[0].Visible = user.IsMember;
                }

                if (!IsProjectClosed)
                {
                    dataItem["DeleteColumn"].Controls[0].Visible = !isProjectAdmin;
                }

                #endregion Edit/Delete buttons
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_ItemCommand(object sender, GridCommandEventArgs e)
        {
            searchUsers.HideNotifications();

            if (e.CommandName == "AddContact")
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int contactUserId = (int)dataItem.GetDataKeyValue("UserId");

                //Check whether this user is already in contacts before saving the new contact
                UserContact contact = DataContext.UserContacts.Where(uc => uc.UserId == UserID && uc.ContactUserId == contactUserId).FirstOrDefault();
                if (contact == null)
                {
                    contact = new UserContact();
                    contact.UserId = UserID;
                    contact.ContactUserId = contactUserId;
                    contact.CreatedByUserId = contact.LastUpdatedByUserId = UserID;
                    contact.CreatedDate = contact.LastUpdatedDate = Now;

                    DataContext.UserContacts.AddObject(contact);
                    DataContext.SaveChanges();
                }

                LoadProjectTeam();
            }
            else if (e.CommandName == "EditPermission")
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int userId = (int)dataItem.GetDataKeyValue("UserId");
                int projectUserId = (int)dataItem.GetDataKeyValue("ProjectUserId");
                ucUserInvitationPopup.ShowEditUserPermission(userId, ProjectId, projectUserId);
                btnApplyUserPermission.CommandArgument = projectUserId.ToString();
                popupEditUserPermission.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApplyUserPermission control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyUserPermission_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                int projectUserId = int.Parse(btnApplyUserPermission.CommandArgument);
                int selectedUserTypeCodeId = ucUserInvitationPopup.SelectedUserType;
                int projectAdminTypeCodeId = Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");
                Data.ProjectUser projectUser = this.GetBL<ProjectBL>().GetProjectUser(projectUserId);
                bool shouldCommit = false;

                #region ChangePermission

                if (projectUser.ProjectUserTypeCodeId != selectedUserTypeCodeId)
                {
                    if (selectedUserTypeCodeId == projectAdminTypeCodeId)
                    {
                        if (Support.IsCompanyAdministrator(Support.GetCompanyByProjectId(ProjectId).CompanyId))
                        {
                            divProjectAdminConfirmMessage.Visible = false;
                        }
                        else
                        {
                            divProjectAdminConfirmMessage.Visible = true;
                            divProjectAdminConfirmMessage.InnerText = "(If you say 'Yes', you will lose the ability to make any further changes to the project team.)";
                        }
                        NewAdminUserId = projectUserId;
                        popupProjectAdminConfirmation.ShowPopup(1001);
                        return;
                    }
                    else
                    {
                        projectUser.ProjectUserTypeCodeId = selectedUserTypeCodeId;
                        projectUser.LastUpdatedByUserId = UserID;
                        projectUser.LastUpdatedDate = Now;
                        //Create Notification for changing permission of the project

                        #region Project Notification

                        string projectUserName = projectUser.User.FirstName + " " + projectUser.User.LastName;
                        Notification nf = CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} changed the project permission of {1}.", Support.UserFullName, projectUserName));
                        this.GetBL<NotificationBL>().AddNotification(nf, false);
                        shouldCommit = true;

                        #endregion Project Notification
                    }
                }

                #endregion ChangePermission

                #region ChangeRole

                shouldCommit = ChangeUserRole(projectUser) || shouldCommit;

                #endregion ChangeRole

                #region ChanageBudgetSummaryPermission

                if (projectUser.CanSeeBudgetSummary != ucUserInvitationPopup.IsBudgetSummaryChecked)
                {
                    projectUser.CanSeeBudgetSummary = ucUserInvitationPopup.IsBudgetSummaryChecked;
                    projectUser.LastUpdatedByUserId = UserID;
                    projectUser.LastUpdatedDate = Now;
                    shouldCommit = true;
                }

                #endregion ChanageBudgetSummaryPermission

                if (shouldCommit)
                {
                    this.GetBL<BaseBL>().SaveChanges();
                }
                popupEditUserPermission.HidePopup();
                LoadProjectTeam();
            }
        }

        /// <summary>
        /// Handles the UpdateCommand event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_UpdateCommand(object sender, GridCommandEventArgs e)
        {
            searchUsers.HideNotifications();

            //Get the GridEditableItem of the RadGrid
            GridEditableItem dataItem = e.Item as GridEditableItem;
            TextBox txtRole = (TextBox)dataItem.FindControl("txtRole");

            int projectUserId = (int)dataItem.GetDataKeyValue("ProjectUserId");

            ProjectUser projUser = DataContext.ProjectUsers.Where(pu => pu.ProjectUserId == projectUserId).FirstOrDefault();

            #region Project Notification

            if (projUser.Role != txtRole.Text.Trim())
            {
                string projectUserName = projUser.User.FirstName + " " + projUser.User.LastName;
                DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} edited the project role of {1}.", Support.UserFullName, projectUserName)));
            }

            #endregion Project Notification

            projUser.Role = txtRole.Text.Trim();
            projUser.LastUpdatedByUserId = UserID;
            projUser.LastUpdatedDate = Now;

            DataContext.SaveChanges();

            rgProjectTeam.EditIndexes.Clear();
            rgProjectTeam.Rebind();
        }

        /// <summary>
        /// Handles the DeleteCommand event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_DeleteCommand(object sender, GridCommandEventArgs e)
        {
            searchUsers.HideNotifications();

            GridDataItem dataItem = (GridDataItem)e.Item;

            int projectUserId = (int)dataItem.GetDataKeyValue("ProjectUserId");
            if (projectUserId > 0)
            {
                ProjectUser projectuser = this.GetBL<ProjectBL>().GetProjectUser(projectUserId);

                #region Project Notification

                string projectUserName = (projectuser.User.FirstName + " " + projectuser.User.LastName).Trim();
                this.GetBL<NotificationBL>().AddNotification(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "DELETE"), string.Format("{0} removed {1} from the project team.", Support.UserFullName, projectUserName)), false);
                //DataContext.Notifications.AddObject();

                #endregion Project Notification

                //Update Project Daily Usage Summary
                //ProjectUsageHandler.UpdateProjectUsage(projectuser.Project, UserID, projectuser.UserId, true, Today, DataContext);

                DataContext.ProjectUsers.DeleteObject(projectuser);
            }
            else
            {
                int invitationId = (int)dataItem.GetDataKeyValue("InvitationId");
                Invitation invitation = DataContext.Invitations.Where(inv => inv.InvitationId == invitationId).FirstOrDefault();

                #region Project Notification

                string invitedUserName = string.Empty;

                if (invitation.ToUserId == null)
                {
                    invitedUserName = invitation.ToName;
                }
                else
                {
                    StageBitz.Data.User invitedUser = DataContext.Users.Where(u => u.UserId == invitation.ToUserId).FirstOrDefault();
                    invitedUserName = (invitedUser.FirstName + " " + invitedUser.LastName).Trim();
                }

                DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "DELETE"), string.Format("{0} removed the project invitation for {1}.", Support.UserFullName, invitedUserName)));

                #endregion Project Notification

                DataContext.DeleteInvitation(invitation.InvitationId);
            }

            DataContext.SaveChanges();
            LoadProjectTeam();
        }

        /// <summary>
        /// Handles the NeedDataSource event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            rgProjectTeam.DataSource = GetTeamMemberList();
        }

        /// <summary>
        /// Handles the SortCommand event of the rgProjectTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void rgProjectTeam_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            searchUsers.HideNotifications();

            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                rgProjectTeam.MasterTableView.SortExpressions.Clear();
                rgProjectTeam.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                rgProjectTeam.Rebind();
            }
        }

        #endregion Grid Events

        /// <summary>
        /// Handles the InvitationSent event of the searchUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void searchUsers_InvitationSent(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadProjectTeam();
            }
        }

        #region Project Permission

        /// <summary>
        /// Handles the Click event of the btnConfirmProjectAdmin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmProjectAdmin_Click(object sender, EventArgs e)
        {
            int projStaffTypeCodeId = Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "STAFF");

            Code projectAdmin = Support.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN");

            projAdminTypeCodeId = projectAdmin.CodeId;

            //Make the current admin a staff member
            ProjectUser projectAdminUser = DataContext.ProjectUsers.Where(pu => pu.ProjectId == ProjectId && pu.ProjectUserTypeCodeId == projAdminTypeCodeId).FirstOrDefault();
            projectAdminUser.ProjectUserTypeCodeId = projStaffTypeCodeId;
            projectAdminUser.LastUpdatedByUserId = UserID;
            projectAdminUser.LastUpdatedDate = Now;

            //Make the specified user the project admin
            ProjectUser projUser = DataContext.ProjectUsers.Where(pu => pu.ProjectUserId == NewAdminUserId).FirstOrDefault();
            projUser.ProjectUserTypeCodeId = projAdminTypeCodeId;
            projUser.LastUpdatedByUserId = UserID;
            projUser.LastUpdatedDate = Now;
            projUser.CanSeeBudgetSummary = true;
            ChangeUserRole(projUser);

            //Create Notification for changing permission of the project

            #region Project Notification

            string projectUserName = projUser.User.FirstName + " " + projUser.User.LastName;
            Notification nf = CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} changed the permission of {1} as {2}.", Support.UserFullName, projectUserName, projectAdmin.Description));
            this.GetBL<NotificationBL>().AddNotification(nf);

            #endregion Project Notification

            popupEditUserPermission.HidePopup();
            popupProjectAdminConfirmation.HidePopup();

            Response.Redirect(Request.RawUrl);
        }

        #endregion Project Permission

        /// <summary>
        /// Handles the CheckedChanged event of the chkActive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                CheckBox chkActive = (CheckBox)sender;

                if (chkActive.Checked && !sbPackageLimitsValidation.Validate())
                {
                    LoadProjectTeam();
                    return;
                }

                searchUsers.HideNotifications();
                GridDataItem dataItem = (GridDataItem)chkActive.Parent.Parent;
                int projectUserId = (int)dataItem.GetDataKeyValue("ProjectUserId");

                ProjectUser projUser = DataContext.ProjectUsers.Where(pu => pu.ProjectUserId == projectUserId).FirstOrDefault();
                projUser.IsActive = chkActive.Checked;
                projUser.LastUpdatedByUserId = UserID;
                projUser.LastUpdatedDate = Now;

                //Create Notification for activate/deactivate project team

                #region Project Notification

                string projectUserName = projUser.User.FirstName + " " + projUser.User.LastName;
                string message;
                if (chkActive.Checked)
                    message = string.Format("{0} activated the project user {1}.", Support.UserFullName, projectUserName);
                else
                    message = string.Format("{0} deactivated the project user {1}.", Support.UserFullName, projectUserName);

                DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), message));

                #endregion Project Notification

                //Update Project Daily Usage Summary
                //ProjectUsageHandler.UpdateProjectUsage(projUser.Project, UserID, projUser.UserId, !chkActive.Checked, Today, DataContext);
                DataContext.SaveChanges();
                LoadProjectTeam();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelProjectAdmin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelProjectAdmin_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupProjectAdminConfirmation.HidePopup();
                ucUserInvitationPopup.RegisterScriptForPermissionSelection();
            }
        }

        #endregion Event Handlers

        #region Support Methods

        /// <summary>
        /// Changes the user role.
        /// </summary>
        /// <param name="projectUser">The project user.</param>
        /// <returns></returns>
        private bool ChangeUserRole(Data.ProjectUser projectUser)
        {
            if (projectUser.Role != ucUserInvitationPopup.ProjectRole)
            {
                projectUser.Role = ucUserInvitationPopup.ProjectRole;
                string projectUserName = projectUser.User.FirstName + " " + projectUser.User.LastName;
                Notification nf = CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} edited the project role of {1}.", Support.UserFullName, projectUserName));
                this.GetBL<NotificationBL>().AddNotification(nf, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the notification.
        /// </summary>
        /// <param name="operationTypeCodeId">The operation type code identifier.</param>
        /// <param name="notification">The notification.</param>
        /// <returns></returns>
        private Notification CreateNotification(int operationTypeCodeId, string notification)
        {
            Notification nf = new Notification();
            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJTEAM");
            nf.OperationTypeCodeId = operationTypeCodeId;
            nf.RelatedId = ProjectId;
            nf.ProjectId = ProjectId;
            nf.Message = notification;
            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
            nf.CreatedDate = nf.LastUpdatedDate = Now;
            return nf;
        }

        /// <summary>
        /// Sets the read only mode.
        /// </summary>
        /// <param name="project">The project.</param>
        private void SetReadOnlyMode(Data.Project project)
        {
            bool userHasAccess = false;

            //if it is a company admin return true
            if (Support.IsCompanyAdministrator(project.CompanyId))
            {
                userHasAccess = true;
            }
            else
            {
                int projectAdminCodeID = Support.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN").CodeId;

                var projAdmin = (from p in DataContext.Projects
                                 join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                                 where p.ProjectId == ProjectId && pu.UserId == UserID && pu.IsActive == true && pu.ProjectUserTypeCodeId == projectAdminCodeID && pu.IsActive == true
                                 select pu).FirstOrDefault();

                userHasAccess = (projAdmin != null);
            }

            IsReadOnly = Support.IsReadOnlyProjectByProjectStatus(ProjectId) || !userHasAccess;
        }

        /// <summary>
        /// Loads the project team.
        /// </summary>
        private void LoadProjectTeam()
        {
            rgProjectTeam.DataSource = GetTeamMemberList();
            rgProjectTeam.DataBind();

            if (!IsReadOnly)
            {
                searchUsers.LoadControl();
            }

            //Hide the delete column depending on the read only mode
            if (IsReadOnly)
            {
                foreach (GridColumn col in rgProjectTeam.MasterTableView.RenderColumns)
                {
                    if (col.UniqueName == "EditPermission")
                    {
                        col.Visible = false;
                    }

                    if (col.UniqueName == "DeleteColumn")
                    {
                        col.Visible = !IsProjectClosed;
                    }
                }
            }

            upnlProjectTeam.Update();
        }

        /// <summary>
        /// Gets the team member list.
        /// </summary>
        /// <returns></returns>
        private List<ProjectUserData> GetTeamMemberList()
        {
            return this.GetBL<ProjectBL>().GetProjectTeamMemberList(ProjectId, UserID);
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectId).FirstOrDefault();
            StageBitz.Data.Company company = project.Company;

            string companyUrl = Support.IsCompanyAdministrator(company.CompanyId) ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", company.CompanyId) : null;

            bc.AddLink(company.CompanyName, companyUrl);
            bc.AddLink(project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?ProjectId={0}", project.ProjectId));
            bc.AddLink(DisplayTitle, null);

            bc.LoadControl();
        }

        #endregion Support Methods
    }
}