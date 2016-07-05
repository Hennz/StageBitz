using NSoup.Safety;
using StageBitz.Data;
using StageBitz.Data.DataTypes.Finance;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace StageBitz.Common
{
    public static class Utils
    {
        #region Encryption/Decryption

        /// <summary>
        /// Generates a hashed password suitable for storing in the datbase
        /// </summary>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");
        }

        private static byte[] _salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
        private static string sharedSecret = "Geveo-StageBitz-SharedSecret";

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static byte[] EncryptStringAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            byte[] outBytes = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt))
                using (aesAlg = new RijndaelManaged()) // Create a RijndaelManaged object
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // prepend the IV
                        msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                //Write all data to the stream.
                                swEncrypt.Write(plainText);
                            }
                        }
                        outBytes = msEncrypt.ToArray();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outBytes;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherBytes">The byte[] to decrypt.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string DecryptStringAES(byte[] cipherBytes)
        {
            if (cipherBytes == null)
                throw new ArgumentNullException("cipherBytes");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt))    // generate the key from the shared secret and the salt
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes)) // Create the streams used for decryption.
                using (aesAlg = new RijndaelManaged())  // Create a RijndaelManaged object with the specified key and IV.
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        /// <summary>
        /// Reads the byte array.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.SystemException">
        /// Stream did not contain properly formatted byte array
        /// or
        /// Did not read byte array properly
        /// </exception>
        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        #endregion Encryption/Decryption

        #region String Truncation

        /// <summary>
        /// Ellipsize string at specified length with a default of 20% length tolerence
        /// </summary>
        public static string Ellipsize(string str, int length)
        {
            int tolerance = (int)(length * 0.2); // 20% of length
            return Ellipsize(str, length, tolerance);
        }

        /// <summary>
        /// Ellipsize a string at specified length and tolerence. Returned string will be trimmed to length (length ± tolernce)
        /// </summary>
        public static string Ellipsize(string str, int length, int tolerance)
        {
            // set a minimum tolerence of 1. Elipsizing would add the "..." charactors and  consider this as one charactor when calculating length
            if (tolerance < 1)
                tolerance = 1;

            int maxLen = length + tolerance;

            if (str.Length <= maxLen)
                return str; // no trim required. Return string without ellipses

            // Find the best position to add ellipses within the length tolerences, preferably at a Seperator point
            char[] seperators = { ' ', '.', ',' };
            const string ellipses = "...";

            // do a forward scan upto tolerence lenth to find a seperator. If found, return with ellipses
            for (int i = length; i < maxLen; i++)
            {
                if (seperators.Contains(str[i]))
                    return str.Substring(0, i).TrimEnd(seperators) + ellipses;
            }

            // do a reverse scan upto tolerence lenth to find a seperator. If found, return with ellipses
            int minLen = length - tolerance;
            for (int i = length - 1; i >= minLen; i--)
            {
                if (seperators.Contains(str[i]))
                    return str.Substring(0, i).TrimEnd(seperators) + ellipses;
            }

            // No seperator found within tolerence range. Trim and return with ellipses
            return str.Substring(0, length).TrimEnd(seperators) + ellipses;
        }

        /// <summary>
        /// Reverses the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Reversed text.</returns>
        public static string Reverse(string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Ellipsize text in reverses manner.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string ReverseEllipsize(string text, int length)
        {
            return Utils.Reverse(Utils.Ellipsize(Utils.Reverse(text), length));
        }

        #endregion String Truncation

        #region Code values and System values

        /// <summary>
        /// Gets the system value.
        /// </summary>
        /// <param name="systemValueName">Name of the system value.</param>
        /// <returns></returns>
        public static string GetSystemValue(string systemValueName)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return SystemCache.SystemValues[systemValueName];
            }
        }

        /// <summary>
        /// Gets all item types.
        /// </summary>
        /// <returns></returns>
        public static List<ItemType> GetALLItemTypes()
        {
            return SystemCache.ItemTypes.OrderBy(it => it.Name).ToList();
        }

        /// <summary>
        /// Gets the item type by item type id.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public static ItemType GetItemTypeById(int itemTypeId)
        {
            return SystemCache.ItemTypes.Where(it => it.ItemTypeId == itemTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the code by code identifier.
        /// </summary>
        /// <param name="codeId">The code identifier.</param>
        /// <returns></returns>
        public static Code GetCodeByCodeId(int codeId)
        {
            return SystemCache.Codes[codeId];
        }

        /// <summary>
        /// Gets the code by value.
        /// </summary>
        /// <param name="codeHeader">The code header.</param>
        /// <param name="codeValue">The code value.</param>
        /// <returns></returns>
        public static Code GetCodeByValue(string codeHeader, string codeValue)
        {
            return SystemCache.CodeHeaders[codeHeader][codeValue];
        }

        /// <summary>
        /// Gets the codes by code header.
        /// </summary>
        /// <param name="codeHeader">The code header.</param>
        /// <returns></returns>
        public static List<Code> GetCodesByCodeHeader(string codeHeader)
        {
            return SystemCache.CodeHeaders[codeHeader].Values.ToList();
        }

        /// <summary>
        /// Gets the code identifier by code value.
        /// </summary>
        /// <param name="codeHeaderName">Name of the code header.</param>
        /// <param name="codeValue">The code value.</param>
        /// <returns></returns>
        public static int GetCodeIdByCodeValue(string codeHeaderName, string codeValue)
        {
            return GetCodeByValue(codeHeaderName, codeValue).CodeId;
        }

        /// <summary>
        /// Gets the code description.
        /// </summary>
        /// <param name="codeId">The code identifier.</param>
        /// <returns></returns>
        public static string GetCodeDescription(object codeId)
        {
            return GetCodeByCodeId((int)codeId).Description;
        }

        #endregion Code values and System values

        /// <summary>
        /// Gets the system project package details.
        /// </summary>
        /// <returns></returns>
        public static List<ProjectPaymentPackageDetails> GetSystemProjectPackageDetails()
        {
            return SystemCache.ProjectPackageDetailList;
        }

        /// <summary>
        /// Gets the system project package detail by payment package type identifier.
        /// </summary>
        /// <param name="paymentPackageTypeId">The payment package type identifier.</param>
        /// <returns></returns>
        public static ProjectPaymentPackageDetails GetSystemProjectPackageDetailByPaymentPackageTypeId(int paymentPackageTypeId)
        {
            return SystemCache.ProjectPackageDetailList.Where(pp => pp.PackageTypeId == paymentPackageTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the system inventory package details.
        /// </summary>
        /// <returns></returns>
        public static List<InventoryPaymentPackageDetails> GetSystemInventoryPackageDetails()
        {
            return SystemCache.InventoryPaymentPackageDetailList;
        }

        /// <summary>
        /// Gets the system inventory package detail by payment package type identifier.
        /// </summary>
        /// <param name="inventoryPackageTypeId">The inventory package type identifier.</param>
        /// <returns></returns>
        public static InventoryPaymentPackageDetails GetSystemInventoryPackageDetailByPaymentPackageTypeId(int inventoryPackageTypeId)
        {
            return SystemCache.InventoryPaymentPackageDetailList.Where(ipp => ipp.PackageTypeId == inventoryPackageTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the free system inventory package detail.
        /// </summary>
        /// <returns></returns>
        public static InventoryPaymentPackageDetails GetFreeSystemInventoryPackageDetail()
        {
            return SystemCache.InventoryPaymentPackageDetailList.Where(ipp => ipp.Amount == 0).FirstOrDefault();
        }

        /// <summary>
        /// Gets the free system project package detail.
        /// </summary>
        /// <returns></returns>
        public static ProjectPaymentPackageDetails GetFreeSystemProjectPackageDetail()
        {
            return SystemCache.ProjectPackageDetailList.Where(pp => pp.Amount == 0).FirstOrDefault();
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="systemValueName">Name of the system value.</param>
        /// <returns></returns>
        public static string GetCountries(string systemValueName)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return SystemCache.SystemValues[systemValueName];
            }
        }

        /// <summary>
        /// Gets the country by countryId.
        /// </summary>
        /// <param name="countryId">The country identifier.</param>
        /// <returns></returns>
        public static Country GetCountryById(int countryId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return SystemCache.Countries.Where(c => c.CountryId == countryId).FirstOrDefault();
            }
        }

        #region System DateTime

        /// <summary>
        /// Returns the configured system date string from the config file, or the database, depending on the configuration.
        /// </summary>
        private static string GetSystemDateConfigurationValue()
        {
            string systemDateConfig = ConfigurationManager.AppSettings["Debug.SystemDate"];
            if (!string.IsNullOrEmpty(systemDateConfig))
            {
                //If the configuration file specified 'systemvalue', get the value from database system values.
                if (systemDateConfig == "systemvalue")
                {
                    systemDateConfig = Utils.GetSystemValue("SystemDate");
                }
            }

            return systemDateConfig;
        }

        /// <summary>
        /// Returns the current system date. If Debug value is provided, it is used instead of the real date.
        /// </summary>
        public static DateTime Today
        {
            get
            {
                return Utils.Now.Date;
            }
        }

        /// <summary>
        /// Returns the current system date and time. If Debug value is provided, it is used instead of the real date and time.
        /// </summary>
        public static DateTime Now
        {
            get
            {
                DateTime now = DateTime.Now;

                if (Utils.IsDebugMode)
                {
                    string systemDateConfig = GetSystemDateConfigurationValue();

                    if (!string.IsNullOrEmpty(systemDateConfig))
                    {
                        DateTime.TryParse(systemDateConfig, out now);
                    }
                }

                return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, MilliSecoundRoundOff(now.Millisecond));
            }
        }

        #endregion System DateTime

        #region Date/Time Formatting

        /// <summary>
        /// Returns long string representation of the specified date in Monday, July 12th format.
        /// </summary>
        public static string GetLongDateHtmlString(DateTime dt)
        {
            string daySuffix = string.Empty;

            switch (dt.Day)
            {
                case 1:
                case 21:
                case 31:
                    daySuffix = "st";
                    break;

                case 2:
                case 22:
                    daySuffix = "nd";
                    break;

                case 3:
                case 23:
                    daySuffix = "rd";
                    break;

                default:
                    daySuffix = "th";
                    break;
            }

            return string.Format("{0}<sup>{1}</sup>", dt.ToString("dddd, MMMM dd"), daySuffix);
        }

        #endregion Date/Time Formatting

        /// <summary>
        /// Indicates whether the application is in the Debugging mode.
        /// </summary>
        public static bool IsDebugMode
        {
            get
            {
                string debugSetting = ConfigurationManager.AppSettings["Debug.Enabled"];
                bool isDebug = false;

                if (!string.IsNullOrEmpty(debugSetting))
                {
                    bool.TryParse(debugSetting, out isDebug);
                }

                return isDebug;
            }
        }

        /// <summary>
        /// Rounds the off integer values to .
        /// </summary>
        /// <param name="i">The integer value.</param>
        /// <returns></returns>
        private static int MilliSecoundRoundOff(int i)
        {
            int max = 990;
            int roundup = ((int)Math.Round(i / 10.0)) * 10;

            if (roundup > max)
            {
                roundup = max;
            }

            return roundup;
        }

        /// <summary>
        /// Formats the currency.
        /// </summary>
        /// <param name="strCurrency">The string currency.</param>
        /// <param name="cultureInfor">The culture infor.</param>
        /// <returns></returns>
        public static string FormatCurrency(object strCurrency, string cultureInfor)
        {
            return string.Format(new System.Globalization.CultureInfo(cultureInfor), "{0:C}", strCurrency);
        }

        /// <summary>
        /// Formats the date.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static string FormatDate(Object dt)
        {
            if (dt != null)
                return FormatDatetime((DateTime)dt, false);
            else
                return string.Empty;
        }

        /// <summary>
        /// Formats the time.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static string FormatTime(Object dt)
        {
            if (dt != null)
                return FormatDatetime((DateTime)dt, true);
            else
                return string.Empty;
        }

        /// <summary>
        /// Formats the number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public static string FormatNumber(object number)
        {
            return String.Format("{0:N0}", number);
        }

        /// <summary>
        /// Formats the datetime.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="isTimeNeed">if set to <c>true</c> [is time need].</param>
        /// <returns></returns>
        public static string FormatDatetime(DateTime dt, bool isTimeNeed)
        {
            try
            {
                if (isTimeNeed)
                {
                    return dt.ToString("dd MMM yyyy hh:mm tt");
                }
                else
                {
                    return dt.ToString("dd MMM yyyy");
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the datetime.
        /// </summary>
        /// <param name="dtString">The dt string.</param>
        /// <param name="isTimeNeed">if set to <c>true</c> [is time need].</param>
        /// <returns></returns>
        public static DateTime? GetDatetime(string dtString, bool isTimeNeed)
        {
            try
            {
                DateTime returnDatetime;
                if (isTimeNeed)
                {
                    if (!DateTime.TryParseExact(dtString, "dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out returnDatetime))
                    {
                        return null;
                    }
                }
                else
                {
                    if (!DateTime.TryParseExact(dtString, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out returnDatetime))
                    {
                        return null;
                    }
                }

                return returnDatetime;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the culture.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns>Culture name of given country code.</returns>
        public static string GetCultureName(string countryCode)
        {
            var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.EndsWith(countryCode)).FirstOrDefault();
            if (cultureInfo == null)
            {
                string globalizationSection = ConfigurationManager.AppSettings["Globalization"];
                if (!string.IsNullOrEmpty(globalizationSection))
                {
                    return globalizationSection;
                }
                else
                {
                    return "en-AU";
                }
            }
            else
            {
                return cultureInfo.Name;
            }
        }

        /// <summary>
        /// Determines whether [is company inventory staff member] [the specified company identifier].
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static bool IsCompanyInventoryStaffMember(int companyId, int userId, int? locationId, StageBitzDB dataContext)
        {
            return dataContext.Users.Select(u => dataContext.IsCompanyInventoryStaffMember(companyId, userId, locationId)).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether [is company inventory staff member] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public static bool HasCompanyInventoryStaffMemberPermissions(int companyId, int userId, int? locationId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return IsCompanyInventoryAdmin(companyId, userId) || IsCompanyInventoryStaffMember(companyId, userId, locationId, dataContext);
            }
        }

        /// <summary>
        /// Determines whether [is company inventory admin] [the specified company identifier].
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyInventoryTeamMember(int companyId, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.Users.Select(u => dataContext.IsCompanyInventoryTeamMember(companyId, userId)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Determines whether [is company inventory admin] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyInventoryAdmin(int companyId, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.Users.Select(u => dataContext.IsCompanyInventoryAdmin(companyId, userId)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public static string GetFullName(User user)
        {
            string fullName = string.Empty;
            if (user != null)
            {
                fullName = string.Format("{0} {1}", user.FirstName, user.LastName);
            }

            return fullName;
        }

        /// <summary>
        /// Determines whether [is location manager] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public static bool HasLocationManagerPermission(int companyId, int userId, int? locationId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.Users.Select(u => dataContext.HasLocationManagerPermission(companyId, userId, locationId)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Determines whether this user can access inventory.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static bool CanAccessInventory(int companyId, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.Users.Select(u => dataContext.CanAccessInventory(companyId, userId)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="reportBytes">The report bytes.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void ExportReport(byte[] reportBytes, string mimeType, string fileNameExtension, string fileName)
        {
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = mimeType;
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"" + fileName + "." + fileNameExtension + "\"");
            HttpContext.Current.Response.BinaryWrite(reportBytes); // create the file
            HttpContext.Current.Response.Flush(); // send it to the client to download
        }

        /// <summary>
        /// Downs the load file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contenttype">The contenttype.</param>
        public static void DownLoadFile(string filePath, string fileName, string contenttype)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            HttpContext.Current.Response.ContentType = contenttype;
            HttpContext.Current.Response.WriteFile(filePath);
            HttpContext.Current.Response.End();
        }

        #region Html Processing

        /// <summary>
        /// Gets the safe HTML fragments.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static string GetSafeHtmlFragments(string html)
        {
            return NSoup.NSoupClient.Clean(html, Whitelist.BasicWithImages);
        }

        #endregion Html Processing

        #region Url

        /// <summary>
        /// Replaces the query string parameter.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>Replaced query strings</returns>
        public static string ReplaceQueryStringParam(string queryStrings, string parameter, string value)
        {
            var nameValues = HttpUtility.ParseQueryString(queryStrings);
            nameValues.Set(parameter, value);
            return nameValues.ToString();
        }

        #endregion Url
    }
}