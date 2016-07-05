using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefTaskBudget
    {
        public decimal? EstimatedCost { get; set; }
        public decimal? TotalCost { get; set; }
        public int ItemBriefTaskStatusCodeId { get; set; }
    }
}
