using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class PaymentSummaryDetails
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int? PaymentMethodCodeId { get; set; }
        public int ProjectPaymentPackageTypeId { get; set; }
        public int InventoryPaymentPackageTypeId { get; set; }
        public CompanyPaymentPackage CompanyPaymentPackage { get; set; }
        public bool ShouldProcess { get; set; }
        public bool  HasPackageChanged { get; set; }
        public bool IsEducationPackage { get; set; }
        public int PaymentDurationTypeCodeId { get; set; }
        public bool IsUserAction { get; set; } 
        public DiscountCodeUsage DiscountCodeUsageToApply { get; set; }
        public DateTime PackageStartDate { get; set; }
    }
}

