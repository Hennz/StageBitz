using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StageBitz.Data;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefTaskDetails
    {
        public ItemBriefTask ItemBriefTask { get; set; }
        public string ItemBriefName { get; set; }
        public int SortOrder { get; set; }
        public decimal? Total { get; set; }
    }
}
