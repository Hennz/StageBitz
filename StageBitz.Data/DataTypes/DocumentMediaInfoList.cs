using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class DocumentMediaInfoList
    {
        public int DocumentMediaId { get; set; }
        public string Name { get; set; }
        public bool IsImageFile { get; set; }
        public int SortOrder { get; set; }
    }
}
