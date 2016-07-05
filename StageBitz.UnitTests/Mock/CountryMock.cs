using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class CountryMock : MockBase
    {
        public CountryMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public Country GetCountry()
        {
            return DataContext.Countries.FirstOrDefault();
        }
    }
}
