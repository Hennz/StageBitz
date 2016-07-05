using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes.Finance
{
    public class ProjectPaymentPackageDetails
    {
        public int PackageTypeId { get; set; }
        public int ProjectPaymentPackageDetailId { get; set; }
        public decimal Amount { get; set; }
        public decimal AnualAmount { get; set; }
        public int ProjectCount { get; set; }
        public int HeadCount { get; set; }
        public string PackageName { get; set; }
        public string PackageDisplayText { get; set; }
        public string PackageDisplayName { get; set; }
        public string PackageTitleDiscription { get; set; }
    }
}
