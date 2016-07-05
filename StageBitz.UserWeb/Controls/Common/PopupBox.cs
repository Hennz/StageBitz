using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
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

        #endregion Global variables

        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
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

        /// <summary>
        /// Gets or sets the width of the Web server control.
        /// </summary>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.Unit" /> that represents the width of the control. The default is <see cref="F:System.Web.UI.WebControls.Unit.Empty" />.</returns>
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

        /// <summary>
        /// Gets or sets the height of the Web server control.
        /// </summary>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.Unit" /> that represents the height of the control. The default is <see cref="F:System.Web.UI.WebControls.Unit.Empty" />.</returns>
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

        /// <summary>
        /// Gets or sets a value indicating whether to show corner close button.
        /// </summary>
        /// <value>
        /// <c>true</c> if need show corner close button; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether to show title bar.
        /// </summary>
        /// <value>
        ///   <c>true</c> if need show title bar; otherwise, <c>false</c>.
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowTitleBar
        {
            get
            {
                if (ViewState["ShowTitleBar"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["ShowTitleBar"];
                }
            }
            set
            {
                ViewState["ShowTitleBar"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show bottom stripe].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show bottom stripe]; otherwise, <c>false</c>.
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowBottomStripe
        {
            get
            {
                if (ViewState["ShowBottomStripe"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["ShowBottomStripe"];
                }
            }
            set
            {
                ViewState["ShowBottomStripe"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is automatic focus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is automatic focus; otherwise, <c>false</c>.
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool IsAutoFocus
        {
            get
            {
                if (ViewState["IsAutoFocus"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["IsAutoFocus"];
                }
            }
            set
            {
                ViewState["IsAutoFocus"] = value;
            }
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

        /// <summary>
        /// Gets the content of the bottom stripe.
        /// </summary>
        /// <value>
        /// The content of the bottom stripe.
        /// </value>
        [Browsable(false)]
        [TemplateContainer(typeof(PlaceHolder))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public virtual PlaceHolder BottomStripeContent
        {
            get;
            private set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            hdnVisualState.ID = string.Format("{0}_VisualState", this.ClientID);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
            //Begin holding div for the whole control's html markup
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "popupBox");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        /// <summary>
        /// Renders the contents.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            //Visual state hidden field
            hdnVisualState.RenderControl(output);

            #region Background overlay

            //Begin and end background overlay div
            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxOverlay");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.RenderEndTag();

            #endregion Background overlay

            #region Dialog box container

            //Begin dialog container div (this is the parent div of the dialog box and its shadow)
            output.AddAttribute(HtmlTextWriterAttribute.Class, IsAutoFocus ? "popupBoxDialogContainer AutoFocus" : "popupBoxDialogContainer");
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

            if (ShowTitleBar)
            {
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
            }

            #endregion Titlebar

            #region Content area

            output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxContent");
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            BodyContent.RenderControl(output);
            output.RenderEndTag();

            #endregion Content area

            #region Bottom stripe area

            if (ShowBottomStripe)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Class, "popupBoxBottomStripe");
                output.RenderBeginTag(HtmlTextWriterTag.Div);
                BottomStripeContent.RenderControl(output);
                output.RenderEndTag();
            }

            #endregion Bottom stripe area

            //End dialog div
            output.RenderEndTag();

            #endregion Dialog box

            //End dialog container div
            output.RenderEndTag();

            #endregion Dialog box container
        }

        #endregion Overrides

        #region Public Methods

        /// <summary>
        /// Display the popup in the browser after the current Http request is completed.
        /// </summary>
        public void ShowPopup()
        {
            string script = string.Format("showPopup('{0}');", this.ClientID);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ShowPopupBox" + ClientID, script, true);
        }

        /// <summary>
        /// Display the popup in the browser after the current Http request is completed.
        /// </summary>
        public void ShowPopup(int zIndex)
        {
            string script = string.Format("showPopup('{0}', {1});", this.ClientID, zIndex);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ShowPopupBox" + ClientID, script, true);
        }

        /// <summary>
        /// Hide the popup (if visible) in the browser after the current Http request is completed.
        /// </summary>
        public void HidePopup()
        {
            hdnVisualState.Value = string.Empty;

            string script = string.Format("hidePopup('{0}');", this.ClientID);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "HidePopupBox" + ClientID, script, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupBox"/> class.
        /// </summary>
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
            hdnVisualState.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            this.Controls.Add(hdnVisualState);

            #endregion Visual state hidden field
        }

        #endregion Public Methods
    }
}