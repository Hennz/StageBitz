using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Common.Helpers
{
    /// <summary>
    /// Base class for master pages.
    /// </summary>
    public class MasterBase : MasterPage
    {
        /// <summary>
        /// Gets or sets the page dirty flag maintained between javascript and server-side.
        /// Indicates whether any user inputs inside 'dirtyValidationArea' containers have been changed. (see global.js)
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is page dirty; otherwise, <c>false</c>.
        /// </value>
        public bool IsPageDirty
        {
            get
            {
                //If this is a the parent master, look for the dirty hidden field on this master page.
                //Otherwise refer to the parent master to get the dirty status.
                if (this.Master == null)
                {
                    HiddenField hdnDirtyField = (HiddenField)this.FindControl("hdnGlobalDirtyFlagField");

                    if (hdnDirtyField != null)
                    {
                        return hdnDirtyField.Value == "1";
                    }

                    return false;
                }
                else
                {
                    return ((MasterBase)this.Master).IsPageDirty;
                }
            }
            set
            {
                if (this.Master == null)
                {
                    HiddenField hdnDirtyField = (HiddenField)this.FindControl("hdnGlobalDirtyFlagField");
                    UpdatePanel upnlGlobalDirtyFlag = (UpdatePanel)this.FindControl("upnlGlobalDirtyFlag");

                    if (hdnDirtyField != null)
                    {
                        hdnDirtyField.Value = (value == true ? "1" : string.Empty);
                        upnlGlobalDirtyFlag.Update();
                        ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "setGlobalDirty", "setGlobalDirty(" + value.ToString().ToLower() + ");", true);
                    }
                }
                else
                {
                    ((MasterBase)this.Master).IsPageDirty = value;
                }
            }
        }

        /// <summary>
        /// The islarge variable.
        /// </summary>
        private bool _isLarge;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is large page.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is large page; otherwise, <c>false</c>.
        /// </value>
        public bool IsLarge
        {
            get
            {
                if (this.Master == null)
                {
                    return _isLarge;
                }
                else
                {
                    return ((MasterBase)this.Master).IsLarge;
                }
            }
            set
            {
                if (this.Master == null)
                {
                    _isLarge = value;
                }
                else
                {
                    ((MasterBase)this.Master).IsLarge = value;
                }
            }
        }

        /// <summary>
        /// The application version string.
        /// </summary>
        private static string applicationVersionString = null;

        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <value>
        /// The application version string.
        /// </value>
        public string ApplicationVersionString
        {
            get
            {
                if (applicationVersionString == null)
                {
                    Version version = System.Reflection.Assembly.GetAssembly(this.GetType().BaseType).GetName().Version;

                    //version.Build = No. of days from 1/1/2000 (MSDN docs)
                    DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build);

                    applicationVersionString = string.Format("v{0}.{1}.{2:yyMMdd}", version.Major, version.Minor, buildDate);
                }
                return applicationVersionString;
            }
        }
    }
}