
namespace StageBitz.Data.DataTypes
{
    public class ItemTypeSummary
    {
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public int ItemCount { get; set; }
        public int CompletedItemCount { get; set; }
        public int InProgressItemCount { get; set; }
        public int NotStartedItemCount { get; set; }
    }
}
