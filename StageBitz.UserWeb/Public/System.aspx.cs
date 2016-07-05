using StageBitz.Common;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Public
{
    /// <summary>
    /// Web page for system functions eg:- clear cache
    /// </summary>
    public partial class System : PageBase
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
                if (Request["ClearCache"] == "1")
                {
                    SystemCache.ClearCache();
                }
            }
        }
    }
}