using System.Web;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for OPTGroup DropDownList.
    /// </summary>
    public class DropDownListOPTGroup : DropDownList
    {
        /// <summary>
        /// Adds the item group.
        /// </summary>
        /// <param name="groupTitle">The group title.</param>
        public void AddItemGroup(string groupTitle)
        {
            this.Items.Add(new ListItem(groupTitle, "$$OPTGROUP$$OPTGROUP$$"));
        }

        /// <summary>
        /// Renders the items in the <see cref="T:System.Web.UI.WebControls.ListControl" /> control.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream used to write content to a Web page.</param>
        protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
        {
            if (this.Items.Count > 0)
            {
                bool selected = false;
                bool optGroupStarted = false;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    ListItem item = this.Items[i];
                    if (item.Enabled)
                    {
                        if (item.Value == "$$OPTGROUP$$OPTGROUP$$")
                        {
                            if (optGroupStarted)
                                writer.WriteEndTag("optgroup");
                            writer.WriteBeginTag("optgroup");
                            writer.WriteAttribute("label", item.Text);
                            writer.Write('>');
                            writer.WriteLine();
                            optGroupStarted = true;
                        }
                        else
                        {
                            writer.WriteBeginTag("option");
                            if (item.Selected)
                            {
                                if (selected)
                                {
                                    this.VerifyMultiSelect();
                                }
                                selected = true;
                                writer.WriteAttribute("selected", "selected");
                            }
                            writer.WriteAttribute("value", item.Value, true);
                            if (item.Attributes.Count > 0)
                            {
                                item.Attributes.Render(writer);
                            }
                            if (this.Page != null)
                            {
                                this.Page.ClientScript.RegisterForEventValidation(
                                    this.UniqueID,
                                    item.Value);
                            }
                            writer.Write('>');
                            HttpUtility.HtmlEncode(item.Text, writer);
                            writer.WriteEndTag("option");
                            writer.WriteLine();
                        }
                    }
                }

                if (optGroupStarted)
                {
                    writer.WriteEndTag("optgroup");
                }
            }
        }
    }
}