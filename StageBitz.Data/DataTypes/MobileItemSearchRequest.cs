using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileItemSearchRequest
    {
        public string ItemName {get;set;}
        public int ItemTypeId { get; set; }
        public int CompanyId { get; set; }
        public byte[] Token { get; set; }
        public string Version { get; set; }
        public int ViewedResultCount { get; set; }       
    }
}
