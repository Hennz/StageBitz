using System;

namespace StageBitz.Reports.UserWeb.Parameters
{
    public class InventoryItemListReportParameters
    {
        public int SharedInventoryCompanyId { get; set; }

        public int CompanyId { get; set; }

        public int UserId { get; set; }

        public string FindByName { get; set; }

        public int FindByItemTypeId { get; set; }

        public DateTime? FindFromDate { get; set; }

        public DateTime? FindToDate { get; set; }

        public bool IsThumbnailMode { get; set; }

        public string SortExpression { get; set; }

        public bool HasNoDateConfigured { get; set; }

        public int? LocationId { get; set; }

        public int? ItemVisibilityCodeId { get; set; }
    }
}