using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Finance.Company;
using StageBitz.Logic.Finance.Project;
using StageBitz.Logic.Support;
using StageBitz.Reports;
using System;

namespace StageBitz.Server.Agent
{
    /// <summary>
    /// Daily task handler.
    /// </summary>
    public class DailyTaskHandler : TaskHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DailyTaskHandler"/> class.
        /// </summary>
        public DailyTaskHandler()
            : base("DAILY")
        {
        }

        /// <summary>
        /// Should run the task.
        /// </summary>
        /// <param name="lastRunDate">The last run date.</param>
        /// <returns></returns>
        protected override bool ShouldRunTask(DateTime lastRunDate)
        {
            TimeSpan elapsed = (Utils.Now - lastRunDate);
            TimeSpan day = new TimeSpan(1, 0, 0, 0);
            //return (elapsed > day);
            bool shouldRunTask = false;
            TimeSpan ts;
            if (TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts))
            {
                if (elapsed >= day || ((Utils.Now >= (Utils.Today + ts)) && (lastRunDate < (Utils.Today + ts))))
                {
                    shouldRunTask = true;
                }
            }
            return shouldRunTask;
        }

        /// <summary>
        /// Clears the temporary document media.
        /// </summary>
        private void ClearTemporaryDocumentMedia()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                dataContext.DeleteUnusedDocumentMedia("Company");
            }
        }

        /// <summary>
        /// Performs business logic actions specified for this task handler.
        /// </summary>
        protected override void PerformActions()
        {
            ClearTemporaryDocumentMedia();

            DateTime dateToConsider = Utils.Today.Date;
            TimeSpan ts;
            if (TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts))
            {
                if (Utils.Now < (Utils.Today + ts))
                {
                    dateToConsider = dateToConsider.AddDays(-1);
                }
            }
            ProjectUsageHandler.CreatePaymentSummaries(dateToConsider);
            //Monthly retry process with in the Graceperiod
            ProjectFinanceHandler.ProcessInvoicesAndPayments(0, dateToConsider, false, 0);

            // Run CompanyStatusHandler and  ProjectStatusHandler in same datacontext
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ProjectStatusHandler.UpdateProjectExpirations(dateToConsider, dataContext);
                CompanyStatusHandler.UpdateCompanyExpirations(dateToConsider, dataContext);

                dataContext.SaveChanges();
            }

            CompanyStatusHandler.SuspendNoPaymentOptionCompanies(dateToConsider);

            SendUserEmailNotifications(dateToConsider);
            SendBookingNotifications();
            SendBookingOverdueAndDelayedEmails(dateToConsider.AddDays(1));
            DeleteOldExportedZipFiles();
        }

        /// <summary>
        /// Deletes the old exported zip files.
        /// </summary>
        public void DeleteOldExportedZipFiles()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                string basePath = Utils.GetSystemValue("ExportFileDirectoryLocation");
                if (!string.IsNullOrEmpty(basePath))
                {
                    ExportFilesHandler fileHandler = new ExportFilesHandler(basePath, dataContext);
                    fileHandler.DeleteOldExportedZipFiles();
                }
            }
        }

        /// <summary>
        /// Sends the booking overdue and delayed emails.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        private void SendBookingOverdueAndDelayedEmails(DateTime dateToConsider)
        {
            BookingNotificationHandler.SendBookingOverdueEmails(dateToConsider);
            BookingNotificationHandler.SendBookingDelayedEmails(dateToConsider);
        }

        /// <summary>
        /// Sends the user email notifications.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        private void SendUserEmailNotifications(DateTime dateToConsider)
        {
            UserEmailNotificationHandler.SendEmailNotifications(dateToConsider);
        }

        /// <summary>
        /// Sends the user email notifications.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        private void SendBookingNotifications()
        {
            BookingNotificationHandler.SendNotifications();
        }
    }
}