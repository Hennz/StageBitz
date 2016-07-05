using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Account
{
    public partial class Actication : PageBase
    {
        #region PROPERTIES

        /// <summary>
        /// Gets or sets the email.
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
                    ViewState["email"] = string.Empty;
                }
                return ViewState["email"].ToString();
            }
            set
            {
                ViewState["email"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get
            {
                if (ViewState["activationkey"] == null)
                {
                    ViewState["activationkey"] = string.Empty;
                }
                return ViewState["activationkey"].ToString();
            }
            set
            {
                ViewState["activationkey"] = value;
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
                //Read query string
                if (Request.QueryString["email"] != null)
                {
                    Email = Server.UrlDecode(Request["email"]);
                }

                if (Request.QueryString["activationKey"] != null)
                {
                    Password = Server.UrlDecode(Request["activationKey"]);
                }

                DefaultViewSettings();

                if (Email != null && Password != null)
                    ActivateUser(Email, Password);
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Defaults the view settings.
        /// </summary>
        private void DefaultViewSettings()
        {
            //Hide all divs.
            divInvalidLink.Visible = false;
            divActivationSucess.Visible = false;
            divAlreadyActivated.Visible = false;

            //Set links
            string homePage = "~/Account/Login.aspx";
            linkSuccess.HRef = homePage;
            linkInvalidLink.HRef = homePage;
            linkAlreadyActivated.HRef = homePage;

            if (Email != null)
            {
                //Set the email for Already activated section
                userEmail.Text = Email;
                litSuccessEmail.Text = Email;
            }
        }

        /// <summary>
        /// Activates the user.
        /// </summary>
        /// <param name="loginName">Name of the login.</param>
        /// <param name="passwordHash">The password hash.</param>
        private void ActivateUser(string loginName, string passwordHash)
        {
            var user = GetBL<PersonalBL>().GetNormalUserByLogInNameAndPasswordHash(loginName, passwordHash);
            if (user != null)
            {
                //If not active
                //Set user IsActiveFlag
                //activation success;
                if (!user.IsActive)
                {
                    user.IsActive = true;
                    user.LastUpdatedByUserId = -1;
                    user.LastUpdatedDate = Now;
                    DataContext.SaveChanges();
                    divActivationSucess.Visible = true;
                }
                else
                {
                    //If active already
                    //Already active message
                    divAlreadyActivated.Visible = true;
                }
            }
            else
            {
                //Invalid activation link throw new exception
                divInvalidLink.Visible = true;
            }
        }

        #endregion Private Methods
    }
}