using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ShoppingList
    {
        public decimal? EstimatedCost { get; set; }
        public decimal? NetCost { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }
        public string ItemName { get; set; }
        public string Status { get; set; }
        public string TaskDescription { get; set; }
        public string Vendor { get; set; }
        public bool IsEstimatedCostNullForShoppingList { get; set; }
    }
}
