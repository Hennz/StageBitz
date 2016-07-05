using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
  public class PackageConfirmationEmailContent
    {
        public string ToEmail { get; set; }
        public string UserName { get; set; }
        public string Position { get; set; }
        public string CompanyName { get; set; }
        public string CompanyURL { get; set; }
        public string ProjectPackage { get; set; }
        public string ProjectPeriodPrice { get; set; }
        public string InventoryPackage { get; set; }
        public string InventoryPeriodPrice { get; set; }
        public string TotalPriceString { get; set; }
        public string PromotionalCodeExpireDate { get; set; }
        public bool IsEducational { get; set; }
        public string Discount { get; set; }
        public string EducationalPosition { get; set; }
        public string FormattedAuthText { get; set; }
        public bool IsInventryProjectOrDurationChanged { get; set; }
    }
}
