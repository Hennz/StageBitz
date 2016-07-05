using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class CompanyMock : MockBase
    {
        public CompanyMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public Company GetCompany()
        {
            int activeStatusCode = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
            return new Company
            {
                CompanyName = "UnitTestCompany",
                Country = new CountryMock(DataContext).GetCountry(),
                CompanyStatusCodeId = activeStatusCode
            };
        }

        public Company GetCompany(int companyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            return companyBL.GetCompany(companyId);
        }
    }
}
