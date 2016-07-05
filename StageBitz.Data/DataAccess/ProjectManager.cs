using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Data.DataAccess
{
    public class ProjectManager : DataManagerBase
    {
        internal ProjectManager(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Retrives all projects visible to the user. Projects with admin rights and projects with non admin rights 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IEnumerable<Project> GetProjectsByUserID(int userId)
        {
            var projects = from p in DataContext.Projects
                           join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                           where pu.UserId == userId
                           select p;                      

            return projects;
        }


    }
}
