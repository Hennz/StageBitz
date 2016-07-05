using System;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI;

namespace StageBitz.AdminWeb.Controls.Common
{
    /// <summary>
    /// Modal popup control.
    /// </summary>
    [DefaultProperty("Title")]
    [ToolboxData("<{0}:PopupBox runat=server></{0}:PopupBox>")]
    public class PopupBox : WebControl
    {
        #region Global variables

        /// <summary>
        /// Hidden field to maintain visual state accross postbacks.
        /// If field value is 1, popup is visible. Else, popup is hidden.
        /// </summary>
        private HiddenField hdnVisualState;

        #endregion

        #region Properties

        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Title
        {
            get
            {
                String s = (String)ViewState["Title"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(300)]
        public new int Width
        {
            get
            {
                if (ViewState["Width"] == null)
                {
                    ViewState["Width"] = 300;
                }

                return (int)ViewState["Width"];
            }
            set
            {
                ViewState["Width"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(150)]
        public new int Height
        {
            get
            {
                if (ViewState["Height"] == null)
                {
                    ViewState["Height"] = 150;
                }

                return (int)ViewState["Height"];
            }
            set
            {
                ViewState["Height"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowCornerCloseButton
        {
            get
            {
                if (ViewState["ShowCornerCloseButton"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["ShowCornerCloseButton"];
                }
            }
            set
            {
                ViewState["ShowCornerCloseButton"] = value;
            }
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

        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder BottomStripeContent
        {
            get;
            private set;
        }

        #endregion

        public PopupBox()
        {
            BodyContent = new PlaceHolder();
            BottomStripeContent = new PlaceHolder();

            this.Controls.Add(BodyContent);
            this.Controls.Add(BottomStripeContent);

            #region Visual state hidden field

            //Instantiate the hidden field for storing the popup visual state.
            //Static client ID is used to make it easier for javascript to access the field value.
            hdnVisualState = new HiddenField();
            hdnVisualState.ClientIDMode = ClientIDMode.Static;
            this.Controls.Add(hdnVisualState);

            #endregion
        }

        #region Overrides

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            hdnVisualState.ID = string.Format("{0}_VisualState", this.ID);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Publish the client script required by the popupbox control to the browser
            Page.ClientScript.RegisterClientScriptInclude("PopupBoxClientScript", ResolveUrl("~/Common/Scripts/popupBox.js"));

            //Check the visual state hidden field and set the initial appearance of the popupbox
            bool popupVisible = (hdnVisualState.Value == "1");
            if (popupVisible)
            {
                ShowPopup();
            }
        }

        protected override string TagName
        {
            get
            {
                return HtmlTextWriterTag.Div.ToString();
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            //Begin holding div for the whole control's html markup
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "popupBox");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            //Visual state hidden field
            hdnVisualState.RenderControl(output);

            #region Background overlay

            //Begin and end background overlay div
            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxOverlay");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderEndTag();

            #endregion

            #region Dialog box container

            //Begin dialog container div (this is the parent div of the dialog box and its shadow)
            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxDialogContainer");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            //Begin and End Dialog shadow div
            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxDialogShadow");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderEndTag();

            #region Dialog box

            //Begin dialog div
            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxDialog");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            #region Titlebar

            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxTitlebar");
            //output.AddStyleAttribute(HtmlTextWriterStyle.Height, string.Format("{0}px", TitlebarHeight));
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            output.Write(Title);

            if (ShowCornerCloseButton)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxCornerCloseButton popupBoxCloser");
                output.RenderBeginTag(HtmlTextWriterTag.Div);
                output.RenderEndTag();
            }

            output.RenderEndTag();

            #endregion

            #region Content area

            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxContent");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            BodyContent.RenderControl(output);
            output.RenderEndTag();

            #endregion

            #region Bottom stripe area

            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxBottomStripe");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            BottomStripeContent.RenderControl(output);
            output.RenderEndTag();

            #endregion

            //End dialog div
            output.RenderEndTag();

            #endregion

            //End dialog container div
            output.RenderEndTag();

            #endregion
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Display the popup in the browser after the current Http request is completed.
        /// </summary>
        public void ShowPopup()
        {
            string script = string.Format("showPopup('{0}');", this.ID);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopupBox" + ID, script, true);
        }

        /// <summary>
        /// Hide the popup (if visible) in the browser after the current Http request is completed.
        /// </summary>
        public void HidePopup()
        {
            hdnVisualState.Value = string.Empty;

            string script = string.Format("hidePopup('{0}');", this.ID);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HidePopupBox" + ID, script, true);
        }

        #endregion
    }
}