using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Controls.Common;

namespace StageBitz.AdminWeb.Common.Helpers
{
    public class MasterBase : MasterPage
    {
        /// <summary>
        /// Gets or sets the page dirty flag maintained between javascript and server-side.
        /// Indicates whether any user inputs inside 'dirtyValidationArea' containers have been changed. (see global.js)
        /// </summary>
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
    }
}