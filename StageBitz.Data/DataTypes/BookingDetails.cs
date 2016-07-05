using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class BookingHeaderData
    {
        public string BookingNumber { get; set; }
        public string BookingName { get; set; }
        public DateTime MaxLastUpdatedDate { get; set; }
        public string CompanyName { get; set; }
        public string InventoryManagerName { get; set; }
    }

    public class BookingDetails
    {
        public int BookingId { get; set; }
        public int ItemBookingId { get; set; }
        public int ItemId { get; set; }
        public int ItemBriefId { get; set; }
        public int CompanyId { get; set; }
        public string ItemName { get; set; }
        public string ItemBriefName { get; set; }
        public string CurrentStatus { get; set; }
        public int ItemStatusOrder { get; set; }
        public int Quantity { get; set; }
        public string BookedBy { get; set; }
        public string BookedByEmail { get; set; }
        public int ItemBookingStatusCodeId { get; set; }
        public int InventoryStatusCodeId { get; set; }
        public string ItemTypeName { get; set; }
        public int ItemTypeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsPickedUp { get; set; }
        public int IsPickedUpOrder { get; set; }
        public bool IsReturned { get; set; }
        public int IsReturnedOrder { get; set; }
        public int StatusSortOrder { get; set; }
        public int ConfirmedSortOrder { get; set; }
        public bool IsReject { get; set; }
        public bool IsActive { get; set; }
        public bool CanApprove { get; set; }
        public string ApprovalFailureReason { get; set; }
        public string RejectionFailureReason { get; set; }
        public bool CanReject { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public bool CanAccessItemDetails { get; set; }
        public int ContactLocationManagerId { get; set; }
    }

    public class BookingEdit
    {
        public string BookingNumber { get; set; }
        public string BookingName { get; set; }
        public string MaxLastUpdatedDate { get; set; }
        public DateTime DataDisplayStartDate { get; set; }
        public DateTime PeriodToConsider { get; set; }
        public DateTime ToDay { get; set; }
        public bool IsToDateEdit { get; set; }
        public List<BookingDetailsToEdit> BookingDetailToEditList { get; set; }
    }

    public class BookingDetailsEditRequest
    {
        public int ItemTypeId { get; set; }
        public DateTime ToDay { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public bool IsToDateEdit { get; set; }
        public bool ShouldIgnoreDefaultSort { get; set; }

        public bool IsInventoryManager { get; set; }
    }

    public class BookingDetailsToEdit
    {
        public int ItemBookingId { get; set; }
        public string ItemBriefName { get; set; }
        public string ItemName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? LeftMarginDate { get; set; } //This must be the closest Avaialble "From Date" that the "From" can be adjusted.
        public DateTime? RightMarginDate { get; set; } //This must be the closest Avaialble "To Date" that the "To" Date can be adjusted.
        public DateTime LastUpdatedDate { get; set; }
        public bool IsLocallyEdited { get; set; }
        public bool HasError { get; set; }
    }

    public class BookingDetailsEditedHeader
    {
        public int UserId { get; set; }
        public bool IsToDateEdit { get; set; }
        public int ItemTypeId { get; set; }
        public int BookingId { get; set; }
        public int CompanyId { get; set; }
        public bool IsInventoryManager { get; set; }
        public List<BookingDetailsEdited> BookingDetailsEditedList { get; set; }
    }

    public class BookingDetailsEdited
    {
        public int ItemBookingId { get; set; }
        public DateTime NewDate { get; set; }
    }

    public class BookingDetailsSaveResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public bool HasBookingLinesError { get; set; }
        public BookingEdit BookingEdit { get; set; }
    }

    public class CompanyBookingDetails
    {
        public IEnumerable<BookingDetails> BookingDetailList { get; set; }
        public DateTime MaxLastUpdatedDate { get; set; }
        public int BookingNumber { get; set; }
        public int BookingCount { get; set; }
    }
}
