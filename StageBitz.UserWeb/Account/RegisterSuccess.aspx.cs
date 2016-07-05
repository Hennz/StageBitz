using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;

namespace StageBitz.UserWeb.Account
{
    /// <summary>
    /// Web page for Registration Success.
    /// </summary>
    public partial class RegisterSuccess : PageBase
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
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    Response.Redirect("~/Default.aspx");
                    return;
                }
            }
        }
    }
}