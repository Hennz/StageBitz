using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class DailySummaryRecord
    {
        public DateTime Date { get; set; }
        public int CompanyPaymentPackageId { get; set; }
        public int InstanceCount { get; set; }
        public decimal DailyDiscount { get; set; }
        public bool IsEducational { get; set; }
    }
}
