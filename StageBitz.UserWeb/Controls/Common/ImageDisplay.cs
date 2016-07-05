using StageBitz.UserWeb.Common.Helpers;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for image display.
    /// </summary>
    [ToolboxData("<{0}:ImageDisplay runat=server></{0}:ImageDisplay>")]
    public class ImageDisplay : WebControl
    {
        private const string placeholderThumbUrl = "~/Common/Images/placeholder_thumb.png";

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is thumbnail.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is thumbnail; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        public bool IsThumbnail
        {
            get
            {
                if (ViewState["IsThumbnail"] == null)
                {
                    ViewState["IsThumbnail"] = true;
                }

                return (bool)ViewState["IsThumbnail"];
            }
            set
            {
                ViewState["IsThumbnail"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the document media identifier.
        /// </summary>
        /// <value>
        /// The document media identifier.
        /// </value>
        [DefaultValue(0)]
        public int DocumentMediaId
        {
            get
            {
                if (ViewState["DocumentMediaId"] == null)
                {
                    ViewState["DocumentMediaId"] = 0;
                }

                return (int)ViewState["DocumentMediaId"];
            }
            set
            {
                ViewState["DocumentMediaId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image title.
        /// </summary>
        /// <value>
        /// The image title.
        /// </value>
        public string ImageTitle
        {
            get
            {
                if (ViewState["ImageTitle"] == null)
                {
                    return null;
                }

                return ViewState["ImageTitle"].ToString();
            }
            set
            {
                ViewState["ImageTitle"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the function prefix.
        /// </summary>
        /// <value>
        /// The function prefix.
        /// </value>
        public string FunctionPrefix
        {
            get
            {
                if (ViewState["FunctionPrefix"] == null)
                {
                    return string.Empty;
                }

                return ViewState["FunctionPrefix"].ToString();
            }
            set
            {
                ViewState["FunctionPrefix"] = value;
            }
        }

        /// <summary>
        /// Whether to show the image preview when the image is clicked.
        /// The parent page should have an ImagePreview control for this to work.
        /// (The value of this property is not stored in ViewState for performance reasons.
        /// So always set this property in aspx markup to avoid conflicts between postbacks.)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show image preview]; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(false)]
        public bool ShowImagePreview
        {
            get;
            set;
        }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageDisplay"/> class.
        /// </summary>
        public ImageDisplay()
        {
            ShowImagePreview = false;
        }

        #region Overrides

        /// <summary>
        /// Gets the name of the control tag. This property is used primarily by control developers.
        /// </summary>
        /// <returns>The name of the control tag.</returns>
        protected override string TagName
        {
            get
            {
                return HtmlTextWriterTag.Span.ToString();
            }
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (ImageTitle != null && ImageTitle.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, ImageTitle);
            }

            string clickable = string.Empty;
            if (ShowImagePreview)
            {
                //Image preview launch.
                //For this to work, the parent page should contain ImagePreview control.
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("{0}showDocumentPreview({1});", FunctionPrefix, DocumentMediaId));
                clickable = " clickable";
            }

            //Apply extra css classes if specified
            string extraCssClass = (CssClass != string.Empty) ? (" " + CssClass) : string.Empty;

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "imageDisplay" + clickable + extraCssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
        }

        /// <summary>
        /// Renders the contents.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            using (HtmlImage image = new HtmlImage())
            {
                if (DocumentMediaId == 0)
                {
                    image.Src = ResolveUrl(placeholderThumbUrl);
                    image.Alt = "No image found";
                }
                else
                {
                    image.Src = Support.GetImageFileThumbUrl(DocumentMediaId, IsThumbnail);
                }

                image.RenderControl(output);
            }
        }

        #endregion Overrides
    }
}