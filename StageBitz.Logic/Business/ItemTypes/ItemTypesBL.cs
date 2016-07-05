using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Location;
using StageBitz.Logic.Business.Project;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StageBitz.Logic.Business.ItemTypes
{
    /// <summary>
    ///  Business layer for item type related operations
    /// </summary>
    public class ItemTypesBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTypesBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public ItemTypesBL(StageBitzDB dataContext)
            : base(dataContext) { }

        /// <summary>
        /// Gets all item type data.
        /// </summary>
        /// <returns></returns>
        public List<ItemTypeData> GetAllItemTypeData()
        {
            return (from it in DataContext.ItemTypes
                    select new ItemTypeData
                    {
                        Id = it.ItemTypeId,
                        Name = it.Name
                    }).ToList();
        }

        /// <summary>
        /// Gets the item type fields preferences.
        /// </summary>
        /// <returns></returns>
        public DataTable GetItemTypeFieldsPreferences()
        {
            DataTable ItemTypeTable = new DataTable();
            List<ItemTypeData> allItemTypes = GetAllItemTypeData();
            List<Data.ItemTypeField> allItemTypeFields = DataContext.ItemTypeFields.ToList();

            ItemTypeTable.Columns.Add(new DataColumn("Field", typeof(string)));
            ItemTypeTable.Columns.Add(new DataColumn("GroupId", typeof(string)));
            ItemTypeTable.Columns.Add(new DataColumn("GroupName", typeof(string)));
            ItemTypeTable.Columns.Add(new DataColumn("ProjectUsingField", typeof(string)));
            ItemTypeTable.Columns.Add(new DataColumn("ItemTypeUsingField", typeof(string)));
            foreach (ItemTypeData itemTypeData in allItemTypes)
            {
                ItemTypeTable.Columns.Add(new DataColumn(itemTypeData.Name, typeof(ItemFieldTypeData)));
            }

            List<FieldData> allFields = GetAllFieldData(allItemTypeFields);

            DataRow[] dataRows = new DataRow[allFields.Count];
            int i = 0;

            foreach (FieldData fieldData in allFields)
            {
                dataRows[i] = ItemTypeTable.NewRow();
                dataRows[i]["Field"] = fieldData.FieldName;
                dataRows[i]["GroupId"] = fieldData.GroupId;
                dataRows[i]["GroupName"] = fieldData.GroupName;
                dataRows[i]["ProjectUsingField"] = fieldData.ProjectUsingField;
                dataRows[i]["ItemTypeUsingField"] = fieldData.ItemTypeUsingField;
                foreach (ItemTypeData itemTypeData in allItemTypes)
                {
                    ItemFieldTypeData itemFieldTypeData = new ItemFieldTypeData();
                    itemFieldTypeData.ItemTypeId = itemTypeData.Id;
                    itemFieldTypeData.FieldId = fieldData.FieldId;
                    itemFieldTypeData.IsSelected = allItemTypeFields.Where(itf => itf.ItemTypeId == itemTypeData.Id && itf.FieldId == fieldData.FieldId).FirstOrDefault() != null;
                    dataRows[i][itemTypeData.Name] = itemFieldTypeData;
                }
                ItemTypeTable.Rows.Add(dataRows[i]);
            }

            return ItemTypeTable;
        }

        /// <summary>
        /// Gets all field data.
        /// </summary>
        /// <param name="allItemTypeFields">All item type fields.</param>
        /// <returns></returns>
        public List<FieldData> GetAllFieldData(List<Data.ItemTypeField> allItemTypeFields)
        {
            var projectItemTypes = DataContext.ProjectItemTypes.Select(pit => new { pit.ProjectId, pit.ItemTypeId }).ToList();
            return (from f in DataContext.Fields
                    where f.IsActive == true && f.FieldGroup.IsActive == true
                    select new FieldData
                    {
                        FieldId = f.FieldId,
                        FieldName = f.DisplayName,
                        GroupId = f.GroupId,
                        GroupName = f.FieldGroup.DisplayName,
                        ItemTypeUsingField = (from ibv in DataContext.ItemBriefValues
                                              join ibt in DataContext.ItemBriefTypes on ibv.ItemBriefId equals ibt.ItemBriefId
                                              join itf in DataContext.ItemTypeFields on ibt.ItemTypeId equals itf.ItemTypeId
                                              where ibv.FieldId == f.FieldId && ibt.IsActive == true
                                              select ibv.ItemBriefId).Distinct().Count(),
                        ProjectUsingField = (from ibv in DataContext.ItemBriefValues
                                             join ibt in DataContext.ItemBriefTypes on ibv.ItemBriefId equals ibt.ItemBriefId
                                             join itf in DataContext.ItemTypeFields on ibt.ItemTypeId equals itf.ItemTypeId
                                             where ibv.FieldId == f.FieldId && ibt.IsActive == true
                                             select ibv.ItemBrief.ProjectId).Distinct().Count(),
                    }).ToList();
        }

        /// <summary>
        /// Gets the item brief field values.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public List<ItemBriefValue> GetItemBriefFieldValues(int itemBriefId)
        {
            return DataContext.ItemBriefValues.Where(ibv => ibv.ItemBriefId == itemBriefId).ToList();
        }

        /// <summary>
        /// Gets the item version history values.
        /// </summary>
        /// <param name="ItemVersionHistoryId">The item version history identifier.</param>
        /// <returns></returns>
        public List<ItemVersionHistoryValue> GetItemVersionHistoryValues(int ItemVersionHistoryId)
        {
            return DataContext.ItemVersionHistoryValues.Where(ivhv => ivhv.ItemVersionHistoryId == ItemVersionHistoryId).ToList();
        }

        /// <summary>
        /// Gets the item field values.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public List<ItemValue> GetItemFieldValues(int itemId)
        {
            return DataContext.ItemValues.Where(iv => iv.ItemId == itemId).ToList();
        }

        /// <summary>
        /// Gets the item brief field value.
        /// </summary>
        /// <param name="itemBriefValueId">The item brief value identifier.</param>
        /// <returns></returns>
        public ItemBriefValue GetItemBriefFieldValue(int itemBriefValueId)
        {
            return DataContext.ItemBriefValues.Where(ibv => ibv.ItemBriefValueId == itemBriefValueId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item field value by id.
        /// </summary>
        /// <param name="itemValueId">The item value identifier.</param>
        /// <returns></returns>
        public ItemValue GetItemFieldValueById(int itemValueId)
        {
            return DataContext.ItemValues.Where(iv => iv.ItemValueId == itemValueId).ToList().FirstOrDefault();
        }

        /// <summary>
        /// Gets the item type HTML.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public string GetItemTypeHTML(int itemTypeId)
        {
            return DataContext.ItemTypeHtmls.Where(ith => ith.ItemTypeId == itemTypeId).FirstOrDefault().Html;
        }

        /// <summary>
        /// Gets the item details.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="itemTypeIdToBeLoad">The item type identifier to be load.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int itemId, int itemBriefId, int userId, int? itemTypeIdToBeLoad, DateTime? fromDate = null, DateTime? toDate = null)
        {
            bool isItemCreated = false;
            bool isReadOnly = false;
            int itemTypeId = 0;

            InventoryBL inventoryBL = new InventoryBL(DataContext);
            ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);

            ItemDetails itemDetails = new ItemDetails();
            InitializeIds(ref itemId, ref itemBriefId, ref isItemCreated);

            if (itemId > 0)
            {
                StageBitz.Data.Item item = inventoryBL.GetItem(itemId);
                if (item != null)
                {
                    ItemBooking itemBooking = null;
                    itemDetails.IsEditableToAdminOnly = true;

                    if (itemBriefId > 0)
                    {
                        ProjectBL projectBL = new ProjectBL(DataContext);
                        itemBooking = itemBriefBL.GetInUseItemBooking(itemBriefId);
                        itemDetails.CanEditInItemBrief = inventoryBL.CanEditIteminItemBrief(itemBriefId, itemId);
                        itemDetails.IsEditableToAdminOnly = inventoryBL.IsItemGeneretedFromGivenItemBrief(itemId, itemBriefId) && !projectBL.IsReadOnlyRightsForProject(itemBriefBL.GetItemBrief(itemBriefId).ProjectId, userId, true);
                    }
                    else
                    {
                        itemDetails.MinQuantity = inventoryBL.GetMaxBookedQuantityForAllDuration(item.ItemId);

                        ItemStatusInformationForUser itemStatusInformationForUser = inventoryBL.GetItemStatusInformationForUser(item, item.CompanyId.Value, userId);
                        isReadOnly = itemStatusInformationForUser.IsReadOnly;
                        itemDetails.IsEditableToAdminOnly = itemStatusInformationForUser.CanEditQuantity;
                    }

                    itemDetails.Name = item.Name;
                    itemDetails.Description = item.Description;
                    itemDetails.LocationId = item.LocationId;
                    itemDetails.Quantity = itemBooking != null ? itemBooking.Quantity : item.Quantity.HasValue ? item.Quantity.Value : item.Quantity;

                    User bookingManager = inventoryBL.GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                    if (bookingManager != null)
                    {
                        itemDetails.BookingManagerName = Utils.GetFullName(bookingManager);
                        itemDetails.BookingManagerEmail = bookingManager.Email1;
                    }

                    if (fromDate.HasValue && toDate.HasValue)
                        itemDetails.AvailableQty = DataContext.GetAvailableItemQuantity(itemId, fromDate.Value, toDate.Value, 0).FirstOrDefault().Value;
                    itemDetails.CreatedFor = item.CreatedFor;
                    itemDetails.ItemBriefId = itemBriefId;
                    itemDetails.LastUpdatedDate = item.LastUpdatedDate;
                    itemTypeId = itemTypeIdToBeLoad.HasValue ? itemTypeIdToBeLoad.Value : item.ItemTypeId.Value;

                    itemDetails.LastUpdatedDate = item.LastUpdatedDate;

                    itemDetails.ItemStatus = itemBooking != null ? inventoryBL.GetItemBookingStatus(itemBooking.ItemBookingId).Description : string.Empty;
                    itemDetails.ItemId = itemId;
                    itemDetails.ItemValues = GetItemFieldValues(itemId).Select(iv =>
                                                            new ValuesInfo
                                                            {
                                                                Id = iv.ItemValueId,
                                                                FieldId = iv.FieldId,
                                                                FieldOptionId = iv.FieldOptionId,
                                                                Value = iv.Value
                                                            }).ToList();

                    itemDetails.CountryId = inventoryBL.GetCountryIdByItemId(item.ItemId);
                    itemDetails.ItemTypeId = item.ItemTypeId;
                    itemDetails.Status = "OK";
                }
            }
            else if (itemBriefId > 0)
            {
                Data.ItemVersionHistory itemVersionHistory = inventoryBL.GetItemVersionHistoryByItemBriefId(itemBriefId);
                itemDetails.ItemBriefId = itemBriefId;
                if (itemVersionHistory != null)
                {
                    itemId = itemVersionHistory.ItemId;
                    itemDetails.Name = itemVersionHistory.Name;
                    itemDetails.Description = itemVersionHistory.Description;
                    itemDetails.LocationId = null; //itemVersionHistory.Location;
                    itemDetails.Quantity = itemVersionHistory.Quantity.HasValue ? itemVersionHistory.Quantity.Value : itemDetails.Quantity;
                    itemDetails.IsEditableToAdminOnly = false;
                    itemDetails.ItemBriefId = itemVersionHistory.ItemBriefId;
                    itemDetails.ItemValues = GetItemVersionHistoryValues(itemVersionHistory.ItemVersionHistoryId).Select(ivhv =>
                                                            new ValuesInfo
                                                            {
                                                                Id = ivhv.ItemVersionHistoryId,
                                                                FieldId = ivhv.FieldId,
                                                                FieldOptionId = ivhv.FieldOptionId,
                                                                Value = ivhv.Value
                                                            }).ToList();
                    itemTypeId = itemTypeIdToBeLoad.HasValue ? itemTypeIdToBeLoad.Value : itemVersionHistory.ItemTypeId.Value;
                    itemDetails.CountryId = inventoryBL.GetCountryIdByItemId(itemVersionHistory.ItemId);
                    itemDetails.ItemTypeId = itemVersionHistory.ItemTypeId;
                    itemDetails.Status = "OK";
                }
                else
                {
                    StageBitz.Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemBriefId);
                    if (itemBrief != null)
                    {
                        itemDetails.Name = itemBrief.Name;
                        itemDetails.Description = itemBrief.Description;
                        itemDetails.LocationId = null;
                        itemDetails.IsEditableToAdminOnly = true;
                        itemDetails.ItemBriefId = itemBriefId;
                        itemDetails.Quantity = itemBrief.Quantity.HasValue ? itemBrief.Quantity.Value : itemBrief.Quantity;
                        itemDetails.ItemBriefId = itemBrief.ItemBriefId;
                        itemDetails.ItemValues = GetItemBriefFieldValues(itemBriefId).Select(ibfv =>
                                                            new ValuesInfo
                                                            {
                                                                Id = 0,
                                                                FieldId = ibfv.FieldId,
                                                                FieldOptionId = ibfv.FieldOptionId,
                                                                Value = ibfv.Value
                                                            }).ToList();

                        itemTypeId = itemTypeIdToBeLoad.HasValue ? itemTypeIdToBeLoad.Value : itemBriefBL.GetItemBriefType(itemBrief.ItemBriefId).ItemTypeId;
                        itemDetails.CountryId = itemBriefBL.GetCountryIdByItemBriefId(itemBrief.ItemBriefId);
                        itemDetails.Status = "OK";
                        itemDetails.CanEditInItemBrief = true;
                        itemDetails.ItemTypeId = itemBrief.ItemBriefTypes.FirstOrDefault().ItemTypeId;
                    }
                }
            }

            itemDetails.IsReadOnly = isReadOnly;
            itemDetails.DisplayMarkUp = GetItemTypeHTML(itemTypeId);

            Data.ItemType itemType = Utils.GetItemTypeById(itemDetails.ItemTypeId.Value);
            itemDetails.ItemTypeName = itemType != null ? itemType.Name : string.Empty;

            return itemDetails;
        }

        /// <summary>
        /// Initializes the ids.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="isItemCreated">if set to <c>true</c> [is item created].</param>
        public void InitializeIds(ref int itemId, ref int itemBriefId, ref bool isItemCreated)
        {
            InventoryBL inventoryBL = new InventoryBL(DataContext);
            ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);

            if (itemId > 0)
            {
                StageBitz.Data.Item item = inventoryBL.GetItem(itemId);

                if (item != null)
                {
                    isItemCreated = true;
                }
            }
            else if (itemBriefId > 0)
            {
                StageBitz.Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemBriefId);

                if (itemBrief != null)
                {
                    ItemBooking itemBooking = itemBriefBL.GetInUseItemBooking(itemBriefId);

                    Data.Item item = null;
                    if (itemBooking != null)
                    {
                        item = itemBooking.Item;
                    }

                    if (item != null)
                    {
                        itemId = item.ItemId;
                        isItemCreated = true;
                    }
                    else
                    {
                        isItemCreated = false;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the item details.
        /// </summary>
        /// <param name="itemDetails">The item details.</param>
        /// <param name="skipConcurrencyCheck">if set to <c>true</c> [skip concurrency check].</param>
        /// <param name="shouldCommit">if set to <c>true</c> [should commit].</param>
        /// <returns></returns>
        public ItemResultObject SaveItemDetails(ItemDetails itemDetails, bool skipConcurrencyCheck = false, bool shouldCommit = false)
        {
            ItemResultObject itemResultObject = new ItemResultObject();
            ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);
            InventoryBL inventoryBL = new InventoryBL(DataContext);
            ProjectBL projectBL = new ProjectBL(DataContext);
            LocationBL locationBL = new LocationBL(DataContext);
            CompanyBL companyBL = new CompanyBL(DataContext);

            int itemId = itemDetails.ItemId;
            int companyId = 0;
            int projectId = 0;

            Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemDetails.ItemBriefId);
            Data.Item item = inventoryBL.GetItem(itemId);

            if (itemDetails.ItemId > 0)
            {
                companyId = item.CompanyId.Value;
            }
            else if (itemBrief != null && item == null)
            {
                companyId = itemBrief.Project.CompanyId;
                projectId = itemBrief.ProjectId;

                item = new Item();
                item.CompanyId = companyId;
            }

            bool isInventoryPage = itemDetails.RelatedTable == "Company";
            string relatedTable = "Company";
            int relatedId = companyId;

            if (itemDetails.RelatedTable == "Project" && projectId > 0)
            {
                relatedTable = itemDetails.RelatedTable;
                relatedId = projectId;
            }

            if (!projectBL.ShouldStopProcessing(relatedTable, relatedId, itemDetails.UserId))
            {
                ErrorCodes errorCode;
                if (itemDetails.ItemBriefId > 0 && !inventoryBL.CanEditIteminItemBrief(itemDetails.ItemBriefId, itemDetails.ItemId))
                {
                    itemResultObject.Status = "CONCURRENCY";
                    itemResultObject.ErrorCode = (int)ErrorCodes.NoEditPermissionForItemInItemBrief;
                    itemResultObject.Message = "No Write Permission";

                    return itemResultObject;
                }
                else if (relatedTable == "Company" && itemId > 0 && inventoryBL.CheckPermissionsForItemDetailsPage(itemDetails.UserId, itemId, companyId, out errorCode, false) == null)
                {
                    itemResultObject.Status = "CONCURRENCY";
                    itemResultObject.ErrorCode = (int)errorCode;
                    itemResultObject.Message = "No Write Permission";

                    return itemResultObject;
                }
                else if (!skipConcurrencyCheck && itemId > 0 && itemDetails.LastUpdatedDate != item.LastUpdatedDate)
                {
                    itemResultObject.Status = "NOTOK";
                    itemResultObject.Message = "Can not update";
                    return itemResultObject;
                }

                item.Name = itemDetails.Name;
                item.Description = itemDetails.Description;

                if (itemDetails.IsEditableToAdminOnly && item.Quantity != itemDetails.Quantity)//Commit if changes exist
                {
                    bool canSaveQty = true;
                    if (item.Quantity > itemDetails.Quantity)//Only when reduces Units
                    {
                        int maxBookedQty = inventoryBL.GetMaxBookedQuantityForAllDuration(itemDetails.ItemId);

                        if (maxBookedQty > itemDetails.Quantity)
                        {
                            canSaveQty = false;
                        }
                    }

                    if (canSaveQty)
                    {
                        item.Quantity = itemDetails.Quantity;
                    }
                    else
                    {
                        itemResultObject.Status = "CONCURRENCY";
                        itemResultObject.ErrorCode = (int)ErrorCodes.QuantityUpdateFailed;
                        itemResultObject.Message = "Can not reduce quantity";
                        return itemResultObject;
                    }
                }
                item.LastUpdatedByUserId = itemDetails.UserId;
                item.LastUpdatedDate = Utils.Now;

                if (isInventoryPage)
                {
                    if (companyBL.HasEditPermissionForInventoryStaff(companyId, itemDetails.UserId, itemDetails.LocationId))
                    {
                        item.CreatedFor = itemDetails.CreatedFor;

                        if (itemDetails.LocationId.HasValue)
                        {
                            Data.Location location = locationBL.GetLocation(itemDetails.LocationId.Value);
                            if (location == null)
                            {
                                itemResultObject.Status = "CONCURRENCY";
                                itemResultObject.ErrorCode = (int)ErrorCodes.InventoryLocationDeleted;
                                itemResultObject.Message = "Item Location not available";
                                return itemResultObject;
                            }
                        }

                        item.LocationId = itemDetails.LocationId;
                    }
                    else
                    {
                        itemResultObject.Status = "CONCURRENCY";
                        itemResultObject.ErrorCode = (int)ErrorCodes.NoEditPermissionForInventory;
                        itemResultObject.Message = "No Edit Permission For Item In Inventory";
                        return itemResultObject;
                    }
                }

                if (itemDetails.ItemTypeId.HasValue)
                {
                    item.ItemTypeId = itemDetails.ItemTypeId;
                }

                //Update ItemBooking
                if (itemBrief != null)
                {
                    ItemBooking itemBooking = inventoryBL.GetItemBookingByItemID(itemId, itemBrief.ItemBriefId, "ItemBrief");
                    itemBooking.Quantity = itemDetails.Quantity.Value;
                    itemBooking.LastUpdateDate = Utils.Now;
                    itemBooking.LastUpdatedBy = itemDetails.UserId;
                }

                if (itemDetails.ItemValues != null)
                {
                    foreach (ValuesInfo iValue in itemDetails.ItemValues)
                    {
                        if (iValue.Id == 0)
                        {
                            //It is an Insert
                            ItemValue iValueNew = new ItemValue();
                            iValueNew.ItemId = item.ItemId;
                            iValueNew.FieldId = iValue.FieldId;
                            iValueNew.Value = iValue.Value;
                            iValueNew.FieldOptionId = iValue.FieldOptionId;
                            DataContext.ItemValues.AddObject(iValueNew);
                        }
                        else
                        {
                            //Update
                            ItemValue itemValue = GetItemFieldValueById(iValue.Id);
                            if (itemValue != null)
                            {
                                if (iValue.Value == string.Empty && iValue.FieldOptionId == null)
                                    DataContext.ItemValues.DeleteObject(itemValue);
                                else
                                {
                                    itemValue.FieldOptionId = iValue.FieldOptionId;
                                    itemValue.Value = iValue.Value;
                                }
                            }
                        }
                    }
                }

                if (shouldCommit)
                    DataContext.SaveChanges();

                itemResultObject.Status = "OK";
                return itemResultObject;
            }
            else
            {
                itemResultObject.Status = "STOPPROCESS";
                return itemResultObject;
            }
        }
    }
}