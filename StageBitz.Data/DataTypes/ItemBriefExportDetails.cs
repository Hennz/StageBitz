using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefExportDetails
    {
        public int ItemBriefId { get; set; }
        public string ItemBriefName { get; set; }
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public string Category { get; set; }
        public string ItemTypeName { get; set; }
        public string Preset { get; set; }
        public string RehearsalItem { get; set; }
        public string Act { get; set; }
        public string Scene { get; set; }
        public string Page { get; set; }
        public string Usage { get; set; }
        public string Brief { get; set; }
        public string Approver { get; set; }
        public string Considerations { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
        public string FieldGroupName { get; set; }
        public string Character { get; set; }

        public int FieldGroupId { get; set; }
    }
}
