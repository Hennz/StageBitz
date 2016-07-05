using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemBookingData
    {
        public string Status { get; set; }
        public int StatusSortOrder { get; set; }
        public string BookingName { get; set; }
        public string ItemBrief { get; set; }
        public string BookedBy { get; set; }
        public string BookedByEmail { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int BookedQuantity { get; set; }
    }
}
