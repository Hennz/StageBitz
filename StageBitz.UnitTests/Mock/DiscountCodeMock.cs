using StageBitz.Common;
using StageBitz.Data;

namespace StageBitz.UnitTests.Mock
{
    internal class DiscountCodeMock : MockBase
    {
        public DiscountCodeMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public DiscountCode GetDiscountCode(string name, decimal discount, int weeks, int instanceCount)
        {
            return new DiscountCode
            {
                Code = name,
                Discount = discount,
                Duration = weeks,
                ExpireDate = Utils.Today.AddDays(7 * weeks),
                InstanceCount = instanceCount
            };
        }
    }
}