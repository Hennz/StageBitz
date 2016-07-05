using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class CompanySearchData
    {
        public Data.Company Company { get; set; }
        public int AccessToMyCompanyStatusCodeId { get; set; }
        public int AccessToSelectedCompanyStatusCodeId { get; set; }
        public int ThumbnailMediaId { get; set; }
    }
}
