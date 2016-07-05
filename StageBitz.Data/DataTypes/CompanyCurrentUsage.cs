using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class CompanyCurrentUsage
    {
        public int ProjectCount { get; set; }
        public int FreeTrialProjectCount { get; set; }
        public int InventoryCount { get; set; }
        public int UserCount { get; set; }
    }
}
