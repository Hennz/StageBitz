using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Booking;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Location;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StageBitz.Logic.Business.Inventory
{
    /// <summary>
    /// BL class for inventory.
    /// </summary>
    public class InventoryBL : BaseBL
    {
        /// <summary>
        /// The project booking prefix
        /// </summary>
        public const string ProjectBookingPrefix = "P-";

        /// <summary>
        /// The non project booking prefix
        /// </summary>
        public const string NonProjectBookingPrefix = "NP-";

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public InventoryBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="itemid">The itemid.</param>
        /// <returns></returns>
        public Data.Item GetItem(int itemid)
        {
            return (from i in DataContext.Items
                    where i.ItemId == itemid
                    select i).FirstOrDefault();
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void AddItem(Item item, bool commit)
        {
            DataContext.Items.AddObject(item);
            if (commit)
            {
                this.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the item version history.
        /// </summary>
        /// <param name="itemVersionHistoryId">The item version history identifier.</param>
        /// <returns></returns>
        public ItemVersionHistory GetItemVersionHistory(int itemVersionHistoryId)
        {
            return (from ivh in DataContext.ItemVersionHistories
                    where ivh.ItemVersionHistoryId == itemVersionHistoryId
                    select ivh).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item version history.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public ItemVersionHistory GetItemVersionHistory(int itemId, int itemBriefId)
        {
            return (from ivh in DataContext.ItemVersionHistories
                    where ivh.ItemId == itemId && ivh.ItemBriefId == itemBriefId
                    select ivh).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether user can keep given item.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool CanKeepItem(int itemBriefId, int itemId)
        {
            int pinnedCodeid = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            var itemBookingId = DataContext.ItemBookings.Where(ibs => ibs.RelatedId == itemBriefId && ibs.RelatedTable == "ItemBrief" &&
                ibs.ItemId == itemId && ibs.IsActive && (ibs.ToDate == null || ibs.ToDate >= Utils.Today) &&
                ibs.ItemBookingStatusCodeId == pinnedCodeid).FirstOrDefault();

            if (itemBookingId != null)
            {
                Code status = GetItemBookingStatus(itemBookingId.ItemBookingId);
                if (status != null && status.CodeId != Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNEDDELAYED"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether user can edit item with in item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="canComplete">if set to <c>true</c> [can complete].</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool CanEditIteminItemBrief(int itemBriefId, out bool canComplete, int itemId = 0)
        {
            ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);
            Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemBriefId);

            bool isItemInCompletedState = itemBrief.ItemBriefStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "STOREDININVENTORY")
                    || itemBrief.ItemBriefStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "DISPOSEDOF");

            if (isItemInCompletedState)
            {
                canComplete = false;
                return false;
            }
            else
            {
                //Inorder to edit, Item needs to be fall into their period with no any other Itembrief has "In Use" or "In Use Complete"
                var itemBookings = GetCurrentItemBookingDetails(itemBriefId, itemId);

                var activeBookings = DataContext.ItemBookings.Where(ibi => ibi.RelatedId == itemBriefId && ibi.RelatedTable == "ItemBrief" && ibi.IsActive == true);

                // if it has item id and there is no booking, that mean there is concurrency issue.(make it read only).
                if (itemId > 0 && activeBookings.Count() == 0)
                {
                    canComplete = false;
                    return false;
                }

                bool isCurrentItemBookingEditable = itemBookings.Where(ib => ib.ItemBooking.RelatedId == itemBriefId && ib.ItemBooking.RelatedTable == "ItemBrief" && !ib.IsDelayed).Count() == 1;
                bool isOtherBookingsNotUsingCurrentItem = itemBookings.Where(ib => ib.ItemBooking.RelatedId != itemBriefId && ib.ItemBooking.RelatedTable == "ItemBrief" && !ib.IsDelayed).Count() == 0;
                bool hasOverlapedBookings = itemBookings.Where(ib => ib.ItemBooking.RelatedId != itemBriefId && ib.ItemBooking.RelatedTable == "ItemBrief" && !ib.IsDelayed).Count() > 0;

                bool canEdit = (isCurrentItemBookingEditable && isOtherBookingsNotUsingCurrentItem || activeBookings.Count() == 0);
                canComplete = canEdit || (isCurrentItemBookingEditable && hasOverlapedBookings);
                return canEdit;
            }
        }

        /// <summary>
        /// Determines whether user can edit item in item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool CanEditIteminItemBrief(int itemBriefId, int itemId = 0)
        {
            bool canComplete = false;
            return CanEditIteminItemBrief(itemBriefId, out canComplete, itemId);
        }

        /// <summary>
        /// Gets the current item booking details.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public List<ItemBookingInfo> GetCurrentItemBookingDetails(int itemBriefId, int itemId = 0)
        {
            int inUseCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inUseCompleteCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");
            int pinnedCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");

            //Get InUse or InUse Complete Item.
            int selectedItemId = itemId;
            if (itemId == 0)
            {
                ItemBooking itemBooking = GetInUseOrCompleteItemBooking(itemBriefId);
                selectedItemId = itemBooking != null ? itemBooking.ItemId : 0;
            }

            // Take overdue and currently active bookings
            var itemBookings = from ibs in DataContext.ItemBookings
                               where ibs.IsActive == true && ibs.ItemId == selectedItemId && (ibs.ItemBookingStatusCodeId == inUseCodeID || ibs.ItemBookingStatusCodeId == inUseCompleteCodeID
                               || (ibs.ItemBookingStatusCodeId == pinnedCodeID && ibs.RelatedId == itemBriefId && ibs.RelatedTable == "ItemBrief")) && ibs.FromDate <= Utils.Today && (!ibs.ToDate.HasValue || ibs.ToDate >= Utils.Today)
                               select new ItemBookingInfo
                               {
                                   ItemBooking = ibs,
                                   IsDelayed = DataContext.IsItemBookingDelayed(ibs.ItemBookingId)
                               };

            return itemBookings.ToList();
        }

        /// <summary>
        /// Gets the booking details.
        /// </summary>
        /// <param name="bookingDetailsEditRequest">The booking details edit request.</param>
        /// <returns></returns>
        public BookingEdit GetBookingDetails(BookingDetailsEditRequest bookingDetailsEditRequest)
        {
            var bookingDetails = (from b in DataContext.Bookings
                                  join cbn in DataContext.CompanyBookingNumbers on b.BookingId equals cbn.BookingId
                                  from p in DataContext.Projects.Where(p => p.ProjectId == b.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                  from npb in DataContext.NonProjectBookings.Where(npb => npb.NonProjectBookingId == b.RelatedId && b.RelatedTable == "NonProject").DefaultIfEmpty().Take(1)
                                  //join p in DataContext.Projects on b.RelatedId equals p.ProjectId into ps
                                  //from p in ps.DefaultIfEmpty() where b.RelatedTable == "Project"
                                  //join npb in DataContext.NonProjectBookings on b.RelatedId equals npb.NonProjectBookingId into npbs
                                  //from npb in npbs.DefaultIfEmpty()
                                  where b.BookingId == bookingDetailsEditRequest.BookingId && (bookingDetailsEditRequest.CompanyId == 0 || cbn.CompanyId == bookingDetailsEditRequest.CompanyId)
                                  let name = (p != null ? p.ProjectName : npb.Name)
                                  select new { b.RelatedId, cbn.BookingNumber, Name = name }).FirstOrDefault();

            int pinnedCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int notPickedUpStatusCodeId = StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode", "NOTPICKEDUP");
            int relatedtableId = 0;

            if (bookingDetails != null)
            {
                relatedtableId = bookingDetails.RelatedId;
            }
            bool isToDateEdit = bookingDetailsEditRequest.IsToDateEdit;

            var bookingLines = (from b in DataContext.Bookings
                                join ibs in DataContext.ItemBookings on b.BookingId equals ibs.BookingId
                                join i in DataContext.Items on ibs.ItemId equals i.ItemId
                                from ib in DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                from ibt in DataContext.ItemBriefTypes.Where(ibt => ibt.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                //join ib in DataContext.ItemBriefs on ibs.RelatedId equals ib.ItemBriefId into ibss
                                //from igrpb in ibss.DefaultIfEmpty()
                                //join ibt in DataContext.ItemBriefTypes on ibs.RelatedId equals ibt.ItemBriefId into ibts
                                //from ibt in ibts.DefaultIfEmpty()
                                where b.BookingId == bookingDetailsEditRequest.BookingId && (bookingDetailsEditRequest.CompanyId == 0 || i.CompanyId == bookingDetailsEditRequest.CompanyId) //Filter Booking by ItemBriefs Itemtype when it's in Porject related pages else by Items ItemType
                                && ibs.ToDate != null && i.IsActive && ibs.IsActive//Need to get only active records
                                && (bookingDetailsEditRequest.ItemTypeId == 0 || ((bookingDetailsEditRequest.CompanyId > 0 && i.ItemTypeId == bookingDetailsEditRequest.ItemTypeId) || (bookingDetailsEditRequest.CompanyId == 0 && ibt.ItemTypeId == bookingDetailsEditRequest.ItemTypeId)))
                                && !(ibs.ItemBookingStatusCodeId == pinnedCodeID && ibs.IsActive == false && ibs.InventoryStatusCodeId == notPickedUpStatusCodeId)
                                && (!bookingDetailsEditRequest.IsInventoryManager
                                    || DataContext.IsCompanyInventoryAdmin(bookingDetailsEditRequest.CompanyId, bookingDetailsEditRequest.UserId) 
                                    || DataContext.IsCompanyInventoryStaffMember(bookingDetailsEditRequest.CompanyId, bookingDetailsEditRequest.UserId, i.LocationId))
                                select new BookingDetailsToEdit
                                {
                                    FromDate = ibs.FromDate,
                                    ToDate = ibs.ToDate.Value,
                                    LeftMarginDate = !isToDateEdit ? DataContext.GetMaxMarginDateOfItemAvailable(ibs.ItemBookingId, isToDateEdit) : ibs.FromDate,//This is required when editing 'From' Date
                                    RightMarginDate = !isToDateEdit ? ibs.ToDate : DataContext.GetMaxMarginDateOfItemAvailable(ibs.ItemBookingId, isToDateEdit),//This is required when editing 'To' Date
                                    ItemBookingId = ibs.ItemBookingId,
                                    ItemBriefName = ib != null ? ib.Name : string.Empty,
                                    ItemName = i.Name,
                                    LastUpdatedDate = ibs.LastUpdateDate == null ? ibs.CreatedDate : ibs.LastUpdateDate,
                                    IsLocallyEdited = false
                                }).AsEnumerable<BookingDetailsToEdit>();

            int bookingCount = bookingLines.Count();

            BookingEdit bookingEdit = new BookingEdit()
            {
                BookingDetailToEditList = bookingDetailsEditRequest.ShouldIgnoreDefaultSort ? bookingLines.ToList<BookingDetailsToEdit>() : bookingLines.OrderBy(bl => bl.ItemName).ToList<BookingDetailsToEdit>(),
                BookingName = bookingDetails.Name,
                BookingNumber = bookingDetails.BookingNumber.ToString(),
                IsToDateEdit = isToDateEdit,
                DataDisplayStartDate = bookingCount > 0 ? isToDateEdit ? bookingLines.Max(b => b.ToDate) : bookingLines.Min(b => b.FromDate) : Utils.Today,
                ToDay = Utils.Today
            };
            return bookingEdit;
        }

        /// <summary>
        /// Saves the booking details.
        /// </summary>
        /// <param name="bookingDetailsEditedHeader">The booking details edited header.</param>
        /// <returns></returns>
        public BookingDetailsSaveResult SaveBookingDetails(BookingDetailsEditedHeader bookingDetailsEditedHeader)
        {
            BookingDetailsSaveResult bookingDetailsSaveResult = new BookingDetailsSaveResult();
            ProjectBL projectBL = new ProjectBL(DataContext);

            if (bookingDetailsEditedHeader != null)
            {
                int companyId = 0;

                if (bookingDetailsEditedHeader.CompanyId > 0)
                {
                    companyId = bookingDetailsEditedHeader.CompanyId;
                }
                else
                {
                    companyId = GetCompanyBookingNumber(bookingDetailsEditedHeader.BookingId, bookingDetailsEditedHeader.CompanyId).CompanyId;
                }

                bool isToDateEdit = bookingDetailsEditedHeader.IsToDateEdit;

                //Records that are not in "resultSucces" has to be moved in to a seperate list

                List<int> failedibiList = new List<int>();
                List<int> locationManagerList = new List<int>();

                bool canCommit = false;
                foreach (BookingDetailsEdited bookingDetailsEdited in bookingDetailsEditedHeader.BookingDetailsEditedList)
                {
                    ItemBooking ibiObject = GetItemBooking(bookingDetailsEdited.ItemBookingId);
                    if (ibiObject != null)
                    {
                        if (bookingDetailsEditedHeader.IsInventoryManager)
                        {
                            if (!(Utils.IsCompanyInventoryAdmin(companyId, bookingDetailsEditedHeader.UserId)
                                    || Utils.IsCompanyInventoryStaffMember(companyId, bookingDetailsEditedHeader.UserId, ibiObject.Item.LocationId, DataContext)))
                            {
                                failedibiList.Add(bookingDetailsEdited.ItemBookingId);
                                break;
                            }
                        }

                        int qtyAvailable = DataContext.GetAvailableItemQuantity(ibiObject.ItemId, isToDateEdit ? ibiObject.FromDate : bookingDetailsEdited.NewDate, isToDateEdit ? bookingDetailsEdited.NewDate : ibiObject.ToDate, ibiObject.ItemBookingId).FirstOrDefault().Value;
                        if (qtyAvailable >= ibiObject.Quantity)
                        {
                            if (isToDateEdit)
                            {
                                if (ibiObject.ToDate != bookingDetailsEdited.NewDate)
                                {
                                    #region Remove Email history of overdue bookings

                                    int overdueEmailCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGOVERDUE");
                                    Code itemBookingStatus = GetItemBookingStatus(bookingDetailsEdited.ItemBookingId);
                                    if (itemBookingStatus != null && itemBookingStatus.CodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "OVERDUE").CodeId)
                                    {
                                        DataContext.DeleteEmailHistory(bookingDetailsEdited.ItemBookingId, "ItemBooking", overdueEmailCodeId);
                                    }

                                    #endregion Remove Email history of overdue bookings

                                    ibiObject.ToDate = bookingDetailsEdited.NewDate;
                                    canCommit = true;
                                }
                            }
                            else
                            {
                                if (ibiObject.FromDate != bookingDetailsEdited.NewDate)
                                {
                                    ibiObject.FromDate = bookingDetailsEdited.NewDate;
                                    canCommit = true;
                                }
                            }

                            ibiObject.LastUpdateDate = Utils.Now;
                            ibiObject.LastUpdatedBy = bookingDetailsEditedHeader.UserId;

                            #region Add Booking Notification

                            Item item = ibiObject.Item;
                            User locationManager = GetContactBookingManager(item.CompanyId.Value, item.LocationId);

                            if (canCommit && !locationManagerList.Contains(locationManager.UserId) && ibiObject.CreatedBy > 0)
                            {
                                var bookingNotification = DataContext.DailyBookingNotifications.Where(dbn =>
                                        dbn.UserId == ibiObject.CreatedBy &&
                                        dbn.BookingId == bookingDetailsEditedHeader.BookingId &&
                                        dbn.CompanyBookingNumber.CompanyId == bookingDetailsEditedHeader.CompanyId &&
                                        !dbn.IsInventoryManager).FirstOrDefault();

                                bool shouldSendNotification = (bookingDetailsEditedHeader.IsInventoryManager && ibiObject.CreatedBy != bookingDetailsEditedHeader.UserId) ||
                                                                    (!bookingDetailsEditedHeader.IsInventoryManager && locationManager.UserId != bookingDetailsEditedHeader.UserId);

                                if (bookingNotification == null && shouldSendNotification)
                                {
                                    bookingNotification = new Data.DailyBookingNotification
                                    {
                                        IsInventoryManager = !bookingDetailsEditedHeader.IsInventoryManager,//To indicate that whether the receiver Z IM or not.
                                        UserId = bookingDetailsEditedHeader.IsInventoryManager ? ibiObject.CreatedBy : locationManager.UserId,
                                        BookingId = bookingDetailsEditedHeader.BookingId,
                                        CompanyBookingNumberId = GetCompanyBookingNumber(bookingDetailsEditedHeader.BookingId, bookingDetailsEditedHeader.CompanyId).CompanyBookingNumberId,
                                        CreatedByUserId = bookingDetailsEditedHeader.UserId,
                                        LastUpdatedByUserId = bookingDetailsEditedHeader.UserId,
                                        CreatedDate = Utils.Now,
                                        LastUpdatedDate = Utils.Now
                                    };

                                    DataContext.DailyBookingNotifications.AddObject(bookingNotification);
                                }

                                if (!bookingDetailsEditedHeader.IsInventoryManager) //For IM only one Notification should ben generated.
                                {
                                    locationManagerList.Add(locationManager.UserId);
                                }
                            }

                            #endregion Add Booking Notification
                        }
                        else
                        {
                            failedibiList.Add(bookingDetailsEdited.ItemBookingId);
                        }
                        canCommit = false; //Reset to the original
                    }
                }

                DataContext.SaveChanges();

                //Get a freshData pick
                BookingDetailsEditRequest bookingDetailsEditRequest = new BookingDetailsEditRequest();
                bookingDetailsEditRequest.BookingId = bookingDetailsEditedHeader.BookingId;
                bookingDetailsEditRequest.CompanyId = bookingDetailsEditedHeader.CompanyId;
                bookingDetailsEditRequest.ItemTypeId = bookingDetailsEditedHeader.ItemTypeId;
                bookingDetailsEditRequest.IsToDateEdit = isToDateEdit;
                bookingDetailsEditRequest.ToDay = Utils.Today;
                bookingDetailsEditRequest.UserId = bookingDetailsEditedHeader.UserId;
                bookingDetailsEditRequest.ShouldIgnoreDefaultSort = true;
                bookingDetailsEditRequest.IsInventoryManager = bookingDetailsEditedHeader.IsInventoryManager;
                BookingEdit bookingEdit = GetBookingDetails(bookingDetailsEditRequest);

                if (bookingEdit != null)
                {
                    foreach (int ibiID in failedibiList)
                    {
                        var bookingDetailsListToMark = bookingEdit.BookingDetailToEditList.Where(ibi => ibi.ItemBookingId == ibiID).FirstOrDefault();
                        if (bookingDetailsListToMark != null)
                            bookingDetailsListToMark.HasError = true;
                    }

                    if (failedibiList.Count > 0 && bookingEdit.BookingDetailToEditList.Count > 0)
                    {
                        bookingEdit.BookingDetailToEditList = bookingEdit.BookingDetailToEditList.OrderByDescending(ibi => ibi.HasError).ThenBy(ibi => ibi.ItemName).ToList<BookingDetailsToEdit>();
                    }

                    bookingDetailsSaveResult.BookingEdit = bookingEdit;
                }
                bookingDetailsSaveResult.HasBookingLinesError = failedibiList.Count > 0;
                //If non of the operation can't be performed, update the header DS.
            }

            bookingDetailsSaveResult.Status = "OK";

            return bookingDetailsSaveResult;
        }

        /// <summary>
        /// Gets the booking companies.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <returns></returns>
        public IEnumerable<Data.Company> GetBookingCompanies(int bookingId)
        {
            var companyList = DataContext.CompanyBookingNumbers.Where(cbn => cbn.BookingId == bookingId).Select(b => b.Company).OrderBy(c => c.CompanyName);
            return companyList;
        }

        /// <summary>
        /// Gets the booking details.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="showMybookingsOnly">if set to <c>true</c> [show mybookings].</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<CompanyBookingDetails> GetBookingDetails(int bookingId, int companyId, int itemTypeId, bool showMybookingsOnly, int userId)
        {
            Code overdueCode = Utils.GetCodeByValue("ItemBookingStatusCode", "OVERDUE");
            int inuseCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inuseCompleteCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");
            int overDueCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "OVERDUE");
            int onHoldCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int pickedUpStatusCodeID = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "PICKEDUP");
            int pinnedCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int notPickedUpStatusCodeId = StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode", "NOTPICKEDUP");
            int returnedStatusCodeID = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "RETURNED");
            Code rejectedStatus = Utils.GetCodeByValue("ItemBookingStatusCode", "REJECTED");
            Code notApproved = Utils.GetCodeByValue("ItemBookingStatusCode", "NOTAPPROVED");
            Code releasedStatus = Utils.GetCodeByValue("ItemBookingStatusCode", "RELEASED");
            Code returnedStatus = Utils.GetCodeByValue("InventoryStatusCode", "RETURNED");

            int companyIdForQuery = companyId > 0 ? companyId : GetCompanyBookingNumber(bookingId, companyId).CompanyId;

            var itemBookings = (from b in DataContext.Bookings
                                join ibs in DataContext.ItemBookings on b.BookingId equals ibs.BookingId
                                join i in DataContext.Items on ibs.ItemId equals i.ItemId
                                from ib in DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                from ibt in DataContext.ItemBriefTypes.Where(ibt => ibt.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                where b.BookingId == bookingId && (companyId == 0 || i.CompanyId == companyId) //Since a Booking can have Items from multiple companies
                                && ibs.ToDate != null && i.IsActive
                                && (itemTypeId == 0 || ((companyId > 0 && i.ItemTypeId == itemTypeId) || ibt.ItemTypeId == itemTypeId))//Filter Booking by ItemBriefs Itemtype when it's in Porject related pages else by Items ItemType
                                && !(ibs.ItemBookingStatusCodeId == pinnedCodeID && ibs.IsActive == false && ibs.InventoryStatusCodeId == notPickedUpStatusCodeId)
                                && (!showMybookingsOnly || ibs.CreatedBy == userId)
                                //group ibs by i.CompanyId into grp // 1 is just an temporary value
                                group new { ibs, b } by i.CompanyId into grp
                                select new
                                {
                                    ItemBookings = grp,
                                    RelatedTableName = grp.Select(b => b.b.RelatedTable).FirstOrDefault(),
                                    MinFromDate = grp.Min(ibs => ibs.ibs.FromDate),
                                    MaxToDate = grp.Max(ibs => ibs.ibs.ToDate)
                                }).FirstOrDefault();

            if (itemBookings != null)
            {
                var results =
                            (from bkInfoAll in
                                 (from ibsGrp in itemBookings.ItemBookings
                                  join i in DataContext.Items on ibsGrp.ibs.ItemId equals i.ItemId
                                  from ib in DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ibsGrp.ibs.RelatedId && ibsGrp.b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                  from ibt in DataContext.ItemBriefTypes.Where(ibt => ibt.ItemBriefId == ibsGrp.ibs.RelatedId && ibsGrp.b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                  join ivh in DataContext.ItemVersionHistories on ibsGrp.ibs.ItemBookingId equals ivh.ItemBookingId into iv
                                  from ivhc in iv.DefaultIfEmpty()
                                  let user = ibsGrp.b.RelatedTable == "Project" ? (from u in DataContext.Users where u.UserId == ibsGrp.ibs.CreatedBy select u).FirstOrDefault() : null
                                  let status = DataContext.GetCurrentItemBookingStatus(ibsGrp.ibs.ItemBookingId).FirstOrDefault()
                                  // Item is considered  as Reject, If an ItemBrief Item does not have a version.
                                  let isReject = ibsGrp.b.RelatedTable == "Project" ? ibsGrp.ibs.IsActive == false && ivhc == null : ibsGrp.ibs.IsActive == false && ibsGrp.ibs.ItemBookingStatusCodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE") && ibsGrp.ibs.InventoryStatusCodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode", "NOTPICKEDUP")
                                  let isReleased = (ibsGrp.ibs.IsActive == false && ivhc != null)
                                  let itemType = companyId > 0 ? ibsGrp.ibs.Item.ItemTypeId.Value : ibt.ItemTypeId
                                  let isPickedUp = (ibsGrp.ibs.InventoryStatusCodeId == pickedUpStatusCodeID || ibsGrp.ibs.InventoryStatusCodeId == returnedStatusCodeID)
                                  let isReturned = ibsGrp.ibs.InventoryStatusCodeId == returnedStatusCodeID
                                  let hasBookingPeriodPassed = ibsGrp.ibs.ToDate < Utils.Today
                                  let canApprove = ibsGrp.b.RelatedTable == "NonProject" && ibsGrp.ibs.IsActive && status.CodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED") && !hasBookingPeriodPassed
                                  let canReject = ibsGrp.b.RelatedTable == "NonProject" && ibsGrp.ibs.ItemBookingStatusCodeId != StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE") && ibsGrp.ibs.ItemBookingStatusCodeId != StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "NOTAPPROVED")
                                  where ibsGrp.b.RelatedTable == "Project" || (ibsGrp.b.RelatedTable == "NonProject" && !isReject)
                                  select new BookingDetails
                                  {
                                      BookingId = ibsGrp.b.BookingId,
                                      ItemBookingId = ibsGrp.ibs.ItemBookingId,
                                      ItemId = i.ItemId,
                                      Location = DataContext.SPGetLocationPath(i.LocationId, i.CompanyId.Value).FirstOrDefault(),
                                      LocationId = i.LocationId,
                                      ItemBriefId = ibsGrp.ibs.RelatedId,//As long as relatedTable is "ItemBrief"
                                      CurrentStatus = isReject ? rejectedStatus.Description : ibsGrp.b.RelatedTable == "Project" && isReleased ?
                                                        releasedStatus.Description : ibsGrp.b.RelatedTable == "NonProject" && isReturned ? returnedStatus.Description : status.Description,
                                      ItemStatusOrder = status.SortOrder,
                                      CompanyId = i.CompanyId.Value,
                                      ItemName = i.Name,
                                      ItemBriefName = ib != null ? ib.Name : string.Empty,
                                      Quantity = ibsGrp.ibs.Quantity,
                                      BookedBy = user != null ? user.FirstName + " " + user.LastName : "",
                                      BookedByEmail = user != null ? user.Email1 : "",
                                      ItemBookingStatusCodeId = ibsGrp.ibs.ItemBookingStatusCodeId,
                                      InventoryStatusCodeId = ibsGrp.ibs.InventoryStatusCodeId,
                                      ItemTypeId = itemType,
                                      ItemTypeName = Utils.GetItemTypeById(itemType).Name,
                                      FromDate = ibsGrp.ibs.FromDate,
                                      ToDate = ibsGrp.ibs.ToDate.Value,
                                      IsReject = isReject,
                                      IsPickedUp = isPickedUp,
                                      IsPickedUpOrder = isPickedUp ? 0 : 1,
                                      IsReturned = isReturned,
                                      IsReturnedOrder = isReturned ? 0 : 1,
                                      IsActive = ibsGrp.ibs.IsActive,
                                      LastUpdatedDate = ibsGrp.ibs.LastUpdateDate,
                                      StatusSortOrder = ibsGrp.b.RelatedTable == "Project" ? status.CodeId == overdueCode.CodeId ? overdueCode.SortOrder : isReject ?
                                      rejectedStatus.SortOrder : isReleased ? releasedStatus.SortOrder : status.SortOrder
                                      : status.CodeId == overdueCode.CodeId ? 0 : status.CodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED") ? 1 : isReject ?
                                      rejectedStatus.SortOrder : status.SortOrder,
                                      CanApprove = canApprove,
                                      ApprovalFailureReason = ibsGrp.b.RelatedTable == "NonProject" && !canApprove ?
                                      status.CodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE") ?
                                        "This Booking has already been approved." : ibsGrp.ibs.ItemBookingStatusCodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "NOTAPPROVED") ?
                                        "This Booking has already been denied." : hasBookingPeriodPassed ?
                                        "This Booking has passed its booking period." : status.CodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNEDDELAYED") ?
                                        "This Item is overdue in another booking." : "This booking can’t be approved." : string.Empty,
                                      CanReject = canReject,
                                      RejectionFailureReason = ibsGrp.b.RelatedTable == "NonProject" && !canReject ?
                                      ibsGrp.ibs.ItemBookingStatusCodeId == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE") ?
                                      "This Booking has already been approved." : "This Booking has already been denied." : "",
                                      CanAccessItemDetails = i.Code.SortOrder >= GetUserInventoryVisibilityLevel(companyIdForQuery, userId, i.LocationId, false).SortOrder,
                                      ContactLocationManagerId = GetContactBookingManager(i.CompanyId.Value, i.LocationId).UserId
                                  })
                             group bkInfoAll by bkInfoAll.CompanyId into grp
                             select new CompanyBookingDetails
                             {
                                 BookingCount = grp.Count(g => g.CompanyId == grp.Key),
                                 MaxLastUpdatedDate = grp.Max(g => g.LastUpdatedDate),
                                 BookingDetailList = (from g in grp select g),
                             }).AsEnumerable<CompanyBookingDetails>();

                return results;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the non project booking.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <returns></returns>
        public NonProjectBooking GetNonProjectBooking(int bookingId)
        {
            return (from b in DataContext.Bookings
                    join npb in DataContext.NonProjectBookings on b.RelatedId equals npb.NonProjectBookingId
                    where b.BookingId == bookingId
                    select npb).FirstOrDefault();
        }

        /// <summary>
        /// Removes the in use item from item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool RemoveInUseItemFromItemBrief(int itemBriefId, int userId)
        {
            return DataContext.RemoveInUseItemFromItemBrief(itemBriefId, true, true, userId, true) > 0;
        }

        /// <summary>
        /// Approves the booking.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public BookingResponse ApproveBooking(int itemBookingId, int userId)
        {
            BookingResponse bookingResponse = new BookingResponse();

            ItemBooking itemBooking = GetItemBooking(itemBookingId);

            int currentItemBookingStatuscodeId = DataContext.GetCurrentItemBookingStatus(itemBooking.ItemBookingId).FirstOrDefault().CodeId;

            if (itemBooking != null && currentItemBookingStatuscodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED")
                && (itemBooking.ToDate >= Utils.Today))
            {
                //Check if the user can approve the Booking
                if (Utils.HasLocationManagerPermission(itemBooking.Item.CompanyId.Value, userId, itemBooking.Item.LocationId))
                {
                    itemBooking.ItemBookingStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
                    itemBooking.LastUpdateDate = Utils.Now;
                    itemBooking.LastUpdatedBy = userId;
                    DataContext.SaveChanges();
                    bookingResponse.Status = "OK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.None;
                }
                else
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                    bookingResponse.Message = "Can not Approve the Booking.";
                }
            }
            else
            {
                if (currentItemBookingStatuscodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNEDDELAYED"))
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.ItemBookingDelayed;
                    bookingResponse.Message = "This Item is overdue in another booking.";
                }
                else if (itemBooking.ToDate < Utils.Today)
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.ItemBookingPeriodPassed;
                    bookingResponse.Message = "This Item has passed its booking period so cannot be approved.";
                }
                else
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                    bookingResponse.Message = "This Booking has already been denied.";
                }
            }

            //Need to implement
            return bookingResponse;
        }

        /// <summary>
        /// Rejects the booking.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public BookingResponse RejectBooking(int itemBookingId, int userId)
        {
            BookingResponse bookingResponse = new BookingResponse();
            ItemBooking itemBooking = GetItemBooking(itemBookingId);
            int currentItemBookingStatuscodeId = DataContext.GetCurrentItemBookingStatus(itemBooking.ItemBookingId).FirstOrDefault().CodeId;
            if (itemBooking != null && currentItemBookingStatuscodeId != Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE"))
            {
                //Check if the user can approve the Booking
                if (Utils.HasLocationManagerPermission(itemBooking.Item.CompanyId.Value, userId, itemBooking.Item.LocationId))
                {
                    itemBooking.ItemBookingStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "NOTAPPROVED");
                    itemBooking.IsActive = false;
                    itemBooking.LastUpdateDate = Utils.Now;
                    itemBooking.LastUpdatedBy = userId;
                    DataContext.SaveChanges();
                    bookingResponse.Status = "OK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.None;
                }
                else
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                    bookingResponse.Message = "Can not Reject the Booking.";
                }
            }
            else
            {
                bookingResponse.Status = "NOTOK";
                bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                bookingResponse.Message = "This Booking has already been approved.";
            }
            return bookingResponse;
        }

        /// <summary>
        /// Gets the default message to display in complete item tab.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public string GetDefaultMessageToDisplayInCompleteItemTab(int itemId, int itemBriefId)
        {
            var itemBookings = GetCurrentItemBookingDetails(itemBriefId, itemId);

            ItemBooking itemBooking = GetItemBookingByItemID(itemId, itemBriefId, "ItemBrief");
            if (itemBooking != null)
            {
                int itemBriefItemStatus = DataContext.GetCurrentItemBookingStatus(itemBooking.ItemBookingId).FirstOrDefault().CodeId;

                if (itemBriefItemStatus == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "DELAYED"))
                {
                    Data.User locationManager = GetContactBookingManager(itemBooking.Item.CompanyId.Value, itemBooking.Item.LocationId);
                    string locationManagerName = string.Concat(locationManager.FirstName, " ", locationManager.LastName);
                    return "<b>Unfortunately, this Item has not been returned from its previous booking and is now overdue. " +
                            "Please contact the Booking Manager <a style='font-weight:bold;' href='mailto:" + locationManager.Email1 + "'>" + locationManagerName + "</a> for details.</b>";
                }
                else
                {
                    ItemBookingInfo currentBookingInfo = itemBookings.Where(ibs => ibs.ItemBooking.RelatedId == itemBriefId && ibs.ItemBooking.RelatedTable == "ItemBrief").FirstOrDefault();
                    if (currentBookingInfo != null)
                    {
                        ItemBooking currentItemBooking = currentBookingInfo.ItemBooking;
                        ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);
                        Data.Item currentItem = currentItemBooking.Item;
                        if (currentItem.IsHidden)
                        {
                            return "<b>You've reached the Item limit for your Inventory subscription. We'll keep a record of this Item, " +
                                "but you'll need to upgrade your subscription or reduce your current number of Items to be able to view it.</b>";
                        }
                        else if (itemBookings.Where(ibs => ibs.ItemBooking.RelatedId != itemBriefId && ibs.ItemBooking.RelatedTable == "ItemBrief" && !ibs.IsDelayed).Count() > 0)
                        {
                            ProjectBL projectBL = new ProjectBL(DataContext);

                            Data.Project project = itemBriefBL.GetItemBrief(currentItemBooking.RelatedId).Project;
                            Data.Booking booking = GetBooking(project.ProjectId, GlobalConstants.RelatedTables.Bookings.Project);
                            string popupText = string.Format(
                                "The following changes have been made to &#39;{0}&#39; during it&#39;s booking on &#39;{1}&#39;. Booking number &#39;{2}&#39;."
                                , currentItem.Name, project.ProjectName, GetCompanyBookingNumber(booking.BookingId, GetItem(itemId).CompanyId.Value).BookingNumber.ToString(CultureInfo.InvariantCulture));
                            return "<b>Some units from this Item are being used in another booking so you cannot edit the information." +
                                    " You can <a style='font-weight:bold;' href='javascript:;' onclick='ShowBookingOverlapContactIMPopup(\"" + popupText +
                                    "\");'>notify the Inventory Administrator</a> of any changes.</b>";
                        }
                        else if (IsItemGeneretedFromGivenItemBrief(itemId, itemBriefId))
                        {
                            return "<b>This is how this Item will appear in your Company Inventory when the Project is complete.</b>";
                        }
                        else
                        {
                            return "<b>This has been booked from the Inventory.</b> </br>This is how the Item will appear in your Company Inventory.";
                        }
                    }
                    else
                    {
                        if (itemBooking.ToDate < Utils.Now)
                        {
                            return "<b>This Item has passed its booking period so changes can no longer be made. Please contact the Inventory Administrator to make changes or extend the booking period.</b>";
                        }
                        else
                        {
                            return "<b>You will be able to edit this item during your booking period " + Utils.FormatDate(itemBooking.FromDate) + " to " + Utils.FormatDate(itemBooking.ToDate) + ".</b>";
                        }
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the item status information for given user.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public ItemStatusInformationForUser GetItemStatusInformationForUser(Item item, int companyId, int userId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            FinanceBL financeBL = new FinanceBL(DataContext);

            bool isCompanyAdmin = companyBL.IsCompanyAdministrator(companyId, userId);
            bool isInventoryStaff = false;
            if (item.LocationId.HasValue)
            {
                isInventoryStaff = Utils.IsCompanyInventoryStaffMember(companyId, userId, item.LocationId, DataContext);
            }
            else
            {
                isInventoryStaff = IsCompanyInventoryStaffMemberAnyLocation(companyId, userId);
            }

            bool isItemInUse = IsItemInUse(item.ItemId);

            bool hasPaymentSetuped = financeBL.HasPackageSelectedForFreeTrailEndedCompany(companyId);
            bool hasSuspended = companyBL.IsCompanySuspended(companyId);

            bool isReadOnly = !(isCompanyAdmin || isInventoryStaff)
                || (IsCompanyInSharedInventory(companyId) && item.CompanyId != companyId)
                || companyBL.HasCompanySuspendedbySBAdmin(companyId)
                || !hasPaymentSetuped || hasSuspended;

            ItemStatusInformationForUser itemStatusInformationForUser = new Data.DataTypes.ItemStatusInformationForUser();
            itemStatusInformationForUser.IsCompanyAdmin = isCompanyAdmin;
            itemStatusInformationForUser.IsInventoryManager = isInventoryStaff;
            itemStatusInformationForUser.IsItemInUse = isItemInUse;
            itemStatusInformationForUser.HasPaymentSetuped = hasPaymentSetuped;
            itemStatusInformationForUser.HasSuspended = hasSuspended;
            //Inorder to edit Quantity "isItemInUse" is irrespective
            itemStatusInformationForUser.CanEditQuantity = !isReadOnly;
            //Inorder to set isReadOnly "isItemInUse" needs to be considered
            isReadOnly = isReadOnly || isItemInUse;
            itemStatusInformationForUser.IsReadOnly = isReadOnly;

            return itemStatusInformationForUser;
        }

        /// <summary>
        /// Removes the item from item brief.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        public void RemoveItemFromItemBrief(int itemId, int itemBriefId)
        {
            ItemBooking itemBooking = GetItemBookingByItemID(itemId, itemBriefId, "ItemBrief");

            if (itemBooking != null)
            {
                itemBooking.IsActive = false;
                ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);

                Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemBriefId);
                if (itemBrief != null && itemBrief.ItemBriefStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED"))
                {
                    itemBrief.ItemBriefStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
                }

                DataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the item version history by item brief identifier.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public ItemVersionHistory GetItemVersionHistoryByItemBriefId(int itemBriefId)
        {
            return (from ivh in DataContext.ItemVersionHistories
                    where ivh.ItemBriefId == itemBriefId
                    select ivh).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item booking by item identifier.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <returns></returns>
        public ItemBooking GetItemBookingByItemID(int itemId, int relatedId, string relatedTable)
        {
            return DataContext.ItemBookings.Where(ibs => ibs.ItemId == itemId && ibs.RelatedId == relatedId && ibs.RelatedTable == relatedTable && ibs.IsActive).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item booking by item identifier.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="shouldConsiderActiveStatus">if set to <c>true</c> [should consider active status].</param>
        /// <returns></returns>
        public ItemBooking GetItemBookingByItemID(int itemId, bool shouldConsiderActiveStatus = false)
        {
            return DataContext.ItemBookings.Where(ibs => ibs.ItemId == itemId && (!shouldConsiderActiveStatus || ibs.IsActive == true)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item booking.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <returns></returns>
        public ItemBooking GetItemBooking(int itemBookingId)
        {
            return DataContext.ItemBookings.Where(ibs => ibs.ItemBookingId == itemBookingId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item booking by related table.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="shouldConsiderActiveStatus">if set to <c>true</c> [should consider active status].</param>
        /// <returns></returns>
        public ItemBooking GetItemBookingByRelatedTable(int relatedId, string relatedTable, bool shouldConsiderActiveStatus = false)
        {
            ItemBooking currentBookingInfo = DataContext.ItemBookings.Where(ibs => ibs.RelatedId == relatedId && ibs.RelatedTable == relatedTable && (!shouldConsiderActiveStatus || ibs.IsActive == true)).FirstOrDefault();
            return currentBookingInfo;
        }

        /// <summary>
        /// Gets all item booking by related table.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="shouldConsiderActiveStatus">if set to <c>true</c> [should consider active status].</param>
        /// <returns></returns>
        public List<ItemBooking> GetAllItemBookingByRelatedTable(int relatedId, string relatedTable, bool shouldConsiderActiveStatus = false)
        {
            return DataContext.ItemBookings.Where(ibs => ibs.RelatedId == relatedId && ibs.RelatedTable == relatedTable && (!shouldConsiderActiveStatus || ibs.IsActive == true)).ToList<ItemBooking>();
        }

        /// <summary>
        /// Determines whether given item is genereted from given item brief.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public bool IsItemGeneretedFromGivenItemBrief(int itemId, int itemBriefId)
        {
            var firstRecord = GetItemBookingByItemID(itemId);

            return (firstRecord != null && firstRecord.IsActive && firstRecord.ToDate == null && !firstRecord.Item.IsManuallyAdded);
        }

        /// <summary>
        /// Gets the item booking status.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <returns></returns>
        public Data.Code GetItemBookingStatus(int itemBookingId)
        {
            Data.Code status = null;
            if (itemBookingId > 0)
            {
                var statusObj = DataContext.GetCurrentItemBookingStatus(itemBookingId).FirstOrDefault();
                if (statusObj != null)
                {
                    status = Utils.GetCodeByCodeId(statusObj.CodeId);
                }
            }

            return status;
        }

        /// <summary>
        /// Gets the item status.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public string GetItemStatus(int itemId)
        {
            // TODO : This is just a temporary fix for mobile app. this need to be removed later. (this is hard coded for the moment)
            return GetAvailableItemQuantity(itemId, Utils.Today, Utils.Today) > 0 ? "Available" : "Not Available";
        }

        /// <summary>
        /// Gets the item brief item release date.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public DateTime GetItemBriefItemReleaseDate(int itemBriefId)
        {
            ItemVersionHistory itemVersionHistory = GetItemVersionHistoryByItemBriefId(itemBriefId);
            if (itemVersionHistory != null)
                return itemVersionHistory.CreatedDate.Value;
            else
                return Utils.Today;
        }

        #region InventoryProjectPanel

        /// <summary>
        /// Gets the booking list.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<UserBookings> GetBookingList(int companyId, int userId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            List<UserBookings> bookings = new List<UserBookings>();
            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            #region Add Project bookings

            List<UserBookings> projectBookings;

            if (companyBL.IsCompanyAdministrator(companyId, userId))
            {
                projectBookings = DataContext.Projects.Where(
                    p => p.CompanyId == companyId && p.IsActive == true && p.ProjectStatusCodeId != closedProjectStatusCodeId).OrderBy(
                    p => p.ProjectName).Select(p =>
                        new UserBookings
                        {
                            BookingCode = ProjectBookingPrefix + SqlFunctions.StringConvert((decimal)p.ProjectId).Trim(),
                            Name = p.ProjectName
                        }).ToList();
            }
            else
            {
                projectBookings = (from p in DataContext.Projects
                                   join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                                   where pu.UserId == userId && pu.IsActive && pu.Project.CompanyId == companyId && p.ProjectStatusCodeId != closedProjectStatusCodeId
                                   select new UserBookings
                                   {
                                       BookingCode = ProjectBookingPrefix + SqlFunctions.StringConvert((decimal)p.ProjectId).Trim(),
                                       Name = p.ProjectName
                                   }).OrderBy(p => p.Name).ToList();
            }

            bookings.AddRange(projectBookings);

            #endregion Add Project bookings

            #region Add Non-Project bookings

            List<UserBookings> nonProjectBookings;
            nonProjectBookings = (from npb in DataContext.NonProjectBookings.Where(npb => npb.CreatedBy == userId && npb.IsActive)
                                  from b in DataContext.Bookings.Where(b => b.RelatedTable == GlobalConstants.RelatedTables.Bookings.NonProject && npb.NonProjectBookingId == b.RelatedId).DefaultIfEmpty().Take(1)
                                  where (b == null || !b.IsArchived)
                                  select new UserBookings
                                  {
                                      BookingCode = NonProjectBookingPrefix + SqlFunctions.StringConvert((decimal)npb.NonProjectBookingId).Trim(),
                                      Name = npb.Name,
                                  }).ToList();

            bookings.AddRange(nonProjectBookings);

            #endregion Add Non-Project bookings

            return bookings;
        }

        /// <summary>
        /// Gets the item types.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public List<Data.ItemType> GetItemTypes(int projectId)
        {
            List<Data.ItemType> itemTypes = (from pit in DataContext.ProjectItemTypes
                                             join it in DataContext.ItemTypes on pit.ItemTypeId equals it.ItemTypeId
                                             where pit.ProjectId == projectId
                                             select it).OrderBy(it => it.Name).ToList<Data.ItemType>();

            return itemTypes;
        }

        /// <summary>
        /// Gets all system item types.
        /// </summary>
        /// <returns></returns>
        public List<ItemTypeData> GetAllSystemItemTypes()
        {
            return (from it in DataContext.ItemTypes
                    select new ItemTypeData
                    {
                        Id = it.ItemTypeId,
                        Name = it.Name
                    }).OrderBy(it => it.Name).ToList<ItemTypeData>();
        }

        /// <summary>
        /// Gets the item briefs.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public IEnumerable<Data.DataTypes.ItemBookingAllDetails> GetItemBriefs(int itemTypeId, int projectId)
        {
            var itemBriefItems = (from ib in DataContext.ItemBriefs
                                  join ibt in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibt.ItemBriefId
                                  where ibt.ItemTypeId == itemTypeId && ib.ProjectId == projectId
                                  select new ItemBookingAllDetails
                                  {
                                      ItemBrief = ib,
                                      ItemBookingList = (from ibs in DataContext.ItemBookings where ibs.RelatedId == ib.ItemBriefId && ibs.RelatedTable == "ItemBrief" && ibs.IsActive == true select ibs)
                                  });
            return itemBriefItems;
        }

        #endregion InventoryProjectPanel

        /// <summary>
        /// Gets the type of the user last visited item.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public UserLastVisitedtItemType GetUserLastVisitedItemType(int projectId, int userId)
        {
            return (from uli in DataContext.UserLastVisitedtItemTypes
                    where uli.UserId == userId && uli.ProjectId == projectId
                    select uli).FirstOrDefault();
        }

        /// <summary>
        /// Gets the type of the project item.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="selectedItemTypeId">The selected item type identifier.</param>
        /// <returns></returns>
        public ProjectItemType GetProjectItemType(int projectId, int selectedItemTypeId)
        {
            return DataContext.ProjectItemTypes.Where(pit => pit.ItemTypeId == selectedItemTypeId && pit.ProjectId == projectId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the project by item version history identifier.
        /// </summary>
        /// <param name="itemVersionHistoryId">The item version history identifier.</param>
        /// <returns></returns>
        public Data.Project GetProjectByItemVersionHistoryId(int itemVersionHistoryId)
        {
            return (from ivh in DataContext.ItemVersionHistories
                    join ib in DataContext.ItemBriefs on ivh.ItemBriefId equals ib.ItemBriefId
                    join p in DataContext.Projects on ib.ProjectId equals p.ProjectId
                    where ivh.ItemVersionHistoryId == itemVersionHistoryId
                    select p).FirstOrDefault();
        }

        /// <summary>
        /// Gets the in use or complete item booking.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public ItemBooking GetInUseOrCompleteItemBooking(int itemBriefId)
        {
            int inuseCompleteCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;
            int inuseCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;

            return DataContext.ItemBookings.Where(ibi => ibi.RelatedId == itemBriefId && ibi.RelatedTable == "ItemBrief" && ibi.IsActive == true &&
                (ibi.ItemBookingStatusCodeId == inuseCompleteCodeId || ibi.ItemBookingStatusCodeId == inuseCodeId)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the pinned item data.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public PinnedItemData GetPinnedItemData(int itemId)
        {
            PinnedItemData pinnedItemData = (from i in DataContext.Items
                                             join ibs in DataContext.ItemBookings on i.ItemId equals ibs.ItemId
                                             join ib in DataContext.ItemBriefs on ibs.RelatedId equals ib.ItemBriefId
                                             join u in DataContext.Users on ibs.CreatedBy equals u.UserId
                                             where ibs.IsActive == true && i.ItemId == itemId
                                             select new PinnedItemData
                                             {
                                                 PinnedToProjectName = ib.Project.ProjectName,
                                                 PinnedByUserName = u.FirstName + " " + u.LastName,
                                                 PinnedByUserEmail = u.Email1
                                             }).FirstOrDefault();
            return pinnedItemData;
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="shouldNotifyUsers">if set to <c>true</c> [should notify users].</param>
        public void DeleteItem(int itemId, int userId, bool shouldNotifyUsers = false)
        {
            Data.Item item = DataContext.Items.Where(i => i.ItemId == itemId).FirstOrDefault();
            if (item.IsActive)
            {
                var itembookings = GetFutureBookingsForItem(itemId);
                int resultCount = DataContext.DeleteItem(itemId, userId);
                if (shouldNotifyUsers && resultCount > 0)
                {
                    PersonalBL personalBL = new PersonalBL(DataContext);
                    ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);

                    User contactPerson = GetContactBookingManager(item.CompanyId.Value, item.LocationId);

                    foreach (ItemBooking ibs in itembookings)
                    {
                        if (ibs.CreatedBy > 0)
                        {
                            User toPerson = personalBL.GetUser(ibs.CreatedBy);
                            string toUserName = (toPerson.FirstName + " " + toPerson.LastName).Trim();
                            if (ibs.RelatedTable == "ItemBrief")
                            {
                                Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(ibs.RelatedId);
                                EmailSender.SendItemRemovalEmail(toPerson.Email1, toUserName, item.Name, itemBrief.Name, itemBrief.Project.ProjectName,
                                    item.Company.CompanyName, contactPerson.FirstName + " " + contactPerson.LastName,
                                    contactPerson.Email1, Utils.FormatDate(ibs.FromDate), ibs.ToDate.HasValue ? Utils.FormatDate(ibs.ToDate.Value) : null,
                                    Utils.GetSystemValue("FeedBackAndTechSupportURL"));
                            }
                            else
                            {
                                NonProjectBooking nonProjetBooking = GetNonProjectBooking(ibs.BookingId.Value);
                                EmailSender.SendItemRemovalEmailForNonProjectBooking(toPerson.Email1, toUserName, item.Name, nonProjetBooking.Name,
                                    item.Company.CompanyName, contactPerson.FirstName + " " + contactPerson.LastName,
                                    contactPerson.Email1, Utils.FormatDate(ibs.FromDate), ibs.ToDate.HasValue ? Utils.FormatDate(ibs.ToDate.Value) : null,
                                    Utils.GetSystemValue("FeedBackAndTechSupportURL"));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the inventory admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public User GetInventoryAdmin(int companyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);

            //Get the Inventory Admin
            User inventoryAdmin = companyBL.GetCompanyInventoryAdmin(companyId);
            if (inventoryAdmin == null)
            {
                //If no Inventory Admin, get the Company Primary Admin.
                inventoryAdmin = companyBL.GetCompanyPrimaryAdministrator(companyId);
            }

            return inventoryAdmin;
        }

        /// <summary>
        /// Determines whether [is item deleted] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool IsItemDeleted(int itemId)
        {
            Data.Item item = DataContext.Items.Where(i => i.ItemId == itemId).FirstOrDefault();
            return !item.IsActive;
        }

        /// <summary>
        /// Determines whether [is company inventory sharing removed] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="viewingCompanyId">The viewing company identifier.</param>
        /// <returns></returns>
        public bool IsCompanyInventorySharingRemoved(int companyId, int viewingCompanyId)
        {
            int activeSharingStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "ACTIVE");
            Data.CompanyInventorySharingDetail companyInventorySharingDetail = (from cisd in DataContext.CompanyInventorySharingDetails
                                                                                where cisd.ViewingCompanyId == viewingCompanyId && cisd.CompanyId == companyId
                                                                                && cisd.CompanySharingStatusCodeId == activeSharingStatusCodeId && cisd.IsActive
                                                                                select cisd).FirstOrDefault();
            return companyInventorySharingDetail == null;
        }

        /// <summary>
        /// Gets the delete item data.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public DeletedItemDatails GetDeleteItemData(int itemId)
        {
            DeletedItemDatails deleteItemdData = (from ia in DataContext.ItemArchives
                                                  join u in DataContext.Users on ia.ItemDeletedBy equals u.UserId
                                                  //join cu in DataContext.CompanyUsers on u.UserId equals cu.UserId
                                                  //join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                  where ia.ItemId == itemId
                                                  select new DeletedItemDatails
                                                  {
                                                      // ItemDeletedUserPermission  = (cur.CompanyUserTypeCodeId == companyAdminUserTypeCode.CodeId) ? companyAdminUserTypeCode.Description : inventoryManagerUserTypeCode.Description,
                                                      ItemDeletedUser = u.FirstName + " " + u.LastName,
                                                      ItemDeletedUserEmail = u.Email1,
                                                      ItemDeletedDate = ia.ItemDeletedDate
                                                  }).FirstOrDefault();

            return deleteItemdData;
        }

        /// <summary>
        /// Gets the inventory items.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyIdToView">The company identifier to view.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="findName">Name of the find.</param>
        /// <param name="findByItemTypeId">The find by item type identifier.</param>
        /// <param name="findFromDate">The find from date.</param>
        /// <param name="findToDate">The find to date.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="filterByVisibilityCodeId">The filter by visibility code identifier.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="isAutoComplete">if set to <c>true</c> [is automatic complete].</param>
        /// <param name="isManageInventory">if set to <c>true</c> [is manage inventory].</param>
        /// <param name="errorIds">The error ids.</param>
        /// <returns></returns>
        public List<InventoryItemData> GetInventoryItems(int userId, int companyIdToView, int companyId, string findName,
            int findByItemTypeId, DateTime? findFromDate, DateTime? findToDate, int? locationId, int? filterByVisibilityCodeId,
            int? pageSize, int? pageIndex, string sortOrder, out int totalCount, bool isAutoComplete = false, bool isManageInventory = false, Dictionary<int, string> errorIds = null)
        {
            List<int> companyIdList = GetSharedInventoryListForCompanyCanAccess(companyId).Select(c => c.CompanyId).ToList<int>();
            string companyids = string.Empty;
            if (companyIdToView == -1)
            {
                companyids = string.Join(",", companyIdList);
            }
            else
            {
                if (companyIdList.Contains(companyIdToView))
                {
                    companyids = companyIdToView.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    totalCount = 0;
                    return new List<InventoryItemData>();
                }
            }

            if (errorIds == null)
            {
                errorIds = new Dictionary<int, string>();
            }

            System.Data.Objects.ObjectParameter outParam = new System.Data.Objects.ObjectParameter("TotalCount", typeof(int?));

            var items = DataContext.GetInventorySearchItems(userId, companyId, companyids, findFromDate, findToDate,
                 locationId, findName, findByItemTypeId, filterByVisibilityCodeId, pageSize, pageIndex, sortOrder, isAutoComplete, isManageInventory, outParam).Select(ii => new InventoryItemData
                 {
                     Item = new Item
                     {
                         CompanyId = ii.CompanyId,
                         CreatedFor = ii.CreatedFor,
                         Description = ii.Description,
                         ItemId = ii.ItemId,
                         ItemTypeId = ii.ItemTypeId,
                         LocationId = ii.LocationId,
                         Name = ii.Name,
                         Quantity = ii.Quantity,                         
                     },
                     AvailableQty = ii.AvailableQuantity.Value,
                     CompanyName = ii.CompanyName,
                     ItemTypeName = ii.ItemTypeName,
                     LocationPath = ii.LocatinPath,   
                     IsWatchListItem = ii.WatchListItemId.HasValue,
                     ThumbnailMediaId = ii.DocumentMediaId.Value,
                     HasError = errorIds.Keys.Contains(ii.ItemId),
                     ErrorMessage = errorIds.Keys.Contains(ii.ItemId) ? errorIds[ii.ItemId] : string.Empty

                 }).ToList();

            totalCount = outParam.Value != System.DBNull.Value ? (int)outParam.Value : 0;
            return items;
        }

        /// <summary>
        /// Gets the watch list items.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="findFromDate">The find from date.</param>
        /// <param name="findToDate">The find to date.</param>
        /// <param name="shouldIncludeThumbImage">if set to <c>true</c> [should include thumb image].</param>
        /// <param name="watchListHeaderId">The watch list header identifier.</param>
        /// <param name="filterByCompany">if set to <c>true</c> [filter by company].</param>
        /// <returns></returns>
        public List<WatchListItemDetails> GetWatchListItems(int companyId, int userId, DateTime? findFromDate, DateTime? findToDate, bool shouldIncludeThumbImage = true, int watchListHeaderId = 0, bool filterByCompany = false)
        {
            List<int> companyIdList = GetSharedInventoryListForCompanyCanAccess(companyId).Select(c => c.CompanyId).ToList<int>();
            List<int> companyList = new List<int>();
            if (!filterByCompany)
            {
                foreach (int sharedCompanyId in companyIdList)
                {
                    companyList.Add(sharedCompanyId);
                }
            }
            else
            {
                if (companyIdList.Contains(companyId))
                {
                    companyList.Add(companyId);
                }
                else
                {
                    return new List<WatchListItemDetails>();
                }
            }

            if (watchListHeaderId == 0)
            {
                var watchListHeader = DataContext.WatchListHeaders.Where(wlh => wlh.CompanyId == companyId & wlh.UserId == userId).FirstOrDefault();
                if (watchListHeader != null)
                    watchListHeaderId = watchListHeader.WatchListHeaderId;
            }

            if (watchListHeaderId > 0)
            {
                bool hasDateFilter = findFromDate.HasValue && findToDate.HasValue;

                List<WatchListItemDetails> watchListItems = (from i in DataContext.Items
                                                             join c in DataContext.Codes on i.VisibilityLevelCodeId equals c.CodeId
                                                             join wli in DataContext.WatchListItems on i.ItemId equals wli.ItemId
                                                             join comp in DataContext.Companies on i.CompanyId equals comp.CompanyId
                                                             from mediaId in
                                                                 (from m in DataContext.DocumentMedias
                                                                  where shouldIncludeThumbImage && m.RelatedTableName == "Item" && m.RelatedId == i.ItemId && m.SortOrder == 1 && m.IsImageFile
                                                                  select m.DocumentMediaId).DefaultIfEmpty().Take(1)
                                                             from ibi in DataContext.ItemBookings.Where(ibs => ibs.IsActive && ibs.ItemId == i.ItemId).DefaultIfEmpty().Take(1)
                                                             where wli.WatchListHeaderId == watchListHeaderId
                                                                        && ((!filterByCompany && companyList.Contains(comp.CompanyId)) || (filterByCompany && comp.CompanyId == companyId))
                                                             orderby i.Name ascending
                                                             select new
                                                             {
                                                                 Name = i.Name,
                                                                 ThumbnailMediaId = mediaId,
                                                                 WatchListHeaderId = wli.WatchListHeader.WatchListHeaderId,
                                                                 Description = i.Description,
                                                                 CompanyName = comp.CompanyName,
                                                                 CompanyId = comp.CompanyId,
                                                                 Quantity = i.Quantity,
                                                                 AvailableQuantity = hasDateFilter ? DataContext.GetItemAvailableQuantity(i.ItemId, findFromDate.Value, findToDate.Value, 0) : 0,
                                                                 ItemId = i.ItemId,
                                                                 VisibilitySortOrder = c.SortOrder,
                                                                 LocationId = i.LocationId
                                                             }).AsEnumerable().Where(wlid =>
                                                                 wlid.VisibilitySortOrder >=
                                                                        GetUserInventoryVisibilityLevel(wlid.CompanyId, userId, wlid.LocationId, true).SortOrder).Select(wli =>
                                                                            new WatchListItemDetails
                                                                            {
                                                                                Name = wli.Name,
                                                                                ThumbnailMediaId = wli.ThumbnailMediaId,
                                                                                WatchListHeaderId = wli.WatchListHeaderId,
                                                                                Description = wli.Description,
                                                                                CompanyName = wli.CompanyName,
                                                                                CompanyId = wli.CompanyId,
                                                                                Quantity = wli.Quantity,
                                                                                AvailableQuantity = wli.AvailableQuantity,
                                                                                ItemId = wli.ItemId,
                                                                                VisibilitySortOrder = wli.VisibilitySortOrder,
                                                                                LocationId = wli.LocationId,
                                                                                ContactBookingManagerId = GetContactBookingManager(wli.CompanyId, wli.LocationId).UserId
                                                                            })
                                                                         .ToList<WatchListItemDetails>();

                if (hasDateFilter)
                {
                    return (from wli in watchListItems
                            where wli.AvailableQuantity > 0
                            select wli).ToList<WatchListItemDetails>();
                }

                return watchListItems;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the watch list item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteWatchListItem(int itemId, int companyId, int userId)
        {
            int watchListHeaderId = (from wli in DataContext.WatchListItems
                                     join wlh in DataContext.WatchListHeaders on wli.WatchListHeaderId equals wlh.WatchListHeaderId
                                     where wli.ItemId == itemId && wlh.CompanyId == companyId && wlh.UserId == userId
                                     select wli.WatchListHeaderId).FirstOrDefault();

            if (watchListHeaderId != 0)
            {
                DataContext.DeleteObject(DataContext.WatchListItems.First(wli => wli.ItemId == itemId && wli.WatchListHeaderId == watchListHeaderId));
                DataContext.SaveChanges();
                var watchListItems = DataContext.WatchListItems.Where(wli => wli.WatchListHeaderId == watchListHeaderId).FirstOrDefault();
                if (watchListItems == null)
                {
                    DataContext.DeleteObject(DataContext.WatchListHeaders.First(wlh => wlh.WatchListHeaderId == watchListHeaderId));
                    DataContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the inventory search items.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="findByItemTypeId">The find by item type identifier.</param>
        /// <param name="isSharedInventory">if set to <c>true</c> [is shared inventory].</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public Data.Item[] GetInventorySearchItems(int userId, int companyId, string keyword, int findByItemTypeId, bool isSharedInventory,
            DateTime? fromDate, DateTime? toDate, int? locationId, int? filterByVisibilityCodeId)
        {
            List<int> companyIdList = GetSharedInventoryListForCompanyCanAccess(companyId).Select(c => c.CompanyId).ToList<int>();
            bool hasDateFilter = fromDate.HasValue && toDate.HasValue;

            LocationBL locationBL = new LocationBL(DataContext);
            int?[] locationIDList = locationBL.GetAllSubLocations(locationId, companyId, userId);

            int count = 0;
            List<InventoryItemData> items = GetInventoryItems(userId, companyId, companyId,
                            keyword, findByItemTypeId, fromDate, toDate, locationId, filterByVisibilityCodeId,
                            10, 0, string.Empty, out count, isAutoComplete: true);
            Data.Item[] result;
            if (hasDateFilter)
            {
                result = (from i in items
                          where i.AvailableQty > 0
                          select i.Item).Distinct().Take(10).ToArray();
            }
            else
            {
                result = (from i in items
                          select i.Item).Distinct().Take(10).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Gets the shared inventory list for company can access.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<SharedInventoryCompaniesData> GetSharedInventoryListForCompanyCanAccess(int companyId)
        {
            int activeCompanyInventorySharingStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "ACTIVE");
            CompanyBL companyBL = new CompanyBL(DataContext);
            List<int> suspendedCompanyStatusList = companyBL.GetSuspendedCompanyStatusList();
            List<SharedInventoryCompaniesData> sppCompanies = (from cis in DataContext.CompanyInventorySharingDetails
                                                               join c in DataContext.Companies on cis.CompanyId equals c.CompanyId
                                                               where c.HasSuspended == false && !suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId)
                                                               && cis.CompanySharingStatusCodeId == activeCompanyInventorySharingStatusCodeId
                                                               && cis.ViewingCompanyId == companyId && cis.IsActive
                                                               select new SharedInventoryCompaniesData
                                                               {
                                                                   CompanyName = c.CompanyName,
                                                                   CompanyId = cis.CompanyId
                                                               }).ToList<SharedInventoryCompaniesData>();
            Data.Company selectedCompany = companyBL.GetCompany(companyId);
            sppCompanies.Add(new SharedInventoryCompaniesData() { CompanyId = companyId, CompanyName = selectedCompany.CompanyName });
            return sppCompanies;
        }

        /// <summary>
        /// Gets the booking tab data.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public List<ItemBookingData> GetBookingTabData(int itemId)
        {
            int inUseCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inUseCompleteCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");
            Code overdueCodeID = Utils.GetCodeByValue("ItemBookingStatusCode", "OVERDUE");

            List<ItemBookingData> bookings =
                           (from ibs in DataContext.ItemBookings
                            join c in DataContext.Codes on ibs.ItemBookingStatusCodeId equals c.CodeId
                            from ib in DataContext.ItemBriefs.Where(ib => ibs.RelatedTable == "ItemBrief" && ibs.RelatedId == ib.ItemBriefId).DefaultIfEmpty().Take(1)
                            from npb in DataContext.NonProjectBookings.Where(npb => ibs.RelatedTable == GlobalConstants.RelatedTables.Bookings.NonProject && ibs.RelatedId == npb.NonProjectBookingId).DefaultIfEmpty().Take(1)
                            from p in DataContext.Projects.Where(p => ib.ProjectId == p.ProjectId).DefaultIfEmpty().Take(1)
                            join u in DataContext.Users on ibs.CreatedBy equals u.UserId
                            where ibs.IsActive == true && ibs.ItemId == itemId
                                && ((!ibs.ToDate.HasValue && ibs.FromDate <= Utils.Today) || ibs.ToDate.Value >= Utils.Today || ibs.ItemBookingStatusCodeId == inUseCodeID || ibs.ItemBookingStatusCodeId == inUseCompleteCodeID)
                            let Status = DataContext.Codes.Where(cc => cc.CodeId == DataContext.GetItemBookingStatus(ibs.ItemBookingId)).FirstOrDefault()
                            select new ItemBookingData
                            {
                                Status = Status.Description,
                                StatusSortOrder = Status.SortOrder,
                                BookingName = p != null ? p.ProjectName : npb != null ? npb.Name : string.Empty,
                                ItemBrief = ib != null ? ib.Name : string.Empty,
                                BookedBy = string.Concat(u.FirstName, " ", u.LastName),
                                BookedByEmail = u.Email1,
                                FromDate = ibs.FromDate,
                                ToDate = ibs.ToDate,
                                BookedQuantity = ibs.Quantity
                            }).ToList<ItemBookingData>();

            return bookings;
        }

        /// <summary>
        /// Checks the permissions for item details page.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Data.Item CheckPermissionsForItemDetailsPage(int userId, int itemId, int companyId, out ErrorCodes errorCode, bool throwException = true)
        {
            errorCode = ErrorCodes.None;

            if (GetItem(itemId) == null)
            {
                errorCode = ErrorCodes.NoEditPermissionForInventory;
                return null;
            }

            Data.Item item = null;
            if (IsItemDeleted(itemId))
            {
                if (throwException)
                {
                    StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ItemDelete, itemId, "CompanyId=" + companyId.ToString()));
                }

                errorCode = ErrorCodes.ItemDeleted;
                return null;
            }

            List<int> companyList = new List<int>();
            List<int> tempCompanyList = GetSharedInventoryListForCompanyCanAccess(companyId).Select(c => c.CompanyId).ToList<int>();
            if (tempCompanyList.Count() > 1) //If Company in Shared Inventory.
            {
                companyList = tempCompanyList;
            }

            if (!companyList.Contains(companyId))
            {
                companyList.Add(companyId);
            }

            item = (from i in DataContext.Items
                    from cu in DataContext.CompanyUsers.Where(cu => companyList.Contains(cu.CompanyId) && cu.IsActive).DefaultIfEmpty()
                    from cur in DataContext.CompanyUserRoles.Where(cur => cur.CompanyUserId == cu.CompanyUserId && cur.IsActive).DefaultIfEmpty()
                    where (cu.UserId == userId) && i.ItemId == itemId && i.IsActive && !i.IsHidden && (cur != null)
                        && i.CompanyId.HasValue && companyList.Contains(i.CompanyId.Value)
                    select i).FirstOrDefault();

            if (item == null)
            {
                int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

                item = (from i in DataContext.Items
                        from p in DataContext.Projects.Where(p => companyList.Contains(p.CompanyId) && p.ProjectStatusCodeId != closedProjectStatusCodeId)
                        join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                        where (pu.UserId == userId) && i.ItemId == itemId && i.IsActive && !i.IsHidden
                            && i.CompanyId.HasValue && companyList.Contains(i.CompanyId.Value)
                        select i).FirstOrDefault();
            }

            #region Validation

            if (item == null)
            {
                item = (from i in DataContext.Items
                        from p in DataContext.Projects.Where(p => companyList.Contains(p.CompanyId))
                        join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                        where (pu.UserId == userId) && i.ItemId == itemId && i.IsActive
                            && i.CompanyId.HasValue && companyList.Contains(i.CompanyId.Value)
                        select i).FirstOrDefault();

                if (item != null)
                {
                    item = null;
                    errorCode = ErrorCodes.NoEditPermissionForInventory;

                    if (throwException)
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, 0, "This Project Is Closed."));
                    }
                }
            }
            else
            {
                Code userVisibilityLevel = GetUserInventoryVisibilityLevel(item.CompanyId.Value, userId, item.LocationId, true);


                bool hasBooking = (from ibooking in DataContext.ItemBookings.Where(ib => ib.ItemId == itemId && ib.IsActive)
                                   join ib in DataContext.ItemBriefs on ibooking.RelatedId equals ib.ItemBriefId
                                   join pu in DataContext.ProjectUsers.Where(pu => pu.UserId == userId) on ib.ProjectId equals pu.ProjectId
                                   where pu.IsActive && pu.UserId == userId && ibooking.RelatedTable == GlobalConstants.RelatedTables.Bookings.ItemBrief
                                   select ibooking).Count() > 0;


                if (!hasBooking && item.Code.SortOrder < userVisibilityLevel.SortOrder)
                {
                    if (throwException)
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ItemNotVisibile, item.ItemId, "You don’t have permission to view the details for this Item."));
                    }

                    errorCode = ErrorCodes.ItemNotVisible;
                    item = null;
                }
            }

            #endregion Validation

            return item;
        }

        /// <summary>
        /// Checks the permissions for item details page.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns></returns>
        public Data.Item CheckPermissionsForItemDetailsPage(int userId, int itemId, int companyId, bool throwException = true)
        {
            ErrorCodes errorCode = ErrorCodes.NoEditPermissionForInventory;
            return CheckPermissionsForItemDetailsPage(userId, itemId, companyId, out errorCode, throwException);
        }

        /// <summary>
        /// Determines whether [is company in shared inventory] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool IsCompanyInSharedInventory(int companyId)
        {
            var companyList = GetSharedInventoryListForCompanyCanAccess(companyId);
            return companyList.Count > 1;
        }

        /// <summary>
        /// Gets the inventory sharing details with selected company.
        /// </summary>
        /// <param name="companyIds">The company ids.</param>
        /// <returns></returns>
        public List<CompanyInventorySharingDetail> GetInventorySharingDetailsWithSelectedCompany(List<int> companyIds)
        {
            var companyInventoryDetails = (from cis in DataContext.CompanyInventorySharingDetails
                                           where companyIds.Contains(cis.CompanyId) && companyIds.Contains(cis.ViewingCompanyId)
                                           && cis.IsActive == true
                                           select cis).ToList();
            return companyInventoryDetails;
        }

        /// <summary>
        /// Determines whether [is item in use] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool IsItemInUse(int itemId)
        {
            int inUseCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inUseCompleteCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");

            Data.Item item = (from i in DataContext.Items
                              join ibs in DataContext.ItemBookings on i.ItemId equals ibs.ItemId
                              where ibs.IsActive == true && i.ItemId == itemId &&
                              (ibs.FromDate <= Utils.Today && (!ibs.ToDate.HasValue || ibs.ToDate >= Utils.Today)
                              || (ibs.ToDate < Utils.Today && (ibs.ItemBookingStatusCodeId == inUseCodeID || ibs.ItemBookingStatusCodeId == inUseCompleteCodeID)))
                              select i).FirstOrDefault();

            if (item != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is item over due] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool IsItemOverDue(int itemId)
        {
            return DataContext.IsOverdueItem(itemId).FirstOrDefault().Value;
        }

        /// <summary>
        /// Determines whether [has future bookings for item] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool HasFutureBookingsForItem(int itemId)
        {
            return GetFutureBookingsForItem(itemId).Count() > 0;
        }

        /// <summary>
        /// Gets the future bookings for item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        private List<ItemBooking> GetFutureBookingsForItem(int itemId)
        {
            return DataContext.ItemBookings.Where(ibs => ibs.ItemId == itemId && ibs.FromDate > Utils.Today && ibs.IsActive == true).ToList();
        }

        /// <summary>
        /// Determines whether [is item hidden] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public bool IsItemHidden(int itemId)
        {
            Data.Item item = DataContext.Items.Where(i => i.ItemId == itemId).FirstOrDefault();
            return item != null && item.IsHidden;
        }

        /// <summary>
        /// Determines whether this instance [can delete media] the specified related table.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public bool CanDeleteMedia(string relatedTable, int relatedId)
        {
            switch (relatedTable)
            {
                case "Item":
                    if (IsItemDeleted(relatedId))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
            return true;
        }

        /// <summary>
        /// Shares the inventory.
        /// </summary>
        /// <param name="viewingcompanyId">The viewingcompany identifier.</param>
        /// <param name="companyIdToView">The company identifier to view.</param>
        /// <param name="isBoth">if set to <c>true</c> [is both].</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="systemURL">The system URL.</param>
        /// <param name="requestedCompanyId">The requested company identifier.</param>
        /// <returns></returns>
        public string ShareInventory(int viewingcompanyId, int companyIdToView, bool isBoth, int userId, string systemURL, int requestedCompanyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            int activeStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "ACTIVE");
            int pendingStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyInventorySharingStatus", "PENDING");
            //Check if both companies are not being Suspended
            if (companyBL.IsCompanySuspended(viewingcompanyId, true) || companyBL.IsCompanySuspended(companyIdToView, true))
                return "Company has been suspended";

            int toCompanyId = viewingcompanyId;

            if (requestedCompanyId != companyIdToView)
                toCompanyId = companyIdToView;

            // make sure that, If the Company Has already been shared based on the Radio option
            List<int> companyIds = new List<int> { viewingcompanyId, companyIdToView };
            var companyInventoryDetails = GetInventorySharingDetailsWithSelectedCompany(companyIds);

            int optionCount = companyInventoryDetails.Count;

            switch (optionCount)
            {
                case 1:
                    //Need to decide to which direction
                    CompanyInventorySharingDetail companyInventorySharingDetail = companyInventoryDetails.FirstOrDefault();
                    //Check for any duplicates
                    //We do not check for Both Option in this Scenareo, because we let it hapen because there is no harm in it.
                    if (!isBoth && companyInventorySharingDetail.CompanyId == companyIdToView && companyInventorySharingDetail.ViewingCompanyId == viewingcompanyId)
                    {
                        if (companyInventorySharingDetail.CompanySharingStatusCodeId == activeStatusCodeId)
                            return "Already shared with the Company";
                        else
                            return "Already has a pending request";
                    }

                    break;

                case 2:
                    return "Already shared with the Company";
            }
            PersonalBL personalBL = new Personal.PersonalBL(DataContext);
            Data.User fromUser = personalBL.GetUser(userId);
            Data.Company company = companyBL.GetCompany(requestedCompanyId);

            string companyAddress = string.Empty;
            if (company.City == null)
                companyAddress = company.Country.CountryName;
            else
                companyAddress = company.City + ", " + company.Country.CountryName;

            string feedbackAndTechSupport = Utils.GetSystemValue("FeedBackAndTechSupportURL");

            //Inventory Sharing Page URL, with a parameter to open the Admin tab.
            string inventorySharingUrl = string.Format("{0}/Inventory/InventorySharing.aspx?CompanyId={1}&TabId=1#{2}", systemURL, toCompanyId, requestedCompanyId);
            bool shouldIncludeSharingURL = isBoth;
            int emailTempleTypeCodeId = 0;
            // Now we can share.
            if (isBoth)
            {
                CreateCompanyInventorySharingDetail(viewingcompanyId, companyIdToView, userId, (requestedCompanyId == viewingcompanyId ? pendingStatusCodeId : activeStatusCodeId));
                CreateCompanyInventorySharingDetail(companyIdToView, viewingcompanyId, userId, (requestedCompanyId == viewingcompanyId ? activeStatusCodeId : pendingStatusCodeId));
                emailTempleTypeCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "REQUESTTOSHAREANDVIEWINVENTORY");
            }
            else
            {
                CreateCompanyInventorySharingDetail(viewingcompanyId, companyIdToView, userId, (requestedCompanyId == viewingcompanyId ? pendingStatusCodeId : activeStatusCodeId));
                //Send the Email
                if (requestedCompanyId == viewingcompanyId)
                {
                    emailTempleTypeCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "REQUESTTOVIEWINVENTORY");
                    shouldIncludeSharingURL = true;
                }
                else
                {
                    emailTempleTypeCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "REQUESTTOSHAREINVENTORY");
                }
            }

            User inventoryAdmin = GetInventoryAdmin(toCompanyId);
            EmailSender.RequestToShareInventory(inventoryAdmin.Email1, inventoryAdmin.FirstName, fromUser.FirstName + " " + fromUser.LastName, fromUser.Email1,
                    company.CompanyName, companyAddress, emailTempleTypeCodeId, feedbackAndTechSupport, shouldIncludeSharingURL ? inventorySharingUrl : null);

            DataContext.SaveChanges();
            return string.Empty;
        }

        /// <summary>
        /// Creates the company inventory sharing detail.
        /// </summary>
        /// <param name="viewingcompanyId">The viewingcompany identifier.</param>
        /// <param name="companyIdToView">The company identifier to view.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="statusCodeId">The status code identifier.</param>
        private void CreateCompanyInventorySharingDetail(int viewingcompanyId, int companyIdToView, int userId, int statusCodeId)
        {
            CompanyInventorySharingDetail companyInventorySharingDetail = GetCompanyInventorySharingDetail(viewingcompanyId, companyIdToView);
            if (companyInventorySharingDetail == null)
            {
                companyInventorySharingDetail = new CompanyInventorySharingDetail();
                companyInventorySharingDetail.CompanyId = companyIdToView;
                companyInventorySharingDetail.ViewingCompanyId = viewingcompanyId;
                companyInventorySharingDetail.CompanySharingStatusCodeId = statusCodeId;
                companyInventorySharingDetail.CreatedByUserId = companyInventorySharingDetail.LastUpdatedByUserId = userId;
                companyInventorySharingDetail.CreatedDate = companyInventorySharingDetail.LastUpdatedDate = Utils.Today;
                companyInventorySharingDetail.IsActive = true;
                DataContext.CompanyInventorySharingDetails.AddObject(companyInventorySharingDetail);
            }
        }

        /// <summary>
        /// Gets the companies by name and location.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public List<CompanySearchData> GetCompaniesByNameAndLocation(int companyId, string companyName, string location)
        {
            CompanyBL companyBL = new CompanyBL(this.DataContext);
            List<int> suspendedCompanyStatusList = companyBL.GetSuspendedCompanyStatusList();

            return (from c in DataContext.Companies
                    from mediaId in
                        (from m in DataContext.DocumentMedias
                         where m.RelatedTableName == "Company" && m.RelatedId == c.CompanyId && m.SortOrder == 1 && m.IsImageFile
                         select m.DocumentMediaId).DefaultIfEmpty().Take(1)
                    where (companyName == string.Empty || c.CompanyName.ToLower().Contains(companyName)) &&
                          (location == string.Empty || c.City.ToLower().Contains(location) || c.Country.CountryName.ToLower().Contains(location)) &&
                          c.IsActive && !c.HasSuspended && c.CompanyId != companyId && !suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId)
                          && c.IsCompanyVisibleForSearchInInventory
                    orderby c.CompanyName ascending
                    select new CompanySearchData
                    {
                        Company = c,
                        AccessToMyCompanyStatusCodeId = DataContext.CompanyInventorySharingDetails.Where(cisd => cisd.ViewingCompanyId == c.CompanyId && cisd.CompanyId == companyId && cisd.IsActive).Select(csid => csid.CompanySharingStatusCodeId).DefaultIfEmpty(0).FirstOrDefault(),
                        AccessToSelectedCompanyStatusCodeId = DataContext.CompanyInventorySharingDetails.Where(cisd => cisd.ViewingCompanyId == companyId && cisd.CompanyId == c.CompanyId && cisd.IsActive).Select(csid => csid.CompanySharingStatusCodeId).DefaultIfEmpty(0).FirstOrDefault(),
                        ThumbnailMediaId = mediaId,
                    }).ToList();
        }

        /// <summary>
        /// Gets the company search names for inventory share.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        public Data.Company[] GetCompanySearchNamesForInventoryShare(int companyId, string keyword)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            List<int> suspendedCompanyStatusList = companyBL.GetSuspendedCompanyStatusList();

            return (from c in DataContext.Companies
                    where c.CompanyId != companyId && c.CompanyName.Contains(keyword)
                    && c.IsActive && !c.HasSuspended && !suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId)
                    && c.IsCompanyVisibleForSearchInInventory
                    select c).Take(10).ToArray();
        }

        /// <summary>
        /// Gets the shared company details.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public List<CompanySearchData> GetSharedCompanyDetails(int companyId)
        {
            return (from c in DataContext.Companies
                    from cisd in
                        (from cisd in DataContext.CompanyInventorySharingDetails
                         where (cisd.CompanyId == companyId || cisd.ViewingCompanyId == companyId) && cisd.IsActive
                         select cisd)
                    where (c.IsActive && c.CompanyId != companyId && (c.CompanyId == cisd.ViewingCompanyId || c.CompanyId == cisd.CompanyId))
                    orderby c.CompanyName ascending
                    select new CompanySearchData
                    {
                        Company = c,
                        AccessToMyCompanyStatusCodeId = DataContext.CompanyInventorySharingDetails.Where(cisd1 => cisd1.ViewingCompanyId == c.CompanyId && cisd1.CompanyId == companyId && cisd1.IsActive).Select(csid1 => csid1.CompanySharingStatusCodeId).DefaultIfEmpty(0).FirstOrDefault(),
                        AccessToSelectedCompanyStatusCodeId = DataContext.CompanyInventorySharingDetails.Where(cisd2 => cisd2.ViewingCompanyId == companyId && cisd2.CompanyId == c.CompanyId && cisd2.IsActive).Select(csid2 => csid2.CompanySharingStatusCodeId).DefaultIfEmpty(0).FirstOrDefault()
                    }).Distinct().ToList();
        }

        /// <summary>
        /// Gets the company inventory sharing detail.
        /// </summary>
        /// <param name="viewingcompanyId">The viewingcompany id.</param>
        /// <param name="companyIdToView">The company id to view.</param>
        /// <returns></returns>
        public CompanyInventorySharingDetail GetCompanyInventorySharingDetail(int viewingcompanyId, int companyIdToView)
        {
            return DataContext.CompanyInventorySharingDetails.Where(
                cis => cis.ViewingCompanyId == viewingcompanyId && cis.CompanyId == companyIdToView && cis.IsActive).FirstOrDefault();
        }

        /// <summary>
        /// Gets the watch list item removed company list.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public string GetWatchListItemRemovedCompanyList(int companyId, int userId)
        {
            List<Data.Company> companyList = (from rwlin in DataContext.RemovedWatchListItemsNotifications
                                              join c in DataContext.Companies on rwlin.RemovedCompany equals c.CompanyId
                                              where rwlin.CompanyToNotify == companyId && rwlin.UserToNotify == userId && rwlin.IsActive
                                              select c).Distinct().ToList();

            string companyNames = string.Empty;
            if (companyList.Count() > 0)
            {
                if (companyList.Count() == 1)
                {
                    companyNames = companyList.First().CompanyName + " is";
                }
                else
                {
                    foreach (Data.Company company in companyList)
                    {
                        companyNames = companyNames + company.CompanyName + "<br/>";
                    }
                    companyNames = companyNames + "are";
                }
            }
            return companyNames;
        }

        /// <summary>
        /// Sets the response to watch list items notifications.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void SetResponseToWatchListItemsNotifications(int companyId, int userId)
        {
            List<Data.RemovedWatchListItemsNotification> watchListItemNotifications = (from rwlin in DataContext.RemovedWatchListItemsNotifications
                                                                                       where rwlin.CompanyToNotify == companyId && rwlin.UserToNotify == userId
                                                                                       && rwlin.IsActive
                                                                                       select rwlin).ToList();
            foreach (RemovedWatchListItemsNotification watchListItemNotification in watchListItemNotifications)
            {
                watchListItemNotification.IsActive = false;
            }
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Determines whether [is item already shared to booking] [the specified item identifier].
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <returns></returns>
        public bool IsItemAlreadySharedToBooking(int itemId, int relatedId, string relatedTable)
        {
            return GetItemBookingByItemID(itemId, relatedId, relatedTable) != null;
        }

        /// <summary>
        /// Determines whether [has default image set] [the specified related identifier].
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <returns></returns>
        public bool HasDefaultImageSet(int relatedId, string relatedTable)
        {
            DocumentMedia defaultImage = DataContext.DocumentMedias.Where(dm => dm.RelatedId == relatedId && dm.RelatedTableName == relatedTable && dm.IsImageFile && dm.SortOrder == 1).FirstOrDefault();
            return (relatedId > 0 && defaultImage != null);
        }

        /// <summary>
        /// Gets the country identifier by item identifier.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public int GetCountryIdByItemId(int itemId)
        {
            return (from i in DataContext.Items
                    join c in DataContext.Companies on i.CompanyId equals c.CompanyId
                    where i.ItemId == itemId
                    select c.CountryId.Value).FirstOrDefault();
        }

        /// <summary>
        /// Picks up item.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <param name="isPickUp">if set to <c>true</c> [is pick up].</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public BookingResponse PickUpItem(int itemBookingId, bool isPickUp, int userId)
        {
            ItemBooking itemBooking = GetItemBooking(itemBookingId);

            BookingResponse bookingResponse = new BookingResponse();
            if (itemBooking != null)
            {
                if (!(Utils.IsCompanyInventoryAdmin(itemBooking.Item.CompanyId.Value, userId) ||
                    Utils.IsCompanyInventoryStaffMember(itemBooking.Item.CompanyId.Value, userId, itemBooking.Item.LocationId, DataContext)))
                {
                    bookingResponse.Status = "NOTOK";
                    bookingResponse.ErrorCode = (int)Common.ErrorCodes.NoEditPermissionForInventory;
                    bookingResponse.Message = "Could not pick up the Item.";
                    return bookingResponse;
                }

                //Concurrency scenareo - Non-Project Bookings can not perform Re-Pickups after it's being Returned
                if (GetBooking(itemBooking.BookingId.Value).RelatedTable == "NonProject")
                {
                    if (itemBooking.InventoryStatusCodeId == Utils.GetCodeIdByCodeValue("InventoryStatusCode", "RETURNED") || !itemBooking.IsActive)
                    {
                        bookingResponse.Status = "NOTOK";
                        bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                        bookingResponse.Message = "Could not pick up the Item.";
                        return bookingResponse;
                    }
                }

                if (isPickUp)
                {
                    itemBooking.InventoryStatusCodeId = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "PICKEDUP");
                }
                else
                {
                    itemBooking.InventoryStatusCodeId = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "NOTPICKEDUP");
                }
                itemBooking.LastUpdatedBy = userId;
                itemBooking.LastUpdateDate = Utils.Now;

                DataContext.SaveChanges();
                bookingResponse.Status = "OK";
            }
            else
            {
                bookingResponse.Status = "NOTOK";
                bookingResponse.ErrorCode = (int)Common.ErrorCodes.ActionNotPerformed;
                bookingResponse.Message = "Could not pick up the Item.";
            }
            return bookingResponse;
        }

        /// <summary>
        /// Returns the item.
        /// </summary>
        /// <param name="itemBookingId">The item booking identifier.</param>
        /// <param name="isReturn">if set to <c>true</c> [is return].</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public BookingResponse ReturnItem(int itemBookingId, bool isReturn, int userId)
        {
            ItemBooking itemBooking = GetItemBooking(itemBookingId);

            BookingResponse bookingResponse = new BookingResponse();

            if (itemBooking != null &&
                (!(Utils.IsCompanyInventoryAdmin(itemBooking.Item.CompanyId.Value, userId) ||
                    Utils.IsCompanyInventoryStaffMember(itemBooking.Item.CompanyId.Value, userId, itemBooking.Item.LocationId, DataContext))))
            {
                bookingResponse.Status = "NOTOK";
                bookingResponse.ErrorCode = (int)Common.ErrorCodes.NoEditPermissionForInventory;
                bookingResponse.Message = "Could not return the Item.";
                return bookingResponse;
            }

            //To Return or Undo Return the Item needs to be Confirmed and Picked up.
            if (itemBooking != null && (itemBooking.ItemBookingStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE")
                || itemBooking.ItemBookingStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE")) &&
                itemBooking.InventoryStatusCodeId != Utils.GetCodeIdByCodeValue("InventoryStatusCode", "NOTPICKEDUP"))
            {
                if (isReturn)
                {
                    if (GetBooking(itemBooking.BookingId.Value).RelatedTable == "NonProject")
                    {
                        itemBooking.IsActive = false;
                    }

                    itemBooking.InventoryStatusCodeId = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "RETURNED");
                }
                else
                {
                    if (itemBooking.RelatedTable != "NonProject")
                        itemBooking.InventoryStatusCodeId = Utils.GetCodeIdByCodeValue("InventoryStatusCode", "PICKEDUP");
                }
                itemBooking.LastUpdatedBy = userId;
                itemBooking.LastUpdateDate = Utils.Now;
                //Add to log table

                DataContext.SaveChanges();
                bookingResponse.Status = "OK";
            }
            else
            {
                bookingResponse.Status = "NOTOK";
            }
            return bookingResponse;
        }

        /// <summary>
        /// Gets the available item quantity.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public int GetAvailableItemQuantity(int itemId, DateTime? fromDate, DateTime? toDate)
        {
            return DataContext.GetAvailableItemQuantity(itemId, fromDate.HasValue ? fromDate.Value : Utils.Today, toDate.HasValue ? toDate.Value : Utils.Today, 0).FirstOrDefault().Value;
        }

        /// <summary>
        /// Gets the maximum duration of the booked quantity for all.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public int GetMaxBookedQuantityForAllDuration(int itemId)
        {
            var allActiveBookings = GetBookingTabData(itemId);
            if (allActiveBookings.Count > 0)
            {
                var bookingsHasToDates = allActiveBookings.Where(ibi => ibi.ToDate.HasValue);
                if (bookingsHasToDates.Count() > 0)
                {
                    DateTime minFromDate = allActiveBookings.Min(ibi => ibi.FromDate).Value;
                    DateTime maxToDate = bookingsHasToDates.Max(ibi => ibi.ToDate).Value;
                    return DataContext.GetMaxBookedQuantityForItem(itemId, minFromDate, maxToDate, 0).FirstOrDefault().Value;
                }
                else
                {
                    return 0;//Means no item is being booked
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the booking panel item brief tool tip result for item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="selectedProjectId">The selected project identifier.</param>
        /// <param name="selectedItemBriefTypeId">The selected item brief type identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="quantityBooked">The quantity booked.</param>
        /// <returns></returns>
        public BookingPanelToolTipResultForItem GetBookingPanelItemBriefToolTipResultForItem(int itemId, int selectedProjectId, int selectedItemBriefTypeId,
            int userId, DateTime? fromDate, DateTime? toDate, int quantityBooked)
        {
            string toolTip = null;
            bool canPin = false;
            ProjectBL projectBL = new ProjectBL(DataContext);
            int availableQty = 0;
            bool hasDateFilter = fromDate.HasValue && toDate.HasValue;

            if (!hasDateFilter)
            {
                toolTip = "Please choose a booking period";
            }
            else if (itemId > 0)
            {
                Data.Item item = GetItem(itemId);
                availableQty = hasDateFilter ? GetAvailableItemQuantity(itemId, fromDate.Value, toDate.Value) : 0;

                Data.Project project = projectBL.GetProject(selectedProjectId);
                if (project != null)
                {
                    bool isReadOnlyRightsForProject = projectBL.IsReadOnlyRightsForProject(project.ProjectId, userId);

                    if (projectBL.IsProjectClosed(project.ProjectId))
                    {
                        toolTip = "This Project is finished and has been closed";
                    }
                    else if (!item.IsActive)
                    {
                        toolTip = "The Item has been deleted from the Inventory";
                    }
                    else if (item.CompanyId != project.CompanyId)
                    {
                        toolTip = "Adding Items from Inventories apart from your own is not available.  Please add Items to your Watch List to contact their owner about their availability.";
                    }
                    else if (isReadOnlyRightsForProject)
                    {
                        if (project.ProjectStatusCodeId == Utils.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId || project.ProjectStatusCodeId == Utils.GetCodeByValue("ProjectStatus", "PAYMENTFAILED").CodeId)
                            toolTip = "This Project is currently suspended. So you are not able to add anything to it. If you contact the Company Administrator they can reactivate the Project.";
                        else
                            toolTip = "As you have read only access to this Project. You are not able to add anything to it. If you contact the Project Administrator they can alter your permission level.";
                    }
                    else if (availableQty == 0 || availableQty < quantityBooked)
                    {
                        toolTip = "The Item does not have sufficient units available for this booking period.";
                    }
                    else
                    {
                        toolTip = string.Format("Add Item to the selected {0} brief", Utils.GetItemTypeById(selectedItemBriefTypeId).Name);
                        canPin = true;
                    }
                }
            }
            else
            {
                toolTip = "Select an Item to be able to add it to the Project";
            }

            BookingPanelToolTipResultForItem projectPanelToolTipResultForItem = new BookingPanelToolTipResultForItem();
            projectPanelToolTipResultForItem.ToolTip = toolTip;
            projectPanelToolTipResultForItem.AvailableQuantity = availableQty;
            projectPanelToolTipResultForItem.CanPin = canPin;
            return projectPanelToolTipResultForItem;
        }

        /// <summary>
        /// Gets the booking panel my booking tool tip result for item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="quantityBooked">The quantity booked.</param>
        /// <returns></returns>
        public BookingPanelToolTipResultForItem GetBookingPanelNonProjectBookingToolTipResultForItem(int itemId, int userId, int nonProjectBookingId, DateTime? fromDate, DateTime? toDate, int quantityBooked)
        {
            string toolTip = null;
            bool canPin = false;
            ProjectBL projectBL = new ProjectBL(DataContext);
            CompanyBL companyBL = new CompanyBL(DataContext);

            int availableQty = 0;
            bool hasDateFilter = fromDate.HasValue && toDate.HasValue;

            if (!hasDateFilter)
            {
                toolTip = "Please choose a booking period.";
            }
            else if (itemId > 0)
            {
                Data.Item item = GetItem(itemId);
                availableQty = hasDateFilter ? GetAvailableItemQuantity(itemId, fromDate.Value, toDate.Value) : 0;

                if (!item.IsActive)
                {
                    toolTip = "The Item has been deleted from the Inventory.";
                }
                else if (companyBL.IsCompanySuspended(item.CompanyId.Value, true))
                {
                    toolTip = "This Company is currently suspended. So you are not able to add anything from it. Please contact the Company Administrator.";
                }
                else if (IsItemAlreadySharedToBooking(itemId, nonProjectBookingId, GlobalConstants.RelatedTables.Bookings.NonProject))
                {
                    toolTip = "This Item has already been added to this booking.";
                }
                else if (availableQty == 0 || availableQty < quantityBooked)
                {
                    toolTip = "The Item does not have sufficient units available for this booking period.";
                }
                else
                {
                    toolTip = "Add Item to the selected booking.";
                    canPin = true;
                }
            }
            else
            {
                toolTip = "Select an Item to be able to add it to the Booking.";
            }

            BookingPanelToolTipResultForItem projectPanelToolTipResultForItem = new BookingPanelToolTipResultForItem();
            projectPanelToolTipResultForItem.ToolTip = toolTip;
            projectPanelToolTipResultForItem.AvailableQuantity = availableQty;
            projectPanelToolTipResultForItem.CanPin = canPin;
            return projectPanelToolTipResultForItem;
        }

        /// <summary>
        /// Gets the booking info by company.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="filterByKeyword">The filter by keyword.</param>
        /// <param name="filterByBookingStatus">The filter by booking status.</param>
        /// <param name="showArchived">if set to <c>true</c> [show archived].</param>
        /// <param name="isAutoComplete">if set to <c>true</c> [is auto complete].</param>
        /// <returns></returns>
        public List<BookingInfo> GetBookingInfo(int? companyId, int? userId, string filterByKeyword, int? filterByBookingStatus, bool showArchived, bool isAutoComplete = false)
        {
            return DataContext.GetBookingInfo(companyId, userId, filterByKeyword, isAutoComplete, filterByBookingStatus, showArchived).Select(bi => new BookingInfo
            {
                BookingCount = bi.BookingCount.Value,
                BookingId = bi.BookingId,
                BookingName = bi.BookingName,
                BookingNumber = bi.BookingNumber.Value,
                CompanyName = bi.CompanyName,
                FromDate = bi.FromDate.Value,
                IsArchived = bi.IsArchived.Value,
                IsDifferentFromDate = bi.IsDifferentFromDate.Value,
                LastUpdatedDate = bi.LastUpdatedDate.Value,
                RelatedTable = bi.RelatedTable,
                Status = Utils.GetCodeByCodeId(bi.StatusCodeId.Value).Description,
                StatusCodeId = bi.StatusCodeId.Value,
                StatusSortOrder = Utils.GetCodeByCodeId(bi.StatusCodeId.Value).SortOrder,
                ToDate = bi.ToDate.Value
            }).ToList();
        }

        /// <summary>
        /// Gets the booking.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <returns></returns>
        public Booking GetBooking(int bookingId)
        {
            return DataContext.Bookings.Where(b => b.BookingId == bookingId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the booking.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <returns></returns>
        public Booking GetBooking(int relatedId, string relatedTableName)
        {
            return DataContext.Bookings.Where(b => b.RelatedId == relatedId && b.RelatedTable == relatedTableName).FirstOrDefault();
        }

        /// <summary>
        /// Gets the company booking number.
        /// </summary>
        /// <param name="bookingId">The booking identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyBookingNumber GetCompanyBookingNumber(int bookingId, int companyId)
        {
            return DataContext.CompanyBookingNumbers.Where(cbn => cbn.BookingId == bookingId && (companyId == 0 || cbn.CompanyId == companyId)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the created for search items.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<string> GetCreatedForSearchItems(int companyId, string keyword)
        {
            return (from i in DataContext.Items
                    where i.CompanyId.HasValue && i.CompanyId == companyId && i.IsActive && !i.IsHidden
                            && i.CreatedFor != null && i.CreatedFor.Contains(keyword)
                    select i.CreatedFor).Distinct().Take(10).ToList();
        }

        /// <summary>
        /// Saves the non project booking.
        /// </summary>
        /// <param name="nonProjectBooking">The non project booking.</param>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void SaveNonProjectBooking(NonProjectBooking nonProjectBooking, bool commit)
        {
            DataContext.NonProjectBookings.AddObject(nonProjectBooking);
            if (commit)
            {
                this.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the item bookings by related table.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="isActive">The is active.</param>
        /// <returns></returns>
        public List<CompanyItemBooking> GetItemBookingsByRelatedTable(string relatedTable, int relatedId, bool? isActive = null)
        {
            return (from ib in DataContext.ItemBookings.Where(ib => ib.RelatedTable == relatedTable && ib.RelatedId == relatedId && (!isActive.HasValue || ib.IsActive == isActive.Value))
                    join i in DataContext.Items on ib.ItemId equals i.ItemId
                    where i.IsActive
                    select new CompanyItemBooking
                    {
                        Company = i.Company,
                        ItemBooking = ib
                    }).ToList();
        }

        /// <summary>
        /// Gets the inventory export details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<InventoryExportDetails> GetInventoryExportDetails(int companyId, int itemTypeId)
        {
            var inventoryDetails = DataContext.GetInventoryItems(companyId, itemTypeId);
            if (inventoryDetails != null)
            {
                var data = (from invd in inventoryDetails
                            select new InventoryExportDetails
                            {
                                FieldGroupId = invd.FieldGroupId,
                                FieldGroupName = invd.FieldGroupName,
                                FieldId = invd.FieldId,
                                FieldName = invd.FieldName,
                                ItemId = invd.ItemId.Value,
                                ItemName = invd.Name,
                                Value = invd.value,
                                Description = invd.Description,
                                CreatedFor = invd.CreatedFor,
                                ItemTypeName = invd.ItemTypeName,
                                Quantity = invd.Quantity.Value,
                                Location = invd.Location
                            }).ToList<InventoryExportDetails>();
                return data;
            }
            return null;
        }

        /// <summary>
        /// Gets the inventory role.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="companyUserId">The company user identifier.</param>
        /// <returns></returns>
        public List<CompanyUserRole> GetInventoryRoles(int companyId, int companyUserId)
        {
            int inventoryStaffCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
            int inventoryAdminCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").CodeId;
            int inventoryObserverCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
            int noAccessCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "NOINVENTORYACCESS").CodeId;

            return (from cu in DataContext.CompanyUsers
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where (cur.CompanyUserTypeCodeId == inventoryAdminCodeId ||
                            cur.CompanyUserTypeCodeId == inventoryObserverCodeId ||
                            cur.CompanyUserTypeCodeId == inventoryStaffCodeId ||
                            cur.CompanyUserTypeCodeId == locationManagerCodeId ||
                            cur.CompanyUserTypeCodeId == noAccessCodeId)
                            && cur.IsActive && cu.IsActive && cu.CompanyId == companyId && cu.CompanyUserId == companyUserId
                    select cur).ToList();
        }

        /// <summary>
        /// Gets the company user.
        /// </summary>
        /// <param name="companyUserId">The company user identifier.</param>
        /// <returns></returns>
        public CompanyUser GetCompanyUser(int companyUserId)
        {
            return DataContext.CompanyUsers.Where(cu => cu.CompanyUserId == companyUserId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the highest inventory role.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public CompanyUserRole GetHighestInventoryRole(int companyId, int userId)
        {
            int inventoryStaffCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
            int inventoryAdminCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").CodeId;
            int inventoryObserverCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
            int noAccessCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "NOINVENTORYACCESS").CodeId;

            return (from cu in DataContext.CompanyUsers
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    join c in DataContext.Codes on cur.CompanyUserTypeCodeId equals c.CodeId
                    where (cur.CompanyUserTypeCodeId == inventoryAdminCodeId ||
                            cur.CompanyUserTypeCodeId == inventoryObserverCodeId ||
                            cur.CompanyUserTypeCodeId == inventoryStaffCodeId ||
                            cur.CompanyUserTypeCodeId == locationManagerCodeId ||
                            cur.CompanyUserTypeCodeId == noAccessCodeId)
                            && cur.IsActive && cu.IsActive && cu.CompanyId == companyId && cu.UserId == userId
                    orderby c.SortOrder ascending
                    select cur).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether current user is company inventory staff(admin or staff member) for he specified company.
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool IsCompanyInventoryStaffMemberAnyLocation(int companyID, int userId)
        {
            CompanyUserRole highestRole = GetHighestInventoryRole(companyID, userId);
            return highestRole != null && highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").SortOrder;
        }

        /// <summary>
        /// Determines whether [is company location manager any location] [the specified company identifier].
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool IsCompanyLocationManagerAnyLocation(int companyID, int userId)
        {
            CompanyUserRole highestRole = GetHighestInventoryRole(companyID, userId);
            return highestRole != null && highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").SortOrder;
        }

        /// <summary>
        /// Gets all company inventory admin role.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyUserRole GetAllCompanyInventoryAdminRole(int companyId)
        {
            int inventoryAdminCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").CodeId;
            return (from cu in DataContext.CompanyUsers
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where cur.CompanyUserTypeCodeId == inventoryAdminCodeId && cur.IsActive && cu.IsActive && cu.CompanyId == companyId
                    select cur).FirstOrDefault();
        }

        /// <summary>
        /// Gets the user inventory visibility level.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Code GetUserInventoryVisibilityLevel(int companyId, int userId, int? locationId, bool shouldValidateSharedInventory)
        {
            return Utils.GetCodeByCodeId(
                DataContext.Users.Select(u => DataContext.GetUserInventoryVisibilityLevel(companyId, userId, locationId, shouldValidateSharedInventory)).FirstOrDefault());
        }

        /// <summary>
        /// Gets the location manager.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public User GetLocationManager(int companyId, int locationId)
        {            
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;

            return (from u in DataContext.Users
                    join cu in DataContext.CompanyUsers on u.UserId equals cu.UserId
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where cur.CompanyUserTypeCodeId == locationManagerCodeId && cur.IsActive && cu.IsActive
                            && cu.CompanyId == companyId && cur.LocationId.HasValue && cur.LocationId.Value == DataContext.GetTier2Location(locationId)
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Gets the contact booking manager.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public User GetContactBookingManager(int companyId, int? locationId)
        {
            User contactBookingManager = locationId.HasValue ? GetLocationManager(companyId, locationId.Value) : null;
            if (contactBookingManager == null)
            {
                contactBookingManager = GetInventoryAdmin(companyId);
            }

            return contactBookingManager;
        }

        /// <summary>
        /// Gets the location manager role.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public CompanyUserRole GetLocationManagerRole(int companyId, int locationId)
        {
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;
            return (from cu in DataContext.CompanyUsers
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where cur.CompanyUserTypeCodeId == locationManagerCodeId && cur.IsActive && cu.IsActive
                            && cu.CompanyId == companyId && cur.LocationId.HasValue && cur.LocationId.Value == locationId
                    select cur).FirstOrDefault();
        }

        /// <summary>
        /// Gets the inventory roles by location identifier.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public List<CompanyUserRole> GetInventoryRolesByLocationId(int locationId)
        {
            return (from cur in DataContext.CompanyUserRoles
                    where cur.IsActive && cur.LocationId.HasValue && cur.LocationId.Value == DataContext.GetTier2Location(locationId)
                    select cur).ToList();
        }

        /// <summary>
        /// Gets the type of the inventory roles by location identifier and user.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="companyUserTypeCodeId">The company user type code identifier.</param>
        /// <returns></returns>
        public List<User> GetInventoryRolesByLocationIdAndUserType(int locationId, int companyUserTypeCodeId)
        {
            return (from u in DataContext.Users
                    join cu in DataContext.CompanyUsers on u.UserId equals cu.UserId
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where cur.IsActive && cur.LocationId.HasValue && cur.LocationId.Value == DataContext.GetTier2Location(locationId)
                        && cur.CompanyUserTypeCodeId == companyUserTypeCodeId
                    select u).ToList();
        }
    }
}