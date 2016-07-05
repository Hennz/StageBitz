namespace StageBitz.Data.DataTypes
{
    public class ItemRequestDetails
    {
        public int UserId { get; set; }

        public int ItemId { get; set; }

        public int ItemBriefId { get; set; }

        public int? ItemTypeId { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }
    }

    public class ItemResultObject
    {
        public int ItemId { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public int ErrorCode { get; set; }
    }
}