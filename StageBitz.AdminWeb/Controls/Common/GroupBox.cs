using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.AdminWeb.Controls.Common
{
    [ToolboxData("<{0}:GroupBox runat=server></{0}:GroupBox>")]
    public class GroupBox : WebControl
    {
        #region Properties

        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder TitleLeftContent
        {
            get;
            private set;
        }

        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder TitleRightContent
        {
            get;
            private set;
        }

        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder BodyContent
        {
            get;
            private set;
        }

        #endregion

        public GroupBox()
        {
            TitleLeftContent = new PlaceHolder();
            TitleRightContent = new PlaceHolder();
            BodyContent = new PlaceHolder();

            this.Controls.Add(TitleLeftContent);
            this.Controls.Add(TitleRightContent);
            this.Controls.Add(BodyContent);
        }

        #region Overrides

        protected override string TagName
        {
            get
            {
                return HtmlTextWriterTag.Div.ToString();
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "groupBox");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            #region Header div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "groupboxHeaders");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            #region Header left div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "left groupboxHeadersLeft");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            TitleLeftContent.RenderControl(output);
            output.RenderEndTag();

            #endregion

            #region Header right div

            //Header right
            output.AddAttribute(HtmlTextWriterAttribute.Class, "right groupboxHeadersRight");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            TitleRightContent.RenderControl(output);
            output.RenderEndTag();

            #endregion

            #region Clear both

            output.Write("<br style='clear:both;' />");

            #endregion

            //End header div
            output.RenderEndTag();

            #endregion

            #region Content div

            output.AddAttribute(HtmlTextWriterAttribute.Class, "groupBoxContent");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            BodyContent.RenderControl(output);
            output.RenderEndTag();

            #endregion
        }

        #endregion
    }
}
