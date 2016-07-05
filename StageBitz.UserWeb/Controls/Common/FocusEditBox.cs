using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// Displays a label when displaying the value. When clicked, displays a textbox to edit the value.
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:FocusEditBox runat=server></{0}:FocusEditBox>")]
    public class FocusEditBox : WebControl
    {
        #region Properties

        /// <summary>
        /// The text input
        /// </summary>
        private TextBox txtInput;

        /// <summary>
        /// The label display
        /// </summary>
        private Label lblDisplay;

        /// <summary>
        /// Gets the text box.
        /// </summary>
        /// <value>
        /// The text box.
        /// </value>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public TextBox TextBox
        {
            get { return txtInput; }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return txtInput.Text; }
            set { txtInput.Text = value; }
        }

        /// <summary>
        /// Gets the display label.
        /// </summary>
        /// <value>
        /// The display label.
        /// </value>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Label DisplayLabel
        {
            get { return lblDisplay; }
        }

        /// <summary>
        /// Gets or sets the display length of the maximum.
        /// </summary>
        /// <value>
        /// The display length of the maximum.
        /// </value>
        public int DisplayMaxLength
        {
            get
            {
                if (ViewState["DisplayMaxLength"] == null)
                {
                    ViewState["DisplayMaxLength"] = 0;
                }

                return (int)ViewState["DisplayMaxLength"];
            }
            set
            {
                ViewState["DisplayMaxLength"] = value;
            }
        }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusEditBox"/> class.
        /// </summary>
        public FocusEditBox()
        {
            txtInput = new TextBox();
            txtInput.ID = "EditInput";
            this.Controls.Add(txtInput);

            lblDisplay = new Label();
            lblDisplay.ID = "EditDisplay";
            this.Controls.Add(lblDisplay);
        }

        #region Public Methods

        #endregion Public Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Publish the client script required by this control to the browser
            Page.ClientScript.RegisterClientScriptInclude("FocusEditBoxClientScript", ResolveUrl("~/Common/Scripts/focusEditBox.js"));
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            SetTruncatedLabelText();
            lblDisplay.CssClass = "focusEditBoxDisplay " + lblDisplay.CssClass;
            lblDisplay.Attributes["onclick"] = string.Format("focusEdit_SwitchToEditMode('{0}','{1}');", txtInput.ClientID, lblDisplay.ClientID);
            lblDisplay.Attributes.Add("data-displaylength", DisplayMaxLength.ToString(CultureInfo.InvariantCulture));

            txtInput.Style["display"] = "none;";
            txtInput.Width = txtInput.Width.IsEmpty ? this.Width : txtInput.Width;
            txtInput.CssClass = "focusEditBoxInput " + txtInput.CssClass;
            txtInput.Attributes["onblur"] = string.Format("focusEdit_SwitchToDisplayMode('{0}','{1}',{2});", txtInput.ClientID, lblDisplay.ClientID, DisplayMaxLength);
        }

        /// <summary>
        /// Renders the contents.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            lblDisplay.RenderControl(output);
            txtInput.RenderControl(output);
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Sets the truncated label text.
        /// </summary>
        private void SetTruncatedLabelText()
        {
            if (DisplayMaxLength == 0 || txtInput.Text.Length <= DisplayMaxLength)
            {
                lblDisplay.Text = (txtInput.Text.Trim().Length == 0) ? "&nbsp;" : txtInput.Text;
            }
            else
            {
                string truncatedText = txtInput.Text.Substring(0, DisplayMaxLength);
                lblDisplay.Text = truncatedText.Trim() + "...";

                lblDisplay.ToolTip = txtInput.Text;
            }
        }

        #endregion Private Methods
    }
}