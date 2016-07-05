using StageBitz.Common;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// Modal popup control for error messages.
    /// </summary>
    public class ErrorPopupBox : PopupBox
    {
        #region Properties

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public ErrorCodes ErrorCode
        {
            get
            {
                if (ViewState["ErrorCodeId"] == null)
                {
                    ViewState["ErrorCodeId"] = ErrorCodes.None;
                }

                return (ErrorCodes)ViewState["ErrorCodeId"];
            }
            set
            {
                ViewState["ErrorCodeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control is default error popup for the page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is default error popup for the page; otherwise, <c>false</c>.
        /// </value>
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool IsDefault
        {
            get
            {
                if (ViewState["IsDefault"] == null)
                {
                    ViewState["IsDefault"] = false;
                }

                return (bool)ViewState["IsDefault"];
            }
            set
            {
                ViewState["IsDefault"] = value;
            }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorPopupBox"/> class.
        /// </summary>
        public ErrorPopupBox()
            : base()
        {
        }

        #endregion Constructor

        #region Overrides

        /// <summary>
        /// Renders the begin tag.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (this.ErrorCode != ErrorCodes.None)
            {
                int errorCodeid = (int)this.ErrorCode;
                writer.AddAttribute("data-errorcode", errorCodeid.ToString(CultureInfo.InvariantCulture));

                if (this.IsDefault)
                {
                    writer.AddAttribute("data-isdefault", "default");
                }
            }

            base.RenderBeginTag(writer);
        }

        #endregion Overrides
    }
}