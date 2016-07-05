using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StageBitz.Data.DataAccess;

namespace StageBitz.Data
{
    public class DBAccessManager
    {
        private StageBitzDB dataContext = null;
        private UserManager userManager = null;
        private UtilityManager utilityManager = null;
       

        /// <summary>
        /// Returns the Entity Framework data context for system data access.
        /// </summary>
        public StageBitzDB DataContext
        {
            get
            {
                if (dataContext == null)
                {
                    dataContext = new StageBitzDB();
                }

                return dataContext;
            }
        }

        public UserManager UserManager
        {
            get
            {
                if (userManager == null)
                {
                    userManager = new UserManager(DataContext);
                }

                return userManager;
            }
        }

        public UtilityManager UtilityManager
        {
            get
            {
                if (utilityManager == null)
                {
                    utilityManager = new UtilityManager(DataContext);
                }

                return utilityManager;
            }
        }
        

        public void SaveData()
        {
            DataContext.SaveChanges();
        }

    }
}
