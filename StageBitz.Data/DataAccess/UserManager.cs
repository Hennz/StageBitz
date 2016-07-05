using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Data.DataAccess
{
    public class UserManager : DataManagerBase
    {
        internal UserManager(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public User AuthenticateUser(string username, string passwordHash)
        {
            var user = from u in DataContext.Users
                       join code in DataContext.Codes on u.UserAccountTypeCodeID equals code.CodeId
                       where u.LoginName == username && u.Password == passwordHash && u.IsActive == true && code.Value == "USER"
                       select u;

            if (user.Count<User>() == 1)
            {
                return user.First<User>();
            }

            return null;
        }

        public void SaveNewUser(User user)
        {
            DataContext.Users.AddObject(user);
            DataContext.SaveChanges();
        }

        public bool CheckLogInNameAvailability(string loginName)
        {
            var user = from u in DataContext.Users
                       where u.LoginName == loginName && u.IsActive == true
                       select u;

            if (user.Count<User>() > 0)
            {
                return true;
            }
            return false;
        }

        public User GetUserById(int userId)
        {
            var user = from u in DataContext.Users
                       where u.UserId == userId
                       select u;

            if (user.Count<User>() == 1)
            {
                return user.First<User>();
            }

            return null;
        }
    }
}
