using StageBitz.Common;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for Project User Invitation.
    /// </summary>
    public partial class ProjectUserInvitation : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets the project role.
        /// </summary>
        /// <value>
        /// The project role.
        /// </value>
        public string ProjectRole
        {
            get
            {
                return txtProjectRole.Text.Trim();
            }
        }

        /// <summary>
        /// Gets the name of the project invite person.
        /// </summary>
        /// <value>
        /// The name of the project invite person.
        /// </value>
        public string ProjectInvitePersonName
        {
            get
            {
                return txtProjInvitePersonName.Text.Trim();
            }
        }

        /// <summary>
        /// Gets a value indicating whether project observer radio checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if project observer radio checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsProjectObserverRadioChecked
        {
            get
            {
                return radioProjectObserver.Checked;
            }
        }

        /// <summary>
        /// Gets a value indicating whether budget summary checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if budget summary checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsBudgetSummaryChecked
        {
            get
            {
                return chkBudgetSummary.Checked;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user is admin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this user is admin; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdmin
        {
            get
            {
                if (ViewState["IsAdmin"] == null)
                {
                    ViewState["IsAdmin"] = false;
                }

                return (bool)ViewState["IsAdmin"];
            }
            set
            {
                ViewState["IsAdmin"] = value;
            }
        }

        /// <summary>
        /// Gets the type of the selected user.
        /// </summary>
        /// <value>
        /// The type of the selected user.
        /// </value>
        public int SelectedUserType
        {
            get
            {
                if (radioProjectAdmin.Checked)
                {
                    return Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");
                }
                else if (radioProjectStaff.Checked)
                {
                    return Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "STAFF");
                }
                else
                {
                    return Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "OBSERVER");
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Initializes the validation group.
        /// </summary>
        /// <param name="validationGroup">The validation group.</param>
        public void InitializeValidationGroup(string validationGroup)
        {
            //string validationGroup = string.Concat(this.ClientID, "ProjectInvitation");
            reqProjInvitePersonName.ValidationGroup = validationGroup;
            reqProjectRole.ValidationGroup = validationGroup;
            valsumProjTeamInvitation.ValidationGroup = validationGroup;
        }

        /// <summary>
        /// Creates the invite new user layout.
        /// </summary>
        /// <param name="invitationText">The invitation text.</param>
        public void CreateInviteNewUserLayout(string invitationText)
        {
            columnProjectAdminHeader.Visible = false;
            columnProjectAdmin.Visible = false;
            ltrlProjectTeamInvitationText.Text = invitationText;//string.Format("Invite '{0}' to the Project.", Support.TruncateString(SelectedUserEmail, 30));
            trProjInviteName.Visible = true;
            txtProjInvitePersonName.Text = string.Empty;
            txtProjectRole.Text = string.Empty;
            radioProjectObserver.Checked = true;
            radioProjectStaff.Checked = false;
            txtProjInvitePersonName.Focus();
            chkBudgetSummary.Checked = true;
        }

        /// <summary>
        /// Creates the invite existing user layout.
        /// </summary>
        /// <param name="userFullName">Full name of the user.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        public void CreateInviteExistingUserLayout(string userFullName, int userId, int projectId)
        {
            columnProjectAdminHeader.Visible = false;
            columnProjectAdmin.Visible = false;
            ltrlProjectTeamInvitationText.Text = string.Format("Invite '{0}' to the Project.", userFullName);
            trProjInviteName.Visible = false;
            txtProjectRole.Text = string.Empty;
            radioProjectObserver.Checked = true;
            radioProjectStaff.Checked = false;
            txtProjectRole.Focus();
            chkBudgetSummary.Checked = true;
            chkBudgetSummary.Enabled = !(Support.IsProjectAdministrator(projectId, userId) || Support.IsCompanyAdministrator(Support.GetCompanyByProjectId(projectId).CompanyId, userId));
        }

        /// <summary>
        /// Shows the edit user permission.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="projectUserId">The project user identifier.</param>
        public void ShowEditUserPermission(int userId, int projectId, int projectUserId)
        {
            Data.User user = this.GetBL<PersonalBL>().GetUser(userId);
            Data.ProjectUser projectUser = this.GetBL<ProjectBL>().GetProjectUser(projectUserId);
            int projectAdminUserTypeCodeId = Utils.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN").CodeId;
            int staffUserTypeCodeId = Utils.GetCodeByValue("ProjectUserTypeCode", "STAFF").CodeId;
            int observerUserTypeCodeId = Utils.GetCodeByValue("ProjectUserTypeCode", "OBSERVER").CodeId;
            bool isProjectAdmin = projectUser.ProjectUserTypeCodeId == projectAdminUserTypeCodeId;

            if (isProjectAdmin)
            {
                radioProjectAdmin.Enabled = false;
                radioProjectObserver.Enabled = false;
                radioProjectStaff.Enabled = false;
            }
            else
            {
                radioProjectAdmin.Enabled = true;
                radioProjectObserver.Enabled = true;
                radioProjectStaff.Enabled = true;
            }

            ltrlProjectTeamInvitationText.Text = string.Format("Change '{0}' permission settings for this Project.", (user.FirstName + " " + user.LastName));
            trProjInviteName.Visible = false;
            txtProjectRole.Text = string.Empty;
            radioProjectAdmin.Checked = isProjectAdmin;
            radioProjectStaff.Checked = projectUser.ProjectUserTypeCodeId == staffUserTypeCodeId;
            radioProjectObserver.Checked = projectUser.ProjectUserTypeCodeId == observerUserTypeCodeId;
            txtProjectRole.Text = projectUser.Role;
            chkBudgetSummary.Checked = projectUser.CanSeeBudgetSummary;

            bool isAdmin = isProjectAdmin || Support.IsCompanyAdministrator(Support.GetCompanyByProjectId(projectId).CompanyId, userId);
            this.IsAdmin = isAdmin;
            if (IsAdmin)
            {
                chkBudgetSummary.Enabled = false;
                chkBudgetSummary.Checked = true;
            }
            else
            {
                chkBudgetSummary.Enabled = true;
            }
            RegisterScriptForPermissionSelection();
        }

        /// <summary>
        /// Registers the script for permission selection.
        /// </summary>
        public void RegisterScriptForPermissionSelection()
        {
            string script = string.Format("PermissionSelection('{0}', '{1}', '{2}', '{3}', {4});",
                    radioProjectAdmin.ClientID,
                    radioProjectStaff.ClientID,
                    radioProjectObserver.ClientID,
                    chkBudgetSummary.ClientID,
                    this.IsAdmin ? "true" : "false");
            ScriptManager.RegisterStartupScript(this, GetType(), "PermissionSelection", script, true);
        }

        #endregion Public Methods
    }
}