using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class MockBase
    {
        public StageBitzDB DataContext
        {
            get;
            set;
        }

        public MockBase(StageBitzDB dataContext)
        {
            DataContext = dataContext;
        }
    }
}
