using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileItemImageDeleteRequestDetails
    {
        public int DocumentMediaId { get; set; }
        public string MobileImageId { get; set; }
        public int CompanyId { get; set; }        
        public int ItemId { get; set; }       
        public byte[] Token { get; set; }        
        public string Version { get; set; }
    }
}
