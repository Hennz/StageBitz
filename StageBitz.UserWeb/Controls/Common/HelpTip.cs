using System.Web.UI;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for Help tips.
    /// </summary>
    [ToolboxData("<{0}:HelpTip runat=server></{0}:HelpTip>")]
    public class HelpTip : RadToolTip
    {
        /// <summary>
        /// The icon Image.
        /// </summary>
        private HtmlGenericControl icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpTip"/> class.
        /// </summary>
        public HelpTip()
        {
            this.IsClientID = true;
            this.ViewStateMode = ViewStateMode.Disabled;
            this.CssClass = "helpTipContent";
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            icon = new HtmlGenericControl();
            icon.ID = this.ClientID + "_icon";
            icon.TagName = "div";
            icon.ClientIDMode = ClientIDMode.Static;
            icon.Attributes["class"] = "helpTipIcon";

            this.TargetControlID = icon.ID;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (icon != null)
            {
                //Render the help icon
                icon.RenderControl(writer);

                base.RenderControl(writer);
            }
        }
    }
}