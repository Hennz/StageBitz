using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Data.DataAccess
{
    public class UtilityManager : DataManagerBase
    {
        internal UtilityManager(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public List<Country> GetAllCountries()
        {
            var countries = from c in DataContext.Countries
                            where c.IsActive == true
                            select c;

            return countries.ToList<Country>();
        }



    }
}
