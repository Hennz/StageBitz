using StageBitz.Data;
using StageBitz.IO.Helpers;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Notification;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for file upload.
    /// </summary>
    public partial class FileUpload : UserControlBase
    {
        #region Events

        /// <summary>
        /// Occurs when [file uploaded].
        /// </summary>
        public event EventHandler FileUploaded;

        #endregion Events

        #region Enums

        /// <summary>
        /// Media type enum.
        /// </summary>
        public enum MediaType
        {
            Images,
            Documents,
            AllFiles
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return btnUploadLaunch.Text;
            }
            set
            {
                popupFileUploader.Title = value;
                btnUploadLaunch.Text = value;
                lnkbtnUploadLaunch.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display launcher as link].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display launcher as link]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayLauncherAsLink
        {
            get
            {
                return lnkbtnUploadLaunch.Visible;
            }
            set
            {
                lnkbtnUploadLaunch.Visible = value;
                btnUploadLaunch.Visible = !value;
            }
        }

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
        /// Gets or sets the type of the upload media.
        /// </summary>
        /// <value>
        /// The type of the upload media.
        /// </value>
        public MediaType UploadMediaType
        {
            get
            {
                if (ViewState["UploadMediaType"] == null)
                {
                    return MediaType.AllFiles;
                }

                return (MediaType)ViewState["UploadMediaType"];
            }
            set
            {
                if (value == MediaType.Images)
                {
                    AllowedFileExtensions = new string[] { "png", "gif", "jpg", "jpeg" };
                }
                else if (value == MediaType.Documents)
                {
                    AllowedFileExtensions = new string[] { "png", "jpg", "jpeg", "gif", "pdf", "doc", "docx", "xls", "xlsx", "cad", "zip", "rar", "txt" };
                }
                else
                {
                    AllowedFileExtensions = null;
                }

                ViewState["UploadMediaType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed file extensions.
        /// </summary>
        /// <value>
        /// The allowed file extensions.
        /// </value>
        private string[] AllowedFileExtensions
        {
            get
            {
                return radUploader.AllowedFileExtensions;
            }
            set
            {
                radUploader.AllowedFileExtensions = value;
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

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupJavascriptUniqueReferences();

                int maxFileSizeMB = GetMaxFileSizeMB();

                ltrlUploadTips.Text = string.Format("Maximum file size is {0}MB.", maxFileSizeMB);
                radUploader.MaxFileSize = maxFileSizeMB * 1048576;

                if (AllowedFileExtensions != null && AllowedFileExtensions.Length > 0)
                {
                    ltrlUploadTips.Text += string.Format(" Allowed file types are {0}.", String.Join(", ", AllowedFileExtensions));
                }
            }

            popupFileUploader.ID = this.ClientID + "popupFileUploader";
        }

        /// <summary>
        /// Handles the FileUploaded event of the radUploader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploadedEventArgs"/> instance containing the event data.</param>
        protected void radUploader_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //This event will be fired for any postback from the page after the file have been asynchronosly uploaded.
                //So we should verify whether we should accept files, using a hidden field value which is set only for OK button click.

                //Only OK button will set the hidden field value to '1' from client side
                if (hdnAcceptStatus.Value == "1" && e.IsValid)
                {
                    //Reset the accept status value.
                    hdnAcceptStatus.Value = string.Empty;

                    DocumentMedia media = new DocumentMedia();

                    //File extension
                    string extension = e.File.GetExtension();
                    if (extension.Length > 0 && extension[0] == '.')
                    {
                        extension = (extension.Length > 1) ? extension.Substring(1, extension.Length - 1) : string.Empty;
                    }

                    media.IsImageFile = ImageHelper.IsImageFileType(extension);

                    switch (RelatedTableName)
                    {
                        case "ItemBrief":
                            SetMediaSortOrder(media);
                            break;

                        case "Item":
                            if (this.GetBL<InventoryBL>().IsItemDeleted(RelatedId))
                            {
                                int companyId = (int)this.GetBL<InventoryBL>().GetItem(RelatedId).CompanyId;
                                popupItemDeletedWarning.ShowItemDeleteMessagePopup(RelatedId, companyId);
                                return;
                            }

                            SetMediaSortOrder(media);
                            break;

                        case "User":
                        case "Company":
                            //User and Company can only have images via AllowedFileExtensions.
                            //But we are performing the isImageFile check just to be safe.
                            if (media.IsImageFile)
                            {
                                //Check for existing images and delete them.
                                //(Only one image can exist)
                                int[] documentMediaIds = (from m in DataContext.DocumentMedias
                                                          where m.RelatedTableName == RelatedTableName && m.RelatedId == RelatedId
                                                          select m.DocumentMediaId).ToArray();

                                foreach (int mediaId in documentMediaIds)
                                {
                                    DataContext.DeleteDocumentMedia(mediaId);
                                }

                                //Set the uploaded image as the default profile image
                                media.SortOrder = 1;
                            }
                            break;

                        default:
                            break;
                    }

                    media.RelatedTableName = RelatedTableName;
                    media.RelatedId = RelatedId;
                    media.FileExtension = extension;
                    media.CreatedBy = media.LastUpdatedBy = UserID;
                    media.CreatedDate = media.LastUpdatedDate = Now;

                    if (RelatedTableName == "ItemBrief" || RelatedTableName == "Item" || RelatedTableName == "Project")
                    {
                        string label = txtName.Text.Trim();
                        if (label.Length > 0)
                            media.Name = label;
                    }

                    if (media.IsImageFile)
                    {
                        //Set image contents
                        media.DocumentMediaContent = ImageHelper.GetResizedImage(e.File.InputStream, false, extension);
                        media.Thumbnail = ImageHelper.GetResizedImage(e.File.InputStream, true, extension);
                    }
                    else
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            e.File.InputStream.CopyTo(memoryStream);
                            media.DocumentMediaContent = memoryStream.ToArray();
                            media.Thumbnail = null;
                        }
                    }

                    DataContext.DocumentMedias.AddObject(media);
                    DataContext.SaveChanges();
                    DocumentMediaId = media.DocumentMediaId;

                    //Geneate notifications for projcts and item briefs. If u need notifications for different types, add them here.
                    if (RelatedTableName == "Project" || RelatedTableName == "ItemBrief")
                    {
                        this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Add);
                    }

                    //Make a Copy to the Item if available. This needs to be done at the end
                    if (RelatedTableName == "ItemBrief" && GetBL<InventoryBL>().CanEditIteminItemBrief(RelatedId))
                    {
                        ItemBooking ItemBooking = this.GetBL<InventoryBL>().GetInUseOrCompleteItemBooking(RelatedId);
                        if (ItemBooking != null && this.GetBL<InventoryBL>().CanEditIteminItemBrief(RelatedId, ItemBooking.ItemId))
                        {
                            //Since we are going to generate an Item file, we need to pass "Item" as the Releted table. Only if it is being "INUSECOMPLETE,INUSE" state.
                            DataContext.CopyMediaFiles(DocumentMediaId, "Item", ItemBooking.ItemId, UserID);
                        }
                    }

                    //Fire the file uploaded event
                    if (FileUploaded != null)
                    {
                        FileUploaded(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnOK_Click(object sender, EventArgs e)
        {
            popupFileUploader.HidePopup();
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the UI.
        /// </summary>
        public void LoadUI()
        {
            if (RelatedTableName != "ItemBrief" && RelatedTableName != "Item" && RelatedTableName != "Project")
            {
                trDocumentLabel.Visible = false;
            }
            else
            {
                trDocumentLabel.Visible = true;
            }

            btnUploadLaunch.Enabled = !IsReadOnly;
            lnkbtnUploadLaunch.Enabled = !IsReadOnly;
            btnOK.Enabled = !IsReadOnly;
            radUploader.Visible = !IsReadOnly;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Get the Max file upload size in MegaBytes from the web.config.
        /// </summary>
        private int GetMaxFileSizeMB()
        {
            int maxLengthBytes = 33554432; //32MB

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Server.MapPath("~/Web.config"));

                XmlNode node = xmlDoc.SelectSingleNode("/configuration/system.webServer/security/requestFiltering/requestLimits[@maxAllowedContentLength]");

                if (node != null)
                {
                    int.TryParse((((XmlElement)node).Attributes[0]).Value, out maxLengthBytes);
                }
            }
            catch
            {
            }

            //Convert to MegaBytes and return
            return maxLengthBytes / 1048576;
        }

        /// <summary>
        /// Prefix javascript function names with the dynamic client ID to avoid conflicts
        /// with the other instances of FileUpload on the same page.
        /// </summary>
        private void SetupJavascriptUniqueReferences()
        {
            btnUploadLaunch.OnClientClick = lnkbtnUploadLaunch.OnClientClick = string.Format("{0}showFileUploader(); return false;", this.ClientID);
            btnOK.OnClientClick = string.Format("{0}setUploadAcceptedStatus();", this.ClientID);
            btnCancel.OnClientClick = string.Format("{0}hideUploader(); return false;", this.ClientID);

            radUploader.OnClientFileUploaded = this.ClientID + "OnClientFileUploaded";
            radUploader.OnClientFileSelected = this.ClientID + "fileUploaderFileSelected";
            radUploader.OnClientFileUploadRemoved = this.ClientID + "fileUploaderFileRemoved";
            radUploader.OnClientValidationFailed = this.ClientID + "fileUploaderValidationFailed";
        }

        /// <summary>
        /// Sets the media sort order.
        /// </summary>
        /// <param name="media">The media.</param>
        private void SetMediaSortOrder(DocumentMedia media)
        {
            if (media.IsImageFile)
            {
                //Check if this item brief already has images.
                //If not set the sort order of the new image to 1.
                int existingImageCount = (from m in DataContext.DocumentMedias
                                          where m.RelatedTableName == RelatedTableName && m.RelatedId == RelatedId && m.IsImageFile == true
                                          select m).Count();

                if (existingImageCount == 0)
                {
                    //This image will be the default image.
                    media.SortOrder = 1;
                }
            }
        }

        #endregion Private Methods
    }
}