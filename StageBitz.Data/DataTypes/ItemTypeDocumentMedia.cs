using System.Collections.Generic;

namespace StageBitz.Data.DataTypes
{
    public class ItemTypeDocumentMedia
    {
        public IEnumerable<DocumentMediaInfo> DocumentMedias { get; set; }

        public string ItemTypeName { get; set; }

        public int ItemTypeId { get; set; }
    }

    public class DocumentMediaInfo
    {
        public int DocumentMediaId { get; set; }

        public int EntityId { get; set; }

        public string EntityName { get; set; }
    }
}