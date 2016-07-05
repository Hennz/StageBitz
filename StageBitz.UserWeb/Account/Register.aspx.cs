using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;

namespace StageBitz.UserWeb.Account
{
    /// <summary>
    /// Web page for register new users.
    /// </summary>
    public partial class Register : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the invitation code.
        /// </summary>
        /// <value>
        /// The invitation code.
        /// </value>
        private string InvitationCode
        {
            get
            {
                if (Request["invitationCode"] == null)
                {
                    return string.Empty;
                }

                return Request["invitationCode"];
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    Response.Redirect("~/Default.aspx");
                    return;
                }
                if (!string.IsNullOrEmpty(InvitationCode))
                {
                    lnkLogin.HRef = "~/Account/Login.aspx?invitationCode=" + Request["invitationCode"];
                }
            }

            errormsg.Visible = false;
        }

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void RegisterUser(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var existingUser = GetBL<PersonalBL>().GetUserByEmail(txtEmail.Text.Trim());

            if (existingUser == null)
            {
                #region User

                User user = new User();
                user.FirstName = txtFirstName.Text.Trim();
                user.LastName = txtLastName.Text.Trim();
                user.LoginName = txtEmail.Text;
                user.Email1 = txtEmail.Text;
                user.Phone1 = txtPhone1.Text.Trim();
                user.Phone2 = txtPhone2.Text.Trim();

                Code userAccounttype = Support.GetCodeByValue("UserAccountType", "USER");
                user.UserAccountTypeCodeID = userAccounttype.CodeId;

                user.Password = Utils.HashPassword(txtPassWord.Text.Trim());
                user.CountryId = ucCountryList.CountryID;
                user.CreatedDate = Now;
                user.LastUpdatedDate = Now;
                user.IsActive = false; //Activation flag set to false. User has to activate by the link sent to the email
                user.IsEmailVisible = chkEmailVisibletoAll.Checked;
                DataContext.Users.AddObject(user);

                // set default email notification settings
                int dailyEmailNotificationCodeId = Support.GetCodeByValue("UserEmailNotificationType", "DAILY").CodeId;
                user.CompanyEmailNotificationCodeId = dailyEmailNotificationCodeId;
                user.ProjectEmailNotificationCodeId = dailyEmailNotificationCodeId;

                #endregion User

                DataContext.SaveChanges();

                #region Invitation

                try
                {
                    //If an invitation code exists, attach the invitation with the newly registered user ID.
                    if (!string.IsNullOrEmpty(InvitationCode))
                    {
                        string message = string.Empty;
                        Support.ProcessInvitationCode(InvitationCode, user.UserId, out message);
                    }
                }
                catch
                {
                }

                #endregion Invitation

                #region Activation Email

                //Send activation email
                //Construct the activation page url to include in the email.
                //e.g.http://localhost/StageBitz.DevUserWeb/Account/Activation.aspx?email=dhanuka@geveo.com&activationKey=496A4BDBB2E4B467B5171C3C120EC732
                string activationLink = Support.GetUserActivationLink(user.Email1, user.Password);

                //Activation link will be sent to the user.
                StageBitz.Common.EmailSender.StageBitzUrl = Support.GetSystemUrl();
                StageBitz.Common.EmailSender.SendUserActivationLink(user.Email1, activationLink, user.FirstName);

                //Notification email to StageBitzAdmins.Get the StageBitz Admin
                StageBitz.Common.EmailSender.SendUserRegistrationMailToStageBitzAdmin(
                    Support.GetSystemValue("AdminEmail"), Support.GetAdminPortalUserDetailsLink(user.UserId), Utils.GetFullName(user));

                #endregion Activation Email

                Response.Redirect("~/Account/RegisterSuccess.aspx");
            }
            else
            {
                errormsg.InnerText = "This email address is already in use.";
                errormsg.Visible = true;
            }
        }

        #endregion Events
    }
}