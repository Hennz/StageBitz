using StageBitz.Data;

namespace StageBitz.Reports.UserWeb.Parameters
{
    public class BookingDetailsReportParameters
    {
        public int ItemTypeId { get; set; }

        public int BookingId { get; set; }

        public string BookingName { get; set; }

        public int CompanyId { get; set; }

        public string SortExpression { get; set; }

        public int UserId { get; set; }

        public User ContactPerson { get; set; }

        public string DisplayMode { get; set; }

        public string RelatedTable { get; set; }

        public bool ShowMyBookingsOnly { get; set; }
    }
}