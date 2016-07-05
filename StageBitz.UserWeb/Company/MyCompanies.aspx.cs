using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Company
{
    public partial class MyCompanies : PageBase
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
                //My Companies page is removed in the sprint 16, refer PBI 12399
                Response.Redirect("~/Default.aspx");
            }
        }
    }
}