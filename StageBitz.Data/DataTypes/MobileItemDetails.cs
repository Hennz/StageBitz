using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileItemDetails
    {
        public int ItemId { get; set; }       
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemStatus { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }        
        public DateTime LastUpdatedDate { get; set; }
        public List<int> DocumentMediaIdList { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public int CanEditItem { get; set; }
    }
}
     