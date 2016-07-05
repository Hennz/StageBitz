using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class MobileInitialData
    {
        public byte[] UserToken { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public List<ItemTypeData> ItemTypeList { get; set; }
        public List<CompanyListOfUser> CompanyList { get; set; }
    }
}

