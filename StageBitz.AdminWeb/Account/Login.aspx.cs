using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;

namespace StageBitz.AdminWeb.Account
{
    public partial class Login : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSignIn_Click(object sender, EventArgs e)
        {
            if (!this.IsValid)
            {
                return;
            }

            //Hash the password and compare credentials with the DB
            string passwordHash = Utils.HashPassword(txtPassword.Text);
            StageBitz.Data.User user = AuthenticateUser(txtUsername.Text, passwordHash);

            if (user == null)
            {
                divInvalidLogin.Visible = true;
                txtPassword.Text = string.Empty;
                txtUsername.Focus();
            }
            else
            {
                Support.SetUserSessionData(user);

                //Store the user id as the username inside asp.net auth cookie (if remember me is checked)
                FormsAuthentication.RedirectFromLoginPage(user.UserId.ToString(), chkRememberMe.Checked);
            }
        }

        private StageBitz.Data.User AuthenticateUser(string loginName, string passwordHash)
        {
            var user = (from u in DataContext.Users
                        join code in DataContext.Codes on u.UserAccountTypeCodeID equals code.CodeId
                        where u.LoginName == loginName && u.Password == passwordHash && code.Value == "ADMIN" && u.IsActive == true
                        select u).FirstOrDefault();
            return user;
        }
    }
}