using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class BudgetSummaryBody
    {
        public int ItemBriefId { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public decimal Budget { get; set; }
    }
}
