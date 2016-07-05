using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for document list.
    /// </summary>
    public partial class DocumentList : UserControlBase
    {
        #region Event Definitions

        /// <summary>
        /// Occurs when a document is picked.
        /// </summary>
        public event EventHandler<DocumentListDocumentPickedEventArgs> DocumentPicked;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the name of the related table.
        /// </summary>
        /// <value>
        /// The name of the related table.
        /// </value>
        public string RelatedTableName
        {
            get
            {
                if (ViewState["RelatedTableName"] == null)
                {
                    ViewState["RelatedTableName"] = string.Empty;
                }

                return ViewState["RelatedTableName"].ToString();
            }
            set
            {
                ViewState["RelatedTableName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId
        {
            get
            {
                if (ViewState["RelatedId"] == null)
                {
                    ViewState["RelatedId"] = 0;
                }

                return (int)ViewState["RelatedId"];
            }
            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow picking documents].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow picking documents]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowPickingDocuments
        {
            get
            {
                if (ViewState["AllowPickingDocuments"] == null)
                {
                    return false;
                }

                return (bool)ViewState["AllowPickingDocuments"];
            }
            set
            {
                ViewState["AllowPickingDocuments"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the excluded document media ids.
        /// </summary>
        /// <value>
        /// The excluded document media ids.
        /// </value>
        public int[] ExcludedDocumentMediaIds
        {
            get
            {
                if (ViewState["ExcludedDocumentMediaIds"] == null)
                {
                    ViewState["ExcludedDocumentMediaIds"] = new int[] { };
                }

                return (int[])ViewState["ExcludedDocumentMediaIds"];
            }
            set
            {
                ViewState["ExcludedDocumentMediaIds"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show images only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show images only]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowImagesOnly
        {
            get
            {
                if (ViewState["ShowImagesOnly"] == null)
                {
                    return false;
                }

                return (bool)ViewState["ShowImagesOnly"];
            }
            set
            {
                ViewState["ShowImagesOnly"] = value;
            }
        }

        /// <summary>
        /// Total number of media items loaded into the control.
        /// </summary>
        public int LoadedDocumentCount
        {
            get
            {
                if (ViewState["LoadedDocumentCount"] == null)
                {
                    return 0;
                }

                return (int)ViewState["LoadedDocumentCount"];
            }
            set
            {
                ViewState["LoadedDocumentCount"] = value;
            }
        }

        /// <summary>
        /// Number of Images loaded into the control out of all media items.
        /// </summary>
        public int LoadedImageCount
        {
            get
            {
                if (ViewState["LoadedImageCount"] == null)
                {
                    return 0;
                }

                return (int)ViewState["LoadedImageCount"];
            }
            set
            {
                ViewState["LoadedImageCount"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the document media ids to exclude.
        /// </summary>
        /// <value>
        /// The document media ids to exclude.
        /// </value>
        public List<int> DocumentMediaIdsToExclude
        {
            get
            {
                if (ViewState["DocumentMediaIdsToExclude"] == null)
                {
                    return new List<int>();
                }

                return (List<int>)ViewState["DocumentMediaIdsToExclude"];
            }
            set
            {
                ViewState["DocumentMediaIdsToExclude"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the document media ids.
        /// </summary>
        /// <value>
        /// The document media ids.
        /// </value>
        public List<int> DocumentMediaIds
        {
            get
            {
                if (ViewState["DocumentMediaIds"] == null)
                {
                    return new List<int>();
                }

                return (List<int>)ViewState["DocumentMediaIds"];
            }
            set
            {
                ViewState["DocumentMediaIds"] = value;
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

        #endregion Properties

        #region Public Method

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            var medias = this.GetBL<UtilityBL>().GetDocumentMedias(RelatedTableName, RelatedId, ShowImagesOnly, DocumentMediaIdsToExclude);

            LoadedDocumentCount = medias.Count();
            LoadedImageCount = medias.Count(m => m.IsImageFile == true);
            DocumentMediaIds = medias.Select(m => m.DocumentMediaId).ToList<int>();

            lvDocuments.DataSource = medias;
            lvDocuments.DataBind();
        }

        #endregion Public Method

        #region Events

        /// <summary>
        /// Handles the ItemDataBound event of the lvDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvDocuments_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic media = e.Item.DataItem as dynamic;

                ImageDisplay imgDisplay = (ImageDisplay)e.Item.FindControl("thumbImage");
                imgDisplay.ImageTitle = media.Name;
                imgDisplay.DocumentMediaId = media.DocumentMediaId;
                imgDisplay.IsThumbnail = true;
                imgDisplay.FunctionPrefix = FunctionPrefix;

                LinkButton lnkbtnDocument = (LinkButton)e.Item.FindControl("lnkbtnDocument");
                lnkbtnDocument.Enabled = AllowPickingDocuments;

                //Selected Image
                if (AllowPickingDocuments && ExcludedDocumentMediaIds.Contains((int)media.DocumentMediaId))
                {
                    imgDisplay.CssClass = "highlighted";
                    lnkbtnDocument.Enabled = false;
                }

                //Disable show preview feature in picking mode
                imgDisplay.ShowImagePreview = !AllowPickingDocuments;

                if (AllowPickingDocuments)
                {
                    lnkbtnDocument.CommandArgument = media.DocumentMediaId.ToString();
                }
            }
        }

        /// <summary>
        /// When an image is clicked, inform the picked DocumentMediaId to event subscribers.
        /// </summary>
        protected void lvDocuments_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int pickedDocumentMediaId = 0;
                int.TryParse(e.CommandArgument.ToString(), out pickedDocumentMediaId);

                if (pickedDocumentMediaId > 0 && DocumentPicked != null)
                {
                    DocumentListDocumentPickedEventArgs args = new DocumentListDocumentPickedEventArgs();
                    args.DocumentMediaId = pickedDocumentMediaId;
                    DocumentPicked(this, args);
                }
            }
        }

        #endregion Events
    }

    /// <summary>
    /// Document List Document Picked EventArgs class.
    /// </summary>
    public class DocumentListDocumentPickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the document media identifier.
        /// </summary>
        /// <value>
        /// The document media identifier.
        /// </value>
        public int DocumentMediaId
        {
            get;
            set;
        }
    }
}