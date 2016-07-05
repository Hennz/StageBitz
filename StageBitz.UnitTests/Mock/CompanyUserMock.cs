using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class CompanyUserMock : MockBase
    {
        public CompanyUserMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public CompanyUser GetCompanyUser()
        {
            return new CompanyUser
            {
                Company = new CompanyMock(DataContext).GetCompany(),
                User = new UserMock(DataContext).GetWebUser(),
                IsActive = true
            };
        }
    }
}
