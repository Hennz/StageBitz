using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace StageBitz.UserWeb.Public
{
    /// <summary>
    /// Landing Page for primary email change request.
    /// </summary>
    public partial class EmailChange : PageBase
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
                if (this.ViewState["email"] == null)
                {
                    this.ViewState["email"] = string.Empty;
                }

                return this.ViewState["email"].ToString();
            }

            set
            {
                this.ViewState["email"] = value;
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
                if (this.ViewState["activationkey"] == null)
                {
                    this.ViewState["activationkey"] = string.Empty;
                }

                return this.ViewState["activationkey"].ToString();
            }

            set
            {
                this.ViewState["activationkey"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the change request id.
        /// </summary>
        /// <value>
        /// The change request id.
        /// </value>
        public int ChangeRequestId
        {
            get
            {
                int changeRequestId = 0;
                if (this.ViewState["changerequestid"] != null)
                {
                    int.TryParse(this.ViewState["changerequestid"].ToString(), out changeRequestId);
                }

                return changeRequestId;
            }

            set
            {
                this.ViewState["changerequestid"] = value;
            }
        }

        #endregion PROPERTIES

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Read query string
                this.LoadQueryStringData();
                this.SignOutCurrentUser();
                this.DefaultViewSettings();

                if (this.Email != null && this.Password != null && this.ChangeRequestId > 0)
                {
                    this.ChangePrimaryEmail();
                }
            }
        }

        #endregion Event Handlers

        #region Support Methods

        /// <summary>
        /// Loads the query string data.
        /// </summary>
        private void LoadQueryStringData()
        {
            if (Request.QueryString["email"] != null)
            {
                this.Email = Server.UrlDecode(Request["email"]);
            }

            if (Request.QueryString["activationKey"] != null)
            {
                this.Password = Server.UrlDecode(Request["activationKey"]);
            }

            if (Request.QueryString["changerequestid"] != null)
            {
                int changeRequestId;
                if (int.TryParse(Request.QueryString["changerequestid"], out changeRequestId))
                {
                    this.ChangeRequestId = changeRequestId;
                }
            }
        }

        /// <summary>
        /// Changes the primary email.
        /// </summary>
        private void ChangePrimaryEmail()
        {
            int pendingCodeId = Support.GetCodeByValue("EmailChangeRequestStatus", "PENDING").CodeId;
            int verifiedCodeId = Support.GetCodeByValue("EmailChangeRequestStatus", "VERIFIED").CodeId;

            EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                     join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                     where r.StatusCode == pendingCodeId &&
                                                        r.EmailChangeRequestId == this.ChangeRequestId
                                                     select r).FirstOrDefault();

            if (emailChangeRequest != null)
            {
                var user = (from u in DataContext.Users
                            where u.UserId == emailChangeRequest.UserId && u.Password == this.Password
                            select u).FirstOrDefault();

                if (user != null && emailChangeRequest != null && emailChangeRequest.Email == this.Email)
                {
                    try
                    {
                        var usersWithSameEmail = (from u in DataContext.Users
                                                  where u.LoginName.Equals(emailChangeRequest.Email, StringComparison.InvariantCultureIgnoreCase)
                                                  select u).FirstOrDefault();
                        if (usersWithSameEmail == null)
                        {
                            user.Email1 = emailChangeRequest.Email;
                            user.LoginName = emailChangeRequest.Email;

                            if (user.Email1 == user.Email2)
                            {
                                user.Email2 = null;
                            }

                            emailChangeRequest.StatusCode = verifiedCodeId;

                            DataContext.SaveChanges();
                            divEmailChanged.Visible = true;
                        }
                        else
                        {
                            divEmailChangeFailed.Visible = true;
                        }
                    }
                    catch
                    {
                        divEmailChangeFailed.Visible = true;
                    }
                }
                else
                {
                    // Invalid activation link throw new exception
                    divEmailChangeFailed.Visible = true;
                }
            }
            else
            {
                divEmailChangeFailed.Visible = true;
            }
        }

        /// <summary>
        /// Sign out the current user.
        /// </summary>
        private void SignOutCurrentUser()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
            }
        }

        private void DefaultViewSettings()
        {
            // Hide all div.
            divEmailChanged.Visible = false;
            divEmailChangeFailed.Visible = false;

            // Set links
            string homePage = "~/Account/Login.aspx";
            lnkEmailChanged.HRef = homePage;
            lnkEmailChangeFailed.HRef = homePage;
        }

        #endregion Support Methods
    }
}