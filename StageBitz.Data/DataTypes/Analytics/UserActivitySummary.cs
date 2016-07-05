using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes.Analytics
{
    public class UserActivitySummary
    {
        public int UserId { get; set; }
        public int ProjectCount { get; set; }
        public int DaysCount { get; set; }
        public int CompanyCount { get; set; }
    }
}
