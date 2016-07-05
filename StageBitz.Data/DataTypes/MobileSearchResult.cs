using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileSearchResult
    {
        public List<MobileSearchItem> MobileSearchItems { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public int HasMoreResults { get; set; }
    }
}
