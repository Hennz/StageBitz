using System;

namespace StageBitz.Data.DataTypes
{
    /// <summary>
    /// Poco class for booking infos
    /// </summary>
    public class BookingInfo
    {
        public int BookingId { get; set; }

        public string BookingName { get; set; }

        public int BookingNumber { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public string CompanyName { get; set; }

        public int BookingCount { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public bool IsDifferentFromDate { get; set; }

        public bool IsDifferentToDate { get; set; }

        public string Status { get; set; }

        public int StatusSortOrder { get; set; }

        public int StatusCodeId { get; set; }

        public bool IsArchived { get; set; }

        public string RelatedTable { get; set; }
    }
}