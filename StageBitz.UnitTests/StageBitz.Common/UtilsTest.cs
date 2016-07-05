using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StageBitz.Data;
using System.Transactions;
using System.Data.Common;
using System.Linq;
using StageBitz.Common;
using StageBitz.UnitTests.Mock;

namespace StageBitz.UnitTests.StageBitz.Common
{
    [TestClass]
    public class UtilsTest : UnitTestBase
    {
        [TestMethod]
        public void IsInventoryStaff()
        {
            int inventoryManagerCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");
            CompanyUserRole inventoryManagerUserRole = new CompanyUserRoleMock(DataContext).GetCompanyUserRole(inventoryManagerCodeId);
            DataContext.CompanyUserRoles.AddObject(inventoryManagerUserRole);
            DataContext.SaveChanges();

            Assert.IsTrue(Utils.IsCompanyInventoryStaffMember(inventoryManagerUserRole.CompanyUser.CompanyId, inventoryManagerUserRole.CompanyUser.UserId, null, DataContext));
        }
    }
}
