using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Data.DataAccess
{
    public class DataManagerBase
    {
        protected StageBitzDB DataContext
        {
            get;
            private set;
        }

        internal DataManagerBase(StageBitzDB dataContext)
        {
            DataContext = dataContext;
        }

    }
}
