using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileItem
    {
        public int ItemId { get; set; }
        public string DeviceItemId { get; set; }
        public int CompanyId { get; set; }
        public byte[] Token { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
