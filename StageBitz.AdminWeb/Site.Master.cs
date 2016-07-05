using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.AdminWeb.Common.Helpers;
using System.Reflection;

namespace StageBitz.AdminWeb
{
    public partial class SiteMaster : MasterBase
    {
        /// <summary>
        /// Stores the pplication version number string for the application life time.
        /// </summary>
        private static string applicationVersionString = null;

        #region Properties

        /// <summary>
        /// Retrieves the pplication version number string.
        /// </summary>
        protected string ApplicationVersionString
        {
            get
            {
                if (applicationVersionString == null)
                {
                    Version version = Assembly.GetAssembly(this.GetType().BaseType).GetName().Version;

                    //version.Build = No. of days from 1/1/2000 (MSDN docs)
                    DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build);

                    applicationVersionString = string.Format("v{0}.{1}.{2:yyMMdd}", version.Major, version.Minor, buildDate);
                }
                return applicationVersionString;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Header.DataBind(); //This is for resolving relative paths within the Head tag
        }
    }
}
