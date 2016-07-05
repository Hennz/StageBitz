using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Finance.Project
{
    /// <summary>
    /// Handler class for project useages
    /// </summary>
    public static class ProjectUsageHandler
    {
        /// <summary>
        /// Gets the discount rate by date.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        /// <param name="CompanyId">The company identifier.</param>
        /// <returns></returns>
        public static decimal GetDiscountRateByDate(DateTime datetime, int CompanyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                //Get the closest discount code usage record id for a given date.
                var discountCodeUsages = (from dcu in dataContext.DiscountCodeUsages
                                          where dcu.CompanyId == CompanyId && dcu.StartDate.Value <= datetime && dcu.EndDate >= datetime
                                               && (dcu.IsActive || !dcu.IsActive && dcu.EndDate != datetime)
                                          select dcu).ToList();

                if (discountCodeUsages.Count != 0)
                {
                    int discountCodeUsageId = discountCodeUsages.Max(dcu => dcu.DiscountCodeUsageId);
                    return dataContext.DiscountCodeUsages.Where(dcu => dcu.DiscountCodeUsageId == discountCodeUsageId).FirstOrDefault().DiscountCode.Discount;
                }
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the prorata amount.
        /// </summary>
        /// <param name="paymentSummaryDetail">The payment summary detail.</param>
        /// <param name="currentCompanyPaymentPackage">The current company payment package.</param>
        /// <param name="currentDiscountCodeUsage">The current discount code usage.</param>
        /// <param name="newtotalAmount">The newtotal amount.</param>
        /// <param name="currentTotalAmount">The current total amount.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="dateDifference">The date difference.</param>
        /// <returns></returns>
        public static decimal GetProrataAmount(PaymentSummaryDetails paymentSummaryDetail, CompanyPaymentPackage currentCompanyPaymentPackage, DiscountCodeUsage currentDiscountCodeUsage, decimal newtotalAmount, decimal currentTotalAmount, DateTime endDate, int dateDifference)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int anualDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
                decimal educationalDiscount = decimal.Parse(Utils.GetSystemValue("EducationalDiscount"));
                int datesPerCycle = 0;
                FinanceBL financeBL = new FinanceBL(dataContext);

                if (currentCompanyPaymentPackage.PaymentDurationCodeId == anualDurationCodeId)
                {
                    datesPerCycle = (int)(endDate - endDate.AddYears(-1)).TotalDays;
                }
                else
                {
                    datesPerCycle = (int)(endDate - endDate.AddMonths(-1)).TotalDays;
                }

                newtotalAmount = newtotalAmount * dateDifference / datesPerCycle;
                currentTotalAmount = currentTotalAmount * dateDifference / datesPerCycle;

                if (paymentSummaryDetail.IsEducationPackage)
                {
                    newtotalAmount = newtotalAmount * (100 - educationalDiscount) / 100;
                }
                else if (paymentSummaryDetail.DiscountCodeUsageToApply != null)
                {
                    newtotalAmount = financeBL.GetDiscountedAmount(paymentSummaryDetail.CompanyId, newtotalAmount, paymentSummaryDetail.PaymentDurationTypeCodeId, paymentSummaryDetail.DiscountCodeUsageToApply, paymentSummaryDetail.PackageStartDate, endDate);
                }

                if (currentCompanyPaymentPackage.IsEducationalPackage)
                {
                    currentTotalAmount = currentTotalAmount * (100 - educationalDiscount) / 100;
                }
                else if (currentDiscountCodeUsage != null)
                {
                    currentTotalAmount = financeBL.GetDiscountedAmount(paymentSummaryDetail.CompanyId, currentTotalAmount, currentCompanyPaymentPackage.PaymentDurationCodeId, currentDiscountCodeUsage, paymentSummaryDetail.PackageStartDate, endDate);
                }

                return (newtotalAmount - currentTotalAmount);
            }
        }

        /// <summary>
        /// Creates the company payment summary.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="nextCycleStartingDate">The next cycle starting date.</param>
        /// <param name="paymentSummaryDetail">The payment summary detail.</param>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="shouldMonthlyAgentProcessed">if set to <c>true</c> [should monthly agent processed].</param>
        /// <param name="paymentMethodCodeId">The payment method code identifier.</param>
        /// <param name="companyPaymentPackage">The company payment package.</param>
        /// <returns></returns>
        private static CompanyPaymentSummary CreateCompanyPaymentSummary(decimal amount, DateTime nextCycleStartingDate,
            PaymentSummaryDetails paymentSummaryDetail, DiscountCodeUsage discountCodeUsage,
            DateTime fromDate, DateTime toDate, bool shouldMonthlyAgentProcessed, int? paymentMethodCodeId, CompanyPaymentPackage companyPaymentPackage)
        {
            CompanyPaymentSummary companyPaymentSummary = new CompanyPaymentSummary()
            {
                CompanyId = paymentSummaryDetail.CompanyId,
                ShouldProcess = (paymentSummaryDetail.PaymentMethodCodeId != null && paymentSummaryDetail.ShouldProcess),
                IsMonthlyAgentProcessed = shouldMonthlyAgentProcessed,
                CompanyPaymentPackage = companyPaymentPackage,
                IsImmidiateFutureRecordCreated = false,
                Amount = decimal.Round((amount <= 0 ? 0 : amount), 2),
                NextPaymentCycleStartingDate = nextCycleStartingDate,
                FromDate = fromDate,
                ToDate = toDate,
                CreatedBy = paymentSummaryDetail.UserId,
                LastUpdatedBy = paymentSummaryDetail.UserId,
                CreatedDate = Utils.Today,
                LastUpdatedDate = Utils.Today,
                PaymentMethodCodeId = paymentMethodCodeId
            };
            if (!paymentSummaryDetail.IsEducationPackage && discountCodeUsage != null) //we should only include the discound usage to summary if the company is not educational
            {
                companyPaymentSummary.DiscountCodeUsageId = discountCodeUsage.DiscountCodeUsageId;
            }
            return companyPaymentSummary;
        }

        /// <summary>
        /// Gets the payment package summaries.
        /// </summary>
        /// <param name="paymentSummaryDetail">The payment summary detail.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static List<CompanyPaymentSummary> GetPaymentPackageSummaries(PaymentSummaryDetails paymentSummaryDetail, DateTime dateToConsider, StageBitzDB dataContext)
        {
            int anualDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
            int invoiceMethodCodeId = Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE");
            List<CompanyPaymentSummary> summaryList = new List<CompanyPaymentSummary>();
            DateTime nextCycleStartDate = Utils.Today;
            decimal totalDue = 0;
            decimal educationalDiscount = decimal.Parse(Utils.GetSystemValue("EducationalDiscount"));
            FinanceBL financeBL = new FinanceBL(dataContext);
            CompanyBL companyBL = new CompanyBL(dataContext);
            CompanyPaymentSummary existingCompanyPackageSummary = dataContext.CompanyPaymentSummaries.Where(cps => cps.CompanyId == paymentSummaryDetail.CompanyId).OrderByDescending(cps => cps.CompanyPaymentSummaryId).FirstOrDefault();
            CompanyPaymentPackage currentCompanyPaymentPackage = financeBL.GetPaymentPackageForCompanyByDay(dateToConsider, paymentSummaryDetail.CompanyId);

            if (existingCompanyPackageSummary != null && companyBL.IsFreeTrialCompany(paymentSummaryDetail.CompanyId)) //check whether there is an upgrade/downgrade during the freetrial
            {
                existingCompanyPackageSummary = null;
            }

            if (existingCompanyPackageSummary == null || currentCompanyPaymentPackage == null)
            {
                //This is for the very first time from UI. So return the amount based on promotional and educational discount amount.
                totalDue = financeBL.CalculateALLPackageAmountsByPeriod(paymentSummaryDetail.ProjectPaymentPackageTypeId, paymentSummaryDetail.InventoryPaymentPackageTypeId, paymentSummaryDetail.PaymentDurationTypeCodeId);
                if (paymentSummaryDetail.IsEducationPackage)
                {
                    totalDue = totalDue * (100 - educationalDiscount) / 100;
                }
                else if (paymentSummaryDetail.DiscountCodeUsageToApply != null)
                {
                    DateTime endDate = paymentSummaryDetail.PaymentDurationTypeCodeId == anualDurationCodeId ? paymentSummaryDetail.PackageStartDate.AddYears(1) : paymentSummaryDetail.PackageStartDate.AddMonths(1);
                    totalDue = financeBL.GetDiscountedAmount(paymentSummaryDetail.CompanyId, totalDue, paymentSummaryDetail.PaymentDurationTypeCodeId, paymentSummaryDetail.DiscountCodeUsageToApply, paymentSummaryDetail.PackageStartDate, endDate);
                }
                nextCycleStartDate = paymentSummaryDetail.PaymentDurationTypeCodeId == anualDurationCodeId ? paymentSummaryDetail.PackageStartDate.AddYears(1) : paymentSummaryDetail.PackageStartDate.AddMonths(1);
                summaryList.Add(CreateCompanyPaymentSummary(totalDue, nextCycleStartDate, paymentSummaryDetail, paymentSummaryDetail.DiscountCodeUsageToApply, paymentSummaryDetail.PackageStartDate, nextCycleStartDate, (paymentSummaryDetail.PaymentMethodCodeId != null && paymentSummaryDetail.PaymentMethodCodeId == invoiceMethodCodeId), paymentSummaryDetail.PaymentMethodCodeId, paymentSummaryDetail.CompanyPaymentPackage));
            }
            else
            {
                //This happens when a user upgrades or daily agent makes repeat payments at the end of the Payment Cycle.
                DateTime endDate = existingCompanyPackageSummary.NextPaymentCycleStartingDate;

                int dateDifference = 0;
                InventoryPaymentPackageDetails currentInventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(currentCompanyPaymentPackage.InventoryPaymentPackageTypeId);
                InventoryPaymentPackageDetails newInventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(paymentSummaryDetail.InventoryPaymentPackageTypeId);
                ProjectPaymentPackageDetails currentProjectPaymentPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(currentCompanyPaymentPackage.ProjectPaymentPackageTypeId);
                ProjectPaymentPackageDetails newProjectPaymentPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(paymentSummaryDetail.ProjectPaymentPackageTypeId);

                decimal currentTotalAmount, newTotalAmount;

                //Just Get the Amounts from tables
                if (currentCompanyPaymentPackage.PaymentDurationCodeId == anualDurationCodeId)
                {
                    currentTotalAmount = currentInventoryPaymentPackageDetails.AnualAmount + currentProjectPaymentPackageDetails.AnualAmount;
                    newTotalAmount = newInventoryPaymentPackageDetails.AnualAmount + newProjectPaymentPackageDetails.AnualAmount;
                }
                else
                {
                    currentTotalAmount = currentInventoryPaymentPackageDetails.Amount + currentProjectPaymentPackageDetails.Amount;
                    newTotalAmount = newInventoryPaymentPackageDetails.Amount + newProjectPaymentPackageDetails.Amount;
                }
                if (dateToConsider < endDate && paymentSummaryDetail.HasPackageChanged) //this happens only when a user upgrades during the payment cycle
                {
                    dateDifference = (int)(endDate - dateToConsider).TotalDays;

                    totalDue = GetProrataAmount(paymentSummaryDetail, currentCompanyPaymentPackage, existingCompanyPackageSummary.DiscountCodeUsage, newTotalAmount, currentTotalAmount, endDate, dateDifference);
                    summaryList.Add(CreateCompanyPaymentSummary(totalDue, endDate, paymentSummaryDetail, paymentSummaryDetail.DiscountCodeUsageToApply, dateToConsider, endDate, (paymentSummaryDetail.PaymentMethodCodeId != null && paymentSummaryDetail.PaymentMethodCodeId == invoiceMethodCodeId), paymentSummaryDetail.PaymentMethodCodeId, paymentSummaryDetail.CompanyPaymentPackage));
                }
                else
                {
                    //this get executed by a user when changing the package during Agent down or on package virtual end date or during a gap filling execution
                    // currentCompanyPaymentPackage will always be the existing package.
                    DateTime startDate, tempNextCycleDate = endDate;
                    while (tempNextCycleDate <= dateToConsider)
                    {
                        if (tempNextCycleDate == dateToConsider && paymentSummaryDetail.IsUserAction)
                        {
                            break;
                        }
                        //Find the next Package Start Date based on the duration
                        startDate = tempNextCycleDate;

                        tempNextCycleDate = currentCompanyPaymentPackage.PaymentDurationCodeId == anualDurationCodeId ? tempNextCycleDate.AddYears(1) : tempNextCycleDate.AddMonths(1);
                        decimal recordTotalAmount = currentTotalAmount;
                        DiscountCodeUsage discountCodeUsage = null;
                        //Get the relavent education or Discount
                        if (currentCompanyPaymentPackage.IsEducationalPackage)
                        {
                            recordTotalAmount = recordTotalAmount * (100 - educationalDiscount) / 100;
                        }
                        else
                        {
                            //Get the DiscountCode Usage for the Day
                            discountCodeUsage = financeBL.GetDiscountCodeUsageByDate(startDate, currentCompanyPaymentPackage.CompanyId);
                            if (discountCodeUsage != null)
                            {
                                recordTotalAmount = financeBL.GetDiscountedAmount(currentCompanyPaymentPackage.CompanyId, recordTotalAmount, currentCompanyPaymentPackage.PaymentDurationCodeId, discountCodeUsage, startDate, tempNextCycleDate);
                            }
                        }
                        if (paymentSummaryDetail.PaymentMethodCodeId != null)
                        { //this will set is monthly agent processed to true for the past records when gap filling if the user has selected the invoice option.
                            summaryList.Add(CreateCompanyPaymentSummary(recordTotalAmount, tempNextCycleDate, paymentSummaryDetail, discountCodeUsage, startDate, tempNextCycleDate, (paymentSummaryDetail.PaymentMethodCodeId == invoiceMethodCodeId), paymentSummaryDetail.PaymentMethodCodeId, currentCompanyPaymentPackage));
                        }
                        else
                        {
                            summaryList.Add(CreateCompanyPaymentSummary(recordTotalAmount, tempNextCycleDate, paymentSummaryDetail, discountCodeUsage, startDate, tempNextCycleDate, (currentCompanyPaymentPackage.PaymentMethodCodeId != null && currentCompanyPaymentPackage.PaymentMethodCodeId == invoiceMethodCodeId), currentCompanyPaymentPackage.PaymentMethodCodeId, currentCompanyPaymentPackage));
                        }
                    }
                    if (tempNextCycleDate == dateToConsider && paymentSummaryDetail.IsUserAction) //if the user do any pricing plan change on the same summmnary end date, this will calculate amounts according to the new selections
                    {
                        startDate = tempNextCycleDate;
                        tempNextCycleDate = paymentSummaryDetail.PaymentDurationTypeCodeId == anualDurationCodeId ? tempNextCycleDate.AddYears(1) : tempNextCycleDate.AddMonths(1);
                        decimal recordTotalAmount = newTotalAmount;

                        if (paymentSummaryDetail.PaymentDurationTypeCodeId == anualDurationCodeId)
                        {
                            recordTotalAmount = newInventoryPaymentPackageDetails.AnualAmount + newProjectPaymentPackageDetails.AnualAmount;
                        }
                        else
                        {
                            recordTotalAmount = newInventoryPaymentPackageDetails.Amount + newProjectPaymentPackageDetails.Amount;
                        }
                        //Get the relevant education or Discount
                        if (paymentSummaryDetail.IsEducationPackage)
                        {
                            recordTotalAmount = recordTotalAmount * (100 - educationalDiscount) / 100;
                        }
                        else if (paymentSummaryDetail.DiscountCodeUsageToApply != null)
                        {
                            recordTotalAmount = financeBL.GetDiscountedAmount(paymentSummaryDetail.CompanyId, recordTotalAmount, paymentSummaryDetail.PaymentDurationTypeCodeId, paymentSummaryDetail.DiscountCodeUsageToApply, startDate, tempNextCycleDate);
                        }
                        summaryList.Add(CreateCompanyPaymentSummary(recordTotalAmount, tempNextCycleDate, paymentSummaryDetail, paymentSummaryDetail.DiscountCodeUsageToApply, startDate, tempNextCycleDate, paymentSummaryDetail.PaymentMethodCodeId != null && paymentSummaryDetail.PaymentMethodCodeId == invoiceMethodCodeId, paymentSummaryDetail.PaymentMethodCodeId, paymentSummaryDetail.CompanyPaymentPackage));
                    }
                    if (paymentSummaryDetail.HasPackageChanged && dateToConsider > endDate) // user upgrade after the yearly cycle (Calculcate Pro rata)
                    {
                        dateDifference = (int)(tempNextCycleDate - dateToConsider).TotalDays;
                        totalDue = GetProrataAmount(paymentSummaryDetail, currentCompanyPaymentPackage, summaryList.Count() > 0 ? summaryList.Last().DiscountCodeUsage : null, newTotalAmount, currentTotalAmount, tempNextCycleDate, dateDifference);
                        summaryList.Add(CreateCompanyPaymentSummary(totalDue, tempNextCycleDate, paymentSummaryDetail, paymentSummaryDetail.DiscountCodeUsageToApply, dateToConsider, tempNextCycleDate, paymentSummaryDetail.PaymentMethodCodeId != null && paymentSummaryDetail.PaymentMethodCodeId == invoiceMethodCodeId, paymentSummaryDetail.PaymentMethodCodeId, paymentSummaryDetail.CompanyPaymentPackage));
                    }
                }
            }
            return summaryList;
        }

        /// <summary>
        /// Updates the payment summary for free trial company by SB admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="isCompanySuspend">The is company suspend.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dataContext">The data context.</param>
        public static void UpdatePaymentSummaryForFreeTrialCompanyBySBAdmin(int companyId, DiscountCodeUsage discountCodeUsage, bool? isCompanySuspend, int userId, StageBitzDB dataContext)
        {
            UpdatePaymentSummaryForFreeTrialCompany(companyId, discountCodeUsage, isCompanySuspend, userId, dataContext);
        }

        /// <summary>
        /// Updates the payment summary for free trial company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="isCompanySuspend">The is company suspend.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dataContext">The data context.</param>
        public static void UpdatePaymentSummaryForFreeTrialCompany(int companyId, DiscountCodeUsage discountCodeUsage, bool? isCompanySuspend, int userId, StageBitzDB dataContext)
        {
            CompanyBL companyBL = new CompanyBL(dataContext);

            if (companyBL.IsFreeTrialStatusIncludedFortheDay(companyId, Utils.Today))
            {
                if (isCompanySuspend != null)
                {
                    CompanyPaymentSummary existingCompanyPackageSummary = GetCurrentCompanyPaymentSummary(companyId, dataContext);
                    if (existingCompanyPackageSummary != null)
                    {
                        existingCompanyPackageSummary.ShouldProcess = !isCompanySuspend.Value;
                        existingCompanyPackageSummary.LastUpdatedDate = Utils.Today;
                        existingCompanyPackageSummary.LastUpdatedBy = userId;
                    }
                }
                else
                {
                    FinanceBL financeBL = new FinanceBL(dataContext);
                    CompanyPaymentPackage companyPaymentPackage = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);

                    if (companyPaymentPackage != null)
                    {
                        PaymentSummaryDetails paymentSummaryDetails = new PaymentSummaryDetails()
                        {
                            CompanyPaymentPackage = companyPaymentPackage,
                            CompanyId = companyId,
                            ShouldProcess = true,
                            UserId = userId,
                            PackageStartDate = companyPaymentPackage.StartDate,
                            PaymentMethodCodeId = companyPaymentPackage.PaymentMethodCodeId,
                            HasPackageChanged = false,
                            ProjectPaymentPackageTypeId = companyPaymentPackage.ProjectPaymentPackageTypeId,
                            InventoryPaymentPackageTypeId = companyPaymentPackage.InventoryPaymentPackageTypeId,
                            IsEducationPackage = companyPaymentPackage.IsEducationalPackage,
                            PaymentDurationTypeCodeId = companyPaymentPackage.PaymentDurationCodeId,
                        };

                        if (!companyPaymentPackage.IsEducationalPackage) //we should only include the discound usage to summary if the company is not educational
                        {
                            paymentSummaryDetails.DiscountCodeUsageToApply = discountCodeUsage;
                        }

                        CreateCompanyPaymentSummaries(paymentSummaryDetails, Utils.Today, dataContext);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the pricing plan and payment summary when closing free trial.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dataContext">The data context.</param>
        public static void UpdatePricingPlanAndPaymentSummaryClosingFreeTrial(int companyId, int userId, StageBitzDB dataContext)
        {
            //if this is the last free trial project which is going to be closed we should end the free trial also take the start date of the selected package to Today
            //usually a company will have one free trial project, but when clearing finance a company can have more free trial projects,
            //so closing one free trial project doesn't end the free trial of that company.
            ProjectBL projectBL = new ProjectBL(dataContext);
            FinanceBL financeBL = new FinanceBL(dataContext);
            if (projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId, Utils.Today).Count() == 1)
            {
                CompanyPaymentPackage currentPricingPlan = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);
                if (currentPricingPlan != null)
                {
                    currentPricingPlan.StartDate = Utils.Today;
                }
                DiscountCodeUsage discountCodeUsage = financeBL.GetDiscountCodeUsageByDate(Utils.Today, companyId);
                UpdatePaymentSummaryForFreeTrialCompany(companyId, discountCodeUsage, null, userId, dataContext);
                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the current company payment summary.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        private static CompanyPaymentSummary GetCurrentCompanyPaymentSummary(int companyId, StageBitzDB dataContext)
        {
            return dataContext.CompanyPaymentSummaries.Where(cps => cps.CompanyId == companyId).OrderByDescending(cps => cps.CompanyPaymentSummaryId).FirstOrDefault();
        }

        /// <summary>
        /// Creates the company payment summaries.
        /// This method will be called when a user selects/upgrades/downgrades a pricing plan . Also when the payment cycle ends Daily agent will call it for the repeat payments
        /// </summary>
        /// <param name="paymentSummaryDetail">The payment summary detail.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <param name="dataContext">The data context.</param>
        public static void CreateCompanyPaymentSummaries(PaymentSummaryDetails paymentSummaryDetail, DateTime dateToConsider, StageBitzDB dataContext)
        {
            CompanyBL companyBL = new CompanyBL(dataContext);
            CompanyPaymentSummary existingCompanyPackageSummary = GetCurrentCompanyPaymentSummary(paymentSummaryDetail.CompanyId, dataContext);

            List<CompanyPaymentSummary> paymentPackageSummaries = GetPaymentPackageSummaries(paymentSummaryDetail, dateToConsider, dataContext);
            if (paymentPackageSummaries.Count > 0)
            {
                //check whether there is an upgrade/downgrade during the freetrial
                if (existingCompanyPackageSummary != null && companyBL.IsFreeTrialStatusIncludedFortheDay(paymentSummaryDetail.CompanyId, dateToConsider))
                {
                    dataContext.CompanyPaymentSummaries.DeleteObject(existingCompanyPackageSummary);
                }

                foreach (CompanyPaymentSummary companyPaymentSummary in paymentPackageSummaries)
                {
                    dataContext.CompanyPaymentSummaries.AddObject(companyPaymentSummary);
                }
            }

            //TODO - for invoice option we need to send an email to SB admin (Invoice Request) Repeat Payment Email
        }

        /// <summary>
        /// Creates the payment summaries.
        /// This will create payment summaries/Invoice Requests for the repeat payments to be charged at the end of the cycle
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        public static void CreatePaymentSummaries(DateTime dateToConsider)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                FinanceBL financeBL = new FinanceBL(dataContext);
                CompanyBL companyBL = new CompanyBL(dataContext);

                //This will get all the CompanyPaymentPackages that needs to be Charged for considering cycle
                List<CompanyPaymentPackage> companyPaymentPackages = (from cpp in dataContext.CompanyPaymentPackages
                                                                      join cps in dataContext.CompanyPaymentSummaries on cpp.CompanyId equals cps.CompanyId
                                                                      where cps.IsImmidiateFutureRecordCreated == false
                                                                      && cpp.StartDate <= dateToConsider && (cpp.EndDate > dateToConsider || cpp.EndDate == null)
                                                                      && cps.NextPaymentCycleStartingDate <= dateToConsider
                                                                      select cpp).Distinct().ToList();

                foreach (CompanyPaymentPackage companyPaymentPackage in companyPaymentPackages)
                {
                    Data.Company company = companyPaymentPackage.Company;

                    PaymentSummaryDetails paymentSummaryDetails = new PaymentSummaryDetails()
                    {
                        CompanyPaymentPackage = companyPaymentPackage,
                        CompanyId = companyPaymentPackage.CompanyId,
                        ShouldProcess = !(companyBL.IsCompanySuspended(companyPaymentPackage.CompanyId) || companyBL.HasCompanySuspendedbySBAdmin(companyPaymentPackage.CompanyId)),
                        UserId = 0,
                        PackageStartDate = companyPaymentPackage.StartDate,
                        PaymentMethodCodeId = companyPaymentPackage.PaymentMethodCodeId,
                        HasPackageChanged = false,
                        ProjectPaymentPackageTypeId = companyPaymentPackage.ProjectPaymentPackageTypeId,
                        InventoryPaymentPackageTypeId = companyPaymentPackage.InventoryPaymentPackageTypeId,
                        IsEducationPackage = companyPaymentPackage.IsEducationalPackage,
                        PaymentDurationTypeCodeId = companyPaymentPackage.PaymentDurationCodeId,
                        DiscountCodeUsageToApply = financeBL.GetLatestDiscountCodeUsage(companyPaymentPackage.CompanyId)
                    };
                    //Get IsImmidiateFutureRecordCreated "False" Future Anual Summary records and make them as "True".(To commit as Processed)
                    var unprocessedFutureSummaries = (from cps in dataContext.CompanyPaymentSummaries
                                                      where cps.IsImmidiateFutureRecordCreated == false &&
                                                      cps.CompanyId == companyPaymentPackage.CompanyId &&
                                                      cps.ToDate <= dateToConsider
                                                      select cps).ToList();

                    foreach (CompanyPaymentSummary cps in unprocessedFutureSummaries)
                    {
                        cps.IsImmidiateFutureRecordCreated = true;
                        cps.LastUpdatedDate = Utils.Today;
                        cps.LastUpdatedBy = 0;
                    }

                    CreateCompanyPaymentSummaries(paymentSummaryDetails, dateToConsider, dataContext);
                }

                dataContext.SaveChanges();
            }
        }
    }
}