using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Logic.Business.Finance
{
    /// <summary>
    /// Business layer for finance related operations
    /// </summary>
    public class FinanceBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinanceBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public FinanceBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the company project details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="projectStatusList">The project status list.</param>
        /// <returns></returns>
        public List<CompanyProjectDetails> GetCompanyProjectDetails(int companyId, List<int> projectStatusList)
        {
            List<CompanyProjectDetails> companyDetails =
                (from p in DataContext.Projects
                 where p.CompanyId == companyId
                 join code in DataContext.Codes on p.ProjectStatusCodeId equals code.CodeId
                 where projectStatusList.Contains(p.ProjectStatusCodeId)
                 select new CompanyProjectDetails
                 {
                     ProjectId = p.ProjectId,
                     ProjectName = p.ProjectName,
                     ProjectStatusCodeId = p.ProjectStatusCodeId,
                     ProjectStatus = code.Description,
                     ExpirationDate = (DateTime?)p.ExpirationDate,
                     SortOrder = code.SortOrder,
                     //Get the all the receipts that was paid last and group them to get the last payment amount.
                     LastPayment =
                                 (from receipt in
                                      (from inv in DataContext.Invoices
                                       join r in DataContext.Receipts on inv.TransactionID equals r.ReceiptForTransactionID into receiptJoin
                                       from receipt in receiptJoin.DefaultIfEmpty()
                                       where inv.RelatedTableName == "Project" && inv.RelatedID == p.ProjectId
                                       select new { inv.Amount, receipt.ReceiptDate })
                                  group receipt by receipt.ReceiptDate into g
                                  orderby g.Key descending
                                  select new LastPaymentDetails
                                  {
                                      PaymentDate = (DateTime?)g.Key,
                                      Amount = g.Sum(receipt => receipt.Amount)
                                  }).FirstOrDefault()
                 }).ToList<CompanyProjectDetails>();

            return companyDetails;
        }

        /// <summary>
        /// Gets the latest discount code usage.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public DiscountCodeUsage GetLatestDiscountCodeUsage(int companyId)
        {
            return (from dcu in DataContext.DiscountCodeUsages
                    where dcu.EndDate >= Utils.Today
                    && dcu.CompanyId == companyId
                    && dcu.IsActive
                    select dcu).OrderByDescending(dcu => dcu.DiscountCodeUsageId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the discount code.
        /// </summary>
        /// <param name="discountCodeText">The discount code text.</param>
        /// <returns></returns>
        public DiscountCode GetDiscountCode(string discountCodeText)
        {
            return DataContext.DiscountCodes.Where(dc => dc.Code == discountCodeText).FirstOrDefault();
        }

        /// <summary>
        /// Gets the discount code usages.
        /// </summary>
        /// <param name="discountCodeId">The discount code identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public List<DiscountCodeUsage> GetDiscountCodeUsages(int discountCodeId, bool includeInactive)
        {
            return DataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCodeId == discountCodeId && (includeInactive || dcu.IsActive)).ToList();
        }

        /// <summary>
        /// Adds the discount code usage by sb admin.
        /// </summary>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="userId">The user identifier.</param>
        public void AddDiscountCodeUsageBySBAdmin(DiscountCodeUsage discountCodeUsage, int userId)
        {
            //Get the current discountCode used by the company.
            Data.DiscountCodeUsage discountUsage = GetLatestDiscountCodeUsage(discountCodeUsage.CompanyId);

            if (discountUsage != null)
            {
                //Expire it by today
                discountUsage.EndDate = Utils.Today;
                discountUsage.LastUpdatedByUserId = userId;
                discountUsage.LastUpdatedDate = Utils.Now;
                discountUsage.IsActive = false;
            }

            DataContext.DiscountCodeUsages.AddObject(discountCodeUsage);
            CompanyBL companyBL = new CompanyBL(DataContext);
            companyBL.UpdateDiscountExpireNotifiedRecordForCompany(discountCodeUsage, companyBL.GetCompany(discountCodeUsage.CompanyId), userId, DataContext);
            ProjectUsageHandler.UpdatePaymentSummaryForFreeTrialCompanyBySBAdmin(discountCodeUsage.CompanyId, discountCodeUsage, null, userId, DataContext);
            base.SaveChanges();
        }

        /// <summary>
        /// Calculates the partial discounted amounts.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="daysFordurationType">Type of the days forduration.</param>
        /// <param name="discountedDays">The discounted days.</param>
        /// <param name="discount">The discount.</param>
        /// <returns></returns>
        private decimal CalculatePartialDiscountedAmounts(decimal amount, int daysFordurationType, int discountedDays, decimal discount)
        {
            decimal amountPerDurationType = amount / daysFordurationType;
            decimal totalDiscountedAmount = Convert.ToInt32(discountedDays) * amountPerDurationType * (100 - discount) / 100;
            int totalDaysWithoutDiscount = Math.Abs(daysFordurationType - Convert.ToInt32(discountedDays));
            decimal totalAmountWithoutDiscount = totalDaysWithoutDiscount * amountPerDurationType;
            return decimal.Round((totalAmountWithoutDiscount + totalDiscountedAmount), 2);
        }

        /// <summary>
        /// Gets the discounted amount.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="paymentDurationTypeCodeId">The payment duration type code identifier.</param>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public decimal GetDiscountedAmount(int companyId, decimal amount, int paymentDurationTypeCodeId, DiscountCodeUsage discountCodeUsage, DateTime startDate, DateTime endDate)
        {
            // To find the discount duration(Because we need to get it by days thought it is in DB
            // Check whether the Discount duration falls outside the package duration.(this can be a discount code applied by SB Admin, when the user applies it
            // it should become Inactive
            //In order to calculate pro rata amount, to calculate the amount with old discount, we should know upto which date it is applied. Since we have end dated the old discount code we calculate the
            //virtual end date based on the duration
            DiscountCode discount = discountCodeUsage.DiscountCode;
            DateTime virtualdiscountEndDate = discountCodeUsage.StartDate.Value.AddDays(discount.Duration * 7);
            if (discountCodeUsage == null || startDate > virtualdiscountEndDate)
                return amount;

            int discountDuration = GetDiscountedDuration(startDate, endDate, discountCodeUsage);

            // If the discount usage is less than the period
            if ((endDate - startDate).TotalDays > discountDuration)
            {
                return CalculatePartialDiscountedAmounts(amount, (int)(endDate - startDate).TotalDays, Convert.ToInt32(discountDuration), discountCodeUsage.DiscountCode.Discount);
            }
            return (amount * (100 - discountCodeUsage.DiscountCode.Discount)) / 100;
        }

        /// <summary>
        /// Gets the duration of the discounted.
        /// </summary>
        /// <param name="packageStartDate">The package start date.</param>
        /// <param name="packageEndDate">The package end date.</param>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <returns></returns>
        private int GetDiscountedDuration(DateTime packageStartDate, DateTime packageEndDate, DiscountCodeUsage discountCodeUsage)
        {
            //In order to calculate pro rata amount, to calculate the amount with old discount, we should know upto which date it is applied. Since we have end dated the old discount code we calculate the
            //virtual end date based on the duration
            DiscountCode discount = discountCodeUsage.DiscountCode;
            DateTime virtualdiscountEndDate = discountCodeUsage.StartDate.Value.AddDays(discount.Duration * 7);

            TimeSpan spanDiscountedDuration = virtualdiscountEndDate - discountCodeUsage.StartDate.Value;
            int discountDuration = Convert.ToInt32(spanDiscountedDuration.TotalDays);

            //Days to reduce from Start Date
            int startDiff = Convert.ToInt32(((packageStartDate - discountCodeUsage.StartDate.Value).TotalDays));
            int endDiff = Convert.ToInt32(((virtualdiscountEndDate - packageEndDate).TotalDays));

            int totaldaysToreduce = 0;
            if (startDiff > 0)
                totaldaysToreduce = startDiff;

            if (endDiff > 0)
                totaldaysToreduce += endDiff;

            //Now reduce those from total discount duration
            if (discountDuration >= totaldaysToreduce)
                discountDuration -= totaldaysToreduce;
            return discountDuration;
        }

        /// <summary>
        /// Calculates all package amounts by period.
        /// </summary>
        /// <param name="projectPaymentPackageDetailId">The project payment package detail identifier.</param>
        /// <param name="inventoryPackageDetailId">The inventory package detail identifier.</param>
        /// <param name="periodTypeCodeId">The period type code identifier.</param>
        /// <returns></returns>
        public decimal CalculateALLPackageAmountsByPeriod(int projectPaymentPackageDetailId, int inventoryPackageDetailId, int periodTypeCodeId)
        {
            //Get the Total for each Package (Monthly)
            InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(inventoryPackageDetailId);
            ProjectPaymentPackageDetails projectPaymentPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(projectPaymentPackageDetailId);

            decimal total = 0;

            //if the time frame is Anual, apply anual discount.
            if (periodTypeCodeId == Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId)
            {
                total = (inventoryPaymentPackageDetails == null ? 0 : inventoryPaymentPackageDetails.AnualAmount) + (projectPaymentPackageDetails == null ? 0 : projectPaymentPackageDetails.AnualAmount);
            }
            else
            {
                //Calculate the total
                total = (inventoryPaymentPackageDetails == null ? 0 : inventoryPaymentPackageDetails.Amount) + (projectPaymentPackageDetails == null ? 0 : projectPaymentPackageDetails.Amount);
            }
            return total;
        }

        /// <summary>
        /// Calculatethes the package amount by period.
        /// </summary>
        /// <param name="packageTypeCodeId">The package type code identifier.</param>
        /// <param name="paymentPackageTypeId">The payment package type identifier.</param>
        /// <param name="periodTypeCodeId">The period type code identifier.</param>
        /// <returns></returns>
        public decimal? CalculatethePackageAmountByPeriod(int packageTypeCodeId, int paymentPackageTypeId, int periodTypeCodeId)
        {
            decimal total = 0;
            if (periodTypeCodeId == Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId)
            {
                if (Utils.GetCodeByValue("PaymentPackageType", "PROJECT").CodeId == packageTypeCodeId)
                {
                    total = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(paymentPackageTypeId).AnualAmount;
                }
                else
                {
                    total = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(paymentPackageTypeId).AnualAmount;
                }
            }
            else
            {
                if (Utils.GetCodeByValue("PaymentPackageType", "PROJECT").CodeId == packageTypeCodeId)
                {
                    total = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(paymentPackageTypeId).Amount;
                }
                else
                {
                    total = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(paymentPackageTypeId).Amount;
                }
            }
            return total;
        }

        /// <summary>
        /// Gets the current payment package for company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetCurrentPaymentPackageForCompany(int companyId)
        {
            return GetPaymentPackageForCompanyByDay(Utils.Today, companyId);
        }

        /// <summary>
        /// Gets the payment package for company by day.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetPaymentPackageForCompanyByDay(DateTime dateToConsider, int companyId)
        {
            //Find the correct package for the particular date
            return (from cpp in DataContext.CompanyPaymentPackages
                    where cpp.StartDate <= dateToConsider && (cpp.EndDate > dateToConsider || cpp.EndDate == null) && cpp.CompanyId == companyId
                    select cpp).FirstOrDefault();
        }

        /// <summary>
        /// Gets the current payment package forthe company including free trial.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(int companyId)
        {
            return GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId, Utils.Today);
        }

        /// <summary>
        /// Gets the current payment package forthe company including free trial.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(int companyId, DateTime dateToConsider)
        {
            // if a free trial company, get future package
            CompanyBL companyBL = new CompanyBL(DataContext);
            bool isfreeTrail = companyBL.IsFreeTrialCompany(companyId, dateToConsider);
            bool isFreeTrailEnded = companyBL.IsFreeTrialEndedCompany(companyId);

            var package = (from cpp in DataContext.CompanyPaymentPackages
                           where (cpp.EndDate == null || cpp.EndDate > dateToConsider)
                    && cpp.CompanyId == companyId
                           && (isfreeTrail || cpp.StartDate <= dateToConsider)
                           select cpp).FirstOrDefault();

            // if free trial ended company and there is no current package, get future package
            if (package == null && isFreeTrailEnded)
            {
                package = (from cpp in DataContext.CompanyPaymentPackages
                           where (cpp.EndDate == null || cpp.EndDate > dateToConsider)
                           && cpp.CompanyId == companyId
                           select cpp).FirstOrDefault();
            }

            return package;
        }

        /// <summary>
        /// Gets the current payment package forthe company including free trial.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="lastUpdatedDate">The last updated date.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetCurrentPaymentPackageFortheCompanyIncludingFreeTrialForConcurrency(int companyId, DateTime lastUpdatedDate)
        {
            // if a free trial company, get future package
            CompanyBL companyBL = new CompanyBL(DataContext);
            bool isfreeTrail = companyBL.IsFreeTrialCompany(companyId);
            bool isFreeTrailEnded = companyBL.IsFreeTrialEndedCompany(companyId);

            var package = (from cpp in DataContext.CompanyPaymentPackages
                           where (cpp.EndDate > Utils.Today || cpp.EndDate == null)
                           && cpp.CompanyId == companyId
                           && (isfreeTrail || cpp.StartDate <= Utils.Today) && cpp.LastUpdateDate == lastUpdatedDate
                           select cpp).FirstOrDefault();

            // if free trial ended company and there is no current package, get future package
            if (package == null && isFreeTrailEnded)
            {
                package = (from cpp in DataContext.CompanyPaymentPackages
                           where (cpp.EndDate > Utils.Today || cpp.EndDate == null)
                           && cpp.CompanyId == companyId
                           && cpp.LastUpdateDate == lastUpdatedDate
                           select cpp).FirstOrDefault();
            }
            return package;
        }

        /// <summary>
        /// Gets the latest request for the company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyPaymentPackage GetLatestRequestForTheCompany(int companyId)
        {
            return (from cpp in DataContext.CompanyPaymentPackages
                    where cpp.StartDate > Utils.Today && cpp.EndDate == null && cpp.CompanyId == companyId
                    select cpp).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether company has reached inventory limit.
        /// (change this method to handle company free trial scenario)
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool HasCompanyReachedInventoryLimit(int companyId)
        {
            int currentInventoryItemCount = DataContext.Items.Where(i => i.IsActive && !i.IsHidden && i.CompanyId == companyId).Count();
            CompanyPaymentPackage currentPackage = GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);
            ProjectBL projectBL = new ProjectBL(DataContext);
            int freeTrialInventoryCount = int.Parse(Utils.GetSystemValue("DefaultInventoryCountForCompany"));
            if (currentPackage != null) //if the company has already selected a package
            {
                InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(currentPackage.InventoryPaymentPackageTypeId);
                if (inventoryPaymentPackageDetails.ItemCount > currentInventoryItemCount)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId).Count() > 0 &&
                freeTrialInventoryCount > currentInventoryItemCount) //check whether the company is in free trial, it decide by the free trial projects at the moment (this might change)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Updates the company payment package.
        /// </summary>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void UpdateCompanyPaymentPackage(bool commit)
        {
            if (commit)
            {
                base.SaveChanges();
            }
        }

        /// <summary>
        /// Adds the company payment package.
        /// </summary>
        /// <param name="companyPaymentPackage">The company payment package.</param>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void AddCompanyPaymentPackage(Data.CompanyPaymentPackage companyPaymentPackage, bool commit)
        {
            DataContext.CompanyPaymentPackages.AddObject(companyPaymentPackage);
            if (commit)
            {
                base.SaveChanges();
            }
        }

        /// <summary>
        /// Creates the payment summary record.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="hasPackageChanged">if set to <c>true</c> [has package changed].</param>
        private void CreatePaymentSummaryRecord(PricePlanDetails pricePlanDetails, int userId, bool hasPackageChanged)
        {
            //If the Payment duration is Anual, create an Anual package Summary.
            CompanyBL companyBL = new CompanyBL(DataContext);

            PaymentSummaryDetails paymentSummaryDetails = new PaymentSummaryDetails()
            {
                CompanyId = pricePlanDetails.CompanyId,
                ShouldProcess = true,
                DiscountCodeUsageToApply = pricePlanDetails.DiscountCodeUsage,
                InventoryPaymentPackageTypeId = pricePlanDetails.InventoryPaymentPackageTypeId,
                IsEducationPackage = pricePlanDetails.IsEducationalPackage,
                CompanyPaymentPackage = pricePlanDetails.CompanyPaymentPackage,
                HasPackageChanged = hasPackageChanged,
                ProjectPaymentPackageTypeId = pricePlanDetails.ProjectPaymentPackageTypeId,
                PaymentMethodCodeId = pricePlanDetails.PaymentMethodCodeId,
                UserId = userId,
                IsUserAction = true,
                PaymentDurationTypeCodeId = pricePlanDetails.PaymentDurationCodeId,
                PackageStartDate = pricePlanDetails.PackageStartDate
            };

            ProjectUsageHandler.CreateCompanyPaymentSummaries(paymentSummaryDetails, Utils.Today, DataContext);
        }

        /// <summary>
        /// Gets the payment summary to should process.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CompanyPaymentSummary GetPaymentSummaryToShouldProcess(int companyId)
        {
            return (from cps in DataContext.CompanyPaymentSummaries
                    where cps.IsMonthlyAgentProcessed == false && cps.ShouldProcess == false && cps.CompanyId == companyId
             && (cps.FromDate <= Utils.Today && cps.ToDate >= Utils.Today)
                    select cps).FirstOrDefault();
        }

        /// <summary>
        /// Updates the due-payment summaries to process.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        public void UpdateDuePaymentSummariesToProcess(int companyId)
        {
            //this will update the existing payment summaries to processed. Because they will be added to the invoice option.
            List<CompanyPaymentSummary> unProcessedPaymentSummaries = ProjectFinanceHandler.GetUnProcessedCompanyPaymentSummaries(companyId, DataContext, Utils.Today, true);
            foreach (CompanyPaymentSummary companyPaymentSummary in unProcessedPaymentSummaries)
            {
                companyPaymentSummary.IsMonthlyAgentProcessed = true;
                companyPaymentSummary.PaymentMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE");
            }
        }

        /// <summary>
        /// check whether user has changed his configurtion. If so and current package is monthly based then create a new package and set end date of current date to today.If package
        /// changed and old package is annual, then create a request by setting start date to current package's end date and end date as null. Only one request can exist.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldCompanyPaymentPackage"></param>
        /// <param name="pricePlanDetails"></param>
        /// <param name="shouldCommit"></param>
        /// <returns></returns>
        public void SaveCompanyPackage(int userId, CompanyPaymentPackage oldCompanyPaymentPackage, PricePlanDetails pricePlanDetails, bool shouldCommit)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);

            // Set no pricing plan and no payment method suspended company status to active.
            if (pricePlanDetails.CompanyId > 0)
            {
                int companySuspendedForNoPaymentPackageCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTPACKAGE");
                int companySuspendedForNoPaymentOptionsCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTOPTIONS");

                int companyActiveCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");

                Data.Company company = companyBL.GetCompany(pricePlanDetails.CompanyId);

                //if the company is suspended for no payment options and when they give the payment option(CC/Invoice) update the payment summaries apply for this date
                if (company != null && company.CompanyStatusCodeId == companySuspendedForNoPaymentOptionsCodeId && pricePlanDetails.PaymentMethodCodeId == Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE"))
                {
                    CompanyPaymentSummary companyPaymentSummary = GetPaymentSummaryToShouldProcess(company.CompanyId);
                    if (companyPaymentSummary != null)
                    {
                        companyPaymentSummary.ShouldProcess = true;
                        companyPaymentSummary.IsMonthlyAgentProcessed = true;
                    }
                }

                if (company != null &&
                        (company.CompanyStatusCodeId == companySuspendedForNoPaymentPackageCodeId || company.CompanyStatusCodeId == companySuspendedForNoPaymentOptionsCodeId))
                {
                    company.CompanyStatusCodeId = companyActiveCodeId;
                }
                companyBL.UpdateDiscountExpireNotifiedRecordForCompany(pricePlanDetails.DiscountCodeUsage, company, userId, DataContext);
            }

            // Specifically check for free trial company
            if (pricePlanDetails.CompanyId > 0 && companyBL.IsFreeTrialStatusIncludedFortheDay(pricePlanDetails.CompanyId, Utils.Today))
            {
                bool hasFutureRequest = false;
                CompanyPaymentPackage futurePackage = GetLatestRequestForTheCompany(pricePlanDetails.CompanyId);
                if (futurePackage != null)
                {
                    hasFutureRequest = true;
                }
                else
                {
                    futurePackage = new CompanyPaymentPackage();
                }

                SetPricingPlanDetailsToFuturePackage(futurePackage, oldCompanyPaymentPackage, pricePlanDetails, userId);

                if (!hasFutureRequest)
                {
                    // Set package start date as free trial project expiration date.
                    ProjectBL projectBL = new ProjectBL(DataContext);
                    var freeTrialProjects = projectBL.GetFreeTrialProjectsNotInClosedStatus(pricePlanDetails.CompanyId);
                    Data.Project freeTrialProject = freeTrialProjects.FirstOrDefault();
                    if (freeTrialProject != null && freeTrialProject.ExpirationDate.HasValue)
                    {
                        futurePackage.StartDate = freeTrialProject.ExpirationDate.Value.AddDays(1);
                    }

                    // Save free trial project type as free trial optin
                    int freeTrialOptinTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;
                    int freeTrialTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;
                    foreach (Data.Project project in freeTrialProjects)
                    {
                        if (project != null && project.ProjectTypeCodeId == freeTrialTypeCodeId)
                        {
                            project.ProjectTypeCodeId = freeTrialOptinTypeCodeId;
                            project.LastUpdatedByUserId = userId;
                            project.LastUpdatedDate = Utils.Now;
                        }
                    }
                    pricePlanDetails.PackageStartDate = futurePackage.StartDate;
                    pricePlanDetails.CompanyPaymentPackage = futurePackage;
                    CreatePaymentSummaryRecord(pricePlanDetails, userId, true);
                    AddCompanyPaymentPackage(futurePackage, shouldCommit);
                }
                else
                {
                    pricePlanDetails.PackageStartDate = futurePackage.StartDate;
                    pricePlanDetails.CompanyPaymentPackage = futurePackage;
                    CreatePaymentSummaryRecord(pricePlanDetails, userId, true);
                    if (shouldCommit)
                    {
                        this.SaveChanges();
                    }
                }
            }
            else if (pricePlanDetails.CompanyId == 0 || oldCompanyPaymentPackage == null)  //Configure the payment package for the first time
            {
                StageBitz.Data.CompanyPaymentPackage companyPaymentPackage = new Data.CompanyPaymentPackage();
                companyPaymentPackage.InventoryPaymentPackageTypeId = pricePlanDetails.InventoryPaymentPackageTypeId;
                companyPaymentPackage.ProjectPaymentPackageTypeId = pricePlanDetails.ProjectPaymentPackageTypeId;
                companyPaymentPackage.CompanyId = pricePlanDetails.CompanyId;
                companyPaymentPackage.IsEducationalPackage = pricePlanDetails.IsEducationalPackage;
                companyPaymentPackage.EducationalPosition = pricePlanDetails.Position;
                companyPaymentPackage.PaymentDurationCodeId = pricePlanDetails.PaymentDurationCodeId;
                companyPaymentPackage.PaymentMethodCodeId = pricePlanDetails.PaymentMethodCodeId;
                companyPaymentPackage.LastUpdateDate = companyPaymentPackage.CreatedDate = Utils.Now;
                companyPaymentPackage.CreatedBy = companyPaymentPackage.LastUpdatedBy = userId;
                companyPaymentPackage.StartDate = pricePlanDetails.PackageStartDate = Utils.Today;
                pricePlanDetails.CompanyPaymentPackage = companyPaymentPackage;
                CreatePaymentSummaryRecord(pricePlanDetails, userId, true);
                AddCompanyPaymentPackage(companyPaymentPackage, shouldCommit);
            }
            else
            {
                bool isSamePackage = (oldCompanyPaymentPackage.InventoryPaymentPackageTypeId == pricePlanDetails.InventoryPaymentPackageTypeId)
                                            && (oldCompanyPaymentPackage.ProjectPaymentPackageTypeId == pricePlanDetails.ProjectPaymentPackageTypeId);

                if (!isSamePackage)  //If package has been changed end date the current record and create a new record
                {
                    //CompanyPaymentPackage companyPaymentPackage = null;
                    CompanyPaymentPackage futurePackage = CreateAndUpdateFutureRecords(pricePlanDetails, oldCompanyPaymentPackage, userId, shouldCommit);
                    if (!IsPackageEndToday(oldCompanyPaymentPackage) || IsSummaryCreated(pricePlanDetails.CompanyId))
                    {
                        //this is when calculating the pro rata amount it should calculate for the current payment period, this will happen other than
                        //if the user do upgrade/downgrade for on the same package end date.
                        pricePlanDetails.PaymentDurationCodeId = oldCompanyPaymentPackage.PaymentDurationCodeId;
                        pricePlanDetails.CompanyPaymentPackage = CreateAndUpdatePackages(userId, oldCompanyPaymentPackage, pricePlanDetails, shouldCommit); ;
                    }
                    else
                    {
                        pricePlanDetails.CompanyPaymentPackage = futurePackage;
                        CreateAndUpdatePackages(userId, oldCompanyPaymentPackage, pricePlanDetails, shouldCommit);
                    }

                    CreatePaymentSummaryRecord(pricePlanDetails, userId, true);
                    EndOldCompanyPaymentPackage(oldCompanyPaymentPackage, userId);
                }
                else
                {
                    bool hasPaymentOptionsChanged = false;  //Keep track of whether payment option(CC or invoice) or payment duration (monthly, yearly) has been changed
                    if (pricePlanDetails.PaymentMethodCodeId != null)
                    {
                        hasPaymentOptionsChanged = oldCompanyPaymentPackage.PaymentDurationCodeId != pricePlanDetails.PaymentDurationCodeId
                            || oldCompanyPaymentPackage.PaymentMethodCodeId != pricePlanDetails.PaymentMethodCodeId;
                    }
                    else //Payment method is null for free inventry and 100% promotional discount
                    {
                        hasPaymentOptionsChanged = oldCompanyPaymentPackage.PaymentDurationCodeId != pricePlanDetails.PaymentDurationCodeId;
                    }

                    if (hasPaymentOptionsChanged)
                    {
                        //If it just an option change we should create a request. If there is already a request, we should update it.Existing record's option should be the latest one
                        if (pricePlanDetails.PaymentMethodCodeId != null)
                        {
                            oldCompanyPaymentPackage.PaymentMethodCodeId = pricePlanDetails.PaymentMethodCodeId;
                        }
                        CreatePaymentSummaryRecord(pricePlanDetails, userId, false);
                        CreateAndUpdateFutureRecords(pricePlanDetails, oldCompanyPaymentPackage, userId, shouldCommit);
                    }
                    else   // Payment options are same, but apply an educational discount. End date the existing record and create a new record.End date the future requests as well
                    {
                        if (pricePlanDetails.IsEducationalPackage != IsEducationalCompany(oldCompanyPaymentPackage))
                        {
                            //if it is just the education tickbox change we should create a future request.
                            CreatePaymentSummaryRecord(pricePlanDetails, userId, false);
                            CreateAndUpdateFutureRecords(pricePlanDetails, oldCompanyPaymentPackage, userId, shouldCommit);
                        }
                    }
                }
                if (pricePlanDetails.PaymentMethodCodeId != null && pricePlanDetails.PaymentMethodCodeId == Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE"))
                {
                    UpdateDuePaymentSummariesToProcess(pricePlanDetails.CompanyId);
                }
            }
        }

        /// <summary>
        /// Creates the and update future records.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="shouldCommit">if set to <c>true</c> [should commit].</param>
        /// <returns></returns>
        private CompanyPaymentPackage CreateAndUpdateFutureRecords(PricePlanDetails pricePlanDetails, CompanyPaymentPackage oldCompanyPaymentPackage, int userId, bool shouldCommit)
        {
            bool isNewlyCreated = false;
            CompanyPaymentPackage futurePackage = GetLatestRequestForTheCompany(pricePlanDetails.CompanyId);
            if (futurePackage == null)
            {
                isNewlyCreated = true;
                futurePackage = new CompanyPaymentPackage();
                futurePackage.CreatedDate = Utils.Today;
                futurePackage.CreatedBy = userId;
                int durationDifference = GetDurationDifference(oldCompanyPaymentPackage.StartDate, Utils.Today, oldCompanyPaymentPackage.PaymentDurationCodeId);
                int count = IsPackageEndTodayAndSummaryCreated(oldCompanyPaymentPackage, pricePlanDetails.CompanyId) ? 1 : 0;

                if (oldCompanyPaymentPackage.PaymentDurationCodeId == Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY"))
                {
                    futurePackage.StartDate = oldCompanyPaymentPackage.StartDate.AddMonths(durationDifference + count);
                    oldCompanyPaymentPackage.EndDate = oldCompanyPaymentPackage.StartDate.AddMonths(durationDifference + count);
                }
                else
                {
                    futurePackage.StartDate = oldCompanyPaymentPackage.StartDate.AddYears(durationDifference + count);
                    oldCompanyPaymentPackage.EndDate = oldCompanyPaymentPackage.StartDate.AddYears(durationDifference + count);
                }
            }
            SetPricingPlanDetailsToFuturePackage(futurePackage, oldCompanyPaymentPackage, pricePlanDetails, userId);

            if (isNewlyCreated)
            {
                AddCompanyPaymentPackage(futurePackage, shouldCommit);
            }
            else
            {
                if (shouldCommit)
                    this.SaveChanges();
            }
            return futurePackage;
        }

        /// <summary>
        /// Determines whether this package end today and summary created.
        /// </summary>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private bool IsPackageEndTodayAndSummaryCreated(CompanyPaymentPackage oldCompanyPaymentPackage, int companyId)
        {
            return IsPackageEndToday(oldCompanyPaymentPackage) && IsSummaryCreated(companyId);
        }

        /// <summary>
        /// Determines whether [is summary created] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private bool IsSummaryCreated(int companyId)
        {
            return DataContext.CompanyPaymentSummaries.Where(cps => cps.FromDate == Utils.Today && cps.CompanyId == companyId).Count() > 0;
        }

        /// <summary>
        /// Determines whether package ends today.
        /// </summary>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        /// <returns></returns>
        private bool IsPackageEndToday(CompanyPaymentPackage oldCompanyPaymentPackage)
        {
            int durationDifference = GetDurationDifference(oldCompanyPaymentPackage.StartDate, Utils.Today, oldCompanyPaymentPackage.PaymentDurationCodeId);
            DateTime endDate = oldCompanyPaymentPackage.PaymentDurationCodeId == Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY") ? oldCompanyPaymentPackage.StartDate.AddMonths(durationDifference) : oldCompanyPaymentPackage.StartDate.AddYears(durationDifference);
            return endDate == Utils.Today;
        }

        /// <summary>
        /// Sets the pricing plan details to future package object.
        /// </summary>
        /// <param name="futurePackage">The future package.</param>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="userId">The user id.</param>
        private void SetPricingPlanDetailsToFuturePackage(CompanyPaymentPackage futurePackage, CompanyPaymentPackage oldCompanyPaymentPackage, PricePlanDetails pricePlanDetails, int userId)
        {
            futurePackage.InventoryPaymentPackageTypeId = pricePlanDetails.InventoryPaymentPackageTypeId;
            futurePackage.ProjectPaymentPackageTypeId = pricePlanDetails.ProjectPaymentPackageTypeId;
            futurePackage.CompanyId = pricePlanDetails.CompanyId;
            futurePackage.IsEducationalPackage = pricePlanDetails.IsEducationalPackage;
            futurePackage.EducationalPosition = pricePlanDetails.Position;
            futurePackage.PaymentDurationCodeId = pricePlanDetails.PaymentDurationCodeId;
            futurePackage.PaymentMethodCodeId = pricePlanDetails.PaymentMethodCodeId.HasValue ? pricePlanDetails.PaymentMethodCodeId : (oldCompanyPaymentPackage != null ? oldCompanyPaymentPackage.PaymentMethodCodeId : null);
            futurePackage.LastUpdateDate = Utils.Today;
            futurePackage.CreatedBy = futurePackage.LastUpdatedBy = userId;
        }

        /// <summary>
        /// Creates the and update packages.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="shouldCommit">if set to <c>true</c> [should commit].</param>
        /// <returns></returns>
        private CompanyPaymentPackage CreateAndUpdatePackages(int userId, CompanyPaymentPackage oldCompanyPaymentPackage, PricePlanDetails pricePlanDetails, bool shouldCommit)
        {
            StageBitz.Data.CompanyPaymentPackage companyPaymentPackage = new Data.CompanyPaymentPackage();

            companyPaymentPackage.InventoryPaymentPackageTypeId = pricePlanDetails.InventoryPaymentPackageTypeId;
            companyPaymentPackage.ProjectPaymentPackageTypeId = pricePlanDetails.ProjectPaymentPackageTypeId;
            companyPaymentPackage.CompanyId = pricePlanDetails.CompanyId;
            companyPaymentPackage.IsEducationalPackage = pricePlanDetails.IsEducationalPackage;
            companyPaymentPackage.EducationalPosition = pricePlanDetails.Position;
            companyPaymentPackage.PaymentDurationCodeId = pricePlanDetails.PaymentDurationCodeId;
            companyPaymentPackage.PaymentMethodCodeId = pricePlanDetails.PaymentMethodCodeId.HasValue ? pricePlanDetails.PaymentMethodCodeId : oldCompanyPaymentPackage.PaymentMethodCodeId;
            companyPaymentPackage.LastUpdateDate = companyPaymentPackage.CreatedDate = Utils.Now;
            companyPaymentPackage.CreatedBy = companyPaymentPackage.LastUpdatedBy = userId;
            companyPaymentPackage.StartDate = pricePlanDetails.PackageStartDate = Utils.Today;

            CompanyPaymentPackage futurePackage = GetLatestRequestForTheCompany(pricePlanDetails.CompanyId);
            if (futurePackage != null)
            {
                //futurePackage.StartDate = Utils.Today;
                //futurePackage.EndDate = Utils.Today;
                companyPaymentPackage.EndDate = futurePackage.StartDate;
            }
            else
            {
                int durationDifference = GetDurationDifference(oldCompanyPaymentPackage.StartDate, Utils.Today, oldCompanyPaymentPackage.PaymentDurationCodeId);
                int count = IsPackageEndTodayAndSummaryCreated(oldCompanyPaymentPackage, pricePlanDetails.CompanyId) ? 1 : 0;

                if (oldCompanyPaymentPackage.PaymentDurationCodeId == Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY"))
                {
                    companyPaymentPackage.EndDate = oldCompanyPaymentPackage.StartDate.AddMonths(durationDifference + count);
                }
                else
                {
                    companyPaymentPackage.EndDate = oldCompanyPaymentPackage.StartDate.AddYears(durationDifference + count);
                }
            }
            AddCompanyPaymentPackage(companyPaymentPackage, shouldCommit);
            return companyPaymentPackage;
        }

        /// <summary>
        /// Ends the old company payment package.
        /// </summary>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        /// <param name="userId">The user identifier.</param>
        public void EndOldCompanyPaymentPackage(CompanyPaymentPackage oldCompanyPaymentPackage, int userId)
        {
            oldCompanyPaymentPackage.EndDate = Utils.Today;
            oldCompanyPaymentPackage.LastUpdateDate = Utils.Today;
            oldCompanyPaymentPackage.LastUpdatedBy = userId;
        }

        /// <summary>
        /// Gets the duration difference.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="paymentDurationCodeId">The payment duration code identifier.</param>
        /// <returns></returns>
        public int GetDurationDifference(DateTime startDate, DateTime endDate, int paymentDurationCodeId)
        {
            //this will find the duration gap when to end date the current package and start the future package
            int durationDifference = 0;
            if (paymentDurationCodeId == Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY"))
            {
                durationDifference = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
                durationDifference = (endDate.Day > startDate.Day) ? durationDifference + 1 : durationDifference;
            }
            else
            {
                durationDifference = (endDate.Year - startDate.Year);
                if (endDate.Month > startDate.Month)
                    durationDifference = durationDifference + 1;
                else if (endDate.Month == startDate.Month && endDate.Day > startDate.Day)
                    durationDifference = durationDifference + 1;
            }

            return durationDifference == 0 ? 1 : durationDifference;
        }

        /// <summary>
        /// Determines whether discount code changed for specified company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="discountCodeusage">The discount codeusage.</param>
        /// <returns></returns>
        public bool HasDiscountCodeChanged(int companyId, DiscountCodeUsage discountCodeusage)
        {
            DiscountCodeUsage currentUsage = GetLatestDiscountCodeUsage(companyId);
            if (currentUsage == null && discountCodeusage == null)
            {
                return false;
            }
            else if ((currentUsage == null && discountCodeusage != null) || (currentUsage != null && discountCodeusage == null))
            {
                return true;
            }

            return currentUsage.DiscountCodeId != discountCodeusage.DiscountCodeId;
        }

        /// <summary>
        /// Determines whether company payment package details changed.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <returns></returns>
        public bool HasCompanyPaymentPackageDetailsChanged(PricePlanDetails pricePlanDetails)
        {
            CompanyPaymentPackage companyPaymentPackage = GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(pricePlanDetails.CompanyId);
            bool isPaymentMethodChangd = pricePlanDetails.PaymentMethodCodeId == null ? false : pricePlanDetails.PaymentMethodCodeId != companyPaymentPackage.PaymentMethodCodeId;
            return (companyPaymentPackage.InventoryPaymentPackageTypeId != pricePlanDetails.InventoryPaymentPackageTypeId
                            || companyPaymentPackage.ProjectPaymentPackageTypeId != pricePlanDetails.ProjectPaymentPackageTypeId
                            || IsEducationalCompany(companyPaymentPackage) != pricePlanDetails.IsEducationalPackage
                            || companyPaymentPackage.PaymentDurationCodeId != pricePlanDetails.PaymentDurationCodeId
                            || isPaymentMethodChangd);
        }

        /// <summary>
        /// Determines whether company payment package changed.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <returns></returns>
        public bool IsCompanyPaymentPackageChanged(int companyId, PricePlanDetails pricePlanDetails)
        {
            if (companyId > 0)
            {
                var companyPaymentPackage = GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);

                if (companyPaymentPackage == null)
                {
                    return true;
                }
                else
                {
                    return (HasCompanyPaymentPackageDetailsChanged(pricePlanDetails) || HasDiscountCodeChanged(companyId, pricePlanDetails.DiscountCodeUsage));//if there is a package level change or discount code change auth and terms and conditions should be visible
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the payment final agreement text.
        /// </summary>
        /// <param name="planDetails">The plan details.</param>
        /// <param name="paymentmethodCodeId">The paymentmethod code id.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The Agreement Text.</returns>
        public string GetPaymentFinalAgreementText(PricePlanDetails planDetails, int paymentmethodCodeId, string culture)
        {
            //If you change these strings make sure you do the necessary changes in GetFormattedString method in PaymentDetails.ascx.cs class as well
            string freeTrialText = "<li>I will not be charged anything for the remaining {0} days of my Free Trial and will have full access to the options I selected on the previous page.</li>";
            string startText = "<b>I understand that:</b><ul>";//amount  ,date
            string ccMonthlyNextPaymentString = "<li>My credit card will be charged {0} on {1}.</li>";
            string ccMonthlyContinualString = "<li>With continuing payments of {0} each month.</li>";//amount    ,duration    ,date
            string ccMonthlyContinualStringWithDiscount = string.Concat(ccMonthlyContinualString, "<li>Until the promotional code expires on {1} and then I will be charged {2} each month.</li>");
            string ccYearlyNextPaymentString = "<li>My credit card will be charged {0} for the period of {1} to {2}.</li>";
            string invoiceNextPaymentString = "<li>I will be emailed an invoice for {0} for the period {1} to {2}. </li><li>In the meantime, my company will have full access to the options I selected on the previous page.</li>";
            string educationalString = "<li>This includes my Educational discount.</li>";

            StringBuilder textBuilder = new StringBuilder();
            bool isEducationalPackage = planDetails.IsEducationalPackage;
            int paymentPackageDurationTypeCodeId = planDetails.PaymentDurationCodeId;
            DiscountCodeUsage discountCodeUsage = planDetails.DiscountCodeUsage;
            int projectPaymentPackageTypeId = planDetails.ProjectPaymentPackageTypeId;
            int inventoryPaymentPackageTypeId = planDetails.InventoryPaymentPackageTypeId;
            int companyId = planDetails.CompanyId;
            int anualDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
            int monthlyDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
            int invoicePaymentMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE");
            int creditCardPaymentMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "CREDITCARD");

            CompanyBL companyBL = new CompanyBL(DataContext);
            ProjectBL projectBL = new ProjectBL(DataContext);
            PaymentSummaryDetails paymentSummaryDetail = null;
            DateTime startdate = GetStartDate(companyId);
            List<CompanyPaymentSummary> paymentPackageSummaries = null;
            decimal dueAmount = 0;//this due will be total amount for period if free trial or first time.
            DateTime fromDate = Utils.Today;

            if (discountCodeUsage == null)
            {
                discountCodeUsage = GetLatestDiscountCodeUsage(companyId);
            }

            decimal yearPay = CalculateALLPackageAmountsByPeriod(projectPaymentPackageTypeId, inventoryPaymentPackageTypeId, anualDurationCodeId);
            decimal monthPay = CalculateALLPackageAmountsByPeriod(projectPaymentPackageTypeId, inventoryPaymentPackageTypeId, monthlyDurationCodeId);
            decimal educationalDiscount = decimal.Parse(Utils.GetSystemValue("EducationalDiscount"));

            if (planDetails.IsEducationalPackage)
            {
                yearPay = yearPay * (100 - educationalDiscount) / 100;
                monthPay = monthPay * (100 - educationalDiscount) / 100;
            }

            CompanyPaymentPackage oldPackage = null;

            if (companyId > 0)
            {
                oldPackage = GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);
            }

            #region first time configuration of a package or configure a package in free trial

            if ((companyId > 0 && companyBL.IsFreeTrialStatusIncludedFortheDay(planDetails.CompanyId, Utils.Today)) || companyId == 0 || oldPackage == null)  //free trial scenario and first time configuration
            {
                paymentSummaryDetail = GetPaymentSummaryDetailRecord(planDetails, false);
                paymentPackageSummaries = ProjectUsageHandler.GetPaymentPackageSummaries(paymentSummaryDetail, startdate, DataContext);
                dueAmount = GetTotalDueAmount(paymentPackageSummaries);//this due will be total amount for period if free trial or first time.

                textBuilder.Append(startText);
                if (companyBL.IsFreeTrialCompany(companyId)) //for a free trial company, free trial line will be apended.
                {
                    var freeTrialProjects = projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId);
                    Data.Project freeTrialProject = freeTrialProjects.FirstOrDefault();
                    DateTime freeTrialEndDate = freeTrialProject.ExpirationDate.Value;
                    fromDate = freeTrialEndDate;
                    textBuilder.Append(string.Format(freeTrialText, (freeTrialEndDate - Utils.Today).TotalDays.ToString()));
                }

                if (paymentmethodCodeId == invoicePaymentMethodCodeId)
                {
                    textBuilder.Append(string.Format(invoiceNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(startdate), Utils.FormatDate(startdate.AddYears(1))));
                }
                else
                {
                    if (paymentPackageDurationTypeCodeId == anualDurationCodeId)
                    {
                        textBuilder.Append(string.Format(ccYearlyNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(startdate), Utils.FormatDate(startdate.AddYears(1))));//from date is start date and to date is plus 1
                    }
                    else
                    {
                        DateTime nextBillingDate = GetNextBillingDate(companyId);
                        textBuilder.Append(string.Format(ccMonthlyNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(nextBillingDate)));
                        if (!planDetails.IsEducationalPackage && discountCodeUsage != null && discountCodeUsage.EndDate >= startdate.AddMonths(1))//if discount carry forward for next month, create second line of monthly cc auth text accordingly
                        {
                            textBuilder.Append(string.Format(ccMonthlyContinualStringWithDiscount, Utils.FormatCurrency(GetDiscountedAmount(companyId, monthPay, monthlyDurationCodeId, discountCodeUsage, startdate.AddMonths(1), startdate.AddMonths(2)), culture),  //If we carry forward the discount, new amount should be calculated
                                          Utils.FormatDate(discountCodeUsage.EndDate), Utils.FormatCurrency(monthPay, culture)));
                        }
                        else
                        {
                            textBuilder.Append(string.Format(ccMonthlyContinualString, Utils.FormatCurrency(monthPay, culture)));//if no discount or it doesn't carry forward for next month
                        }
                    }
                }

                if (planDetails.IsEducationalPackage)
                {
                    textBuilder.Append(educationalString);
                }

                textBuilder.Append("</ul>");
                return textBuilder.ToString();
            }

            #endregion first time configuration of a package or configure a package in free trial

            #region change existing package

            else
            {
                textBuilder.Append(startText);
                int durationDifference = GetDurationDifference(oldPackage.StartDate, Utils.Today, oldPackage.PaymentDurationCodeId);

                decimal unprocessedAmount = 0;
                List<CompanyPaymentSummary> unProcessedPaymentSummaries = ProjectFinanceHandler.GetUnProcessedCompanyPaymentSummaries(companyId, DataContext, Utils.Today);
                if (unProcessedPaymentSummaries != null && unProcessedPaymentSummaries.Count() > 0)
                {
                    unprocessedAmount = unProcessedPaymentSummaries.Sum(ups => ups.Amount);
                    fromDate = unProcessedPaymentSummaries.Min(record => record.FromDate);//from date should be the minimum from date among all summaries.
                }

                bool isSamePackage = (oldPackage.InventoryPaymentPackageTypeId == planDetails.InventoryPaymentPackageTypeId)
                                             && (oldPackage.ProjectPaymentPackageTypeId == planDetails.ProjectPaymentPackageTypeId);

                if (isSamePackage)
                {
                    paymentSummaryDetail = GetPaymentSummaryDetailRecord(planDetails, false);//no package level changes
                    paymentPackageSummaries = ProjectUsageHandler.GetPaymentPackageSummaries(paymentSummaryDetail, startdate, DataContext);
                    dueAmount = GetTotalDueAmount(paymentPackageSummaries) + unprocessedAmount;
                }
                else
                {
                    paymentSummaryDetail = GetPaymentSummaryDetailRecord(planDetails, true);
                    paymentPackageSummaries = ProjectUsageHandler.GetPaymentPackageSummaries(paymentSummaryDetail, startdate, DataContext);
                    dueAmount = GetTotalDueAmount(paymentPackageSummaries) + unprocessedAmount;
                }

                if (paymentPackageSummaries != null && paymentPackageSummaries.Count > 0)//if payment summaries exists, get minimum from date.
                {
                    DateTime tempDate = paymentPackageSummaries.Min(record => record.FromDate);
                    fromDate = tempDate < fromDate ? tempDate : fromDate;
                }

                DateTime pkgStartDate = Utils.Today;
                if (oldPackage.PaymentDurationCodeId == anualDurationCodeId)
                {
                    pkgStartDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddYears(durationDifference);
                }
                else
                {
                    pkgStartDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddMonths(durationDifference);
                }

                int dayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));
                DateTime toDate = pkgStartDate;

                if (pkgStartDate == Utils.Today)//If user changes package on the package end date then payment summary details contains the amount for that new package.
                {
                    if (paymentPackageSummaries.Count > 0)
                    {
                        toDate = pkgStartDate = paymentPackageSummaries.Max(record => record.ToDate);
                    }
                    else
                    {
                        toDate = pkgStartDate = (from cpp in DataContext.CompanyPaymentSummaries where cpp.FromDate == Utils.Today && cpp.CompanyId == companyId select cpp).OrderByDescending(cpp => cpp.CompanyPaymentSummaryId).FirstOrDefault().ToDate;
                    }
                }
                else if (pkgStartDate <= GetNextBillingDate(companyId) && pkgStartDate != Utils.Today && planDetails.PaymentDurationCodeId == monthlyDurationCodeId)//due amount and periodic amount included for next billing
                {
                    decimal continualAmount = 0;
                    continualAmount = monthPay;
                    if (!isEducationalPackage && discountCodeUsage != null)
                    {
                        continualAmount = GetDiscountedAmount(companyId, monthPay, monthlyDurationCodeId, discountCodeUsage, pkgStartDate, pkgStartDate.AddMonths(1));
                    }
                    pkgStartDate = pkgStartDate.AddMonths(1);
                    dueAmount = dueAmount + continualAmount;
                }

                if (dueAmount > 0)
                {
                    if (paymentPackageDurationTypeCodeId == anualDurationCodeId)
                    {
                        if (paymentmethodCodeId == invoicePaymentMethodCodeId)
                        {
                            textBuilder.Append(string.Format(invoiceNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(fromDate), Utils.FormatDate(toDate)));
                        }
                        else
                        {
                            textBuilder.Append(string.Format(ccYearlyNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(fromDate), Utils.FormatDate(toDate)));
                        }
                    }
                    else
                    {
                        textBuilder.Append(string.Format(ccMonthlyNextPaymentString, Utils.FormatCurrency(dueAmount, culture), Utils.FormatDate(GetNextBillingDate(companyId))));
                    }
                }
                else//no amount to pay upto today.A downgrade.But we mention amount for next billing.
                {
                    if (paymentPackageDurationTypeCodeId == anualDurationCodeId)
                    {
                        if (!isEducationalPackage && discountCodeUsage != null)
                        {
                            yearPay = GetDiscountedAmount(companyId, yearPay, anualDurationCodeId, discountCodeUsage, pkgStartDate, pkgStartDate.AddYears(1));
                        }

                        if (paymentmethodCodeId == invoicePaymentMethodCodeId)
                        {
                            textBuilder.Append(string.Format(invoiceNextPaymentString, Utils.FormatCurrency(yearPay, culture), Utils.FormatDate(pkgStartDate), Utils.FormatDate(pkgStartDate.AddYears(1))));
                        }
                        else
                        {
                            textBuilder.Append(string.Format(ccYearlyNextPaymentString,
                                           Utils.FormatCurrency(yearPay, culture),  //If we carry forward the discount, new amount should be calculated
                                       Utils.FormatDate(pkgStartDate), Utils.FormatDate(pkgStartDate.AddYears(1)))); //If we carry forward the discount, new amount should be calculated
                        }
                    }
                    else
                    {
                        DateTime nextBillingDate = GetNextBillingDate(companyId);
                        if (!isEducationalPackage && discountCodeUsage != null)
                        {
                            monthPay = GetDiscountedAmount(companyId, monthPay, monthlyDurationCodeId, discountCodeUsage, pkgStartDate, pkgStartDate.AddMonths(1));
                        }
                        textBuilder.Append(string.Format(ccMonthlyNextPaymentString, Utils.FormatCurrency(monthPay, culture), Utils.FormatDate(nextBillingDate)));
                        pkgStartDate = pkgStartDate.AddMonths(1);
                    }
                }

                if (paymentPackageDurationTypeCodeId == monthlyDurationCodeId) //If they are paying by monthly, line 2 and 3 of auth text will be created accordingly.
                {
                    if (!isEducationalPackage && discountCodeUsage != null && discountCodeUsage.EndDate > pkgStartDate)
                    {
                        textBuilder.Append(string.Format(ccMonthlyContinualStringWithDiscount,
                                    Utils.FormatCurrency(GetDiscountedAmount(companyId, monthPay, monthlyDurationCodeId, discountCodeUsage, pkgStartDate, pkgStartDate.AddMonths(1)), culture),  //If we carry forward the discount, new amount should be calculated
                                     Utils.FormatDate(discountCodeUsage.EndDate), Utils.FormatCurrency(monthPay, culture))); //If we carry forward the discount, new amount should be calculated
                    }
                    else
                    {
                        textBuilder.Append(string.Format(ccMonthlyContinualString, Utils.FormatCurrency(monthPay, culture)));
                    }
                }

                if (planDetails.IsEducationalPackage)
                {
                    textBuilder.Append(educationalString);
                }

                textBuilder.Append("</ul>");
                return textBuilder.ToString();
            }

            #endregion change existing package
        }

        /// <summary>
        /// Gets the next payment date.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private DateTime GetNextPaymentDate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the next billing date.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public DateTime GetNextBillingDate(int companyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            ProjectBL projectBL = new ProjectBL(DataContext);
            int dayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));
            DateTime nextBillingDate = new DateTime(Utils.Today.Year, Utils.Today.Month, dayToRun);

            if (companyId > 0 && companyBL.IsFreeTrialStatusIncludedFortheDay(companyId, Utils.Today))  //free trial scenario
            {
                Data.Project freeTrialProject = projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId).FirstOrDefault();
                DateTime freeTrialEndDate = freeTrialProject != null ? freeTrialProject.ExpirationDate.Value : Utils.Today;

                nextBillingDate = new DateTime(freeTrialEndDate.Year, freeTrialEndDate.Month, dayToRun);
                if (freeTrialEndDate >= nextBillingDate)
                    nextBillingDate = nextBillingDate.AddMonths(1);

                return nextBillingDate;
            }
            else
            {
                nextBillingDate = new DateTime(Utils.Today.Year, Utils.Today.Month, dayToRun);
                if (Utils.Today > nextBillingDate)
                    nextBillingDate = nextBillingDate.AddMonths(1);
            }

            return nextBillingDate;
        }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private DateTime GetStartDate(int companyId)
        {
            DateTime startDate = Utils.Today;
            CompanyBL companyBL = new CompanyBL(DataContext);
            ProjectBL projectBL = new ProjectBL(DataContext);
            if (companyId > 0 && companyBL.IsFreeTrialCompany(companyId))  //free trial scenario
            {
                var freeTrialProjects = projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId);
                Data.Project freeTrialProject = freeTrialProjects.FirstOrDefault();
                DateTime freeTrialEndDate = freeTrialProject.ExpirationDate.Value;
                startDate = freeTrialEndDate.AddDays(1);
            }

            return startDate;
        }

        /// <summary>
        /// Gets the total due amount.
        /// </summary>
        /// <param name="paymentPackageSummaries">The payment package summaries.</param>
        /// <returns></returns>
        private decimal GetTotalDueAmount(List<CompanyPaymentSummary> paymentPackageSummaries)
        {
            decimal totalAmount = 0;
            if (paymentPackageSummaries != null && paymentPackageSummaries.Count > 0)
            {
                foreach (CompanyPaymentSummary paymentPackageSummary in paymentPackageSummaries)
                {
                    totalAmount = totalAmount + paymentPackageSummary.Amount;
                }
            }

            return totalAmount;
        }

        /// <summary>
        /// Gets the payment summary detail record.
        /// </summary>
        /// <param name="planDetails">The plan details.</param>
        /// <param name="hasPackageChanged">if set to <c>true</c> [has package changed].</param>
        /// <returns></returns>
        public PaymentSummaryDetails GetPaymentSummaryDetailRecord(PricePlanDetails planDetails, bool hasPackageChanged)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);

            PaymentSummaryDetails paymentSummaryDetails = new PaymentSummaryDetails()
            {
                CompanyId = planDetails.CompanyId,
                ShouldProcess = !(companyBL.IsCompanySuspended(planDetails.CompanyId) || companyBL.HasCompanySuspendedbySBAdmin(planDetails.CompanyId)),
                DiscountCodeUsageToApply = planDetails.DiscountCodeUsage,
                InventoryPaymentPackageTypeId = planDetails.InventoryPaymentPackageTypeId,
                IsEducationPackage = planDetails.IsEducationalPackage,
                HasPackageChanged = hasPackageChanged,
                ProjectPaymentPackageTypeId = planDetails.ProjectPaymentPackageTypeId,
                PaymentMethodCodeId = planDetails.PaymentMethodCodeId,
                UserId = 0,
                PaymentDurationTypeCodeId = planDetails.PaymentDurationCodeId,
                PackageStartDate = planDetails.PackageStartDate,
                IsUserAction = true
            };

            return paymentSummaryDetails;
        }

        /// <summary>
        /// Gets the discount code by code identifier.
        /// </summary>
        /// <param name="discountCodeId">The discount code identifier.</param>
        /// <returns></returns>
        public DiscountCode GetDiscountCodeByCodeId(int discountCodeId)
        {
            //Validate the Discount Code.
            return DataContext.DiscountCodes.Where(dc => dc.DiscountCodeID == discountCodeId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the discount code usage by date.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        /// <param name="CompanyId">The company identifier.</param>
        /// <returns></returns>
        public DiscountCodeUsage GetDiscountCodeUsageByDate(DateTime datetime, int CompanyId)
        {
            //Get the closest discount code usage record id for a given date.
            DiscountCodeUsage discountCodeUsage = (from dcu in DataContext.DiscountCodeUsages
                                                   where dcu.CompanyId == CompanyId && dcu.StartDate.Value <= datetime && dcu.EndDate >= datetime
                                                        && (dcu.IsActive || !dcu.IsActive && dcu.EndDate != datetime)
                                                   select dcu).OrderByDescending(dcu => dcu.DiscountCodeUsageId).FirstOrDefault();

            return discountCodeUsage;
        }

        /// <summary>
        /// Determines whether given discount code valid to use.
        /// </summary>
        /// <param name="discountCodeText">The discount code text.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="errorMsg">The error MSG.</param>
        /// <param name="discountCodeToCheck">The discount code to check.</param>
        /// <returns></returns>
        public bool IsDiscountCodeValidToUse(string discountCodeText, int companyId, out string errorMsg, DiscountCode discountCodeToCheck = null)
        {
            if (discountCodeText.Length == 0)
            {
                errorMsg = "Promotional Code cannot be empty.";
                return false;
            }

            //Validate the Discount Code.
            DiscountCode discountCode = DataContext.DiscountCodes.Where(dc => dc.Code == discountCodeText).FirstOrDefault();
            if (discountCode == null)
            {
                //Invalid Discount Code.
                errorMsg = "Invalid Promotional Code.";
                return false;
            }

            if (discountCode.ExpireDate.Date < Utils.Today)
            {
                //If it is expired
                errorMsg = "Promotional Code has expired.";
                return false;
            }

            //If the usage limit has exceeded.
            if (DataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCode.Code == discountCodeText).Count() == discountCode.InstanceCount)
            {
                errorMsg = "This Promotional Code has reached its maximum instance count. Please contact the StageBitz Administrator.";
                return false;
            }

            //if the new discount code is already being used
            if (DataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCode.Code == discountCodeText && dcu.CompanyId == companyId).FirstOrDefault() != null)
            {
                errorMsg = "Promotional Code has already been used by this Company.";
                return false;
            }

            if (discountCodeToCheck != null)
            {
                if (discountCode.Duration != discountCodeToCheck.Duration || discountCode.Discount != discountCodeToCheck.Discount)
                {
                    errorMsg = "Promotional Code has been changed.";
                    return false;
                }
            }
            errorMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether discount limit reached for specified discount code.
        /// </summary>
        /// <param name="discountCode">The discount code.</param>
        /// <returns></returns>
        public bool HasDiscountLimitReached(DiscountCode discountCode)
        {
            //If the usage limit has exceeded.
            return (DataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCode.Code == discountCode.Code).Count() == discountCode.InstanceCount);
        }

        /// <summary>
        /// Determines whether discount code expired.
        /// </summary>
        /// <param name="discountCode">The discount code.</param>
        /// <returns></returns>
        public bool IsDiscountCodeExpired(DiscountCode discountCode)
        {
            return (discountCode.ExpireDate.Date < Utils.Today);
        }

        /// <summary>
        /// Gets the latest discount usage code.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public DiscountCodeUsage GetLatestDiscountUsageCode(int companyId)
        {
            //Get the closest discount code usage record id for a given date.
            var discountCodeUsages = (from dcu in DataContext.DiscountCodeUsages
                                      where dcu.CompanyId == companyId && dcu.StartDate.Value <= Utils.Today && dcu.EndDate >= Utils.Today
                                      select dcu).ToList();

            if (discountCodeUsages.Count != 0)
            {
                int discountCodeUsageId = discountCodeUsages.Max(dcu => dcu.DiscountCodeUsageId);
                return DataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCodeUsageId == discountCodeUsageId).FirstOrDefault();
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the credit card token.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public CreditCardToken GetCreditCardToken(string relatedTable, int companyId)
        {
            return DataContext.CreditCardTokens.Where(c => c.RelatedTableName == relatedTable && c.RelatedId == companyId && c.IsActive == true).FirstOrDefault();
        }

        /// <summary>
        /// Saves new discount code usage to company and inactivate existing discount usage.
        /// </summary>
        /// <param name="discountCode">The discount code.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="companyId">The company id.</param>
        /// <returns>The new discount usage object.</returns>
        public DiscountCodeUsage SaveDiscountCodeUsageToCompany(DiscountCode discountCode, int userId, int companyId)
        {
            //Get the current discountCode used by the company.
            Data.DiscountCodeUsage discountUsage = DataContext.DiscountCodeUsages.Where(dcu => dcu.EndDate > Utils.Today &&
                    dcu.CompanyId == companyId && dcu.IsActive).OrderByDescending(dcu => dcu.DiscountCodeUsageId).FirstOrDefault();
            if (discountUsage != null)
            {
                //Expire it by today
                discountUsage.EndDate = Utils.Today;
                discountUsage.LastUpdatedByUserId = userId;
                discountUsage.LastUpdatedDate = Utils.Now;
                discountUsage.IsActive = false;
            }

            DiscountCodeUsage newDiscountCodeUsage = new DiscountCodeUsage();
            newDiscountCodeUsage.DiscountCodeId = discountCode.DiscountCodeID;
            newDiscountCodeUsage.CreatedDate = newDiscountCodeUsage.LastUpdatedDate = Utils.Now;
            newDiscountCodeUsage.StartDate = Utils.Today;
            newDiscountCodeUsage.EndDate = Utils.Today.AddDays(discountCode.Duration * 7);
            newDiscountCodeUsage.CreatedByUserId = newDiscountCodeUsage.LastUpdatedByUserId = userId;
            newDiscountCodeUsage.CompanyId = companyId;
            newDiscountCodeUsage.IsActive = true;
            newDiscountCodeUsage.IsAdminApplied = false;
            DataContext.DiscountCodeUsages.AddObject(newDiscountCodeUsage);
            return newDiscountCodeUsage;
        }

        /// <summary>
        /// Gets the credit card token.
        /// </summary>
        /// <param name="CompanyId">The company identifier.</param>
        /// <returns></returns>
        public CreditCardToken GetCreditCardToken(int CompanyId)
        {
            return DataContext.CreditCardTokens.Where(c => c.RelatedTableName == "Company" && c.RelatedId == CompanyId && c.IsActive == true).FirstOrDefault();
        }

        /// <summary>
        /// Gets the text for already configured package.
        /// </summary>
        /// <returns></returns>
        public string GetTextForAlreadyConfiguredPackage()
        {
            return string.Empty;
        }

        /// <summary>
        /// Determines whether [has package selected for free trail ended company] [the specified company id].
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>
        ///   <c>true</c> if [has package selected for free trail ended company] [the specified company id]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasPackageSelectedForFreeTrailEndedCompany(int companyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);

            CompanyPaymentPackage companyPaymentPackage = GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);
            return !(companyBL.IsFreeTrialEndedCompany(companyId) && companyPaymentPackage == null);
        }

        /// <summary>
        /// Determines whether company has exceeded user limits.
        /// </summary>
        /// <param name="isFreeTrialCompany">if set to <c>true</c> [is free trial company].</param>
        /// <param name="projectPaymentPackageDetails">The project payment package details.</param>
        /// <param name="companyCurrentUsage">The company current usage.</param>
        /// <returns>
        ///   <c>true</c> if [has exceeded user limit] [the specified is free trial company]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasExceededUserLimit(int companyId, string invitedUserEmail, bool isFreeTrialCompany,
                CompanyPaymentPackage companyPaymentPackage, ProjectPaymentPackageDetails projectPaymentPackageDetails,
                CompanyCurrentUsage companyCurrentUsage, bool isReactivate = false)
        {
            int defaultFreeTrailUserCount = int.Parse(Utils.GetSystemValue("DefaultProjectHeadCountForCompany"));
            List<string> currentUserList = GetCompanyCurrentUserList(companyId, null);

            // if the inviting user is already accounted for the usage count that user should be ignored.
            if (currentUserList.Count() > 0 && currentUserList.Contains(invitedUserEmail))
            {
                return false;
            }

            // for educational company can have unlimited users
            if (companyPaymentPackage != null && IsEducationalCompany(companyPaymentPackage))
            {
                return false;
            }
            else if (projectPaymentPackageDetails != null)
            {
                return isReactivate ? companyCurrentUsage.UserCount > projectPaymentPackageDetails.HeadCount : companyCurrentUsage.UserCount >= projectPaymentPackageDetails.HeadCount;
            }
            else
            {
                return isFreeTrialCompany ?
                        (isReactivate ? companyCurrentUsage.UserCount > defaultFreeTrailUserCount : companyCurrentUsage.UserCount >= defaultFreeTrailUserCount)
                        : false;
            }
        }

        /// <summary>
        /// Determines whether specified company is educational company.
        /// </summary>
        /// <param name="currentCompanyPaymentPackage">The current company payment package.</param>
        /// <returns></returns>
        public bool IsEducationalCompany(CompanyPaymentPackage currentCompanyPaymentPackage)
        {
            //we need to read the future record in order to get the 'IsEducational' option. This is because we don't update the current payment package if the user only changes the
            //'IsEducational' option because calculations have already being done. First we need to read the future record as it contains the latest changes.
            if (currentCompanyPaymentPackage != null)
            {
                CompanyPaymentPackage futurePackage = GetLatestRequestForTheCompany(currentCompanyPaymentPackage.CompanyId);
                if (futurePackage != null)
                {
                    return futurePackage.IsEducationalPackage;
                }
                else
                {
                    return currentCompanyPaymentPackage.IsEducationalPackage;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether company has exceeded project limits.
        /// </summary>
        /// <param name="isFreeTrialCompany">if set to <c>true</c> [is free trial company].</param>
        /// <param name="projectPaymentPackageDetails">The project payment package details.</param>
        /// <param name="companyCurrentUsage">The company current usage.</param>
        /// <param name="isReactivate">if set to <c>true</c> [is reactivate].</param>
        /// <returns>
        ///   <c>true</c> if [has exceeded project limit] [the specified is free trial company]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasExceededProjectLimit(bool isFreeTrialCompany, ProjectPaymentPackageDetails projectPaymentPackageDetails,
                CompanyCurrentUsage companyCurrentUsage)
        {
            int defaultFreeTrailProjectCount = int.Parse(Utils.GetSystemValue("DefaultProjectCountForCompany"));
            ProjectPaymentPackageDetails freeProjectPackage = Utils.GetFreeSystemProjectPackageDetail();

            if (projectPaymentPackageDetails != null)
            {
                int projectCount = projectPaymentPackageDetails.ProjectCount;

                if (isFreeTrialCompany)
                {
                    projectCount = (projectCount == freeProjectPackage.ProjectCount) ? defaultFreeTrailProjectCount : projectCount;
                }

                return companyCurrentUsage.ProjectCount >= projectCount;
            }
            else
            {
                if (isFreeTrialCompany)
                {
                    return companyCurrentUsage.ProjectCount >= defaultFreeTrailProjectCount;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether company has exceeded inventory limits.
        /// </summary>
        /// <param name="isFreeTrialCompany">if set to <c>true</c> [is free trial company].</param>
        /// <param name="inventoryPaymentPackageDetails">The inventory payment package details.</param>
        /// <param name="companyCurrentUsage">The company current usage.</param>
        /// <returns>
        ///   <c>true</c> if [has exceeded inventory limit] [the specified is free trial company]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasExceededInventoryLimit(bool isFreeTrialCompany, InventoryPaymentPackageDetails inventoryPaymentPackageDetails, CompanyCurrentUsage companyCurrentUsage, int? bulkUploadCount = null)
        {
            int defaultFreeTrailInventoryLimit = int.Parse(Utils.GetSystemValue("DefaultInventoryCountForCompany"));
            int currentInventoryCount = bulkUploadCount.HasValue ? companyCurrentUsage.InventoryCount - 1 + bulkUploadCount.Value : companyCurrentUsage.InventoryCount;
            if (inventoryPaymentPackageDetails != null)
            {
                int? inventoryCount = inventoryPaymentPackageDetails.ItemCount;
                InventoryPaymentPackageDetails freeInventoryPackage = Utils.GetFreeSystemInventoryPackageDetail();

                if (isFreeTrialCompany)
                {
                    inventoryCount = (inventoryCount.HasValue && inventoryCount == freeInventoryPackage.ItemCount.Value) ? defaultFreeTrailInventoryLimit : inventoryCount;
                }

                return inventoryCount.HasValue && currentInventoryCount >= inventoryCount;
            }
            else
            {
                return isFreeTrialCompany ? currentInventoryCount >= defaultFreeTrailInventoryLimit : false;
            }
        }

        /// <summary>
        /// Gets the company current usage.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="mustIncludeProjectId">The must include project identifier.</param>
        /// <returns></returns>
        public CompanyCurrentUsage GetCompanyCurrentUsage(int companyId, List<int> mustIncludeProjectId)
        {
            List<string> uniqueList = GetCompanyCurrentUserList(companyId, mustIncludeProjectId);

            int allitemsCount = GetCompanyCurrentItemCount(companyId);

            int projectCount = GetCompanyCurrentProjectCount(companyId);
            int freeTrialProjectCount = GetCompanyCurrentFreeTrialProjectCount(companyId);

            CompanyCurrentUsage companyCurrentUsage = new CompanyCurrentUsage();
            companyCurrentUsage.InventoryCount = allitemsCount;
            companyCurrentUsage.UserCount = uniqueList.Count;
            companyCurrentUsage.ProjectCount = projectCount;
            companyCurrentUsage.FreeTrialProjectCount = freeTrialProjectCount;
            return companyCurrentUsage;
        }

        /// <summary>
        /// Gets the company current item count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public int GetCompanyCurrentItemCount(int companyId)
        {
            return DataContext.Items.Where(i => i.CompanyId == companyId && i.IsActive && !i.IsHidden).Count();
        }

        /// <summary>
        /// Gets the company current project count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public int GetCompanyCurrentProjectCount(int companyId)
        {
            int projectClosedStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "CLOSED");
            int projectSuspendedStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "SUSPENDED");

            return DataContext.Projects.Where(p => p.CompanyId == companyId && p.IsActive == true &&
                (p.ProjectStatusCodeId != projectClosedStatusCodeId && p.ProjectStatusCodeId != projectSuspendedStatusCodeId)).Count();
        }

        /// <summary>
        /// Gets the company's current free trial project count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public int GetCompanyCurrentFreeTrialProjectCount(int companyId)
        {
            int projectFreeTrialCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "FREETRIAL");
            return DataContext.Projects.Where(p => p.CompanyId == companyId && p.IsActive == true && p.ProjectStatusCodeId == projectFreeTrialCodeId).Count();
        }

        /// <summary>
        /// Gets the company current user list.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="projectList">The project list.</param>
        /// <returns></returns>
        public List<string> GetCompanyCurrentUserList(int companyId, List<int> projectList)
        {
            if (projectList == null)
            {
                projectList = new List<int>();
            }

            int projectClosedStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "CLOSED");
            int projectSuspendedStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "SUSPENDED");

            int companyPrimaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int companySecondaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");
            int pendingStatusCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            int projectInvitationTypeCodeId = Utils.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");
            int companyInvitationTypeCodeId = Utils.GetCodeIdByCodeValue("InvitationType", "COMPANYADMIN");

            List<string> projectusers = (from pu in DataContext.ProjectUsers
                                         join u in DataContext.Users on pu.UserId equals u.UserId
                                         where pu.Project.CompanyId == companyId && pu.IsActive && u.IsActive
                                         && (projectList.Contains(pu.Project.ProjectId) || (pu.Project.ProjectStatusCodeId != projectClosedStatusCodeId && pu.Project.ProjectStatusCodeId != projectSuspendedStatusCodeId))
                                         select u.LoginName).Distinct().ToList();

            List<string> companyUsers = (from cu in DataContext.CompanyUsers
                                         join u in DataContext.Users on cu.UserId equals u.UserId
                                         join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                         where cu.CompanyId == companyId && (cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeId || cur.CompanyUserTypeCodeId == companySecondaryAdminCodeId)
                                         && cu.IsActive && cur.IsActive && u.IsActive
                                         select u.LoginName).Distinct().ToList();

            List<string> companyInvitations = (from i in DataContext.Invitations
                                               join iur in DataContext.InvitationUserRoles on i.InvitationId equals iur.InvitationId
                                               from u in DataContext.Users.Where(u => u.UserId == i.ToUserId).DefaultIfEmpty()
                                               join c in DataContext.Companies on i.RelatedId equals c.CompanyId
                                               where c.CompanyId == companyId && i.RelatedTable == StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies
                                               && i.InvitationTypeCodeId == companyInvitationTypeCodeId && iur.IsActive && (u == null || u.IsActive) && c.IsActive
                                               && i.InvitationStatusCodeId == pendingStatusCodeId
                                               && (iur.UserTypeCodeId == companySecondaryAdminCodeId || iur.UserTypeCodeId == companyPrimaryAdminCodeId)
                                               select new { Email = (u != null ? u.LoginName : i.ToEmail) }).Select(t => t.Email).Distinct().ToList();

            //For already registered users
            List<string> projInvitations = (from i in DataContext.Invitations
                                            from u in DataContext.Users.Where(u => u.UserId == i.ToUserId).DefaultIfEmpty()
                                            join p in DataContext.Projects on i.RelatedId equals p.ProjectId
                                            where p.CompanyId == companyId && i.RelatedTable == StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Projects
                                            && i.InvitationTypeCodeId == projectInvitationTypeCodeId && (u == null || u.IsActive) && p.IsActive
                                            && i.InvitationStatusCodeId == pendingStatusCodeId
                                            && (projectList.Contains(p.ProjectId) || (p.ProjectStatusCodeId != projectClosedStatusCodeId && p.ProjectStatusCodeId != projectSuspendedStatusCodeId))
                                            select new { Email = (u != null ? u.LoginName : i.ToEmail) }).Select(t => t.Email).Distinct().ToList();

            var uniqueList = projectusers.Union(companyUsers).Union(projInvitations).Union(companyInvitations).ToList();

            return uniqueList;
        }

        /// <summary>
        /// Validate the selected company package against it's usage
        /// </summary>
        /// <returns>
        ///  returns if there is an error.
        /// </returns>
        public string GettheValidityOfSelectedPlan(int companyId, PricePlanDetails pricePlanDetails, ProjectPaymentPackageDetails projectPackageDetails, InventoryPaymentPackageDetails inventoryPaymentPackageDetails)
        {
            //If returns a message, means you have an issue with the selceted package/packages
            CompanyCurrentUsage companyCurrentUsage = GetCompanyCurrentUsage(companyId, null);
            StringBuilder textBuilder = new StringBuilder();
            if (companyCurrentUsage != null)
            {
                string errorType = string.Empty;
                string limitReduceKnowlageBaseLink = string.Empty;
                int errorCount = 0;

                int projectCount = projectPackageDetails.ProjectCount;
                int userCount = projectPackageDetails.HeadCount;
                int? inventoryCount = inventoryPaymentPackageDetails.ItemCount;
                int companyCurrentProjectCount = companyCurrentUsage.ProjectCount;

                CompanyBL companyBL = new CompanyBL(DataContext);
                if (companyBL.IsFreeTrialCompany(companyId))
                {
                    ProjectPaymentPackageDetails freeProjectPackage = Utils.GetFreeSystemProjectPackageDetail();
                    InventoryPaymentPackageDetails freeInventoryPackage = Utils.GetFreeSystemInventoryPackageDetail();

                    // int freeTrialProjectCount = int.Parse(Utils.GetSystemValue("DefaultProjectCountForCompany"));
                    int freeTrailUserCount = int.Parse(Utils.GetSystemValue("DefaultProjectHeadCountForCompany"));
                    int freeTrailInvenotryLimit = int.Parse(Utils.GetSystemValue("DefaultInventoryCountForCompany"));

                    // projectCount = (projectCount == freeProjectPackage.ProjectCount) ? freeTrialProjectCount : projectCount;
                    userCount = (userCount == freeProjectPackage.HeadCount) ? freeTrailUserCount : userCount;
                    inventoryCount = (inventoryCount.HasValue && inventoryCount == freeInventoryPackage.ItemCount) ? freeTrailInvenotryLimit : inventoryCount;
                    companyCurrentProjectCount = (projectCount == freeProjectPackage.ProjectCount) ? companyCurrentUsage.ProjectCount - companyCurrentUsage.FreeTrialProjectCount : companyCurrentProjectCount;
                }

                if (companyCurrentProjectCount > projectCount)
                {
                    textBuilder.AppendFormat("- You have {0} active {1}, but you've selected to have a maximum of {2}. <br/>",
                        companyCurrentProjectCount, companyCurrentProjectCount == 1 ? "Project" : "Projects", projectCount);
                    errorType = " active Projects";
                    //limitReduceKnowlageBaseLink = Utils.GetSystemValue("FeedBackAndTechSupportURL");
                    errorCount++;
                }
                if (!pricePlanDetails.IsEducationalPackage && companyCurrentUsage.UserCount > userCount)
                {
                    textBuilder.Append("<br/>");
                    textBuilder.AppendFormat("- You have {0} active {1}, but you've selected to have a maximum of {2}. <br/>",
                        companyCurrentUsage.UserCount, companyCurrentUsage.UserCount == 1 ? "User" : "Users", userCount);
                    if (errorType.Length > 0) //Has Project error
                    {
                        errorType = string.Concat(errorType, ", active Users");
                    }
                    else
                    {
                        errorType = "active Users";
                    }
                    errorCount++;
                }
                if (inventoryCount.HasValue && companyCurrentUsage.InventoryCount > inventoryCount)
                {
                    textBuilder.Append("<br/>");
                    textBuilder.AppendFormat("- You have {0} Inventory {1}, but you've selected to have a maximum of {2}. <br/>",
                            companyCurrentUsage.InventoryCount, companyCurrentUsage.InventoryCount == 1 ? "Item" : "Items", inventoryCount);

                    if (errorType.Contains(',')) // there are two errors
                    {
                        //Add an "And" seperator
                        errorType = string.Concat(errorType, " and Inventory Items");
                    }
                    else
                    {
                        if (errorCount == 1)
                            errorType = string.Concat(errorType, " ,Inventory Items");
                        else
                            errorType = string.Concat(errorType, "Inventory Items");
                    }
                    errorCount++;
                }

                int count = errorType.Count(t => t == ',');
                if (errorCount == 2 && count == 1) //eg. Active Users,Inventory Items. In this case we need to replace the "," with "and"
                {
                    errorType = errorType.Replace(",", " and ");
                }
                if (errorCount > 0)
                {
                    limitReduceKnowlageBaseLink = Utils.GetSystemValue("FeedBackAndTechSupportURL");
                    string htmlText = string.IsNullOrEmpty(limitReduceKnowlageBaseLink) ?
                        "- If you'd like to continue with this pricing plan, you'll need to reduce your number of {0} first." :
                        string.Concat("- If you'd like to continue with this pricing plan, you'll need to reduce your number of {0} first.<br/>- Need help? Take a look at our <a href='", limitReduceKnowlageBaseLink, "' target='_blank'> support articles</a>.");
                    textBuilder.Append("<br/>");
                    textBuilder.AppendFormat(htmlText, errorType);
                }
            }
            return textBuilder.ToString();
        }

        /// <summary>
        /// Sends the stage bitz admin email.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="PricePlanDetails">The price plan details.</param>
        /// <param name="cultureName">Name of the culture.</param>
        /// <param name="educationalPosition">The educational position.</param>
        /// <param name="authText">The authentication text.</param>
        /// <param name="IsInventryProjectOrDurationChanged">if set to <c>true</c> [is inventry project or duration changed].</param>
        public void SendStageBitzAdminEmail(int companyId, int userId, PricePlanDetails PricePlanDetails, string cultureName, string educationalPosition, string authText, bool IsInventryProjectOrDurationChanged)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            PersonalBL personalBL = new PersonalBL(DataContext);
            Data.Company company = companyBL.GetCompany(companyId);
            User user = personalBL.GetUser(userId);
            PackageConfirmationEmailContent emailContent = new PackageConfirmationEmailContent();
            emailContent.ToEmail = Utils.GetSystemValue("AdminEmail");
            emailContent.UserName = string.Concat(user.FirstName, " ", user.LastName);
            emailContent.CompanyName = company.CompanyName;
            emailContent.IsEducational = PricePlanDetails.IsEducationalPackage;
            emailContent.EducationalPosition = educationalPosition;
            emailContent.FormattedAuthText = authText;

            emailContent.IsInventryProjectOrDurationChanged = IsInventryProjectOrDurationChanged;

            List<CompanyUserRole> companyUserRoles = companyBL.GetCompanyUserRoles(userId, companyId);
            string userRole = string.Empty;
            if (companyUserRoles.Count > 0)
            {
                userRole = Utils.GetCodeDescription(companyUserRoles[0].CompanyUserTypeCodeId);
            }
            emailContent.Position = userRole;

            emailContent.CompanyURL = string.Format("{0}/Company/CompanyDetails.aspx?CompanyID={1}", Utils.GetSystemValue("SBAdminWebURL"), companyId);
            string discountCodeExpiryDate = string.Empty;
            string discount = string.Empty;

            if (PricePlanDetails.DiscountCodeUsage != null && !PricePlanDetails.IsEducationalPackage)
            {
                discountCodeExpiryDate = Utils.FormatDate(PricePlanDetails.DiscountCodeUsage.EndDate);
                discount = String.Format("{0:P2}", PricePlanDetails.DiscountCodeUsage.DiscountCode.Discount / 100);
            }
            emailContent.Discount = discount;
            emailContent.PromotionalCodeExpireDate = discountCodeExpiryDate;

            ProjectPaymentPackageDetails projectPakageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(PricePlanDetails.ProjectPaymentPackageTypeId);
            InventoryPaymentPackageDetails inventoryPakageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(PricePlanDetails.InventoryPaymentPackageTypeId);

            emailContent.ProjectPeriodPrice = Utils.FormatCurrency(CalculatethePackageAmountByPeriod(Utils.GetCodeIdByCodeValue("PaymentPackageType", "PROJECT"), projectPakageDetails.PackageTypeId, PricePlanDetails.PaymentDurationCodeId), cultureName);
            emailContent.InventoryPeriodPrice = Utils.FormatCurrency(CalculatethePackageAmountByPeriod(Utils.GetCodeIdByCodeValue("PaymentPackageType", "INVENTORY"), inventoryPakageDetails.PackageTypeId, PricePlanDetails.PaymentDurationCodeId), cultureName);
            decimal totalPrice = CalculateALLPackageAmountsByPeriod(PricePlanDetails.ProjectPaymentPackageTypeId, PricePlanDetails.InventoryPaymentPackageTypeId, PricePlanDetails.PaymentDurationCodeId);

            emailContent.ProjectPackage = projectPakageDetails.PackageDisplayName;
            emailContent.InventoryPackage = inventoryPakageDetails.PackageDisplayName;
            emailContent.TotalPriceString = string.Concat(Utils.FormatCurrency(totalPrice, cultureName), "/", Utils.GetCodeDescription(PricePlanDetails.PaymentDurationCodeId));
            EmailSender.SendInvoiceRequestToSBAdmin(emailContent);
        }

        /// <summary>
        /// Determines whether inventry project or duration changed.
        /// </summary>
        /// <param name="planDetails">The plan details.</param>
        /// <param name="oldPackage">The old package.</param>
        /// <returns></returns>
        public bool HasInventryProjectOrDurationChanged(PricePlanDetails planDetails, CompanyPaymentPackage oldPackage)
        {
            if (oldPackage == null)
            {
                return true;
            }
            else if (oldPackage.ProjectPaymentPackageTypeId != planDetails.ProjectPaymentPackageTypeId || oldPackage.InventoryPaymentPackageTypeId != planDetails.InventoryPaymentPackageTypeId)
            {
                return true;
            }
            else if (oldPackage.PaymentDurationCodeId != planDetails.PaymentDurationCodeId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the transaction history.
        /// </summary>
        /// <param name="FromDate">From date.</param>
        /// <param name="ToDate">To date.</param>
        /// <param name="ShowUnpaidInvoiceTxOnly">if set to <c>true</c> [show unpaid invoice tx only].</param>
        /// <param name="RelatedId">The related identifier.</param>
        /// <param name="processedInvoiceTypeCodeId">The processed invoice type code identifier.</param>
        /// <returns></returns>
        public List<InvoiceTransaction> GetTransactionHistory(DateTime? FromDate, DateTime? ToDate, bool ShowUnpaidInvoiceTxOnly, int RelatedId, int processedInvoiceTypeCodeId)
        {
            List<InvoiceTransaction> ccInvoices = (from inv in DataContext.Invoices
                                                   from paylog in DataContext.PaymentLogs.Where(pl => pl.RelatedTableName == "Invoice" && pl.RelatedId == inv.InvoiceID && pl.IsPaymentSuccess == true).DefaultIfEmpty()
                                                   join r in DataContext.Receipts on inv.TransactionID equals r.ReceiptForTransactionID into receiptJoin
                                                   from receipt in receiptJoin.DefaultIfEmpty()
                                                   join c in DataContext.Companies on inv.RelatedID equals c.CompanyId
                                                   where inv.RelatedTableName == "Company" && inv.RelatedID == RelatedId
                                                   && ((ShowUnpaidInvoiceTxOnly && inv.InvoiceStatusCodeId != processedInvoiceTypeCodeId) || ShowUnpaidInvoiceTxOnly == false)

                                                       //Invoices and corresponding Receipts might not have the same date (due to payment failures). Therefore with the Date Range criteria, we search for both Invoice date and Receipt date separately.
                                                   && (((FromDate == null || inv.InvoiceDate >= FromDate) && (ToDate == null || inv.InvoiceDate <= ToDate))
                                                      || (receipt != null && ((FromDate == null || receipt.ReceiptDate >= FromDate) && (ToDate == null || receipt.ReceiptDate <= ToDate))))
                                                   orderby inv.InvoiceDate descending
                                                   select new InvoiceTransaction
                                                   {
                                                       InvoiceID = inv.InvoiceID,
                                                       Amount = inv.Amount,
                                                       InvoiceDate = inv.InvoiceDate,
                                                       FromDate = inv.FromDate,
                                                       ToDate = inv.ToDate,
                                                       ReceiptID = (receipt == null ? 0 : receipt.ReceiptID),
                                                       ReceiptDate = (receipt == null ? (DateTime?)null : receipt.ReceiptDate),
                                                       PaymentLogReferenceNumber = (paylog == null ? DataContext.PaymentLogs.Where(pl => pl.RelatedTableName == "Invoice" && pl.RelatedId == inv.InvoiceID && pl.IsPaymentSuccess == false).OrderByDescending(pl => pl.PaymentLogId).FirstOrDefault().ReferenceNumber : paylog.ReferenceNumber)
                                                   }).ToList<InvoiceTransaction>();

            int invoicePaymentMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE");
            List<InvoiceTransaction> invoicedInvoices = (from cps in DataContext.CompanyPaymentSummaries
                                                         where cps.PaymentMethodCodeId == invoicePaymentMethodCodeId && ShowUnpaidInvoiceTxOnly == false &&
                                                         cps.CompanyId == RelatedId && (FromDate == null || cps.FromDate >= FromDate) &&
                                                         (ToDate == null || cps.ToDate <= ToDate) && cps.Amount > 0
                                                         select new InvoiceTransaction
                                                         {
                                                             InvoiceID = 0,
                                                             Amount = cps.Amount,
                                                             InvoiceDate = cps.FromDate,
                                                             FromDate = cps.FromDate,
                                                             ToDate = cps.ToDate,
                                                             ReceiptID = 0,
                                                             ReceiptDate = null,
                                                             PaymentLogReferenceNumber = null
                                                         }).ToList<InvoiceTransaction>();

            return ccInvoices.Union(invoicedInvoices).ToList();
        }

        /// <summary>
        /// Gets the pricing plan history data.
        /// </summary>
        /// <param name="paymentMethod">The payment method.</param>
        /// <param name="latest">if set to <c>true</c> [latest].</param>
        /// <returns></returns>
        public List<PricingPlanHistoryData> GetPricingPlanHistoryData(int paymentMethod = 0, bool latest = false)
        {
            int monthlyPaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
            int creditCardPaymentMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "CREDITCARD");
            int primaryCompanyAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            List<PricingPlanHistoryData> pricingPlanHistoryList = null;

            pricingPlanHistoryList = (from cps in DataContext.CompanyPaymentSummaries
                                      join cpp in DataContext.CompanyPaymentPackages on cps.CompanyPaymentPackageId equals cpp.CompanyPaymentPackageId
                                      from cur in DataContext.CompanyUserRoles.Where(cur => cur.CompanyUser.CompanyId == cps.CompanyId && cur.CompanyUserTypeCodeId == primaryCompanyAdminCodeId).DefaultIfEmpty()
                                      where (paymentMethod == 0 || cps.PaymentMethodCodeId == paymentMethod)
                                      orderby cps.CompanyPaymentSummaryId descending
                                      select new PricingPlanHistoryData
                                      {
                                          CompanyId = cpp.Company.CompanyId,
                                          CompanyName = cpp.Company.CompanyName,
                                          CompanyAdminName = cur.CompanyUser.User.FirstName + " " + cur.CompanyUser.User.LastName,
                                          CompanyAdminId = cur.CompanyUser.UserId,
                                          ProjectLevel = (from ppt in DataContext.PaymentPackageTypes where ppt.PaymentPackageTypeId == cpp.ProjectPaymentPackageTypeId select ppt.Name).FirstOrDefault(),
                                          InventoryLevel = (from ppt in DataContext.PaymentPackageTypes where ppt.PaymentPackageTypeId == cpp.InventoryPaymentPackageTypeId select ppt.Name).FirstOrDefault(),
                                          PromotionalCode = cps.DiscountCodeUsage != null ? cps.DiscountCodeUsage.DiscountCode.Code : string.Empty,
                                          Educational = cpp.IsEducationalPackage ? "Yes" : string.Empty,
                                          Period = cpp.PaymentDurationCodeId == monthlyPaymentDurationCodeId ? "Monthly" : "Yearly",
                                          StartDate = cps.FromDate,
                                          TotalCost = cps.Amount,
                                          PaymentMethod = cpp.PaymentMethodCodeId.HasValue ? (cpp.PaymentMethodCodeId == creditCardPaymentMethodCodeId ? "Credit Card" : "Invoice") : string.Empty,
                                      }).ToList<PricingPlanHistoryData>();

            if (latest)
            {
                return (from pp in pricingPlanHistoryList
                        group pp by pp.CompanyId into g
                        select g.First()).ToList();
            }
            return pricingPlanHistoryList;
        }
    }
}