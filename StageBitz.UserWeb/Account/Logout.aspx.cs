using System;
using System.Web.Security;

namespace StageBitz.UserWeb.Account
{
    public partial class Logout : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
                Response.Redirect(FormsAuthentication.LoginUrl);
            }
        }
    }
}