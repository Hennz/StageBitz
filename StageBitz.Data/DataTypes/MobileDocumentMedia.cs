using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileDocumentMedia
    {
        public int DocumentMediaId { get; set; }
        public string MobileImageId { get; set; }
        public int CompanyId { get; set; }
        public int SortOrder { get; set; }
        public int RelatedId { get; set; }
        public string RelatedTable { get; set; }
        public byte[] Token { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public string Image { get; set; }
        public string Version { get; set; }
    }
}
