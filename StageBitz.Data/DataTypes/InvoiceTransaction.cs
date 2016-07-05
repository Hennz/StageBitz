using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
   public class InvoiceTransaction
    {
       public int InvoiceID { get; set; }
       public decimal Amount { get; set; }
       public DateTime? InvoiceDate { get; set; }
       public DateTime? FromDate { get; set; }
       public DateTime? ToDate { get; set; }
        public int RelatedId { get; set; }
        public int ProcessedInvoiceTypeCodeId { get; set; }
        public int ReceiptID  { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string PaymentLogReferenceNumber { get; set; }
    }
}
