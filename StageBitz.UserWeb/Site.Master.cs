using StageBitz.Common;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web.UI;

namespace StageBitz.UserWeb
{
    public partial class SiteMaster : MasterBase
    {
        /// <summary>
        /// Retrieves the
        /// </summary>
        /// <value>
        /// The application release year.
        /// </value>
        protected string ApplicationReleaseYear
        {
            get
            {
                return Utils.Today.Year.ToString();
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsLarge)
            {
                divContainer.Attributes.Add("class", "Container");
                divWrapper.Attributes.Add("class", "wrapper");
            }
            else
            {
                divContainer.Attributes.Add("class", "LargeContainer");
                divWrapper.Attributes.Add("class", "LargeWrapper");
            }

            Page.Header.DataBind(); //This is for resolving relative paths within the Head tag
        }
    }
}