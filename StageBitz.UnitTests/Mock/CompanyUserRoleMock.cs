using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class CompanyUserRoleMock : MockBase
    {
        public CompanyUserRoleMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public CompanyUserRole GetCompanyUserRole(int userRoleCodeId)
        {
            return new CompanyUserRole
            {
                CompanyUser = new CompanyUserMock(DataContext).GetCompanyUser(),
                CompanyUserTypeCodeId = userRoleCodeId,
                IsActive = true
            };
        }
    }
}
