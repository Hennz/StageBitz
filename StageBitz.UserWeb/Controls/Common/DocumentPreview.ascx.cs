using StageBitz.Data;
using StageBitz.IO.Helpers;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// Delegate for inform parent when item deleted.
    /// </summary>
    public delegate void InformItemIsDeleted();

    /// <summary>
    /// User control for document preview.
    /// </summary>
    public partial class DocumentPreview : UserControlBase
    {
        #region Constants

        /// <summary>
        /// The previe w_ size
        /// </summary>
        private const int PREVIEW_SIZE = 400;

        #endregion Constants

        #region Events

        /// <summary>
        /// Occurs when [document attributes changed].
        /// </summary>
        public event EventHandler DocumentAttributesChanged;

        /// <summary>
        /// Occurs when [document deleted].
        /// </summary>
        public event EventHandler DocumentDeleted;

        /// <summary>
        /// The inform when item is deleted
        /// </summary>
        public InformItemIsDeleted InformItemIsDeleted;

        /// <summary>
        /// When document preview.
        /// </summary>
        /// <param name="documentMediaId">The document media identifier.</param>
        public delegate void DocumentPreviewEventHandler(int documentMediaId);

        /// <summary>
        /// Occurs when [document delete clicked].
        /// </summary>
        public event DocumentPreviewEventHandler DocumentDeleteClicked;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this document should remove from database.
        /// </summary>
        /// <value>
        /// <c>true</c> if this document should remove from database; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldRemoveFromDatabase
        {
            get
            {
                if (ViewState["ShouldRemoveFromDatabase"] == null)
                {
                    ViewState["ShouldRemoveFromDatabase"] = true;
                }

                return (bool)ViewState["ShouldRemoveFromDatabase"];
            }
            set
            {
                ViewState["ShouldRemoveFromDatabase"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the document media identifier.
        /// </summary>
        /// <value>
        /// The document media identifier.
        /// </value>
        private int DocumentMediaId
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
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
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
        /// Gets or sets a value indicating whether textboxs is disabled for this instance.
        /// </summary>
        /// <value>
        /// <c>true</c> if textboxs is disabled for this instance; otherwise, <c>false</c>.
        /// </value>
        public bool IsTextboxsDisabled
        {
            get
            {
                if (ViewState["IsTextboxsDisabled"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsTextboxsDisabled"];
                }
            }
            set
            {
                ViewState["IsTextboxsDisabled"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Itemtype.
        /// </summary>
        /// <value>
        /// The Itemtype.
        /// </value>
        public int ItemType
        {
            get
            {
                if (ViewState["ItemType"] == null)
                {
                    return 0;
                }

                return (int)ViewState["ItemType"];
            }
            set
            {
                ViewState["ItemType"] = value;
            }
        }

        #endregion Properties

        #region Events Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializeUI();
            }
        }

        /// <summary>
        /// This will populate the server controls and show the image preview popup.
        /// </summary>
        protected void btnDocumentPreviewLaunch_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //This hidden field contains the value populated from the client side.
                int documentMediaId = 0;
                int.TryParse(hdnDocumentMediaId.Value, out documentMediaId);

                if (documentMediaId > 0)
                {
                    DocumentMedia media = GetBL<UtilityBL>().GetDocumentMedia(documentMediaId);

                    if (media != null && Support.CanAccessMedia(media, DataContext))
                    {
                        DocumentMediaId = documentMediaId;

                        var itemBriefItemDocumentMedia = DataContext.ItemBriefItemDocumentMedias.Where(ibi => ibi.ItemBriefDocumentMediaId == DocumentMediaId).FirstOrDefault();

                        imgPreview.Src = ResolveUrl(Support.GetImageFilePreviewUrl(documentMediaId, PREVIEW_SIZE));
                        lnkDownload.HRef = ResolveUrl(Support.GetImageFileDownloadUrl(documentMediaId));

                        txtName.Text = media.Name;
                        string fileType = "image";
                        if (ImageHelper.IsImageFileType(media.FileExtension))
                        {
                            lblDocumentExtension.Visible = false;
                            popupDocumentPreview.Title = "Preview Image";
                            lnkDownload.Title = "Download full size image";
                            popupDocumentPreviewRemoveConfirmation.Title = "Remove Image";
                            documentLabelWatermark.WatermarkText = "Click to set image label";
                        }
                        else
                        {
                            lblDocumentExtension.Visible = true;
                            lblDocumentExtension.Text = media.FileExtension;
                            popupDocumentPreview.Title = "File Properties";
                            lnkDownload.Title = "Download File";
                            fileType = "file";
                            popupDocumentPreviewRemoveConfirmation.Title = "Remove File";
                            documentLabelWatermark.WatermarkText = "Click to set file label";
                        }
                        if (itemBriefItemDocumentMedia != null)
                            ltrlRemoveConfirmText.Text = string.Format("Deleting this {0} will also remove it from the Complete Item tab. <br /> Are you sure you want to remove this {0}?", fileType);
                        else
                            ltrlRemoveConfirmText.Text = string.Format("Are you sure you want to remove this {0}?", fileType);

                        popupDocumentPreview.ShowPopup();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDone_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (IsReadOnly)
                {
                    popupDocumentPreview.HidePopup();
                    return;
                }

                var media = (from m in DataContext.DocumentMedias
                             where m.DocumentMediaId == DocumentMediaId
                             select m).FirstOrDefault();

                //Compare the old value and new value to identify whether there has been any changes.
                string newLabel = txtName.Text.Trim();
                string oldLabel = string.Empty;
                if (media != null && media.Name != null)
                {
                    oldLabel = media.Name;
                }

                if (newLabel != oldLabel)
                {
                    DataContext.UpdateItemBriefDocumentMediaNameChange(DocumentMediaId, newLabel, UserID);
                    if (media.RelatedTableName == "Project" || media.RelatedTableName == "ItemBrief")
                    {
                        this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Edit, newLabel);
                    }

                    if (DocumentAttributesChanged != null)
                    {
                        DocumentAttributesChanged(this, EventArgs.Empty);
                    }
                }

                //Clear the stored document media id
                DocumentMediaId = 0;

                popupDocumentPreview.HidePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveConfirm_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                var media = (from m in DataContext.DocumentMedias
                             where m.DocumentMediaId == DocumentMediaId
                             select m).FirstOrDefault();

                if (media != null)
                {
                    if (this.GetBL<InventoryBL>().CanDeleteMedia(media.RelatedTableName, media.RelatedId))
                    {
                        //Clear the stored document media id
                        DocumentMediaId = 0;

                        if (DocumentDeleteClicked != null)
                        {
                            DocumentDeleteClicked(media.DocumentMediaId);
                        }

                        if (ShouldRemoveFromDatabase)
                        {
                            DeleteMedia(media);
                        }

                        if (media.RelatedTableName == "Project" || media.RelatedTableName == "ItemBrief")
                        {
                            this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Delete);
                        }

                        popupDocumentPreviewRemoveConfirmation.HidePopup();
                        popupDocumentPreview.HidePopup();

                        if (DocumentDeleted != null)
                        {
                            DocumentDeleted(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        popupDocumentPreviewRemoveConfirmation.HidePopup();
                        popupDocumentPreview.HidePopup();
                        if (InformItemIsDeleted != null)
                        {
                            InformItemIsDeleted();
                        }
                    }
                }
            }
        }

        #endregion Events Handlers

        #region Public Methods

        /// <summary>
        /// Deletes the specified media item from the database depending on it's related table and id
        /// </summary>
        public void DeleteMedia(dynamic media)
        {
            string relatedTable = media.RelatedTableName;

            switch (relatedTable)
            {
                case "ItemBrief":
                    DataContext.DeleteDocumentMediaFromItemBriefItem(0, media.DocumentMediaId);
                    break;

                case "Item":
                    DataContext.DeleteItemFiles(media.DocumentMediaId);
                    break;

                case "Project":
                    DataContext.DeleteDocumentMedia(media.DocumentMediaId);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        public void InitializeUI()
        {
            txtName.Enabled = !(IsReadOnly || IsTextboxsDisabled);
            imgbtnRemoveDocument.Visible = !IsReadOnly;
            btnRemoveConfirm.Enabled = !IsReadOnly;
            documentLabelWatermark.Enabled = !IsReadOnly;
            imgbtnRemoveDocument.OnClientClick = string.Concat("showPopup('", popupDocumentPreviewRemoveConfirmation.ClientID, "'); return false;");
            imgbtnRemoveDocument.ImageUrl = string.Format("~/Common/Images/Remove.png?v={0}", ApplicationVersionString);
            imgDownload.Src = string.Format("~/Common/Images/download.png?v={0}", ApplicationVersionString);
        }

        #endregion Public Methods
    }
}