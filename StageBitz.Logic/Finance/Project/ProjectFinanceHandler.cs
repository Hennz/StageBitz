using FatZebra;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Logic.Finance.Project
{
    /// <summary>
    /// Finance handler for Projects
    /// </summary>
    public static class ProjectFinanceHandler
    {
        /// <summary>
        /// Gets the unprocessed company payment summaries.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <param name="ShouldConsiderAllSummaries">if set to <c>true</c> [should consider all summaries].</param>
        /// <returns></returns>
        public static List<CompanyPaymentSummary> GetUnProcessedCompanyPaymentSummaries(int companyId, StageBitzDB dataContext, DateTime dateToConsider, bool ShouldConsiderAllSummaries = false)
        {
            return (from cps in dataContext.CompanyPaymentSummaries
                    where cps.IsMonthlyAgentProcessed == false && cps.ShouldProcess == true && (companyId == 0 || cps.CompanyId == companyId)
                    && (ShouldConsiderAllSummaries || cps.FromDate <= dateToConsider)
                    select cps).ToList();
        }

        /// <summary>
        /// This is to be called by monthly agent and manual payment.
        /// </summary>
        public static bool ProcessInvoicesAndPayments(int companyId, DateTime dateToConsider, bool shouldCreateInvoices, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                FinanceSupport.InitializePaymentSettings();
                bool isPaymentSuccess = false;
                bool isManualPayment = (companyId == 0) ? false : true;
                List<Invoice> invoiceList = new List<Invoice>();

                if (shouldCreateInvoices)
                {
                    List<CompanyPaymentSummary> unProcessedPaymentSummaries = GetUnProcessedCompanyPaymentSummaries(0, dataContext, dateToConsider);

                    //Only consider companies which has payment packages
                    List<Data.Company> companies = (from c in dataContext.Companies
                                                    join cpp in dataContext.CompanyPaymentPackages on c.CompanyId equals cpp.CompanyId
                                                    select c).Distinct().ToList();

                    if (unProcessedPaymentSummaries.Count > 0)
                    {
                        CompanyBL companyBL = new CompanyBL(dataContext);

                        foreach (Data.Company company in companies)
                        {
                            try
                            {
                                List<CompanyPaymentSummary> unProcessedCompanyPaymentSummaries = unProcessedCompanyPaymentSummaries = unProcessedPaymentSummaries.Where(upcs => upcs.CompanyId == company.CompanyId).OrderBy(ups => ups.CompanyPaymentSummaryId).ToList();

                                //*******Consider payment summaries for the company*********
                                if (unProcessedCompanyPaymentSummaries != null && unProcessedCompanyPaymentSummaries.Count() > 0)
                                {
                                    if (!companyBL.IsFreeTrialCompany(company.CompanyId, dateToConsider))
                                    {
                                        decimal totalAmount = unProcessedCompanyPaymentSummaries.Sum(ups => ups.Amount);
                                        DateTime fromDate = unProcessedCompanyPaymentSummaries.First().FromDate;
                                        DateTime toDate = unProcessedCompanyPaymentSummaries.Last().ToDate;

                                        Invoice invoice = null;
                                        if (totalAmount > 0) //generate the invoice only if there is a due amount to pay
                                        {
                                            invoice = FinanceSupport.CreateInvoice(company.CompanyId, "Company", "PACKAGEFEE", string.Format("Payment for Company {0}", company.CompanyName), decimal.Round(totalAmount, 2), fromDate, toDate, dataContext);
                                        }

                                        foreach (CompanyPaymentSummary companyPackagePaymentSummary in unProcessedCompanyPaymentSummaries)
                                        {
                                            companyPackagePaymentSummary.IsMonthlyAgentProcessed = true;
                                            if (invoice != null)
                                            {
                                                companyPackagePaymentSummary.Invoice = invoice;
                                            }

                                            companyPackagePaymentSummary.LastUpdatedDate = Utils.Today;
                                            companyPackagePaymentSummary.LastUpdatedBy = 0;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                AgentErrorLog.WriteToErrorLog("Failed to create Invoice for projectId" + company.CompanyId);
                                AgentErrorLog.HandleException(ex);
                                isPaymentSuccess = false;
                            }
                        }
                        //Complete upto now.
                        dataContext.SaveChanges();
                    }
                    //Get all the Unprocessed Invoices to send to the payment gateway.
                    invoiceList = FinanceSupport.GetUnProcessedInvoicesByRelatedTable("Company", companyId, "PACKAGEFEE", dataContext);
                }
                else
                {
                    int companyStatusCodeID = 0;
                    if (!isManualPayment)
                    {
                        //Not Manual means, Payment Retry.
                        companyStatusCodeID = Utils.GetCodeByValue("CompanyStatus", "GRACEPERIOD").CodeId;
                    }

                    //Get all the Unprocessed Invoices to send to the payment gateway.
                    //For Grace period payment Retry process projectId is 0.
                    invoiceList = FinanceSupport.GetPaymentFailedInvoices("PACKAGEFEE", companyStatusCodeID, companyId, "Company", dataContext);
                }

                //Because we need Payment Log records before process Invoices.
                foreach (Invoice invoice in invoiceList)
                {
                    FinanceSupport.CreatePaymentLog(invoice.InvoiceID, dataContext);
                }
                dataContext.SaveChanges();

                //Get distinct company Ids within the unprocessed invoice list
                var companyIds = invoiceList.Select(inv => inv.RelatedID).Distinct();

                foreach (int distinctCompanyId in companyIds)
                {
                    isPaymentSuccess = ProcessInvoicesByCompany(invoiceList, distinctCompanyId, isManualPayment, dataContext);
                    if (isPaymentSuccess)
                    {
                        CompanyBL companyBL = new CompanyBL(dataContext);
                        companyBL.ActivateUnProcessedSummaries(distinctCompanyId, userId);
                    }
                }

                dataContext.SaveChanges();
                //For the Monthly Agent Runner the return value is not important but for the manual payment.
                return isPaymentSuccess;
            }
        }

        /// <summary>
        /// Determines whether payment failed invoices exist for given company
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static bool IsPaymentFailedInvoicesExistForCompany(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int packageFeeTypeCodeId = Utils.GetCodeByValue("InvoiceType", "PACKAGEFEE").CodeId;
                int processedInvoiceTypeCodeID = Utils.GetCodeByValue("InvoiceStatus", "PROCESSED").CodeId;

                var pendingInvoiceCount = (from i in dataContext.Invoices
                                           where i.RelatedTableName == "Company" && i.RelatedID == companyId
                                           && i.InvoiceTypeCodeID == packageFeeTypeCodeId
                                           && i.InvoiceStatusCodeId != processedInvoiceTypeCodeID
                                           select i).ToList().Count();
                return (pendingInvoiceCount > 0);
            }
        }

        /// <summary>
        /// Gets the company payment failure details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static string GetCompanyPaymentFailureDetails(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int invoiceFailedStatusCodeId = Utils.GetCodeIdByCodeValue("InvoiceStatus", "FAILED");
                int invoiceUnprocessedStatusCodeId = Utils.GetCodeIdByCodeValue("InvoiceStatus", "UNPROCESSED");

                //Get the latest payment failure Log. Because there can be N number of payment failures.
                var paymentFailureReason = (from i in dataContext.Invoices
                                            join pl in dataContext.PaymentLogs on i.InvoiceID equals pl.RelatedId
                                            where (i.InvoiceStatusCodeId == invoiceFailedStatusCodeId || i.InvoiceStatusCodeId == invoiceUnprocessedStatusCodeId)
                                            && i.RelatedID == companyId && i.RelatedTableName == "Company"
                                            && pl.RelatedTableName == "Invoice"
                                            orderby pl.CreatedDate descending
                                            select pl).FirstOrDefault();

                if (string.IsNullOrEmpty(paymentFailureReason.Description))
                {
                    return "Payment Failed (" + paymentFailureReason.CreatedDate + ")";
                }
                return paymentFailureReason.Description + " (" + paymentFailureReason.CreatedDate + ")";
            }
        }

        /// <summary>
        /// Processes the invoices by company.
        /// </summary>
        /// <param name="invoiceList">The invoice list.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="isManualPayment">if set to <c>true</c> [is manual payment].</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        private static bool ProcessInvoicesByCompany(List<Invoice> invoiceList, int companyId, bool isManualPayment, StageBitzDB dataContext)
        {
            CompanyBL companyBL = new CompanyBL(dataContext);

            List<Invoice> companyInvoices = invoiceList.Where(inv => inv.RelatedID == companyId).ToList();
            Data.Company company = dataContext.Companies.Where(c => c.CompanyId == companyId).FirstOrDefault();

            bool isAllPaymentsSuccess = true;

            foreach (Invoice invoice in companyInvoices)
            {
                bool isPaymentSuccess = ProcessInvoice(invoice, company, dataContext);

                if (!isPaymentSuccess)
                {
                    isAllPaymentsSuccess = false;
                }
            }

            List<Code> projectStatusCodes = Utils.GetCodesByCodeHeader("ProjectStatus");

            int suspendStateCodeId = projectStatusCodes.Where(c => c.Value == "SUSPENDED").SingleOrDefault().CodeId;
            int activeStateCodeId = projectStatusCodes.Where(c => c.Value == "ACTIVE").SingleOrDefault().CodeId;
            int graceperiodStateCodeId = projectStatusCodes.Where(c => c.Value == "GRACEPERIOD").SingleOrDefault().CodeId;
            int paymentfailedStateCodeId = projectStatusCodes.Where(c => c.Value == "PAYMENTFAILED").SingleOrDefault().CodeId;

            List<Code> companyStatusCodes = Utils.GetCodesByCodeHeader("CompanyStatus");
            int activeCompanyStateCodeId = companyStatusCodes.Where(c => c.Value == "ACTIVE").SingleOrDefault().CodeId;
            int gracePeriodCompanyStateCodeId = companyStatusCodes.Where(c => c.Value == "GRACEPERIOD").SingleOrDefault().CodeId;
            int paymentFailedCompanyStateCodeId = companyStatusCodes.Where(c => c.Value == "SUSPENDEDFORPAYMENTFAILED").SingleOrDefault().CodeId;

            List<Data.Project> projectList = dataContext.Projects.Where(p => p.CompanyId == companyId).ToList();

            if (!isAllPaymentsSuccess)
            {
                int previousCompanyStatusCodeId = company.CompanyStatusCodeId;

                // For active project, change it to grace period.
                if (company.CompanyStatusCodeId == activeCompanyStateCodeId)
                {
                    company.CompanyStatusCodeId = gracePeriodCompanyStateCodeId;
                    //Next expiration date is 7 days from today
                    company.ExpirationDate = Utils.Today.AddDays(7);
                    company.LastUpdatedByUserId = 0;
                    company.LastUpdatedDate = Utils.Now;
                }
                // For grace period project, if it exceeded the grace period change it to Payment Failed.
                else if (company.CompanyStatusCodeId == gracePeriodCompanyStateCodeId && company.ExpirationDate <= Utils.Today)
                {
                    company.CompanyStatusCodeId = paymentFailedCompanyStateCodeId;
                    company.LastUpdatedByUserId = 0;
                    company.LastUpdatedDate = Utils.Now;
                    company.ExpirationDate = null;
                }

                // Send email notice on payment failure. (Only for monthly process). exclude email if company is suspended by SB admin
                if (!isManualPayment && !companyBL.HasCompanySuspendedbySBAdmin(companyId))
                {
                    int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;
                    string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
                    string supportEmail = Utils.GetSystemValue("FeedbackEmail");

                    Data.User companyPrimaryAdmin = // project.Company.CompanyUsers.Where(cu => cu.IsActive == true && cu.CompanyUserTypeCodeId == companyPrimaryAdminCodeID).FirstOrDefault().User;
                                                    (from cu in company.CompanyUsers
                                                     join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                     where cu.IsActive && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                                     select cu).FirstOrDefault().User;

                    string companyBillingUrl = string.Format("{0}/Company/CompanyFinancialDetails.aspx?companyid={1}", userWebUrl, companyId);

                    // This sends only when the company status change to payment failed grace period.
                    if (previousCompanyStatusCodeId == activeCompanyStateCodeId && company.CompanyStatusCodeId == gracePeriodCompanyStateCodeId)
                    {
                        EmailSender.SendCompanyExpirationNoticeToCompanyAdmin("COMPANYGRACEPERIOD", companyId, companyPrimaryAdmin.Email1, companyPrimaryAdmin.FirstName, company.CompanyName, companyBillingUrl, supportEmail);
                    }
                    // This sends only when the company status change to Payment failed.
                    else if (previousCompanyStatusCodeId == gracePeriodCompanyStateCodeId && company.CompanyStatusCodeId == paymentFailedCompanyStateCodeId)
                    {
                        EmailSender.SendCompanyExpirationNoticeToCompanyAdmin("COMPANYPAYMENTFAILED", companyId, companyPrimaryAdmin.Email1, companyPrimaryAdmin.FirstName, company.CompanyName, companyBillingUrl, supportEmail);
                    }
                }
            }
            else
            {
                if (company.CompanyStatusCodeId == Utils.GetCodeIdByCodeValue("CompanyStatus", "GRACEPERIOD") ||
                        company.CompanyStatusCodeId == Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORPAYMENTFAILED"))
                {
                    company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
                    company.LastUpdatedByUserId = 0;
                    company.LastUpdatedDate = Utils.Now;
                    company.ExpirationDate = null;
                }
            }

            foreach (Data.Project project in projectList)
            {
                int currentProjectStatusCodeId = project.ProjectStatusCodeId;

                if (isAllPaymentsSuccess)
                {
                    int previousStatusCodeId = ProjectStatusHandler.GetPreviuosProjectStatusFromHistory(project.ProjectId);

                    // We need to consier whether current project status.
                    // If it is "Suspended", we are not going to make any change. Because when user tries to reactivate only, we are going to check for pending invoices.
                    // If it is "Active", we do nothing.
                    if (currentProjectStatusCodeId == paymentfailedStateCodeId || currentProjectStatusCodeId == graceperiodStateCodeId)
                    {
                        project.ProjectStatusCodeId = activeStateCodeId;
                        project.ExpirationDate = null;
                        project.LastUpdatedByUserId = 0;
                        project.LastUpdatedDate = Utils.Now;
                    }
                    else if (currentProjectStatusCodeId == suspendStateCodeId && previousStatusCodeId == graceperiodStateCodeId)
                    {
                        project.ExpirationDate = null;
                        project.LastUpdatedByUserId = 0;
                        project.LastUpdatedDate = Utils.Now;
                    }
                }
                else
                {
                    //Do not update if the Project status is Suspended or Payment Failed
                    if (currentProjectStatusCodeId == activeStateCodeId || currentProjectStatusCodeId == graceperiodStateCodeId)
                    {
                        if (project.ProjectStatusCodeId == activeStateCodeId)
                        {
                            project.ProjectStatusCodeId = graceperiodStateCodeId;
                            //Next expiration date is 7 days from today
                            project.ExpirationDate = Utils.Today.AddDays(7);
                            project.LastUpdatedByUserId = 0;
                            project.LastUpdatedDate = Utils.Now;
                        }
                        else if (project.ExpirationDate <= Utils.Today) //This handles payment retry. For Grace period project, if it exceeded the grace period change it to Payment Failed.
                        {
                            project.ProjectStatusCodeId = paymentfailedStateCodeId;
                            project.LastUpdatedByUserId = 0;
                            project.LastUpdatedDate = Utils.Now;
                        }
                    }
                }
            }

            return isAllPaymentsSuccess;
        }

        /// <summary>
        /// Gets the payment gateway log description.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private static string GetPaymentGatewayLogDescription(Response response)
        {
            StringBuilder sb = new StringBuilder();
            if (response.Errors.Count > 0)
            {
                sb.Append(string.Join("<br />", response.Errors));
            }
            else if (response.Result != null && ((Purchase)response.Result).Message != null)
            {
                sb.Append(string.Join("<br />", ((Purchase)response.Result).Message));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Processes the invoice.
        /// </summary>
        /// <param name="invoice">The invoice.</param>
        /// <param name="company">The company.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        private static bool ProcessInvoice(Invoice invoice, Data.Company company, StageBitzDB dataContext)
        {
            bool isSuccess = true;
            try
            {
                //Get the Payment History record which has not sent to the payment gateway.
                PaymentLog paymentLog = dataContext.PaymentLogs.Where(pl => pl.RelatedTableName == "Invoice" && pl.RelatedId == invoice.InvoiceID && pl.IsSentToPaymentGateway == false).FirstOrDefault();

                //Get the TokenID of the Company to pass over to Payment gateway.
                CreditCardToken creditCardToken = (from ct in dataContext.CreditCardTokens
                                                   where ct.RelatedTableName == "Company" && ct.RelatedId == company.CompanyId
                                                  && ct.IsActive == true
                                                   select ct).FirstOrDefault();

                bool isPaymentSuccess = false;

                if (creditCardToken != null)
                {
                    paymentLog.ReferenceNumber = Utils.GetSystemValue("ReferenceNumberPrefix") + paymentLog.PaymentLogId;

                    //Make the payment via the payment gateway.
                    //Multuply by 100 to send over to FatZebra.
                    //Utils.DecryptStringAES(creditCardToken.Token)
                    var response = Purchase.Create((int)(invoice.Amount * 100), Utils.DecryptStringAES(creditCardToken.Token), null, paymentLog.ReferenceNumber, Utils.GetSystemValue("SBServerIPAddress"));
                    isPaymentSuccess = response.Successful && response.Result != null && response.Result.Successful;

                    if (isPaymentSuccess)
                    {
                        //Update Invoice as Processed.
                        invoice.InvoiceStatusCodeId = Utils.GetCodeByValue("InvoiceStatus", "PROCESSED").CodeId;

                        //Create Receipt for the Invoice
                        FinanceSupport.CreateReceipt(company.CompanyId, "Company", string.Format("Payment for Company {0}", company.CompanyName), invoice.Amount, invoice.Transaction.TransactionID, dataContext);
                    }
                    else
                    {
                        //Update Invoice as Failed.
                        invoice.InvoiceStatusCodeId = Utils.GetCodeByValue("InvoiceStatus", "FAILED").CodeId;
                    }

                    //Update the Payment History record as Sent to payment gateway.
                    paymentLog.IsSentToPaymentGateway = true;
                    paymentLog.IsPaymentSuccess = isPaymentSuccess;
                    if (response.Result != null)
                        paymentLog.ResponseId = response.Result.ID;
                    paymentLog.Description = GetPaymentGatewayLogDescription(response);
                    paymentLog.CreditCardTokenId = creditCardToken.CreditCardTokenID;
                }
                else
                {
                    paymentLog.Description = "Credit card details not provided.";
                    //Update Invoice as Failed.
                    invoice.InvoiceStatusCodeId = Utils.GetCodeByValue("InvoiceStatus", "FAILED").CodeId;
                }

                invoice.LastUpdateDate = Utils.Now;
                invoice.LastUpdatedBy = 0;

                paymentLog.LastUpdatedDate = Utils.Now;
                paymentLog.LastUpdatedByUserId = 0;

                isSuccess = isPaymentSuccess;
            }
            catch (Exception ex)
            {
                AgentErrorLog.WriteToErrorLog("Failed to Process the Invoice for CompanyId " + invoice.RelatedID);
                AgentErrorLog.HandleException(ex);
                isSuccess = false;
            }
            return isSuccess;
        }
    }
}