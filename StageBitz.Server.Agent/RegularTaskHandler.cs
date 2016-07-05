using StageBitz.Common;
using System.Reflection;
using System.IO;
using System;
using StageBitz.Data;
using StageBitz.Reports;
using System.Configuration;
using StageBitz.Logic.Business.Company;

namespace StageBitz.Server.Agent
{
    /// <summary>
    /// Regular tasks. (every time agent runs)
    /// </summary>
    public class RegularTaskHandler : TaskHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegularTaskHandler"/> class.
        /// </summary>
        public RegularTaskHandler()
            : base("REGULAR")
        {
        }

        /// <summary>
        /// Shoulds the run task.
        /// </summary>
        /// <param name="lastRunDate">The last run date.</param>
        /// <returns></returns>
        protected override bool ShouldRunTask(DateTime lastRunDate)
        {
            return true;
        }

        /// <summary>
        /// Sends the queued emails.
        /// </summary>
        private void SendQueuedEmails()
        {
            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            StageBitz.Common.EmailSender.StageBitzUrl = Utils.GetSystemValue("SBUserWebURL");
            StageBitz.Common.EmailSender.StageBitzImagePath = Path.Combine(assemblyPath, @"Resources\StageBitzLogo_small.jpg");

            EmailSender.SendQueuedEmails();
        }

        /// <summary>
        /// Deletes the exported files.
        /// </summary>
        private void DeleteExportedFiles()
        {
            //Delete unwanted zipped files if they are being there for more than the defined days from its creation date
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CompanyBL companyBL = new CompanyBL(dataContext);
                companyBL.RemoveGeneratedExportFiles();
            }
        }

        /// <summary>
        /// Performs business logic actions specified for this task handler.
        /// </summary>
        protected override void PerformActions()
        {
            SendQueuedEmails();
            GenerateExportFiles();
        }

        /// <summary>
        /// Generates the export files.
        /// </summary>
        private void GenerateExportFiles()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                string basePath = Utils.GetSystemValue("ExportFileDirectoryLocation");
                if (!string.IsNullOrEmpty(basePath))
                {
                    ExportFilesHandler fileHandler = new ExportFilesHandler(basePath, dataContext);
                    fileHandler.ExportFiles();
                }
            }
        }
    }

}
