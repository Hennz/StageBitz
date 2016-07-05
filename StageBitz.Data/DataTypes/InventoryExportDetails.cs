using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class InventoryExportDetails
    {
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public int FieldGroupId { get; set; }
        public string FieldGroupName { get; set; }
        public string FieldName { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
        public string ItemTypeName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string CreatedFor { get; set; }
        public string Location { get; set; }
        
    }
}
