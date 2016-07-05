using System.Collections.Generic;

namespace StageBitz.Data.DataTypes
{
    public class BudgetListInfo
    {
        public List<ItemBriefTaskBudget> ItemBriefTaskBudgetList { get; set; }

        public decimal? GetItemTypeTotalBudget { get; set; }

        public decimal? SumExpened { get; set; }

        public decimal? SumBalance { get; set; }

        public decimal? SumRemaining { get; set; }
    }
}