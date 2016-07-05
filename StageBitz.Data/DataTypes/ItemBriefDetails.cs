using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefDetails
    {
        public string DisplayMarkUp { get; set; }
        public ItemBriefInfo ItemBriefInfo { get; set; }
        public List<ValuesInfo> ItemBriefValues { get; set; }
        public ItemDetails ItemDetails { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanSeeBudgetSummary { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public int CountryId { get; set; }
    }

    public class ItemBriefInfo
    {
        public int ItemBriefId { get; set; }
        public int ItemBriefItemTypeId { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public string DueDate { get; set; }
        public string StatusCodeValue { get; set; }
        public string StatusCodeDescription { get; set; }
        public string Description { get; set; }
        public string Brief { get; set; }
        public string Act { get; set; }
        public string Scene { get; set; }
        public string Page { get; set; }
        public string Category { get; set; }
        public string Character { get; set; }
        public string Preset { get; set; }
        public string Approver { get; set; }
        public string RehearsalItem { get; set; }
        public string Usage { get; set; }
        public string Considerations { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string ExpendedAmount { get; set; }
        public string SumRemaining { get; set; }
        public string BalanceAmount { get; set; }
        
        
    }

    public class ItemBriefDetailsForProjectPanel
    {
        public string GroupName { get; set; }
        public int GroupCount { get; set; }
        public List<ItemBrief> itemBriefList { get; set; }
    }

    public class ItemBookingAllDetails
    {
        public ItemBrief ItemBrief { get; set; }
        public IEnumerable<ItemBooking> ItemBookingList { get; set; }
    }
}

