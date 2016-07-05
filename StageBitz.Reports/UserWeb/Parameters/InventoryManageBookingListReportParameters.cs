using System;

namespace StageBitz.Reports.UserWeb.Parameters
{
    public class InventoryManageBookingListReportParameters
    {
        public int? CompanyId { get; set; }

        public int? CreatedByUserId { get; set; }

        public string SortExpression { get; set; }

        public string SearchText { get; set; }

        public bool IsInventoryManagerMode { get; set; }

        public int UserId { get; set; }

        public int? BookingStatus { get; set; }

        public bool ShowArchived { get; set; }
    }
}