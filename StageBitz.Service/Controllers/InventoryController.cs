using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Utility;
using StageBitz.Service.Helpers;
using System;
using System.Linq;
using System.Web.Http;

namespace StageBitz.Service.Controllers
{
    /// <summary>
    /// Web api controller for inventory actions
    /// </summary>
    public class InventoryController : ApiController
    {
        /// <summary>
        /// Gets the initialize data.
        /// </summary>
        /// <param name="userAuthenticationDetailsObj">The user authentication details object.</param>
        /// <returns></returns>
        [HttpPost]
        public MobileInitialData GetInitializeData(InitialRequestDetails userAuthenticationDetailsObj)
        {
            string status = string.Empty;
            string message = string.Empty;
            MobileInitialData mobileInitialData = null;
            try
            {
                bool isValidVersion = Helper.IsValidAppVersion(userAuthenticationDetailsObj.Version, out status);
                if (isValidVersion)
                {
                    int userId = int.Parse(Utils.DecryptStringAES(userAuthenticationDetailsObj.Token));
                    mobileInitialData = Helper.GetAllInitializeDataForUser(userId);
                    if (mobileInitialData == null)
                    {
                        mobileInitialData = new MobileInitialData();
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                status = "ERROR";
                message = "Unknown error.";
            }
            if (mobileInitialData == null)
                mobileInitialData = new MobileInitialData();
            mobileInitialData.Status = status;
            mobileInitialData.Message = message;
            return mobileInitialData;
        }

        /// <summary>
        /// Synchronizes the item.
        /// </summary>
        /// <param name="mobileItem">The mobile item.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemResult SyncItem(MobileItem mobileItem)
        {
            string status = string.Empty;
            string message = string.Empty;
            ItemResult itemResult = new ItemResult();
            bool isValidVersion = true;
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    isValidVersion = Helper.IsValidAppVersion(mobileItem.Version, out status);
                    if (isValidVersion)
                    {
                        if (mobileItem != null)
                        {
                            //Check if this Item has already been created in MobileItem table
                            //If not create
                            InventoryMobileItem inventoryMobileItem = dataContext.InventoryMobileItems.Where(imi => imi.MobileItemId == mobileItem.DeviceItemId).FirstOrDefault();
                            if (inventoryMobileItem == null)
                            {
                                int userId = int.Parse(Utils.DecryptStringAES(mobileItem.Token));

                                //Check if the user can Create the Item
                                CompanyBL companyBL = new CompanyBL(dataContext);
                                FinanceBL financeBL = new FinanceBL(dataContext);
                                InventoryBL inventoryBL = new InventoryBL(dataContext);

                                if (companyBL.HasEditPermissionForInventoryStaff(mobileItem.CompanyId, userId, null))
                                {
                                    //New Creation of Item
                                    if (mobileItem.ItemId == 0)
                                    {
                                        //To create items in the hidden mode, if the limits have reached.
                                        bool isFreeTrailCompany = companyBL.IsFreeTrialCompany(mobileItem.CompanyId);

                                        CompanyPaymentPackage companyPaymentPackage = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(mobileItem.CompanyId);

                                        InventoryPaymentPackageDetails inventoryPaymentPackageDetails = null;
                                        if (companyPaymentPackage != null)
                                        {
                                            inventoryPaymentPackageDetails =
                                                Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(companyPaymentPackage.InventoryPaymentPackageTypeId);
                                        }

                                        CompanyCurrentUsage companyCurrentUsage = financeBL.GetCompanyCurrentUsage(mobileItem.CompanyId, null);

                                        if (!financeBL.HasExceededInventoryLimit(isFreeTrailCompany, inventoryPaymentPackageDetails, companyCurrentUsage))
                                        {
                                            inventoryMobileItem = new InventoryMobileItem();
                                            inventoryMobileItem.MobileItemId = mobileItem.DeviceItemId;
                                            inventoryMobileItem.CreatedBy = userId;
                                            inventoryMobileItem.CreatedDate = Utils.Now;
                                            dataContext.InventoryMobileItems.AddObject(inventoryMobileItem);

                                            Item item = new Item();
                                            item.Name = mobileItem.Name;
                                            item.IsManuallyAdded = true;
                                            item.ItemTypeId = mobileItem.ItemTypeId;
                                            item.Quantity = mobileItem.Quantity;
                                            item.Description = mobileItem.Description;
                                            item.CompanyId = mobileItem.CompanyId;
                                            item.IsActive = true;
                                            item.CreatedByUserId = item.LastUpdatedByUserId = userId;
                                            item.CreatedDate = item.LastUpdatedDate = Utils.Now;
                                            item.VisibilityLevelCodeId = Utils.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");

                                            dataContext.Items.AddObject(item);
                                            dataContext.SaveChanges();
                                            itemResult.Id = item.ItemId;
                                            status = "OK";
                                        }
                                        else
                                        {
                                            status = "LIMITREACHED";
                                            message = "Inventory plan limit reached.";
                                        }
                                    }
                                    else
                                    {
                                        //Edit existing one
                                        Item exItem = inventoryBL.GetItem(mobileItem.ItemId);
                                        if (exItem != null && exItem.IsActive)
                                        {
                                            Code userVisibilityCode = inventoryBL.GetUserInventoryVisibilityLevel(mobileItem.CompanyId, userId, null, false);
                                            if (!inventoryBL.GetItemStatusInformationForUser(exItem, mobileItem.CompanyId, userId).IsReadOnly && exItem.Code.SortOrder >= userVisibilityCode.SortOrder)
                                            {
                                                if (mobileItem.LastUpdateDate == exItem.LastUpdatedDate)
                                                {
                                                    exItem.Name = mobileItem.Name;
                                                    exItem.ItemTypeId = mobileItem.ItemTypeId;
                                                    exItem.Description = mobileItem.Description;
                                                    exItem.Quantity = mobileItem.Quantity;
                                                    exItem.LastUpdatedByUserId = userId;
                                                    exItem.LastUpdatedDate = Utils.Now;
                                                    dataContext.SaveChanges();
                                                    status = "OK";
                                                    itemResult.Id = mobileItem.ItemId;
                                                }
                                                else
                                                {
                                                    status = "ITEMEDITED";
                                                    message = "Item already edited.";
                                                }
                                            }
                                            else
                                            {
                                                status = "NORIGHTS";
                                                message = "Check settings with Company Administrator.";
                                            }
                                        }
                                        else
                                        {
                                            status = "ITEMDELETED";
                                            message = "Item no longer exists.";
                                        }
                                    }
                                }
                                else
                                {
                                    status = "NOACCESS";
                                    message = "Check settings with Company Administrator.";
                                }
                            }
                            else
                            {
                                itemResult.Id = inventoryMobileItem.ItemId;
                                status = "ITEMEXIST";
                                message = "Item already synced.";
                            }
                        }
                    }
                    else
                    {
                        status = "INVALIDAPP";
                        message = "Please update App.";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                status = "ERROR";
                message = "Oops! Unkown error. Sorry...";
            }
            itemResult.MobileId = mobileItem.DeviceItemId;
            itemResult.Status = status;
            itemResult.Message = message;
            return itemResult;
        }

        /// <summary>
        /// Synchronizes the image.
        /// </summary>
        /// <param name="mobileDocumentMedia">The mobile document media.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemResult SyncImage(MobileDocumentMedia mobileDocumentMedia)
        {
            ItemResult itemResult = new ItemResult();
            string message = string.Empty;
            string status = string.Empty;
            bool isValidVersion = true;
            int documentMediaId = 0;
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    if (mobileDocumentMedia != null)
                    {
                        isValidVersion = Helper.IsValidAppVersion(mobileDocumentMedia.Version, out status);
                        if (isValidVersion)
                        {
                            if (mobileDocumentMedia != null)
                            {
                                //Check if the Item is already being synced
                                if (dataContext.InventoryMobileDocumentMedias.Where(imdm => imdm.MobileDocumentMediaId == mobileDocumentMedia.MobileImageId && imdm.RelatedTable == mobileDocumentMedia.RelatedTable && imdm.RelatedId == mobileDocumentMedia.RelatedId).FirstOrDefault() == null)
                                {
                                    //Check if the Item is exist
                                    Data.Item item = dataContext.Items.Where(i => i.ItemId == mobileDocumentMedia.RelatedId).FirstOrDefault();

                                    if (item != null && item.IsActive)
                                    {
                                        int userId = int.Parse(Utils.DecryptStringAES(mobileDocumentMedia.Token));
                                        InventoryBL inventoryBL = new InventoryBL(dataContext);
                                        if (!inventoryBL.GetItemStatusInformationForUser(item, mobileDocumentMedia.CompanyId, userId).IsReadOnly)
                                        {
                                            //Images can be either deleted or added.
                                            if (mobileDocumentMedia.DocumentMediaId == 0)
                                            {
                                                DocumentMedia documentMedia = new DocumentMedia();
                                                documentMedia.DocumentMediaContent = Helper.LoadImage(mobileDocumentMedia.Image, false, mobileDocumentMedia.FileExtension);
                                                documentMedia.Thumbnail = Helper.LoadImage(mobileDocumentMedia.Image, true, mobileDocumentMedia.FileExtension);
                                                documentMedia.RelatedId = mobileDocumentMedia.RelatedId;
                                                documentMedia.RelatedTableName = mobileDocumentMedia.RelatedTable;
                                                documentMedia.SortOrder = inventoryBL.HasDefaultImageSet(mobileDocumentMedia.RelatedId, mobileDocumentMedia.RelatedTable) ? 0 : 1;
                                                documentMedia.IsImageFile = true;
                                                documentMedia.FileExtension = mobileDocumentMedia.FileExtension;
                                                documentMedia.Name = mobileDocumentMedia.Name;
                                                documentMedia.CreatedBy = documentMedia.LastUpdatedBy = userId;
                                                documentMedia.CreatedDate = documentMedia.LastUpdatedDate = Utils.Today;
                                                dataContext.DocumentMedias.AddObject(documentMedia);

                                                InventoryMobileDocumentMedia inventoryMobileDocumentMedia = new InventoryMobileDocumentMedia();
                                                inventoryMobileDocumentMedia.MobileDocumentMediaId = mobileDocumentMedia.MobileImageId;
                                                inventoryMobileDocumentMedia.DocumentMediaId = documentMedia.DocumentMediaId;
                                                inventoryMobileDocumentMedia.RelatedTable = mobileDocumentMedia.RelatedTable;
                                                inventoryMobileDocumentMedia.RelatedId = mobileDocumentMedia.RelatedId;
                                                inventoryMobileDocumentMedia.CreatedBy = userId;
                                                inventoryMobileDocumentMedia.CreatedDate = inventoryMobileDocumentMedia.LastUpdateDate = Utils.Today;
                                                dataContext.InventoryMobileDocumentMedias.AddObject(inventoryMobileDocumentMedia);

                                                dataContext.SaveChanges();
                                                status = "OK";
                                                documentMediaId = documentMedia.DocumentMediaId;
                                            }
                                            else
                                            {
                                                UtilityBL utilityBL = new UtilityBL(dataContext);
                                                DocumentMedia documentMedia = utilityBL.GetDocumentMedia(mobileDocumentMedia.DocumentMediaId);
                                                if (documentMedia != null)
                                                {
                                                    dataContext.DeleteDocumentMedia(mobileDocumentMedia.DocumentMediaId);
                                                    status = "OK";
                                                }
                                                else
                                                {
                                                    status = "OK";
                                                    message = "Image no longer exists.";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            status = "NORIGHTS";
                                            message = "Check settings with Company Administrator.";
                                        }
                                    }
                                    else
                                    {
                                        status = "ITEMDELETED";
                                        message = "Item no longer exists.";
                                    }
                                }
                                else
                                {
                                    status = "EXIST";
                                    message = "Image already synced.";
                                }
                            }
                        }
                        else
                        {
                            status = "INVALIDAPP";
                            message = "Please update App.";
                        }
                    }
                    else
                    {
                        status = "ERROR";
                        message = "Oops! Unkown error. Sorry...";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                status = "ERROR";
                message = "Oops! Unkown error. Sorry...";
            }
            itemResult.Id = documentMediaId;
            itemResult.MobileId = mobileDocumentMedia != null ? mobileDocumentMedia.MobileImageId : "0";
            itemResult.Status = status;
            itemResult.Message = message;
            return itemResult;
        }

        /// <summary>
        /// Gets the item search results.
        /// </summary>
        /// <param name="mobileItemSearchRequest">The mobile item search request.</param>
        /// <returns></returns>
        [HttpPost]
        public MobileSearchResult GetItemSearchResults(MobileItemSearchRequest mobileItemSearchRequest)
        {
            string message = string.Empty;
            string status = string.Empty;
            bool isValidVersion = true;
            MobileSearchResult mobileSearchResult = new MobileSearchResult();
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    isValidVersion = Helper.IsValidAppVersion(mobileItemSearchRequest.Version, out status);
                    if (isValidVersion)
                    {
                        //Check the user security
                        int userId = int.Parse(Utils.DecryptStringAES(mobileItemSearchRequest.Token));

                        if (Utils.CanAccessInventory(mobileItemSearchRequest.CompanyId, userId))
                        {
                            int mobileInventorySearchResultCount = Convert.ToInt32(Utils.GetSystemValue("MobileInventorySearchResultCount"));

                            int itemCount = 0;
                            InventoryBL inventoryBL = new InventoryBL(dataContext);
                            var mobileSearchItems = inventoryBL.GetInventoryItems(userId, mobileItemSearchRequest.CompanyId,
                                mobileItemSearchRequest.CompanyId, mobileItemSearchRequest.ItemName, mobileItemSearchRequest.ItemTypeId,
                                null, null, null, null,
                                mobileInventorySearchResultCount, mobileItemSearchRequest.ViewedResultCount, string.Empty, out itemCount).ToList<InventoryItemData>();

                            int resultCountRemaining = itemCount - mobileItemSearchRequest.ViewedResultCount;
                            bool hasMoreResults = (resultCountRemaining > mobileInventorySearchResultCount);
                            mobileSearchResult.HasMoreResults = hasMoreResults ? 1 : 0;

                            var sortedList = (from sr in mobileSearchItems
                                              select new MobileSearchItem
                                              {
                                                  Name = sr.Item.Name,
                                                  ItemId = sr.Item.ItemId,
                                                  Description = sr.Item.Description,
                                                  Quantity = sr.Item.Quantity.HasValue ? sr.Item.Quantity.Value : 0
                                              }).ToList();

                            mobileSearchResult.MobileSearchItems = sortedList;
                            mobileSearchResult.Status = "OK";
                        }
                        else
                        {
                            mobileSearchResult.Status = "NOTOK";
                            mobileSearchResult.Message = "Check settings with Company Administrator.";
                        }
                    }
                    else
                    {
                        mobileSearchResult.Status = "NOTOK";
                        mobileSearchResult.Message = "Please update App.";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                mobileSearchResult.Status = "ERROR";
                mobileSearchResult.Message = "Oops! Unkown error. Sorry...";
            }
            return mobileSearchResult;
        }

        /// <summary>
        /// Gets the item details.
        /// </summary>
        /// <param name="mobileItemRequestDetails">The mobile item request details.</param>
        /// <returns></returns>
        [HttpPost]
        public MobileItemDetails GetItemDetails(MobileItemRequestDetails mobileItemRequestDetails)
        {
            string message = string.Empty;
            string status = string.Empty;
            bool isValidVersion = true;
            MobileItemDetails mobileItemDetails = new MobileItemDetails();
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    isValidVersion = Helper.IsValidAppVersion(mobileItemRequestDetails.Version, out status);
                    if (isValidVersion)
                    {
                        //Check the user security
                        int userId = int.Parse(Utils.DecryptStringAES(mobileItemRequestDetails.Token));

                        if (Utils.CanAccessInventory(mobileItemRequestDetails.CompanyId, userId))
                        {
                            Item item = dataContext.Items.Where(i => i.ItemId == mobileItemRequestDetails.ItemId).FirstOrDefault();
                            if (item != null)
                            {
                                InventoryBL inventoryBL = new InventoryBL(dataContext);
                                mobileItemDetails.ItemId = item.ItemId;
                                mobileItemDetails.Status = "OK";
                                mobileItemDetails.Name = item.Name;
                                mobileItemDetails.ItemTypeId = item.ItemTypeId.HasValue ? item.ItemTypeId.Value : 0;
                                mobileItemDetails.Description = item.Description;
                                mobileItemDetails.Quantity = item.Quantity.HasValue ? item.Quantity.Value : 0;
                                mobileItemDetails.CanEditItem = inventoryBL.GetItemStatusInformationForUser(item, mobileItemRequestDetails.CompanyId, userId).IsReadOnly ? 0 : 1;
                                //Get Item Status
                                mobileItemDetails.ItemStatus = inventoryBL.GetItemStatus(item.ItemId);

                                mobileItemDetails.LastUpdatedDate = item.LastUpdatedDate.HasValue ? item.LastUpdatedDate.Value : item.CreatedDate.Value;
                                UtilityBL utilityBL = new UtilityBL(dataContext);

                                var medias = utilityBL.GetDocumentMedias("Item", item.ItemId, true, null);
                                if (medias.Count > 0)
                                {
                                    var docMediaIdList = medias.OrderByDescending(m => m.SortOrder).ThenBy(m => m.DocumentMediaId).Select(m => m.DocumentMediaId).ToList<int>();
                                    mobileItemDetails.DocumentMediaIdList = docMediaIdList;
                                }
                            }
                            else
                            {
                                mobileItemDetails.Status = "NOTOK";
                                mobileItemDetails.Message = "Item not exist.";
                            }
                        }
                        else
                        {
                            mobileItemDetails.Status = "NOTOK";
                            mobileItemDetails.Message = "Check settings with Company Administrator.";
                        }
                    }
                    else
                    {
                        mobileItemDetails.Status = "NOTOK";
                        mobileItemDetails.Message = "Please update App.";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                mobileItemDetails.Status = "ERROR";
                mobileItemDetails.Message = "Oops! Unkown error. Sorry...";
            }
            return mobileItemDetails;
        }

        /// <summary>
        /// Gets the image details for item.
        /// </summary>
        /// <param name="mobileItemImageRequestDetails">The mobile item image request details.</param>
        /// <returns></returns>
        [HttpPost]
        public MobileImageDetails GetImageDetailsForItem(MobileItemImageRequestDetails mobileItemImageRequestDetails)
        {
            string message = string.Empty;
            string status = string.Empty;
            bool isValidVersion = true;
            MobileImageDetails mobileImageDetails = new MobileImageDetails();
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    isValidVersion = Helper.IsValidAppVersion(mobileItemImageRequestDetails.Version, out status);
                    if (isValidVersion)
                    {
                        //Check the user security
                        int userId = int.Parse(Utils.DecryptStringAES(mobileItemImageRequestDetails.Token));

                        if (Utils.CanAccessInventory(mobileItemImageRequestDetails.CompanyId, userId))
                        {
                            UtilityBL utilityBL = new UtilityBL(dataContext);

                            var dm = utilityBL.GetDocumentMedia(mobileItemImageRequestDetails.DocumentMediaId);

                            if (dm != null)
                            {
                                mobileImageDetails.DocumentMediaId = dm.DocumentMediaId;
                                mobileImageDetails.Image = Convert.ToBase64String(dm.Thumbnail);
                                mobileImageDetails.Status = "OK";
                            }
                            else
                            {
                                mobileImageDetails.Status = "ERROR";
                                mobileImageDetails.Message = "Oops! Unkown error. Sorry...";
                            }
                        }
                        else
                        {
                            mobileImageDetails.Status = "NOTOK";
                            mobileImageDetails.Message = "Check settings with Company Administrator.";
                        }
                    }
                    else
                    {
                        mobileImageDetails.Status = "NOTOK";
                        mobileImageDetails.Message = "Please update App.";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                mobileImageDetails.Status = "ERROR";
                mobileImageDetails.Message = "Oops! Unkown error. Sorry...";
            }
            return mobileImageDetails;
        }
    }
}