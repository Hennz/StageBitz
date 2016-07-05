using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class CompanyInventoryDetails
    {
        public string ItemType { get; set; }
        public int TotalItems { get; set; }
        public int AvailableItems { get; set; }
        public int PinnedItems { get; set; }
        public int InUseItems { get; set; }
    }
}
