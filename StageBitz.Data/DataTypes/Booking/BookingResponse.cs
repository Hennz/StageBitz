using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes.Booking
{
    public class BookingResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }
    }
}
