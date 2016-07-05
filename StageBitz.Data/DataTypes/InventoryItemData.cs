namespace StageBitz.Data.DataTypes
{
    public class InventoryItemData
    {
        public Data.Item Item { get; set; }

        public int ThumbnailMediaId { get; set; }

        public int AvailableQty { get; set; }

        public string ItemTypeName { get; set; }

        public string CompanyName { get; set; }

        public string LocationPath { get; set; }

        public bool HasError { get; set; }

        public bool IsWatchListItem { get; set; }

        public string ErrorMessage { get; set; }
    }
}