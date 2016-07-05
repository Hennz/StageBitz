namespace StageBitz.Data.DataTypes
{
    public class ItemBriefRequestDetails
    {
        public int UserId { get; set; }

        public int ItemBriefId { get; set; }

        public int? ItemTypeId { get; set; }
    }

    public class ItemBriefResulstObject
    {
        public int ItemBriefId { get; set; }

        public ItemResultObject ItemResultObject { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public int ErrorCode { get; set; }
    }
}