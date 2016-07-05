using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefListInfo
    {
        public ItemBrief ItemBrief { get; set; }
        public int ThumbnailMediaId { get; set; }
        public int StatusSortOrder { get; set; }
        public string Status {get; set;}
        public bool IsEstimatedCostNullForActiveTask { get; set; }
    }
}
