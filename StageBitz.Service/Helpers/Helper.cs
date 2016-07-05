using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.IO.Helpers;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StageBitz.Service.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// Builds the application version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        private static string BuildAppVersion(string version)
        {
            string delimStr = ".";
            char[] delimiter = delimStr.ToCharArray();

            //This would contain upto two "." points
            string[] split = version.Split(delimiter, 3);
            string newVersion = string.Empty;
            if (split.Length > 2)
            {
                newVersion = split.GetValue(0) + "." + split.GetValue(1);
            }
            return newVersion;
        }

        /// <summary>
        /// Determines whether given application version is valid.
        /// </summary>
        /// <param name="versionNumber">The version number.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static bool IsValidAppVersion(string versionNumber, out string status)
        {
            string apiVersion = BuildAppVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            string appVersion = BuildAppVersion(versionNumber);

            if (apiVersion != appVersion)
            {
                //Should not be proceed. Indicate it to the user.
                status = "INVALIDVERSION";
                return false;
            }
            status = "OK";
            return true;
        }

        /// <summary>
        /// Gets all initialize data for user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static MobileInitialData GetAllInitializeDataForUser(int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                MobileInitialData mobileInitialData = new MobileInitialData();

                //Get the Companies that the user can access
                CompanyBL companyBL = new CompanyBL(dataContext);
                List<CompanyListInfo> companyList = companyBL.GetCompanyList(userId, false, false, true);
                var extractedcompanyList = (from cl in companyList
                                            select new CompanyListOfUser
                                            {
                                                Id = cl.CompanyId,
                                                Name = cl.CompanyName,
                                                IsCompanyUser = cl.IsInventoryStaff ? 1 : 0
                                            }).ToList<CompanyListOfUser>();

                mobileInitialData.CompanyList = extractedcompanyList;
                //Get all the system Item Types
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                List<ItemTypeData> itemTypeList = inventoryBL.GetAllSystemItemTypes();
                mobileInitialData.ItemTypeList = itemTypeList;

                return mobileInitialData;
            }
        }

        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <param name="imageString">The image string.</param>
        /// <param name="isThumbNail">if set to <c>true</c> [is thumb nail].</param>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static byte[] LoadImage(string imageString, bool isThumbNail, string extension)
        {
            //get a temp image from bytes, instead of loading from disk
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(imageString);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return ImageHelper.GetResizedImage(ms, isThumbNail, extension);
            }
        }
    }
}