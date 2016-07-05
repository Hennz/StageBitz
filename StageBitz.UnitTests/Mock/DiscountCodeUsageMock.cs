using StageBitz.Common;
using StageBitz.Data;

namespace StageBitz.UnitTests.Mock
{
    internal class DiscountCodeUsageMock : MockBase
    {
        public DiscountCodeUsageMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public DiscountCodeUsage GetDiscountCodeUsage(bool isAdminApplied, int? companyId)
        {
            var discount = new DiscountCodeMock(DataContext).GetDiscountCode("DisCode50", 50.00M, 10, 10);
            Data.Company company = null;
            if (!companyId.HasValue)
            {
                company = new CompanyMock(DataContext).GetCompany();
            }
            else
            {
                company = new CompanyMock(DataContext).GetCompany(companyId.Value);
            }

            DataContext.SaveChanges();

            return new DiscountCodeUsage
            {
                DiscountCode = discount,
                CompanyId = company.CompanyId,
                CreatedByUserId = 0,
                CreatedDate = Utils.Now,
                IsAdminApplied = isAdminApplied,
                EndDate = Utils.Today.AddDays(discount.Duration * 7),
                IsActive = true,
                StartDate = Utils.Today
            };
        }
    }
}