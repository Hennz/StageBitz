using Microsoft.VisualStudio.TestTools.UnitTesting;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Finance;
using StageBitz.UnitTests.Mock;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace StageBitz.UnitTests.StageBitz.Logic
{
    /// <summary>
    /// Class that contains all unit test for finance BL.
    /// </summary>
    [TestClass]
    public class FinanceBLTest : UnitTestBase
    {
        /// <summary>
        /// Test the add discount code usage by SB admin.
        /// </summary>
        [TestMethod]
        public void AddDiscountCodeUsageBySBAdminTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);
            DiscountCodeUsage discountCodeUsage = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, null);

            // SB admin apply discount code.
            financeBL.AddDiscountCodeUsageBySBAdmin(discountCodeUsage, 0);
            int companyId = discountCodeUsage.CompanyId;

            DiscountCodeUsage discountCodeUsageDB = financeBL.GetLatestDiscountCodeUsage(companyId);
            Assert.IsNotNull(discountCodeUsageDB);
            Assert.AreEqual(discountCodeUsage, discountCodeUsageDB);

            // User replace discount code.
            DiscountCode userDiscount = new DiscountCodeMock(DataContext).GetDiscountCode("DisCode60", 60.00M, 5, 5);
            DataContext.DiscountCodes.AddObject(userDiscount);

            financeBL.SaveDiscountCodeUsageToCompany(userDiscount, 0, companyId);
            financeBL.SaveChanges();

            discountCodeUsageDB = financeBL.GetLatestDiscountCodeUsage(companyId);
            Assert.IsNotNull(discountCodeUsageDB);
            Assert.AreEqual(userDiscount, discountCodeUsageDB.DiscountCode);

            // SB admin apply discount code again
            DiscountCodeUsage discountCodeUsageNew = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, companyId);
            financeBL.AddDiscountCodeUsageBySBAdmin(discountCodeUsageNew, 0);

            discountCodeUsageDB = financeBL.GetLatestDiscountCodeUsage(companyId);
            Assert.IsNotNull(discountCodeUsageDB);
            Assert.AreEqual(discountCodeUsageNew, discountCodeUsageDB);
            Assert.AreNotEqual(discountCodeUsage, discountCodeUsageNew);
        }

        /// <summary>
        /// Gets the discount code test.
        /// </summary>
        [TestMethod]
        public void GetDiscountCodeTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);

            DiscountCode userDiscount = new DiscountCodeMock(DataContext).GetDiscountCode("DisCode60", 60.00M, 5, 5);
            DataContext.DiscountCodes.AddObject(userDiscount);
            financeBL.SaveChanges();

            DiscountCode discountDB = financeBL.GetDiscountCode("DisCode60");
            Assert.AreEqual(userDiscount, discountDB);
        }

        /// <summary>
        /// Gets the discount code usages test.
        /// </summary>
        [TestMethod]
        public void GetDiscountCodeUsagesTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);

            // Create 3 companies
            CompanyMock companyMock = new CompanyMock(DataContext);
            Company company1 = companyMock.GetCompany();
            Company company2 = companyMock.GetCompany();
            Company company3 = companyMock.GetCompany();

            // Create 2 discount codes
            DiscountCode discount1 = new DiscountCodeMock(DataContext).GetDiscountCode("DisCode1", 60.00M, 5, 5);
            DiscountCode discount2 = new DiscountCodeMock(DataContext).GetDiscountCode("DisCode2", 50.00M, 5, 5);

            DataContext.DiscountCodes.AddObject(discount1);
            DataContext.DiscountCodes.AddObject(discount2);
            DataContext.SaveChanges();

            // Apply discount codes to compnies
            financeBL.SaveDiscountCodeUsageToCompany(discount1, 0, company1.CompanyId);
            DataContext.SaveChanges();
            financeBL.SaveDiscountCodeUsageToCompany(discount1, 0, company2.CompanyId);
            DataContext.SaveChanges();
            financeBL.SaveDiscountCodeUsageToCompany(discount2, 0, company2.CompanyId);
            DataContext.SaveChanges();
            financeBL.SaveDiscountCodeUsageToCompany(discount2, 0, company3.CompanyId);
            DataContext.SaveChanges();

            // Get Counts
            List<DiscountCodeUsage> dis1UsageWithInActive = financeBL.GetDiscountCodeUsages(discount1.DiscountCodeID, true);
            List<DiscountCodeUsage> dis1UsageWithOutInActive = financeBL.GetDiscountCodeUsages(discount1.DiscountCodeID, false);
            List<DiscountCodeUsage> dis2UsageWithInActive = financeBL.GetDiscountCodeUsages(discount2.DiscountCodeID, true);
            List<DiscountCodeUsage> dis2UsageWithOutInActive = financeBL.GetDiscountCodeUsages(discount2.DiscountCodeID, false);

            // Validate
            Assert.IsTrue(dis1UsageWithInActive.Count == 2);
            Assert.IsTrue(dis1UsageWithOutInActive.Count == 1);
            Assert.IsTrue(dis2UsageWithInActive.Count == 2);
            Assert.IsTrue(dis2UsageWithOutInActive.Count == 2);

        }

        /// <summary>
        /// Gets the discounted amount.
        /// </summary>
        [TestMethod]
        public void GetDiscountedAmountTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);
            List<ProjectPaymentPackageDetails> projectPackageDetails = Utils.GetSystemProjectPackageDetails();
            ProjectPaymentPackageDetails projectMegaPackage = projectPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            List<InventoryPaymentPackageDetails> inventoryPackageDetails = Utils.GetSystemInventoryPackageDetails();
            InventoryPaymentPackageDetails inventoryMegaPackage = inventoryPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            // Create a company
            DiscountCodeUsage discountCodeUsage = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, null);
            DataContext.SaveChanges();

            int monthlyPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
            int annualPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");

            decimal totalDue = financeBL.CalculateALLPackageAmountsByPeriod(projectMegaPackage.PackageTypeId, inventoryMegaPackage.PackageTypeId, monthlyPaymentDurationCodeId);
            decimal discountedAmount = financeBL.GetDiscountedAmount(discountCodeUsage.CompanyId, totalDue, monthlyPaymentDurationCodeId, discountCodeUsage, Utils.Today, Utils.Today.AddMonths(1));

            Assert.IsTrue((projectMegaPackage.Amount + inventoryMegaPackage.Amount) * (discountCodeUsage.DiscountCode.Discount / 100) == discountedAmount);

            totalDue = financeBL.CalculateALLPackageAmountsByPeriod(projectMegaPackage.PackageTypeId, inventoryMegaPackage.PackageTypeId, annualPaymentDurationCodeId);
            discountedAmount = financeBL.GetDiscountedAmount(discountCodeUsage.CompanyId, totalDue, annualPaymentDurationCodeId, discountCodeUsage, Utils.Today, Utils.Today.AddYears(1));


            decimal discountedDays = (discountCodeUsage.EndDate - discountCodeUsage.StartDate.Value).Days;
            decimal totalDays = (Utils.Today.AddYears(1) - Utils.Today).Days;
            decimal noDiscountedDays = totalDays - discountedDays;


            decimal testTotalAmount = (((discountedDays / totalDays) * (discountCodeUsage.DiscountCode.Discount / 100) )
                                + (noDiscountedDays / totalDays)) * (projectMegaPackage.AnualAmount + inventoryMegaPackage.AnualAmount);

            Assert.IsTrue(decimal.Round(testTotalAmount, 2) == discountedAmount);

        }

        [TestMethod]
        public void CalculateALLPackageAmountsByPeriodTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);
            List<ProjectPaymentPackageDetails> projectPackageDetails = Utils.GetSystemProjectPackageDetails();
            ProjectPaymentPackageDetails projectMegaPackage = projectPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            List<InventoryPaymentPackageDetails> inventoryPackageDetails = Utils.GetSystemInventoryPackageDetails();
            InventoryPaymentPackageDetails inventoryMegaPackage = inventoryPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            // Create a company
            DiscountCodeUsage discountCodeUsage = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, null);
            DataContext.SaveChanges();

            int monthlyPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
            int annualPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");

            decimal totalDue = financeBL.CalculateALLPackageAmountsByPeriod(projectMegaPackage.PackageTypeId, inventoryMegaPackage.PackageTypeId, monthlyPaymentDurationCodeId);
            Assert.IsTrue((projectMegaPackage.Amount + inventoryMegaPackage.Amount) == totalDue);

            totalDue = financeBL.CalculateALLPackageAmountsByPeriod(projectMegaPackage.PackageTypeId, inventoryMegaPackage.PackageTypeId, annualPaymentDurationCodeId);
            Assert.IsTrue((projectMegaPackage.AnualAmount + inventoryMegaPackage.AnualAmount) == totalDue);
        }

        [TestMethod]
        public void CalculatethePackageAmountByPeriodTest()
        {
            FinanceBL financeBL = new FinanceBL(DataContext);

            // Get mega project package
            List<ProjectPaymentPackageDetails> projectPackageDetails = Utils.GetSystemProjectPackageDetails();
            ProjectPaymentPackageDetails projectMegaPackage = projectPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            // Get mega inventory package
            List<InventoryPaymentPackageDetails> inventoryPackageDetails = Utils.GetSystemInventoryPackageDetails();
            InventoryPaymentPackageDetails inventoryMegaPackage = inventoryPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

            // get discount code usage
            DiscountCodeUsage discountCodeUsage = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, null);
            DataContext.SaveChanges();

            // get code ids
            int monthlyPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
            int annualPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
            int projectPackageTypeId = Utils.GetCodeIdByCodeValue("PaymentPackageType", "PROJECT");
            int inventoryPackageTypeId = Utils.GetCodeIdByCodeValue("PaymentPackageType", "INVENTORY");

            // validate
            decimal? amount = financeBL.CalculatethePackageAmountByPeriod(inventoryPackageTypeId, inventoryMegaPackage.PackageTypeId, monthlyPaymentDurationCodeId);
            Assert.IsTrue(inventoryMegaPackage.Amount == amount);
            amount = financeBL.CalculatethePackageAmountByPeriod(inventoryPackageTypeId, inventoryMegaPackage.PackageTypeId, annualPaymentDurationCodeId);
            Assert.IsTrue(inventoryMegaPackage.AnualAmount == amount);
            amount = financeBL.CalculatethePackageAmountByPeriod(projectPackageTypeId, projectMegaPackage.PackageTypeId, monthlyPaymentDurationCodeId);
            Assert.IsTrue(projectMegaPackage.Amount == amount);
            amount = financeBL.CalculatethePackageAmountByPeriod(projectPackageTypeId, projectMegaPackage.PackageTypeId, annualPaymentDurationCodeId);
            Assert.IsTrue(projectMegaPackage.AnualAmount == amount);
        }


        //[TestMethod]
        //public void SaveCompanyPackageTest()
        //{
        //    //SaveCompanyPackage(int userId, CompanyPaymentPackage oldCompanyPaymentPackage, PricePlanDetails pricePlanDetails, bool shouldCommit)
        //    FinanceBL financeBL = new FinanceBL(DataContext);

        //    // Get mega project package
        //    List<ProjectPaymentPackageDetails> projectPackageDetails = Utils.GetSystemProjectPackageDetails();
        //    ProjectPaymentPackageDetails projectMegaPackage = projectPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

        //    // Get mega inventory package
        //    List<InventoryPaymentPackageDetails> inventoryPackageDetails = Utils.GetSystemInventoryPackageDetails();
        //    InventoryPaymentPackageDetails inventoryMegaPackage = inventoryPackageDetails.Where(ppd => ppd.PackageName == "MEGA").FirstOrDefault();

        //    DiscountCodeUsage discountCodeUsage = new DiscountCodeUsageMock(DataContext).GetDiscountCodeUsage(true, null);

        //    decimal totalDue = financeBL.CalculateALLPackageAmountsByPeriod(projectMegaPackage.PackageTypeId, inventoryMegaPackage.PackageTypeId, monthlyPaymentDurationCodeId);
        //    decimal discountedAmount = financeBL.GetDiscountedAmount(discountCodeUsage.CompanyId, totalDue, monthlyPaymentDurationCodeId, discountCodeUsage, Utils.Today, Utils.Today.AddMonths(1));

        //    PricePlanDetails pricePlanDetails =  new PricePlanDetails
        //    {
        //        CompanyId = discountCodeUsage.CompanyId,
        //         DiscountCode = discountCodeUsage.DiscountCode,
        //         DiscountCodeUsage =  discountCodeUsage,
        //        InventoryPaymentPackageTypeId = inventoryMegaPackage.PackageTypeId,
        //        ProjectPaymentPackageTypeId = projectMegaPackage.PackageTypeId,
        //        TotalAmount = discountedAmount,
                
        //    };

        //}
    }
}