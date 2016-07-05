using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    [Serializable]
    public class PricePlanDetails
    {
        public int CompanyId { get; set; }
        public int InventoryPaymentPackageTypeId { get; set; }
        public int ProjectPaymentPackageTypeId { get; set; }
        public CompanyPaymentPackage CompanyPaymentPackage { get; set; }
        public bool IsEducationalPackage { get; set; }
        public DiscountCodeUsage DiscountCodeUsage { get; set; }
        public DiscountCode DiscountCode { get; set; }
        public int PaymentDurationCodeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountWithEducationalPackage { get; set; }
        public decimal TotalAmountForPeriod { get; set; }
        public string Position { get; set; }
        public int? PaymentMethodCodeId { get; set; }
        public DateTime PackageStartDate { get; set; }
    }
}
