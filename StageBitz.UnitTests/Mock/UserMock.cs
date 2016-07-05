using StageBitz.Common;
using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.UnitTests.Mock
{
    public class UserMock : MockBase
    {
        public UserMock(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        public User GetWebUser()
        {
            Code userAccounttype = Utils.GetCodeByValue("UserAccountType", "USER");

            User user = new User
            {
                AddressLine1 = "Address1",
                AddressLine2 = "Address2",
                City = "City",
                Country = new CountryMock(DataContext).GetCountry(),
                Email1 = "unitTests@yopmail.com",
                Email2 = "unitTests@yopmail.com",
                FirstName = "FirstName",
                IsActive = true,
                IsEmailVisible = true,
                LastName = "LastName",
                LoginName = "unitTests@yopmail.com",
                Password = Utils.HashPassword("123456"),
                UserAccountTypeCodeID = userAccounttype.CodeId
            };

            return user;
        }
    }
}
