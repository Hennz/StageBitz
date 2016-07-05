using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Finance.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using StageBitz.Logic.Business.Project;

namespace StageBitz.Logic.Finance.Company
{
    public class CompanyStatusHandler
    {
        public enum CompanyWarningStatus
        {
            /// <summary>
            /// Company does not have any warnings.
            /// </summary>
            NoWarning,

            /// <summary>
            /// Free trail ended but No Payment Package configured
            /// </summary>
            FreeTrailEndNoPaymentPackage,

            /// <summary>
            /// Fully functional grace period after payment failure.
            /// </summary>
            PaymentFailedGracePeriod,

            /// <summary>
            /// Manual suspension by SB Admin.
            /// </summary>
            SBAdminSuspended,

            /// <summary>
            /// Payment failure
            /// </summary>
            PaymentFailed,

            /// <summary>
            /// The suspended for no payment options
            /// </summary>
            SuspendedForNoPaymentOptions
        }

        public class CompanyWarningInfo
        {
            public CompanyWarningStatus WarningStatus { get; set; }

            /// <summary>
            /// Days until the project is tranferred to the next expiration state. -1 if not applicable.
            /// </summary>
            public int DaysToExpiration { get; set; }

            public CompanyWarningInfo(CompanyWarningStatus warningStatus, int daysToExpiration)
            {
                WarningStatus = warningStatus;
                DaysToExpiration = daysToExpiration;
            }
        }

        /// <summary>
        /// Returns the warning status of the project based on it's current status and expiration details.
        /// </summary>
        public static CompanyWarningInfo GetCompanyWarningStatus(int companyId, int companyStatusCodeId, DateTime? expirationDate)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CompanyBL companyBL = new CompanyBL(dataContext);
                FinanceBL financeBL = new FinanceBL(dataContext);

                string statusCode = Utils.GetCodeByCodeId(companyStatusCodeId).Value;
                double remainingDays = (statusCode == "ACTIVE" || expirationDate == null) ? -1 : (expirationDate.Value.Date - Utils.Today).TotalDays;

                CompanyWarningStatus warningStatus = CompanyWarningStatus.NoWarning;

                if (companyBL.HasCompanySuspendedbySBAdmin(companyId))
                {
                    warningStatus = CompanyWarningStatus.SBAdminSuspended;
                }
                else if (statusCode == "GRACEPERIOD" && ProjectFinanceHandler.IsPaymentFailedInvoicesExistForCompany(companyId))
                {
                    warningStatus = CompanyWarningStatus.PaymentFailedGracePeriod;
                }
                else if (statusCode == "SUSPENDEDFORPAYMENTFAILED")
                {
                    warningStatus = CompanyWarningStatus.PaymentFailed;
                }
                else if (!financeBL.HasPackageSelectedForFreeTrailEndedCompany(companyId))
                {
                    warningStatus = CompanyWarningStatus.FreeTrailEndNoPaymentPackage;
                }
                else if (statusCode == "SUSPENDEDFORNOPAYMENTOPTIONS")
                {
                    warningStatus = CompanyWarningStatus.SuspendedForNoPaymentOptions;
                }

                return new CompanyWarningInfo(warningStatus, (int)Math.Round(remainingDays, 0));
            }
        }

        /// <summary>
        /// To be called by the daily agent
        /// </summary>
        public static void UpdateCompanyExpirations(DateTime dateToConsider, StageBitzDB dataContext)
        {
            FinanceBL financeBL = new FinanceBL(dataContext);
            CompanyBL companyBL = new CompanyBL(dataContext);
            ProjectBL projectBL = new ProjectBL(dataContext);

            #region SuspendPaymentFailureCompanies

            int companyGracePeriodStatus = Utils.GetCodeIdByCodeValue("CompanyStatus", "GRACEPERIOD");
            int companyPaymentFailedStatus = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORPAYMENTFAILED");
            int companyActiveStatus = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
            int suspendedForNoPaymentPackageStatus = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTPACKAGE");
            int suspendForNoPaymentOptions = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTOPTIONS");

            var companies = from c in dataContext.Companies
                            where (c.CompanyStatusCodeId == companyGracePeriodStatus ||
                                c.CompanyStatusCodeId == companyActiveStatus) &&
                                c.ExpirationDate != null &&
                                dateToConsider >= c.ExpirationDate
                            select c;

            foreach (Data.Company company in companies)
            {
                if (company.CompanyStatusCodeId == companyActiveStatus)
                {
                    //Get the current Company package to check.
                    CompanyPaymentPackage companyPaymentPackage = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(company.CompanyId, dateToConsider);
                    DiscountCodeUsage discountCodeUsage = financeBL.GetDiscountCodeUsageByDate(dateToConsider, company.CompanyId);

                    // suspend payment package not setup companies after free trial.
                    if (companyPaymentPackage == null)
                    {
                        company.CompanyStatusCodeId = suspendedForNoPaymentPackageStatus;
                    }
                    else
                    {

                        decimal totalAmount = financeBL.CalculateALLPackageAmountsByPeriod(companyPaymentPackage.ProjectPaymentPackageTypeId, companyPaymentPackage.InventoryPaymentPackageTypeId, companyPaymentPackage.PaymentDurationCodeId);

                        //Check if it is a Free package.
                        if (!companyPaymentPackage.PaymentMethodCodeId.HasValue && ((discountCodeUsage != null && discountCodeUsage.DiscountCode.Discount != 100) || (discountCodeUsage == null && totalAmount != 0)))
                        {
                            company.CompanyStatusCodeId = suspendForNoPaymentOptions;
                            SuspendProjectsForCompany(company.CompanyId, dataContext);
                        }
                    }
                }
                //For Grace period companies, if it exceeded the grace period change it to Payment Failed.
                else if (companyBL.IsCompanyInPaymentFailedGracePeriod(company.CompanyId))
                {
                    company.CompanyStatusCodeId = companyPaymentFailedStatus;
                    company.LastUpdatedByUserId = 0;
                    company.LastUpdatedDate = Utils.Now;
                }

                company.ExpirationDate = null;
            }
            #endregion
        }

        /// <summary>
        /// Suspends the no payment option companies.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        public static void SuspendNoPaymentOptionCompanies(DateTime dateToConsider)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                FinanceBL financeBL = new FinanceBL(dataContext);
                CompanyBL companyBL = new CompanyBL(dataContext);

                //Change the status to SuspendForNoPaymentOption if there are No option being selected.
                //Get all the No Payment option companies and check if they have to pay a certain amount before a certain period.
                //Send them an email, If they have a due amount to pay 2 weeks ahead (PBI 11444).
                int companyActiveStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");

                List<CompanyPaymentPackage> companyPaymentPackages = (from cpp in dataContext.CompanyPaymentPackages
                                                                      where cpp.Company.CompanyStatusCodeId == companyActiveStatusCodeId
                                                                      && cpp.PaymentMethodCodeId == null && cpp.StartDate <= dateToConsider
                                                                      && (cpp.EndDate > dateToConsider || cpp.EndDate == null)
                                                                      select cpp).Distinct().ToList();

                foreach (CompanyPaymentPackage cpp in companyPaymentPackages)
                {
                    int companyId = cpp.CompanyId;
                    decimal totalDue = financeBL.CalculateALLPackageAmountsByPeriod(cpp.ProjectPaymentPackageTypeId, cpp.InventoryPaymentPackageTypeId, cpp.PaymentDurationCodeId);
                    if (totalDue > 0)
                    {
                        DiscountCodeUsage currentdiscountCodeUsage = financeBL.GetDiscountCodeUsageByDate(dateToConsider, companyId);
                        //Get the current DiscountCodeUsage. If the discount is 100%, check whether use has been notified. If not check for 14 days ahead and record if not.
                        if (currentdiscountCodeUsage != null && currentdiscountCodeUsage.DiscountCode.Discount == 100)
                        {
                            CompanyDiscountNotificatonHistory companyDiscountNotificatonHistory = companyBL.GetCompanyDiscountExpireNotifiedRecord(companyId, dataContext);
                            bool hasNotifiedUser = (companyDiscountNotificatonHistory != null);

                            if (!hasNotifiedUser)
                            {
                                //Get the DiscountCode Usage and check whether it has a 100% code
                                DiscountCodeUsage discountCodeUsage = financeBL.GetDiscountCodeUsageByDate(dateToConsider.AddDays(14), companyId);

                                //If there is no 100% discount
                                if (discountCodeUsage == null || discountCodeUsage != null && discountCodeUsage.DiscountCode.Discount != 100)
                                {
                                    //1. Notify via email

                                    //2. Log in the table
                                    CompanyDiscountNotificatonHistory companyDiscountNotificatonHistories = new CompanyDiscountNotificatonHistory()
                                    {
                                        CompanyId = companyId,
                                        Date = Utils.Today,
                                        IsActive = true,
                                        CreatedDate = Utils.Today,
                                        CreatedByUserId = 0,
                                        LastUpdatedDate = Utils.Today,
                                        LastUpdatedByUserId = 0
                                    };

                                    dataContext.CompanyDiscountNotificatonHistories.AddObject(companyDiscountNotificatonHistories);
                                }
                            }
                        }
                        else if (currentdiscountCodeUsage == null || currentdiscountCodeUsage != null && currentdiscountCodeUsage.DiscountCode.Discount != 100)
                        {
                            //Means We have to suspend the company
                            SuspendProjectsForCompany(companyId, dataContext);
                        }
                    }
                }

                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Suspends the projects for company.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="dataContext">The data context.</param>
        public static void SuspendProjectsForCompany(int companyId, StageBitzDB dataContext)
        {
            int projectSuspendCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "SUSPENDED");
            CompanyBL companyBL = new CompanyBL(dataContext);
            ProjectBL projectBL = new ProjectBL(dataContext);
            Data.Company company = companyBL.GetCompany(companyId);
            company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTOPTIONS");

            //Suspend all active projects
            List<Data.Project> projects = projectBL.GetAllActiveProjects(companyId);
            foreach (Data.Project project in projects)
            {
                project.ProjectStatusCodeId = projectSuspendCodeId;
                project.LastUpdatedByUserId = 0;
                project.LastUpdatedDate = Utils.Today;
            }
        }
    }
}