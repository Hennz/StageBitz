using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Common.Enum;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.IO;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Business.Utility;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StageBitz.Reports
{
    /// <summary>
    /// Class for export zip files.
    /// </summary>
    public class ExportFilesHandler
    {
        #region Properties

        /// <summary>
        /// Gets or sets the base folder path.
        /// </summary>
        /// <value>
        /// The base folder path.
        /// </value>
        private string BaseFolderPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        private StageBitzDB DataContext
        {
            get;
            set;
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFilesHandler"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="dataContext">The data context.</param>
        public ExportFilesHandler(string path, StageBitzDB dataContext)
        {
            BaseFolderPath = path;
            DataContext = dataContext;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Deletes the old exported zip files.
        /// </summary>
        public void DeleteOldExportedZipFiles()
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            var queuedExportFileRequests = companyBL.GetGeneratedOldExportFiles();
            string baseFolder = string.Empty;
            foreach (ExportFile exportFileRequest in queuedExportFileRequests)
            {
                baseFolder = companyBL.GetExportFileLocation(exportFileRequest.RelatedTable, exportFileRequest.RelatedId);

                if (baseFolder.Trim().Length > 0)
                {
                    FileHandler.DeleteFile(string.Concat(baseFolder, ".zip"));
                    exportFileRequest.IsActive = false;
                    exportFileRequest.LastUpdatedByUserId = 0;
                    exportFileRequest.LastUpdatedDate = Utils.Now;
                }
            }

            DataContext.SaveChanges();//Should not be placed with in the foreach
        }

        /// <summary>
        /// Exports the files.
        /// </summary>
        public void ExportFiles()
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            int queuedStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "QUEUED");
            int deletedStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "DELETED");
            long fileSize = 0;

            var queuedExportFileRequests = companyBL.GetAllQueuedExportFileRequests();

            // Loop all export file requests.
            foreach (ExportFile exportFileRequest in queuedExportFileRequests)
            {
                bool isSuccess = false;

                // If a new request
                if (exportFileRequest.ExportFileStatusCodeId == queuedStatusCodeId)
                {
                    switch (exportFileRequest.RelatedTable)
                    {
                        case "Project":
                            isSuccess = CreateProjectExportFile(exportFileRequest.RelatedId, exportFileRequest.CreatedByUserId, out fileSize);
                            break;

                        case "Company":
                            isSuccess = CreateCompanyExportFile(exportFileRequest.RelatedId, exportFileRequest.CreatedByUserId, out fileSize);
                            break;
                    }

                    if (isSuccess)
                    {
                        exportFileRequest.ExportFileStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "COMPLETED");
                        exportFileRequest.FileSize = fileSize;
                    }
                }
                // If a delete request.
                else if (exportFileRequest.ExportFileStatusCodeId == deletedStatusCodeId) //Pending Delete
                {
                    switch (exportFileRequest.RelatedTable)
                    {
                        case "Project":
                            isSuccess = DeletePendingRemovalProjectFiles(exportFileRequest.RelatedId);
                            break;

                        case "Company":
                            isSuccess = DeletePendingRemovalCompanyFiles(exportFileRequest.RelatedId);
                            break;
                    }

                    if (isSuccess)
                    {
                        exportFileRequest.IsActive = false;
                    }
                }
            }

            DataContext.SaveChanges();//Should not be placed with in the foreach
        }

        #endregion Public Methods

        #region Private Methods - Inventory

        /// <summary>
        /// Creates the company export file.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <returns>Is success.</returns>
        private bool CreateCompanyExportFile(int companyId, int userId, out long fileSize)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            string baseFolder = companyBL.GetExportFileLocation(Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Company,companyId);

            if (baseFolder.Trim().Length > 0)
            {
                try
                {
                    // Intialize Cancellation Token.
                    CancellationTokenSource cts = new CancellationTokenSource();
                    ParallelOptions po = new ParallelOptions();
                    po.CancellationToken = cts.Token;

                    string bookingPath = Path.Combine(baseFolder, "Bookings");
                    FileHandler.CreateFolder(bookingPath);

                    // Generate booking reports.(new Task/Thread.)
                    Task bookingReportTask = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            GenerateCompanyInventoryBookingReports(companyId, userId, bookingPath);
                        }
                        catch (Exception ex)
                        {
                            // Request cancel for all other pending tasks. And throw the exception.
                            cts.Cancel();
                            throw ex;
                        }
                    }, cts.Token);

                    // Generate item type reports and item brief attachements.(new Task/Thread.)
                    Task itemTypeTask = Task.Factory.StartNew(() =>
                    {
                        var groupedDocumentMedias = companyBL.GetItemTypeDocumentMediaByCompany(companyId);

                        // Check for the cancel request. (after starting the task).
                        cts.Token.ThrowIfCancellationRequested();

                        Parallel.ForEach<ItemTypeDocumentMedia>(groupedDocumentMedias, po, dm =>
                        {
                            try
                            {
                                // Create Item List with all Specs report
                                Task itemReportTask = Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        string fileName = string.Concat(dm.ItemTypeName, " - Item List with all Specs");
                                        string fileNameExtension = GlobalConstants.FileExtensions.ExcelFile;
                                        string encoding;
                                        string mimeType;
                                        string attachmentPathCI = Path.Combine(baseFolder, dm.ItemTypeName);
                                        byte[] reportBytes = UserWebReportHandler.GenerateInventoryExport(companyId, dm.ItemTypeId, ReportTypes.Excel, out fileNameExtension, out encoding, out mimeType);
                                        FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), attachmentPathCI).Wait();
                                    }
                                    catch (Exception ex)
                                    {
                                        // Request cancel for all other pending tasks. And throw the exception.
                                        cts.Cancel();
                                        //throw ex;
                                    }
                                }, cts.Token);

                                string attachmentPath = Path.Combine(baseFolder, dm.ItemTypeName, "Attachments");
                                FileHandler.CreateFolder(attachmentPath);

                                // Generate attachments.
                                // single threaded.
                                // Due to the fact that this is an heavy IO bound operation which is recomended to do async.
                                List<Task> asyncTaskList = new List<Task>();
                                foreach (DocumentMediaInfo documentMediaInfo in dm.DocumentMedias)
                                {
                                    // Check for the cancel request. (after starting the task).
                                    cts.Token.ThrowIfCancellationRequested();

                                    string filePrefix = string.Format("{0} {1} - ", documentMediaInfo.EntityId, Utils.Ellipsize(documentMediaInfo.EntityName, 50));
                                    asyncTaskList.Add(SaveDocumentMedia(filePrefix, documentMediaInfo.DocumentMediaId, attachmentPath));
                                }

                                // Wait for all async taks.
                                Task.WaitAll(asyncTaskList.ToArray());

                                // Wait for report task.
                                itemReportTask.Wait();
                            }
                            catch (Exception ex)
                            {
                                // Request cancel for all other pending tasks. And throw the exception.
                                cts.Cancel();
                                throw ex;
                            }
                        });
                    }, cts.Token);

                    Task[] tasks = new[] { bookingReportTask, itemTypeTask };
                    try
                    {
                        Task.WaitAll(tasks, cts.Token);
                        string zipPath = string.Concat(baseFolder, ".zip");
                        FileHandler.CreateZipFile(baseFolder, zipPath);
                        fileSize = FileHandler.GetFileSize(zipPath);
                        return true;
                    }
                    catch (OperationCanceledException)
                    {
                        // One or more operations has been canceled. Wait for other running tasks to complete. (Any status => success or failed).
                        Task.Factory.ContinueWhenAll(tasks, _ =>
                        {
                            // Get exceptions from failed tasks.
                            Exception[] exceptions = tasks.Where(t => t.IsFaulted).Select(t => t.Exception).ToArray();
                            foreach (Exception e in exceptions)
                            {
                                // log failures.
                                AgentErrorLog.HandleException(e);
                            }
                        }).Wait();
                    }
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        // log failures.
                        AgentErrorLog.HandleException(e);
                    }
                }
                finally
                {
                    FileHandler.DeleteFolder(baseFolder);
                }
            }

            fileSize = 0;
            return false;
        }

        /// <summary>
        /// Deletes the pending removal company files.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private bool DeletePendingRemovalCompanyFiles(int companyId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            string baseFolder = companyBL.GetExportFileLocation(Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Company, companyId);
            if (baseFolder.Trim().Length > 0)
            {
                FileHandler.DeleteFile(string.Concat(baseFolder, ".zip"));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates the company inventory booking reports.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="bookingPath">The booking path.</param>
        private void GenerateCompanyInventoryBookingReports(int companyId, int userId, string bookingPath)
        {
            string fileName = "Booking List";
            InventoryManageBookingListReportParameters parametersBookingList = new InventoryManageBookingListReportParameters
            {
                BookingStatus = null,
                CompanyId = companyId,
                CreatedByUserId = userId,
                IsInventoryManagerMode = true,
                SearchText = string.Empty,
                ShowArchived = false,
                SortExpression = string.Empty,
                UserId = userId
            };

            string fileNameExtension;
            string encoding;
            string mimeType;

            List<Task> asyncTaskList = new List<Task>();
            byte[] reportBytes = UserWebReportHandler.GenerateInventoryManageBookingListReport(parametersBookingList, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            asyncTaskList.Add(FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), bookingPath));

            InventoryBL inventoryBL = new InventoryBL(DataContext);
            var bookings = inventoryBL.GetBookingInfo(companyId, null, string.Empty, null, false);

            foreach (BookingInfo booking in bookings)
            {
                asyncTaskList.Add(SaveBookingDetailsReport(booking, companyId, userId, bookingPath));
            }

            Task.WaitAll(asyncTaskList.ToArray());
        }

        /// <summary>
        /// Saves the booking details report.
        /// </summary>
        /// <param name="booking">The booking.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="bookingPath">The booking path.</param>
        private Task SaveBookingDetailsReport(BookingInfo booking, int companyId, int userId, string bookingPath)
        {
            InventoryBL inventoryBL = new InventoryBL(DataContext);
            string bookingDetailsFileName = string.Format("{0} {1} details", booking.BookingNumber, Utils.Ellipsize(booking.BookingName, 50));
            BookingDetailsReportParameters parametersBookingDetails = new BookingDetailsReportParameters
            {
                BookingId = booking.BookingId,
                BookingName = booking.BookingName,
                CompanyId = companyId,
                ContactPerson = null,
                DisplayMode = "Admin",
                ItemTypeId = 0,
                RelatedTable = booking.RelatedTable,
                SortExpression = string.Empty,
                UserId = userId
            };

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateBookingDetailsReport(parametersBookingDetails, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType, true);
            return FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", bookingDetailsFileName, fileNameExtension), bookingPath);
        }

        #endregion Private Methods - Inventory

        #region Private Methods - Project

        /// <summary>
        /// Creates the project export file.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <returns>Is success.</returns>
        private bool CreateProjectExportFile(int projectId, int userId, out long fileSize)
        {
            ProjectBL projectBL = new ProjectBL(DataContext);
            CompanyBL companyBL = new CompanyBL(DataContext);
            string baseFolder = companyBL.GetExportFileLocation(Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Project, projectId);

            if (baseFolder.Trim().Length > 0)
            {
                FileHandler.CreateFolder(baseFolder);
                Project project = projectBL.GetProject(projectId);
                try
                {
                    // Intialize Cancellation Token.
                    CancellationTokenSource cts = new CancellationTokenSource();
                    ParallelOptions po = new ParallelOptions();
                    po.CancellationToken = cts.Token;

                    // Create new task (thread) for create project attachments.
                    var projectAttachmentTask = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var documentMediaIds = projectBL.GetProjectDocumentMediaIds(projectId);

                            // Check for the cancel request. (after starting the task).
                            cts.Token.ThrowIfCancellationRequested();

                            var projectAttachmentPath = Path.Combine(baseFolder, "Project Details Attachments");
                            FileHandler.CreateFolder(projectAttachmentPath);

                            // Generate attachments.
                            // single threaded.
                            // Due to the fact that this is an heavy IO bound operation which is recomended to do async.
                            List<Task> asyncTaskList = new List<Task>();
                            foreach (int documentMediaId in documentMediaIds)
                            {
                                // Check for task cancelation request.
                                cts.Token.ThrowIfCancellationRequested();
                                asyncTaskList.Add(SaveDocumentMedia(string.Empty, documentMediaId, projectAttachmentPath));
                            }

                            // Wait for all async taks.
                            Task.WaitAll(asyncTaskList.ToArray());
                        }
                        catch (Exception ex)
                        {
                            // Request cancel for all other pending tasks. And throw the exception.
                            cts.Cancel();
                            throw ex;
                        }
                    }, cts.Token);

                    // Create new task (thread) for create Budget Summary Report.
                    var budgetSummaryReportTask = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            GenerateProjectBudgetSummaryReport(project, userId, baseFolder);
                        }
                        catch (Exception ex)
                        {
                            // Request cancel for all other pending tasks. And throw the exception.
                            cts.Cancel();
                            throw ex;
                        }
                    }, cts.Token);

                    // Create new task (thread) for create item brief attachments.
                    var ItemBriefDocumentMediaTask = Task.Factory.StartNew(() =>
                    {
                        var groupedDocumentMedias = projectBL.GetItemTypeDocumentMediaByProject(projectId);

                        // Check for the cancel request. (after starting the task).
                        cts.Token.ThrowIfCancellationRequested();

                        // Loop all item types for the company.
                        Parallel.ForEach<ItemTypeDocumentMedia>(groupedDocumentMedias, po, dm =>
                        {
                            try
                            {
                                string itemTypePath = Path.Combine(baseFolder, dm.ItemTypeName);
                                string attachmentPath = Path.Combine(itemTypePath, "Attachments");
                                FileHandler.CreateFolder(attachmentPath);

                                // Handle report generation to separate task.
                                Task itemBriefReportTask = Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        GenerateItemBriefExportFile(projectId, baseFolder, dm);
                                        GenerateItemisedPurchaseReport(project, dm.ItemTypeName, dm.ItemTypeId, userId, itemTypePath);
                                        GenerateActiveTaskReport(project, dm.ItemTypeName, dm.ItemTypeId, userId, itemTypePath);
                                    }
                                    catch (Exception ex)
                                    {
                                        // Request cancel for all other pending tasks. And throw the exception.
                                        cts.Cancel();
                                        throw ex;
                                    }
                                }, cts.Token);

                                // Generate attachments.
                                // single threaded.
                                // Due to the fact that this is an heavy IO bound operation which is recomended to do async.
                                List<Task> asyncTaskList = new List<Task>();
                                foreach (DocumentMediaInfo documentMediaInfo in dm.DocumentMedias)
                                {
                                    // Check for task cancelation request.
                                    cts.Token.ThrowIfCancellationRequested();
                                    string filePrefix = string.Format("{0} {1} - ", documentMediaInfo.EntityId, Utils.Ellipsize(documentMediaInfo.EntityName, 50));
                                    asyncTaskList.Add(SaveDocumentMedia(filePrefix, documentMediaInfo.DocumentMediaId, attachmentPath));
                                }

                                // Wait for all async taks.
                                Task.WaitAll(asyncTaskList.ToArray());

                                // Wait for report task.
                                itemBriefReportTask.Wait();
                            }
                            catch (Exception ex)
                            {
                                // Request cancel for all other pending tasks. And throw the exception.
                                cts.Cancel();
                                throw ex;
                            }
                        });
                    }, cts.Token);

                    Task[] tasks = new[] { projectAttachmentTask, budgetSummaryReportTask, ItemBriefDocumentMediaTask };
                    try
                    {
                        Task.WaitAll(tasks, cts.Token);
                        string zipPath = string.Concat(baseFolder, ".zip");
                        FileHandler.CreateZipFile(baseFolder, zipPath);
                        fileSize = FileHandler.GetFileSize(zipPath);
                        return true;
                    }
                    catch (OperationCanceledException)
                    {
                        // One or more operations has been canceled. Wait for other running tasks to complete. (Any status => success or failed).
                        Task.Factory.ContinueWhenAll(tasks, _ =>
                        {
                            // Get exceptions from failed tasks.
                            Exception[] exceptions = tasks.Where(t => t.IsFaulted).Select(t => t.Exception).ToArray();
                            foreach (Exception e in exceptions)
                            {
                                // log failures.
                                AgentErrorLog.HandleException(e);
                            }
                        }).Wait();
                    }
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.Flatten().InnerExceptions)
                    {
                        // log failures.
                        AgentErrorLog.HandleException(e);
                    }
                }
                finally
                {
                    FileHandler.DeleteFolder(baseFolder);
                }
            }

            fileSize = 0;
            return false;
        }

        /// <summary>
        /// Deletes the pending removal project files.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>Is success.</returns>
        private bool DeletePendingRemovalProjectFiles(int projectId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            string baseFolder = companyBL.GetExportFileLocation(Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Project, projectId);
            if (baseFolder.Trim().Length > 0)
            {
                FileHandler.DeleteFile(string.Concat(baseFolder, ".zip"));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates the active task report.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="itemTypeName">Name of the item type.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="folderPath">The folder path.</param>
        private void GenerateActiveTaskReport(Project project, string itemTypeName, int itemTypeId, int userId, string folderPath)
        {
            string fileNameExtension;
            string encoding;
            string mimeType;
            string fileName = string.Format("{0} - Tasks", itemTypeName);

            ActiveTaskListReportParameters parameters = new ActiveTaskListReportParameters
            {
                CultureName = Utils.GetCultureName(project.Country.CountryCode),
                ItemTypeId = itemTypeId,
                ProjectId = project.ProjectId,
                UserId = userId
            };

            byte[] reportBytes = UserWebReportHandler.GenerateActiveTaskListReport(parameters, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), folderPath).Wait();
        }

        /// <summary>
        /// Generates the itemised purchase report.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="itemTypeName">Name of the item type.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="folderPath">The folder path.</param>
        private void GenerateItemisedPurchaseReport(Project project, string itemTypeName, int itemTypeId, int userId, string folderPath)
        {
            ItemisedPurchaseReportParameters parameters = new ItemisedPurchaseReportParameters
            {
                CultureName = Utils.GetCultureName(project.Country.CountryCode),
                ItemTypeId = itemTypeId,
                ProjectId = project.ProjectId,
                SortExpression = string.Empty,
                UserId = userId
            };

            string fileName = string.Format("{0} - Itemised Purchase Report", itemTypeName);
            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateItemisedPurchaseReport(parameters, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), folderPath).Wait();
        }

        /// <summary>
        /// Generates the item brief export file.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="baseFolder">The base folder.</param>
        /// <param name="itemTypeDocumentMedia">The item type document media.</param>
        private void GenerateItemBriefExportFile(int projectId, string baseFolder, ItemTypeDocumentMedia itemTypeDocumentMedia)
        {
            string fileName = string.Concat(itemTypeDocumentMedia.ItemTypeName, " - Item Brief List with all Specs");
            string fileNameExtension = GlobalConstants.FileExtensions.ExcelFile;
            string encoding;
            string mimeType;
            string attachmentPathItemBriefs = Path.Combine(baseFolder, itemTypeDocumentMedia.ItemTypeName);
            byte[] reportBytes = UserWebReportHandler.GenerateItemBriefExport(projectId, itemTypeDocumentMedia.ItemTypeId, ReportTypes.Excel, out fileNameExtension, out encoding, out mimeType);
            FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), attachmentPathItemBriefs).Wait();
        }

        /// <summary>
        /// Generates the project budget summary report.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="folderPath">The folder path.</param>
        private void GenerateProjectBudgetSummaryReport(Project project, int userId, string folderPath)
        {
            BudgetSummaryReportParameters parameters = new BudgetSummaryReportParameters();
            parameters.SortExpression = string.Empty;
            parameters.ItemTypeId = -1;
            parameters.UserId = userId;
            parameters.CultureName = Utils.GetCultureName(project.Country.CountryCode);
            parameters.ProjectId = project.ProjectId;

            string fileName = string.Format("{0} - Budget Summary Report", Utils.Ellipsize(project.ProjectName, 50));

            string fileNameExtension;
            string encoding;
            string mimeType;

            byte[] reportBytes = UserWebReportHandler.GenerateBudgetSummaryReport(parameters, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            FileHandler.SaveFileToDisk(reportBytes, string.Format("{0}.{1}", fileName, fileNameExtension), folderPath).Wait();
        }

        #endregion Private Methods - Project

        #region Private Methods - Common

        /// <summary>
        /// Saves the document media.
        /// </summary>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="documentMediaId">The document media identifier.</param>
        /// <param name="attachmentPath">The attachment path.</param>
        /// <returns></returns>
        private async Task SaveDocumentMedia(string filePrefix, int documentMediaId, string attachmentPath)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                UtilityBL utilityBL = new UtilityBL(dataContext);
                Data.DocumentMedia documentMedia = utilityBL.GetDocumentMedia(documentMediaId);
                if (documentMedia != null)
                {
                    string documentMediaFileName = string.Format("{0} {1} - {2}.{3}", filePrefix, documentMedia.DocumentMediaId, Utils.Ellipsize(documentMedia.Name, 50), documentMedia.FileExtension);
                    await FileHandler.SaveFileToDisk(documentMedia.DocumentMediaContent, FileHandler.GetSafeFileName(documentMediaFileName), attachmentPath);
                }
            }
        }

        #endregion Private Methods - Common
    }
}