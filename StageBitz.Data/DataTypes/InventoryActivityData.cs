using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class InventoryActivityData
    {
        public string ItemType { get; set; }
        public int Quantity { get; set; }
        public int ManuallyAdded { get; set; }
        public int CreatedInProject { get; set; }
        public int Booked { get; set; }
    }
}
