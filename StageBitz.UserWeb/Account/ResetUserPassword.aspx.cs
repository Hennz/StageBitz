using StageBitz.Common;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;

namespace StageBitz.UserWeb.Account
{
    /// <summary>
    /// Web page for rest user password.
    /// </summary>
    public partial class ResetUserPassword : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the login.
        /// </summary>
        /// <value>
        /// The name of the login.
        /// </value>
        public string LoginName
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

        #endregion Properties

        #region Event

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string decrypted = string.Empty;
                if (Request.QueryString["key"] != null)
                {
                    try
                    {
                        decrypted = Request["key"].ToString();

                        decrypted = Utils.DecryptStringAES(HttpServerUtility.UrlTokenDecode(decrypted));
                        string[] splitLines = decrypted.Split(new char[] { '|' });
                        string loginName = splitLines[0].ToString();
                        string passwordHash = splitLines[1].ToString();

                        //Set viewstate properties to be used later
                        this.LoginName = loginName;
                        this.Password = passwordHash;

                        var user = GetBL<PersonalBL>().GetUserByLogInNameAndPasswordHash(LoginName, Password);
                        divLoginName.InnerText = string.Format("{0} {1} ({2})", user.FirstName, user.LastName, loginName);
                        if (user == null)
                        {
                            //Error
                            panel.Visible = false;
                            divNotifications.Visible = true;
                            divLoginName.Visible = false;
                        }
                    }
                    catch (Exception)
                    {
                        panel.Visible = false;
                        divNotifications.Visible = true;
                        divLoginName.Visible = false;
                    }
                }
                else
                {
                    panel.Visible = false;
                    divNotifications.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnResetPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnResetPassword_Click(Object sender, EventArgs e)
        {
            if (IsValid)
            {
                var user = GetBL<PersonalBL>().GetUserByLogInNameAndPasswordHash(LoginName, Password);
                if (user != null)
                {
                    //Update New password
                    user.Password = Utils.HashPassword(txtNewPassword.Text);
                    user.LastUpdatedByUserId = user.UserId;
                    user.LastUpdatedDate = Now;

                    DataContext.SaveChanges();

                    panel.Visible = false;
                    divSucess.Visible = true;
                    //Redirect to login page
                    //Response.Redirect("~/Account/Login.aspx");
                }
                else
                {
                    panel.Visible = false;
                    divNotifications.Visible = true;
                }
            }
        }

        #endregion Event
    }
}