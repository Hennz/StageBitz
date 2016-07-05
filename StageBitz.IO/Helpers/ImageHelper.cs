using StageBitz.Common;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace StageBitz.IO.Helpers
{
    /// <summary>
    /// Helper class for images
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// Gets the resized image.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="newSize">The new size.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static byte[] GetResizedImage(Stream inputStream, double newSize, ImageFormat format)
        {
            //This will resize the image in a bounding box maintaining original aspect ratio.

            using (MemoryStream inputMs = new MemoryStream())
            {
                inputStream.CopyTo(inputMs);

                using (Image image = Image.FromStream(inputStream))
                {
                    #region Calculate resize scale

                    double maxHeight = newSize, maxWidth = newSize;
                    double widthScale = 0, heightScale = 0;

                    if (image.Width != 0)
                        widthScale = maxWidth / (double)image.Width;
                    if (image.Height != 0)
                        heightScale = maxHeight / (double)image.Height;

                    double scale = Math.Min(widthScale, heightScale);

                    #endregion Calculate resize scale

                    //Resize only if the size has to be reduced
                    if (scale < 1.0)
                    {
                        int newWidth = (int)(image.Width * scale);
                        int newHeight = (int)(image.Height * scale);

                        using (Bitmap thumbnail = new Bitmap(newWidth, newHeight, image.PixelFormat))
                        //using (Bitmap thumbnail_dup = new Bitmap(thumbnail.Width, thumbnail.Height))
                        using (Graphics g = Graphics.FromImage(thumbnail))
                        {
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            Rectangle rect = new Rectangle(0, 0, newWidth, newHeight);
                            g.DrawImage(image, rect);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                thumbnail.Save(ms, format);

                                return ms.ToArray();
                            }
                        }
                    }
                    else
                    {
                        return inputMs.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Resizes an image according to the system-wide MaxThumbSize/MaxImageSize settings.
        /// </summary>
        /// <param name="inputStream">The input stream containing image data.</param>
        /// <param name="isThumbnail">Whether to resize to thumbnail size or to max image size.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>
        /// Resized image's array of bytes
        /// </returns>
        public static byte[] GetResizedImage(Stream inputStream, bool isThumbnail, string extension)
        {
            double newSize = 0;
            if (isThumbnail)
            {
                double.TryParse(Utils.GetSystemValue("MaxThumbSize"), out newSize);
            }
            else
            {
                double.TryParse(Utils.GetSystemValue("MaxImageSize"), out newSize);
            }

            ImageFormat format = GetImageFormat(extension);
            return GetResizedImage(inputStream, newSize, format);
        }

        /// <summary>
        /// Determines whether [is image file type] [the specified extension].
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static bool IsImageFileType(string extension)
        {
            return ImageTypeExtensions.Contains(extension.ToLower());
        }

        /// <summary>
        /// Gets the image type extensions.
        /// </summary>
        /// <value>
        /// The image type extensions.
        /// </value>
        public static string[] ImageTypeExtensions
        {
            get
            {
                return new string[] { "png", "jpg", "jpeg", "gif" };
            }
        }

        /// <summary>
        /// Gets the image format.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static ImageFormat GetImageFormat(string extension)
        {
            ImageFormat format = ImageFormat.Jpeg;
            switch (extension.ToLower())
            {
                case "png":
                    format = ImageFormat.Png;
                    break;

                case "jpg":
                case "jpeg":
                    format = ImageFormat.Jpeg;
                    break;

                case "gif":
                    format = ImageFormat.Gif;
                    break;
            }

            return format;
        }

        /// <summary>
        /// Get the mime type string for the specified image (eg. 'image/jpg').
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static string GetMimeType(Image image)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == image.RawFormat.Guid)
                {
                    return codec.MimeType;
                }
            }

            return "image/unknown";
        }
    }
}