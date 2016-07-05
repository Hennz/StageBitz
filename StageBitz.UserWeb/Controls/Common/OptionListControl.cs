using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for option list control.
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:OptionListControl runat=server></{0}:OptionListControl>")]
    public class OptionListControl : System.Web.UI.WebControls.Adapters.WebControlAdapter
    {
        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            DropDownList list = this.Control as DropDownList;

            string currentOptionGroup;
            List<string> renderedOptionGroups = new List<string>();

            foreach (ListItem item in list.Items)
            {
                if (item.Attributes["OptionGroup"] == null)
                {
                    RenderListItem(item, writer);
                }
                else
                {
                    currentOptionGroup = item.Attributes["OptionGroup"];

                    if (renderedOptionGroups.Contains(currentOptionGroup))
                    {
                        RenderListItem(item, writer);
                    }
                    else
                    {
                        if (renderedOptionGroups.Count > 0)
                        {
                            RenderOptionGroupEndTag(writer);
                        }

                        RenderOptionGroupBeginTag(currentOptionGroup, writer);
                        renderedOptionGroups.Add(currentOptionGroup);

                        RenderListItem(item, writer);
                    }
                }
            }

            if (renderedOptionGroups.Count > 0)
            {
                RenderOptionGroupEndTag(writer);
            }
        }

        #region Private Methods

        /// <summary>
        /// Renders the option group begin tag.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="writer">The writer.</param>
        private void RenderOptionGroupBeginTag(string name, HtmlTextWriter writer)
        {
            writer.WriteBeginTag("optgroup");
            writer.WriteAttribute("label", name);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteLine();
        }

        /// <summary>
        /// Renders the option group end tag.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderOptionGroupEndTag(HtmlTextWriter writer)
        {
            writer.WriteEndTag("optgroup");
            writer.WriteLine();
        }

        /// <summary>
        /// Renders the list item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void RenderListItem(ListItem item, HtmlTextWriter writer)
        {
            writer.WriteBeginTag("option");
            writer.WriteAttribute("value", item.Value, true);

            if (item.Selected)
            {
                writer.WriteAttribute("selected", "selected", false);
            }

            foreach (string key in item.Attributes.Keys)
            {
                writer.WriteAttribute(key, item.Attributes[key]);
            }

            writer.Write(HtmlTextWriter.TagRightChar);
            HttpUtility.HtmlEncode(item.Text, writer);
            writer.WriteEndTag("option");
            writer.WriteLine();
        }

        #endregion Private Methods
    }
}