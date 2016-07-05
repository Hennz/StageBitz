using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ExportFileDetails
    {
        public int ExportFileId { get; set; }

        public int RelatedId { get; set; }
        public string RelatedTable { get; set; }
        public int ExportFileStatusCodeId { get; set; }

        public string EntityName { get; set; }

        public int SortOrder { get; set; }

        public double FileSize { get; set; }
    }
}
