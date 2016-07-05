using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace StageBitz.AdminWeb.Account
{
    public partial class Logout : Page
    {
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