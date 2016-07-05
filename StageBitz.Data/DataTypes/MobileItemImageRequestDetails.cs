using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileItemImageRequestDetails
    {
            public int DocumentMediaId { get; set; }
            public int ItemId { get; set; }
            public int CompanyId { get; set; }
            public byte[] Token { get; set; }
            public string Version { get; set; }
    }
}
