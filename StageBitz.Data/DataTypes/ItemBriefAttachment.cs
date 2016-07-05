using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBriefAttachment
    {
        public int DocumentMediaId { get; set; }
        public string Name { get; set; }
        public bool IsImageFile { get; set; }
        public string FileExtension { get; set; }
        public int ItemBriefItemDocumentMediaId { get; set; }
        public string SourceTable { get; set; }
        public string Description { get; set; }
    }
}
