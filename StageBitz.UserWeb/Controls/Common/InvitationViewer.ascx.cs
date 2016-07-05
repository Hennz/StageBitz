using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// Usercontrol for Invitation Viewer.
    /// </summary>
    public partial class InvitationViewer : UserControlBase
    {
        #region Events

        public event EventHandler<InvitationStatusChangedEventArgs> InvitationStatusChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the invitation identifier.
        /// </summary>
        /// <value>
        /// The invitation identifier.
        /// </value>
        private int InvitationId
        {
            get
            {
                if (ViewState["InvitationId"] == null)
                {
                    ViewState["InvitationId"] = 0;
                }

                return (int)ViewState["InvitationId"];
            }
            set
            {
                ViewState["InvitationId"] = value;
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
            if (!IsPostBack)
            {
                popupViewInvitation.ID = this.ClientID + "_popupViewInvitation";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAccept_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                this.GetBL<PersonalBL>().AcceptInvitation(InvitationId, UserID);
                popupViewInvitation.HidePopup();
                if (InvitationStatusChanged != null)
                {
                    InvitationStatusChangedEventArgs args = new InvitationStatusChangedEventArgs();
                    args.Accepted = true;
                    InvitationStatusChanged(this, args);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDecline control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDecline_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Invitation invitation = DataContext.Invitations.Where(inv => inv.InvitationId == InvitationId).FirstOrDefault();

                invitation.InvitationStatusCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "REJECTED");
                invitation.LastUpdatedByUserId = UserID;
                invitation.LastUpdatedDate = Now;

                DataContext.SaveChanges();

                #region Send declined email notice

                Code invitationTypeCode = Support.GetCodeByCodeId(invitation.InvitationTypeCodeId);

                try
                {
                    if (invitationTypeCode.Value == "PROJECTTEAM")
                    {
                        User invitedUser = DataContext.Users.Where(u => u.UserId == invitation.ToUserId).SingleOrDefault();
                        User invitee = DataContext.Users.Where(u => u.UserId == invitation.FromUserId).SingleOrDefault();

                        string invitedPersonName = (invitedUser.FirstName + " " + invitedUser.LastName).Trim();
                        string toPersonName = (invitee.FirstName + " " + invitee.LastName).Trim();
                        string toEmail = invitee.Email1;
                        string role = invitation.ProjectRole;
                        StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == invitation.RelatedId).SingleOrDefault();
                        string projectName = project.ProjectName;
                        string companyName = DataContext.Companies.Where(c => c.CompanyId == project.CompanyId).SingleOrDefault().CompanyName;

                        #region Notifications

                        Notification nf = new Notification();
                        nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "PROJTEAM");
                        nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "DELETE");
                        nf.RelatedId = invitation.RelatedId;
                        nf.ProjectId = invitation.RelatedId;
                        nf.Message = string.Format("{0} declined the Project invitation.", Support.UserFullName);
                        nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                        nf.CreatedDate = nf.LastUpdatedDate = Now;
                        DataContext.Notifications.AddObject(nf);
                        DataContext.SaveChanges();

                        #endregion Notifications

                        EmailSender.SendProjectTeamInvitationDeclinedNotice(toEmail, toPersonName, invitedPersonName, projectName, role, companyName);
                    }
                    else if (invitationTypeCode.Value == "COMPANYADMIN" || invitationTypeCode.Value == "INVENTORYTEAM")
                    {
                        int templateCodeId = 0;
                        if (invitationTypeCode.Value == "COMPANYADMIN")
                        {
                            templateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "COMPANYADMININV_DECLINED");
                        }
                        else if (invitationTypeCode.Value == "INVENTORYTEAM")
                        {
                            templateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "INVENTORYTEAMINV_DECLINED");
                        }

                        User invitedUser = DataContext.Users.Where(u => u.UserId == invitation.ToUserId).SingleOrDefault();
                        User invitee = DataContext.Users.Where(u => u.UserId == invitation.FromUserId).SingleOrDefault();

                        string invitedPersonName = (invitedUser.FirstName + " " + invitedUser.LastName).Trim();
                        string toPersonName = (invitee.FirstName + " " + invitee.LastName).Trim();
                        string toEmail = invitee.Email1;
                        string companyName = DataContext.Companies.Where(c => c.CompanyId == invitation.RelatedId).SingleOrDefault().CompanyName;

                        EmailSender.SendCompanyAdminInvitationDeclinedNotice(toEmail, toPersonName, invitedPersonName, companyName, templateCodeId);
                    }
                }
                catch
                {
                    //Handle exceptions silently
                }

                #endregion Send declined email notice

                popupViewInvitation.HidePopup();

                if (InvitationStatusChanged != null)
                {
                    InvitationStatusChangedEventArgs args = new InvitationStatusChangedEventArgs();
                    args.Accepted = false;
                    InvitationStatusChanged(this, args);
                }
            }
        }

        #endregion Event Handlers

        /// <summary>
        /// Shows the invitation.
        /// </summary>
        /// <param name="invitationId">The invitation identifier.</param>
        public void ShowInvitation(int invitationId)
        {
            InvitationId = invitationId;

            Invitation invitation = DataContext.Invitations.Where(inv => inv.InvitationId == InvitationId).FirstOrDefault();

            Code invitationTypeCode = Support.GetCodeByCodeId(invitation.InvitationTypeCodeId);

            if (invitationTypeCode.Value == "PROJECTTEAM")
            {
                var invData = (from p in DataContext.Projects
                               join c in DataContext.Companies on p.CompanyId equals c.CompanyId
                               join u in DataContext.Users on invitation.FromUserId equals u.UserId
                               where p.ProjectId == invitation.RelatedId
                               select new { u.FirstName, u.LastName, p.ProjectName, c.CompanyName }).FirstOrDefault();

                ltrlInvitationText.Text = string.Format(
                                 @"<p>'{0}' has invited you to join the Project Team for '{1}' at '{2}'.</p><br/>
                                 <p>You can get to work on '{1}' as soon as you accept the invitation.</p><br/>
                                 <p>Would you like to accept?</p>",
                                 (invData.FirstName + " " + invData.LastName).Trim(),
                                 invData.ProjectName,
                                 invData.CompanyName);

                popupViewInvitation.ShowPopup();
            }
            else if (invitationTypeCode.Value == "COMPANYADMIN" || invitationTypeCode.Value == "INVENTORYTEAM")
            {
                var invitationUserRoles = DataContext.InvitationUserRoles.Where(iur => iur.InvitationId == InvitationId && iur.IsActive);
                string roles = string.Empty;
                if (invitationTypeCode.Value == "INVENTORYTEAM")
                {
                    roles = "inventory team member";
                }
                else if (invitationTypeCode.Value == "COMPANYADMIN")
                {
                    foreach (InvitationUserRole invitationUserRole in invitationUserRoles)
                    {
                        Code userRoleCode = Support.GetCodeByCodeId(invitationUserRole.UserTypeCodeId);
                        if (string.IsNullOrEmpty(roles))
                        {
                            roles = userRoleCode.Description;
                        }
                        else
                        {
                            roles = string.Concat(roles, " and ", userRoleCode.Description);
                        }
                    }
                }

                var invData = (from c in DataContext.Companies
                               join u in DataContext.Users on invitation.FromUserId equals u.UserId
                               where c.CompanyId == invitation.RelatedId
                               select new { u.FirstName, u.LastName, c.CompanyName }).FirstOrDefault();

                ltrlInvitationText.Text = string.Format(
                                 @"<p>'{0}' has invited you to be {2} for '{1}'.</p><br/>
                                 <p>Would you like to accept?</p>",
                                 (invData.FirstName + " " + invData.LastName).Trim(),
                                 invData.CompanyName, roles);

                popupViewInvitation.ShowPopup();
            }
        }
    }

    /// <summary>
    /// Event arg class  for invitation status changed.
    /// </summary>
    public class InvitationStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InvitationStatusChangedEventArgs"/> is accepted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if accepted; otherwise, <c>false</c>.
        /// </value>
        public bool Accepted
        {
            get;
            set;
        }
    }
}