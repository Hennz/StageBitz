using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for group box.
    /// </summary>
    [ToolboxData("<{0}:GroupBox runat=server></{0}:GroupBox>")]
    public class GroupBox : WebControl
    {
        #region Properties

        /// <summary>
        /// Gets the content of the title left.
        /// </summary>
        /// <value>
        /// The content of the title left.
        /// </value>
        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder TitleLeftContent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the content of the title right.
        /// </summary>
        /// <value>
        /// The content of the title right.
        /// </value>
        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder TitleRightContent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the content of the body.
        /// </summary>
        /// <value>
        /// The content of the body.
        /// </value>
        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder BodyContent
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBox"/> class.
        /// </summary>
        public GroupBox()
        {
            TitleLeftContent = new PlaceHolder();
            TitleRightContent = new PlaceHolder();
            BodyContent = new PlaceHolder();

            this.Controls.Add(TitleLeftContent);
            this.Controls.Add(TitleRightContent);
            this.Controls.Add(BodyContent);
        }

        #endregion Constructor

        #region Overrides

        /// <summary>
        /// Gets the name of the control tag. This property is used primarily by control developers.
        /// </summary>
        /// <returns>The name of the control tag.</returns>
        protected override string TagName
        {
            get
            {
                return HtmlTextWriterTag.Div.ToString();
            }
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            string cssClass = string.IsNullOrEmpty(this.CssClass) ? string.Empty : string.Concat(" ", this.CssClass);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, string.Concat("groupBox", cssClass));
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        /// <summary>
        /// Renders the contents.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            #region Header div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "blueHeaders");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            #region Header left div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "left blueHeadersLeft");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            TitleLeftContent.RenderControl(output);
            output.RenderEndTag();

            #endregion Header left div

            #region Header right div

            //Header right
            output.AddAttribute(HtmlTextWriterAttribute.Class, "right blueHeadersRight");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            TitleRightContent.RenderControl(output);
            output.RenderEndTag();

            #endregion Header right div

            #region Clear both

            output.Write("<br style='clear:both;' />");

            #endregion Clear both

            //End header div
            output.RenderEndTag();

            #endregion Header div

            #region Content div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "groupBoxContent");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            BodyContent.RenderControl(output);
            output.RenderEndTag();

            #endregion Content div
        }

        #endregion Overrides
    }
}