using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes.Finance
{
    public class InventoryPaymentPackageDetails
    {
        public int PackageTypeId { get; set; }
        public int InventoryPaymentPackageDetailId { get; set; }
        public decimal Amount { get; set; }
        public decimal AnualAmount { get; set; }
        public int? ItemCount { get; set; }
        public string PackageName { get; set; }
        public string PackageDisplayText { get; set; }
        public string PackageDisplayName { get; set; }
    }
}
