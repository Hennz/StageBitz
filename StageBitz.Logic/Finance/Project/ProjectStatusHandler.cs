using System;
using System.Collections.Generic;
using System.Linq;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;

namespace StageBitz.Logic.Finance.Project
{
    /// <summary>
    /// Handler class for project statuses
    /// </summary>
    public class ProjectStatusHandler
    {
        public enum ProjectWarningStatus
        {
            /// <summary>
            /// Project does not have any warnings.
            /// </summary>
            NoWarning,


            /// <summary>
            /// Free Trial End warning
            /// </summary>
            FreeTrialGrace,

            /// <summary>
            /// Fully functional grace period after payment failure.
            /// </summary>
            GracePeriod,

            /// <summary>
            /// Manual suspension by user.
            /// </summary>
            Suspended,

            /// <summary>
            /// Manual closed by user.
            /// </summary>
            Closed,

            /// <summary>
            /// Payment failure
            /// </summary>
            PaymentFailed,

        }

        public class ProjectWarningInfo
        {
            public ProjectWarningStatus WarningStatus { get; set; }

            /// <summary>
            /// Days until the project is tranferred to the next expiration state. -1 if not applicable.
            /// </summary>
            public int DaysToExpiration { get; set; }

            public ProjectWarningInfo(ProjectWarningStatus warningStatus, int daysToExpiration)
            {
                WarningStatus = warningStatus;
                DaysToExpiration = daysToExpiration;
            }
        }

        /// <summary>
        /// Returns the warning status of the project based on it's current status and expiration details.
        /// </summary>
        public static ProjectWarningInfo GetProjectWarningStatus(int projectStatusCodeId, bool hasUserOptIn, DateTime? expirationDate)
        {
            string statusCode = Utils.GetCodeByCodeId(projectStatusCodeId).Value;
            double remainingDays = (statusCode == "ACTIVE" || expirationDate == null) ? -1 : (expirationDate.Value.Date - Utils.Today).TotalDays;

            ProjectWarningStatus warningStatus = ProjectWarningStatus.NoWarning;

            switch (statusCode)
            {
                case "FREETRIAL":
                    if (remainingDays <= 7 && !hasUserOptIn)
                    {
                        warningStatus = ProjectWarningStatus.FreeTrialGrace;
                    }
                    break;
                case "GRACEPERIOD":
                    warningStatus = ProjectWarningStatus.GracePeriod;
                    break;
                case "PAYMENTFAILED":
                    warningStatus = ProjectWarningStatus.PaymentFailed;
                    break;
                case "SUSPENDED":
                    warningStatus = ProjectWarningStatus.Suspended;
                    break;
                case "CLOSED":
                    warningStatus = ProjectWarningStatus.Closed;
                    break;
                default:
                    break;
            }

            return new ProjectWarningInfo(warningStatus, (int)Math.Round(remainingDays, 0));
        }

        /// <summary>
        /// To be called by the daily agent
        /// </summary>
        public static void UpdateProjectExpirations(DateTime dateToConsider, StageBitzDB dataContext)
        {
            //Get project status code ids
            int freeTrialCodeId = Utils.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId;
            int activeCodeId = Utils.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;
            int gracePeriodCodeId = Utils.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;
            int paymentFailedCodeId = Utils.GetCodeByValue("ProjectStatus", "PAYMENTFAILED").CodeId;
            int suspendedCodeId = Utils.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId;
            int freeTrialOptInCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;
            int freeTrialTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;


            FinanceBL financeBL = new FinanceBL(dataContext);
            CompanyBL companyBL = new CompanyBL(dataContext);
            int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;
            int freeTrialProjectTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;
            string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
            string supportEmail = Utils.GetSystemValue("FeedbackEmail");

            #region Free Trial ending pre-notice email

            //Get the free trial projects that are about to expire in 7 days, and notify them via email
            var freeTrialProjects = from p in dataContext.Projects
                                    where (p.ProjectStatusCodeId == freeTrialCodeId && p.ProjectTypeCodeId == freeTrialProjectTypeCodeId) &&
                                        p.ExpirationDate != null
                                    select new
                                    {
                                        Project = p,
                                        PaymentsSpecified = (dataContext.CreditCardTokens
                                                            .Where(tk => tk.RelatedTableName == "Company" && tk.RelatedId == p.CompanyId && tk.IsActive == true)
                                                            .FirstOrDefault() != null)
                                    };

            foreach (var projectInfo in freeTrialProjects)
            {
                StageBitz.Data.Project p = projectInfo.Project;

                int datediff = (p.ExpirationDate.Value.Date - dateToConsider).Days;
                if (datediff <= 7 && datediff >= 0)
                {
                    string freeTrialGraceEmailType = "PROJECTFREETRIALGRACE";

                    //Check if the free trial expiration pre-notice has already been sent
                    Email preNoticeEmail = dataContext.Emails.Where(em => em.EmailType == freeTrialGraceEmailType && em.RelatedId == p.ProjectId).FirstOrDefault();

                    if (preNoticeEmail == null)
                    {
                        Data.User companyPrimaryAdmin = //p.Company.CompanyUsers.Where(cu => cu.IsActive == true && cu.CompanyUserTypeCodeId == companyPrimaryAdminCodeID).FirstOrDefault().User;
                                                        (from cu in p.Company.CompanyUsers
                                                         join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                         where cu.IsActive && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                                         select cu).FirstOrDefault().User;

                        string companyBillingUrl = string.Format("{0}/Company/CompanyFinancialDetails.aspx?companyid={1}", userWebUrl, p.CompanyId);
                        string createProjectUrl = string.Format("{0}/Project/AddNewProject.aspx?companyid={1}", userWebUrl, p.CompanyId);
                        EmailSender.SendProjectExpirationNoticeToCompanyAdmin(freeTrialGraceEmailType, p.ProjectId, companyPrimaryAdmin.Email1, companyPrimaryAdmin.FirstName, p.ProjectName, Utils.GetLongDateHtmlString(p.ExpirationDate.Value), companyBillingUrl, createProjectUrl, supportEmail);
                    }
                }
            }

            #endregion

            #region Project Status Updates

            // this excute after project ExpirationDate is exceded. eg:- if the agent is down for 7 days, project status should be suspended.
            var projects = from p in dataContext.Projects
                           where (p.ProjectStatusCodeId == freeTrialCodeId ||
                               p.ProjectStatusCodeId == gracePeriodCodeId ||
                               p.ProjectStatusCodeId == suspendedCodeId) &&
                               p.ExpirationDate != null &&
                               dateToConsider >= p.ExpirationDate
                           select new
                           {
                               Project = p,
                               PaymentsSpecified = (dataContext.CreditCardTokens
                                                   .Where(tk => tk.RelatedTableName == "Company" && tk.RelatedId == p.CompanyId && tk.IsActive == true)
                                                   .FirstOrDefault() != null)
                           };

            foreach (var projectInfo in projects)
            {
                StageBitz.Data.Project p = projectInfo.Project;

                Data.User companyPrimaryAdmin = // p.Company.CompanyUsers.Where(cu => cu.IsActive == true && cu.CompanyUserTypeCodeId == companyPrimaryAdminCodeID).FirstOrDefault().User;
                                                (from cu in p.Company.CompanyUsers
                                                 join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                 where cu.IsActive && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                                 select cu).FirstOrDefault().User;

                string emailTemplateType = string.Empty;

                //Next expiration date is 7 days from current expiration date
                DateTime nextExpirationDate = p.ExpirationDate.Value.Date.AddDays(7);

                if (p.ProjectStatusCodeId == freeTrialCodeId)
                {
                    //Get the current Company package to check.
                    CompanyPaymentPackage companyPaymentPackage = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(p.CompanyId, dateToConsider);
                    DiscountCodeUsage discountCodeUsage = financeBL.GetDiscountCodeUsageByDate(dateToConsider, p.CompanyId);
                    //There are only two possibilities. Either user has given his permission or nothing has done.
                    //Check whether user has given the approval to continue the Free trial project.
                    if (p.ProjectTypeCodeId == freeTrialOptInCodeId)
                    {
                        // He has optin not to continue
                        if (companyPaymentPackage == null)
                        {
                            p.ProjectStatusCodeId = suspendedCodeId;
                        }
                        // He has optin to continue, with zero project package
                        else if ((companyPaymentPackage != null
                                && Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(companyPaymentPackage.ProjectPaymentPackageTypeId).ProjectCount == 0) ||
                                (!companyPaymentPackage.PaymentMethodCodeId.HasValue && (discountCodeUsage == null || discountCodeUsage.DiscountCode.Discount != 100)))
                        {
                            p.ProjectStatusCodeId = suspendedCodeId;
                        }
                        else
                        {
                            p.ProjectStatusCodeId = activeCodeId;
                        }
                    }
                    else
                    {
                        p.ProjectStatusCodeId = suspendedCodeId;
                        emailTemplateType = "PROJECTFREETRIALSUSPENDED";
                    }

                    p.ExpirationDate = null;
                    p.LastUpdatedDate = Utils.Now;
                    p.LastUpdatedByUserId = 0;
                }
                else if (p.ProjectStatusCodeId == gracePeriodCodeId)
                {
                    p.ProjectStatusCodeId = paymentFailedCodeId;
                    p.LastUpdatedDate = Utils.Now;
                    p.LastUpdatedByUserId = 0;
                }
                else if (p.ProjectStatusCodeId == suspendedCodeId && (p.ProjectTypeCodeId == freeTrialOptInCodeId || p.ProjectTypeCodeId == freeTrialTypeCodeId))
                {
                    // if free trial project is manually suspended during free trial, set ExpirationDate to null at the end of free trial period.
                    p.ExpirationDate = null;
                }

                //Send the email notice if required
                if (emailTemplateType != string.Empty)
                {
                    string companyBillingUrl = string.Format("{0}/Company/CompanyFinancialDetails.aspx?companyid={1}", userWebUrl, p.CompanyId);
                    string createProjectUrl = string.Format("{0}/Project/AddNewProject.aspx?companyid={1}", userWebUrl, p.CompanyId);
                    string expirationDate = string.Empty;

                    if (p.ExpirationDate != null)
                    {
                        expirationDate = Utils.GetLongDateHtmlString(p.ExpirationDate.Value);
                    }
                    string pricingPlanURL = string.Format("{0}/Company/CompanyPricingPlans.aspx?companyId={1}", userWebUrl, p.CompanyId);

                    EmailSender.SendProjectExpirationNoticeToCompanyAdmin(emailTemplateType, p.ProjectId, companyPrimaryAdmin.Email1, companyPrimaryAdmin.FirstName, p.ProjectName, expirationDate, companyBillingUrl, createProjectUrl, supportEmail, pricingPlanURL);
                }
            }

            #endregion
        }


        /// <summary>
        /// Gets the previuos project status from history.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public static int GetPreviuosProjectStatusFromHistory(int projectId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                var currentStatusHistory = (from psh in dataContext.ProjectStatusHistories
                                            where psh.ProjectId == projectId
                                            select psh).OrderByDescending(psh => psh.ProjectStatusHistoryId).Take(2);

                if (currentStatusHistory != null && currentStatusHistory.Count() == 2)
                {
                    ProjectStatusHistory projectStatusHistory = (ProjectStatusHistory)currentStatusHistory.AsEnumerable().Last();
                    return projectStatusHistory.ProjectStatusCodeId;
                }
                return 0;
            }
        }
    }
}
