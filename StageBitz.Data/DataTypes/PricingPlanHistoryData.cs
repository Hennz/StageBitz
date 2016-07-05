using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class PricingPlanHistoryData
    {
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyAdminName { get; set; }
        public int CompanyAdminId { get; set; }
        public string ProjectLevel { get; set; }
        public string InventoryLevel { get; set; }
        public string PromotionalCode { get; set; }
        public string Educational { get; set; }
        public string Period { get; set; }
        public DateTime StartDate { get; set; }
        public decimal TotalCost { get; set; }
        public string PaymentMethod { get; set; }
    }
}
