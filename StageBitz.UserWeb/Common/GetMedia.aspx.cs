using StageBitz.Data;
using StageBitz.IO.Helpers;
using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace StageBitz.UserWeb.Common
{
    /// <summary>
    /// Get media handler.
    /// </summary>
    public partial class GetMedia : PageBase
    {
        /// <summary>
        /// The placeholder thumb URL.
        /// </summary>
        private const string placeholderThumbUrl = "~/Common/Images/placeholder_thumb.png";

        #region Properties

        /// <summary>
        /// Gets the document media identifier.
        /// </summary>
        /// <value>
        /// The document media identifier.
        /// </value>
        private int DocumentMediaId
        {
            get
            {
                int documentMediaId = 0;

                if (Request["documentMediaId"] != null)
                {
                    int.TryParse(Request["documentMediaId"], out documentMediaId);
                }

                return documentMediaId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is thumbnail.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is thumbnail; otherwise, <c>false</c>.
        /// </value>
        private bool IsThumbnail
        {
            get
            {
                return (Request["thumb"] == "1");
            }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        private int Size
        {
            get
            {
                int size = 0;

                if (Request["size"] != null)
                {
                    int.TryParse(Request["size"], out size);
                }

                return size;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this request is for download.
        /// </summary>
        /// <value>
        /// <c>true</c> if this request is download; otherwise, <c>false</c>.
        /// </value>
        private bool IsDownload
        {
            get
            {
                return (Request["download"] == "1");
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SendMediaFromDB();
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the file data specified by the 'documentMediaId' and sends it to the browser.
        /// </summary>
        private void SendMediaFromDB()
        {
            try
            {
                DocumentMedia media = GetBL<UtilityBL>().GetDocumentMedia(DocumentMediaId);
                if (Support.CanAccessMedia(media, DataContext))
                {
                    //If a media name has not been specified, use the media ID as the file name.
                    string fileName = (media.Name == null) ? media.DocumentMediaId.ToString() : media.Name;
                    fileName = fileName.Replace("\"", string.Empty);

                    if (media.IsImageFile)
                    {
                        byte[] bytes = null;

                        #region Get image bytes

                        //If a specific size has not been specified, get the thumbnail or full content.
                        //Otherwise send the resized image.
                        if (Size == 0)
                        {
                            bytes = IsThumbnail ? media.Thumbnail : media.DocumentMediaContent;
                        }
                        else
                        {
                            using (MemoryStream ms = new MemoryStream(media.DocumentMediaContent))
                            {
                                bytes = ImageHelper.GetResizedImage(ms, Size, ImageHelper.GetImageFormat(media.FileExtension));
                            }
                        }

                        #endregion Get image bytes

                        if (bytes != null)
                        {
                            using (MemoryStream ms = new MemoryStream(bytes))
                            using (Image image = Image.FromStream(ms))
                            {
                                WriteImageResponse(image, fileName, media.FileExtension);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (IsDownload)
                        {
                            WriteBinaryResponse(media.DocumentMediaContent, fileName, media.FileExtension);
                            return;
                        }
                        else
                        {
                            using (Image image = Image.FromFile(GetDocumentIconUrl(media.FileExtension)))
                            {
                                WriteImageResponse(image, "icon", "png");
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            //If any error occurs, or permission is denied, send the palceholder image to the browser.
            using (Image image = Image.FromFile(Server.MapPath(placeholderThumbUrl)))
            {
                WriteImageResponse(image, "noimage", "png");
                return;
            }
        }

        /// <summary>
        /// Writes the specified image content into the response,
        /// considering whether this is a display request or a download request.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="extension">The extension.</param>
        private void WriteImageResponse(Image image, string fileName, string extension)
        {
            if (IsDownload)
            {
                if (extension.Length > 0)
                    extension = "." + extension;

                Response.AppendHeader("Content-Disposition", string.Format("attachment; filename=\"{0}{1}\"", @fileName, extension));
            }

            Response.ContentType = ImageHelper.GetMimeType(image);
            image.Save(Response.OutputStream, image.RawFormat);
            Response.End();
        }

        /// <summary>
        /// Writes the specified binary content into the response.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="extension">The extension.</param>
        private void WriteBinaryResponse(byte[] contents, string fileName, string extension)
        {
            if (extension.Length > 0)
                extension = "." + extension;

            Response.AppendHeader("Content-Disposition", string.Format("attachment; filename=\"{0}{1}\"", @fileName, extension));

            switch (extension)
            {
                case ".doc":
                case ".docx":
                    Response.ContentType = "application/msword";
                    break;

                case ".ppt":
                case ".pptx":
                    Response.ContentType = "application/vnd.ms-powerpoint";
                    break;

                case ".xls":
                case ".xlsx":
                    Response.ContentType = "application/vnd.ms-excel";
                    break;

                case ".pdf":
                    Response.ContentType = "application/pdf";
                    break;

                case ".txt":
                    Response.ContentType = "text/plain";
                    break;

                case ".m4a":
                    Response.ContentType = "audio/mp4a-latm";
                    break;

                case ".mov":
                    Response.ContentType = "video/quicktime";
                    break;

                case ".mpeg":
                case ".mpg":
                    Response.ContentType = "video/mpeg";
                    break;

                case ".avi":
                    Response.ContentType = "video/x-msvideo";
                    break;

                case ".zip":
                    Response.ContentType = "application/zip";
                    break;

                case ".mp3":
                    Response.ContentType = "audio/mpeg";
                    break;

                case ".mp4":
                    Response.ContentType = "video/mp4";
                    break;

                case ".midi":
                case ".mid":
                    Response.ContentType = "audio/midi";
                    break;

                default:
                    Response.ContentType = "application/octet-stream";
                    break;
            }

            Response.OutputStream.Write(contents, 0, contents.Length);
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Gets the document icon URL.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        private string GetDocumentIconUrl(string extension)
        {
            extension = extension.ToLower();

            string iconFileName = "ftype_generic.png";
            string[] recognizedFileTypes = new string[] { "avi", "doc", "docx", "mov", "mp3", 
                    "mp4", "mpeg", "pdf", "ppt", "pptx", "psd", "rar", "txt",
                    "wav", "wma", "wmv", "xls", "xlsx", "zip" };

            if (recognizedFileTypes.Contains(extension))
            {
                iconFileName = string.Format("ftype_{0}.png", extension);
            }

            return Server.MapPath("~/Common/Images/FileTypes/" + iconFileName);
        }

        #endregion Private Methods
    }
}