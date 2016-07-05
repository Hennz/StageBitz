using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class CompanyListInfo
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int InvitationId { get; set; }
        public bool IsCompanyUser { get; set; }
        public bool IsInventoryStaff { get; set; }
        public int ProjectId { get; set; }

    }

    public class CompanyListInfoComparer : IEqualityComparer<CompanyListInfo>
    {
        public static readonly CompanyListInfoComparer Instance = new CompanyListInfoComparer();

        public bool Equals(CompanyListInfo x, CompanyListInfo y)
        {
            return x.CompanyId == y.CompanyId;
        }

        public int GetHashCode(CompanyListInfo obj)
        {
            return obj.CompanyId.GetHashCode();
        }
    }
}
