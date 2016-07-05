using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;
using System.Web.Security;

namespace StageBitz.UserWeb.Account
{
    public partial class Login : PageBase
    {
        #region PROPERTIES

        /// <summary>
        /// Email is saved in the viewstate for activation email sending.
        /// When user clicks the login button,
        /// we put that email in view state for sending the email to that email on "send activation email" link."
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email
        {
            get
            {
                if (ViewState["email"] == null)
                {
                    ViewState["email"] = null;
                }
                return ViewState["email"].ToString();
            }
            set
            {
                ViewState["email"] = value;
            }
        }

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
                if (ViewState["invitationCode"] == null)
                {
                    if (Request["invitationCode"] == null)
                    {
                        ViewState["invitationCode"] = string.Empty;
                    }
                    else
                    {
                        ViewState["invitationCode"] = Request["invitationCode"];
                    }
                }

                return ViewState["invitationCode"].ToString();
            }
        }

        #endregion PROPERTIES

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

                txtUsername.Focus();

                if (!string.IsNullOrEmpty(InvitationCode))
                {
                    divInvitation.Visible = true;
                    lnkRegister.NavigateUrl = "~/Account/Register.aspx?invitationCode=" + Request["invitationCode"];
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSignIn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSignIn_Click(object sender, EventArgs e)
        {
            if (!this.IsValid)
            {
                return;
            }

            //Hash the password and compare credentials with the DB
            string passwordHash = Utils.HashPassword(txtPassword.Text);
            string username = txtUsername.Text;
            StageBitz.Data.User user = GetBL<PersonalBL>().AuthenticateUser(username, passwordHash);

            //Assign username and password to send activation email
            Email = username;

            if (user == null)
            {
                int pendingEmailTypeCodeId = Support.GetCodeByValue("EmailChangeRequestStatus", "PENDING").CodeId;
                EmailChangeRequest emailChangeRequest =
                    GetBL<UtilityBL>().GetEmailChangeRequestsByUsernameAndEmailTypeCodeId(username, pendingEmailTypeCodeId);
                if (emailChangeRequest != null)
                {
                    //Check the password by getting the current active userID.
                    int userId = emailChangeRequest.UserId;
                    //If the PassWord is matched, we know that the user is valid where as he did not follow the link.
                    if (GetBL<PersonalBL>().GetUserByUserIdAndPasswordHash(userId, passwordHash) != null)
                    {
                        divActivationMailSentPrimaryEmailChange.Visible = true;
                        divPendingActivation.Visible = false;
                        divInvalidLogin.Visible = false;
                        divActivationMailSent.Visible = false;
                        txtPassword.Text = string.Empty;
                        litPrimaryEmailSent.Text = username;
                        return;
                    }
                }
                divPendingActivation.Visible = false;
                divInvalidLogin.Visible = true;
                divActivationMailSent.Visible = false;
                divActivationMailSentPrimaryEmailChange.Visible = false;
                txtPassword.Text = string.Empty;
                txtUsername.Focus();
            }
            else
            {
                if (user.IsActive == true)
                {
                    Support.SetUserSessionData(user);

                    string cookieData = username + " " + passwordHash;

                    //Store the user id as the username inside asp.net auth cookie (if remember me is checked)
                    if (string.IsNullOrEmpty(InvitationCode))
                    {
                        FormsAuthentication.RedirectFromLoginPage(cookieData, chkRememberMe.Checked);
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(cookieData, chkRememberMe.Checked);
                        Response.Redirect("~/Default.aspx?invitationCode=" + InvitationCode);
                    }

                    //To Record the date where the user gets reset the session.
                    user.LastLoggedInDate = Now;
                    DataContext.SaveChanges();
                }
                else
                {
                    //User is not activated yet
                    divInvalidLogin.Visible = false;
                    divActivationMailSentPrimaryEmailChange.Visible = false;
                    divActivationMailSent.Visible = false;
                    divPendingActivation.Visible = true;
                    txtPassword.Text = string.Empty;
                    pendingActivationEmail.Text = user.Email1;
                    txtUsername.Focus();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the linkSendEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void linkSendEmail_Click(object sender, EventArgs e)
        {
            var user = GetBL<PersonalBL>().GetUserByEmail(this.Email);

            if (user != null)
            {
                //Send activation email
                string activationLink = Support.GetUserActivationLink(user.Email1, user.Password);
                StageBitz.Common.EmailSender.StageBitzUrl = Support.GetSystemUrl();
                StageBitz.Common.EmailSender.SendUserActivationLink(Email, activationLink, user.FirstName);

                litSucessActivationMail.Text = Email;
                divPendingActivation.Visible = false;
                divActivationMailSent.Visible = true;
            }
            else
            {
                //No valid email - will not happen, but for safe side to display a error message
                divPendingActivation.Visible = false;
                divInvalidLogin.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReset_Click(object sender, EventArgs e)
        {
            //1. Validate email formatting      //not a well-formed email address
            //2. User account validate          //User account does not exist
            //3. Activation validation          //Your account is not activated yet. An activation email has been sent to {0}. Please click on the activation link in the email.
            //4. Send activation email
            //5. Clean up
            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmailAddress.Text.Trim();
            errormsg.InnerText = string.Empty;

            var user = GetBL<PersonalBL>().GetUserByEmail(email);

            if (user != null)
            {
                if (user.IsActive)
                {
                    byte[] bytes = Utils.EncryptStringAES(string.Format("{0}|{1}", user.LoginName, user.Password));
                    string pipeSeperatedString = HttpServerUtility.UrlTokenEncode(bytes);

                    //Send forgotpassword email
                    string link = string.Format("{0}/Account/ResetUserPassword.aspx?key={1}", Support.GetSystemUrl(), pipeSeperatedString);
                    StageBitz.Common.EmailSender.StageBitzUrl = Support.GetSystemUrl();
                    EmailSender.SendForgotPasswordLink(user.Email1, link, user.FirstName);

                    //If success
                    txtEmailAddress.Text = string.Empty;
                    popupForgotPassword.HidePopup();
                    popupResetSucess.ShowPopup();
                }
                else
                {
                    //If pending activation
                    errormsg.InnerText = string.Format(
                        "Your account is not activated yet. The activation email was sent to {0}. Please click on the activation link in the email.", user.Email1);
                    errormsg.Style["display"] = "Inline-block";
                }
            }
            else
            {
                errormsg.InnerText = "User account does not exist.";
                errormsg.Style["display"] = "Inline-block";
            }
        }

        #endregion Events
    }
}