using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class WatchListItemDetails
    {
        public string Name { get; set; }
        public int ThumbnailMediaId { get; set; }
        public int WatchListHeaderId { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public int? Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int ItemId { get; set; }
        public int VisibilitySortOrder { get; set; }
        public int? LocationId { get; set; }
        public int ContactBookingManagerId { get; set; }
    }
}
