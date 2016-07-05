using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Booking;
using StageBitz.Logic.Business.Inventory;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace StageBitz.UserWeb.Services
{
    /// <summary>
    /// Web api POCO class for item status details
    /// </summary>
    public class ItemStatusRequestDetails
    {
        public int ItemId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Web api POCO class for booking details
    /// </summary>
    public class BookingDetailInfor
    {
        public int ItemBookingId { get; set; }

        public bool IsSelect { get; set; }

        public int UserId { get; set; }
    }

    /// <summary>
    /// Web api POCO class for pinning details of Item.
    /// </summary>
    public class PinningDetailsForItem
    {
        public int AvailableQuantity { get; set; }
    }

    /// <summary>
    /// Web api POCO class for inventory details of item.
    /// </summary>
    public class InventoryDetailsRequest
    {
        public int ItemId { get; set; }

        public int ProjectId { get; set; }

        public int ItemTypeId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int BookedQty { get; set; }

        public int UserId { get; set; }
    }

    /// <summary>
    /// Search request class for create for field.
    /// </summary>
    public class CreateForSearchRequest
    {
        public int CompanyId { get; set; }

        public string Keyword { get; set; }
    }

    /// <summary>
    /// Inventory Service Controller.
    /// </summary>
    public class InventoryServiceController : ApiController
    {
        /// <summary>
        /// Picks up item.
        /// </summary>
        /// <param name="bookingDetailInfor">The booking detail infor.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingResponse PickUpItem(BookingDetailInfor bookingDetailInfor)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                return inventoryBL.PickUpItem(bookingDetailInfor.ItemBookingId, bookingDetailInfor.IsSelect, bookingDetailInfor.UserId);
            }
        }

        /// <summary>
        /// Returns the item.
        /// </summary>
        /// <param name="bookingDetailInfor">The booking detail infor.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingResponse ReturnItem(BookingDetailInfor bookingDetailInfor)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                return inventoryBL.ReturnItem(bookingDetailInfor.ItemBookingId, bookingDetailInfor.IsSelect, bookingDetailInfor.UserId);
            }
        }

        /// <summary>
        /// Releases the item.
        /// </summary>
        /// <param name="bookingDetailInfor">The booking detail infor.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingResponse ReleaseItem(BookingDetailInfor bookingDetailInfor)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                ItemBooking itemBooking = inventoryBL.GetItemBooking(bookingDetailInfor.ItemBookingId);
                BookingResponse bookingResponse = new BookingResponse();
                if (itemBooking != null)
                {
                    if (itemBooking != null &&
                        (!(Utils.IsCompanyInventoryAdmin(itemBooking.Item.CompanyId.Value, bookingDetailInfor.UserId)
                            || Utils.IsCompanyInventoryStaffMember(itemBooking.Item.CompanyId.Value, bookingDetailInfor.UserId, itemBooking.Item.LocationId, dataContext))))
                    {
                        bookingResponse.Status = "NOTOK";
                        bookingResponse.ErrorCode = (int)ErrorCodes.NoEditPermissionForInventory;
                        bookingResponse.Message = "Could not release the Item.";
                        return bookingResponse;
                    }

                    bool isSuccess = inventoryBL.RemoveInUseItemFromItemBrief(itemBooking.RelatedId, bookingDetailInfor.UserId);
                    bookingResponse.Status = isSuccess ? "OK" : "NOTOK";
                }
                else
                {
                    bookingResponse.Status = "NOTOK";
                }

                return bookingResponse;
            }
        }

        /// <summary>
        /// Approves the booking.
        /// </summary>
        /// <param name="bookingDetailInfor">The booking detail infor.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingResponse ApproveBooking(BookingDetailInfor bookingDetailInfor)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                BookingResponse bookingResponse = inventoryBL.ApproveBooking(bookingDetailInfor.ItemBookingId, bookingDetailInfor.UserId);
                return bookingResponse;
            }
        }

        /// <summary>
        /// Rejects the booking.
        /// </summary>
        /// <param name="bookingDetailInfor">The booking detail infor.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingResponse RejectBooking(BookingDetailInfor bookingDetailInfor)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                BookingResponse bookingResponse = inventoryBL.RejectBooking(bookingDetailInfor.ItemBookingId, bookingDetailInfor.UserId);
                return bookingResponse;
            }
        }

        /// <summary>
        /// Gets the booking details.
        /// </summary>
        /// <param name="bookingDetailsEditRequest">The booking details edit request.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingEdit GetBookingDetails(BookingDetailsEditRequest bookingDetailsEditRequest)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                BookingEdit bookingEdit = inventoryBL.GetBookingDetails(bookingDetailsEditRequest);

                return bookingEdit;
            }
        }

        /// <summary>
        /// Saves the booking details.
        /// </summary>
        /// <param name="bookingDetailsEditedHeader">The booking details edited header.</param>
        /// <returns></returns>
        [HttpPost]
        public BookingDetailsSaveResult SaveBookingDetails(BookingDetailsEditedHeader bookingDetailsEditedHeader)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                BookingDetailsSaveResult bookingDetailsSaveResult = inventoryBL.SaveBookingDetails(bookingDetailsEditedHeader);
                return bookingDetailsSaveResult;
            }
        }

        /// <summary>
        /// Gets the created for search items.
        /// </summary>
        /// <param name="reqest">The reqest.</param>
        /// <returns></returns>
        [HttpPost]
        public List<string> GetCreatedForSearchItems(CreateForSearchRequest reqest)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                return inventoryBL.GetCreatedForSearchItems(reqest.CompanyId, reqest.Keyword); ;
            }
        }
    }
}