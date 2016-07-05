using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StageBitz.IO
{
    /// <summary>
    /// Handles all file related operations
    /// </summary>
    public class FileHandler
    {
        /// <summary>
        /// Saves the file to disk.
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public async static Task SaveFileToDisk(byte[] byteStream, string fileName, string path)
        {
            using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await fs.WriteAsync(byteStream, 0, byteStream.Length);
            }
        }

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Creates the zip file.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="zipLocation">The zip location.</param>
        public static void CreateZipFile(string folder, string zipLocation)
        {
            DeleteFile(zipLocation);
            ZipFile.CreateFromDirectory(folder, zipLocation);
        }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static long GetFileSize(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            return info.Length;
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public static void DeleteFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileLocation">The file location.</param>
        public static void DeleteFile(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        /// <summary>
        /// Gets the name of the safe file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetSafeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }
    }
}