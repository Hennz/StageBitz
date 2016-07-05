using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class CompanyProjectDetails
    {
        public int ProjectId {get;set;}
        public string ProjectName {get;set;}
        public int ProjectStatusCodeId {get;set;}
        public string ProjectStatus {get;set;}
        public int SortOrder { get; set; }
        public DateTime? ExpirationDate {get;set;}
        public LastPaymentDetails LastPayment { get; set; }
    }

    public class LastPaymentDetails
    {
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }
}
