using Microsoft.Reporting.WebForms;
using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Business.Report;
using StageBitz.Logic.Business.Utility;
using StageBitz.Reports.UserWeb.Parameters;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StageBitz.Reports.UserWeb.Helper
{
    /// <summary>
    /// Report handler class for user web site.
    /// </summary>
    public class UserWebReportHandler : ReportHandlerBase
    {
        /// <summary>
        /// The report namespace for user web.
        /// </summary>
        public const string ReportNamespace = "StageBitz.Reports.UserWeb.";

        /// <summary>
        /// Generates the active task list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateActiveTaskListReport(ActiveTaskListReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ReportBL reportBL = new ReportBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    Project project = projectBL.GetProject(parameters.ProjectId);

                    // Bind data and Export pdf report
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ActiveTaskList.rdlc");

                    string reportTitle = string.Empty;

                    if (parameters.ItemTypeId != 0)
                    {
                        reportTitle = "Active Task List - " + Utils.GetItemTypeById(parameters.ItemTypeId).Name;
                    }
                    else
                    {
                        reportTitle = "Active Task List";
                    }

                    var tasks = itemBriefBL.GetActiveTasksForActiveTaskList(parameters.ProjectId, parameters.ItemTypeId);
                    var details = reportBL.GetReportHeaderDetails(parameters.ProjectId, user.FirstName + " " + user.LastName, reportTitle);

                    int companyId = project.CompanyId;
                    var companyLogo = from d in dataContext.DocumentMedias
                                      where d.RelatedTableName == "Company" && d.RelatedId == companyId
                                      select new { Logo = d.Thumbnail };

                    ReportDataSource reportDataSource = new ReportDataSource("ActiveTasks", tasks);
                    ReportDataSource reportHeaderDataSource = new ReportDataSource("Header", details);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDataSource);
                    localReport.DataSources.Add(reportHeaderDataSource);
                    localReport.DataSources.Add(logo);
                    int estimatedCostNullCount = tasks.Where(tl => tl.IsEstimatedCostNullForActiveTask == true).Count();
                    localReport.SetParameters(new ReportParameter("ShouldShowNoEstimatedCostIcon", (estimatedCostNullCount > 0 ? "true" : "false")));
                    localReport.SetParameters(new ReportParameter("CultureInfo", parameters.CultureName));

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            fileNameExtension = encoding = mimeType = string.Empty;
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the item brief list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateItemBriefListReport(ItemBriefListReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ReportBL reportBL = new ReportBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);

                    // string reportPath = @"Reports\ItemBriefList.rdlc";
                    User user = personalBL.GetUser(parameters.UserId);
                    Project project = projectBL.GetProject(parameters.ProjectId);

                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ItemBriefList.rdlc");
                    var itemBriefs = itemBriefBL.GetItemBriefListForReport(parameters.ProjectId, parameters.ItemTypeId);

                    #region EXPORT SORT SETTINGS

                    var sortedItemBriefs = itemBriefs;

                    // set sorting
                    switch (parameters.SortExpression)
                    {
                        case "ItemBrief.Name ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Name).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Name DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Name).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Category ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Category).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Category DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Category).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Act ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Act).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Act DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Act).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Page ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Page).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Page DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Page).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Quantity ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Quantity).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Quantity DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Quantity).ToList<ItemBriefListInfo>();
                            break;

                        case "StatusSortOrder ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.StatusSortOrder).ToList<ItemBriefListInfo>();
                            break;

                        case "StatusSortOrder DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.StatusSortOrder).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Preset ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Preset).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Preset DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Preset).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Character ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.Character).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.Character DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.Character).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.RehearsalItem ASC":
                            sortedItemBriefs = itemBriefs.OrderBy(ib => ib.ItemBrief.RehearsalItem).ToList<ItemBriefListInfo>();
                            break;

                        case "ItemBrief.RehearsalItem DESC":
                            sortedItemBriefs = itemBriefs.OrderByDescending(ib => ib.ItemBrief.RehearsalItem).ToList<ItemBriefListInfo>();
                            break;

                        default:
                            break;
                    }

                    #endregion EXPORT SORT SETTINGS

                    string reportTitle = string.Empty;
                    if (parameters.ItemTypeId != 0)
                    {
                        reportTitle = string.Format("{0} Brief List", Utils.GetItemTypeById(parameters.ItemTypeId).Name); ;
                    }
                    else
                    {
                        reportTitle = "Item List";
                    }

                    var headerData = reportBL.GetReportHeaderDetails(parameters.ProjectId, (user.FirstName + " " + user.LastName), reportTitle);

                    var itemBriefDetail = from ib in sortedItemBriefs
                                          select new
                                          {
                                              Name = ib.ItemBrief.Name,
                                              Description = ib.ItemBrief.Description,
                                              Category = ib.ItemBrief.Category,
                                              Act = ib.ItemBrief.Act,
                                              Scene = ib.ItemBrief.Scene,
                                              Page = ib.ItemBrief.Page,
                                              Quantity = ib.ItemBrief.Quantity,
                                              Status = ib.Status,
                                              Character = ib.ItemBrief.Character,
                                              Preset = ib.ItemBrief.Preset,
                                              Rehearsal = ib.ItemBrief.RehearsalItem,
                                              IsEstimatedCostNull = ib.IsEstimatedCostNullForActiveTask
                                          };

                    int companyId = project.CompanyId;

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", companyId) });

                    ReportDataSource reportHeader = new ReportDataSource("ItemBriefListHeader", headerData);
                    ReportDataSource reportDetail = new ReportDataSource("ItemBriefListDetail", itemBriefDetail);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDetail);
                    localReport.DataSources.Add(reportHeader);
                    localReport.DataSources.Add(logo);
                    localReport.SetParameters(new ReportParameter("ShouldShowNoEstimatedCostIcon", itemBriefBL.HasEmptyEstimateCostInProject(parameters.ProjectId, parameters.ItemTypeId) ? "true" : "false"));

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the budget summary report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateBudgetSummaryReport(BudgetSummaryReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ReportBL reportBL = new ReportBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);

                    // string reportPath = @"Reports\ItemBriefList.rdlc";
                    User user = personalBL.GetUser(parameters.UserId);
                    Project project = projectBL.GetProject(parameters.ProjectId);

                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, parameters.ItemTypeId > 0 ? "BudgetSummary.rdlc" : "BudgetSummary_AllTypes.rdlc");
                    DataView dvItems = itemBriefBL.GetBudgetDetails(parameters.ItemTypeId, parameters.ProjectId).DefaultView;
                    if (!string.IsNullOrEmpty(parameters.SortExpression))
                    {
                        dvItems.Sort = parameters.SortExpression;
                    }

                    string reportTitle = string.Empty;
                    if (parameters.ItemTypeId > 0)
                    {
                        reportTitle = "Budget Summary Report - " + Utils.GetItemTypeById(parameters.ItemTypeId).Name;
                    }
                    else
                    {
                        reportTitle = "Budget Summary Report";
                    }

                    var details = reportBL.GetReportHeaderDetails(parameters.ProjectId, user.FirstName + " " + user.LastName, reportTitle);

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", project.CompanyId) });

                    DataTable dtBudgetSummary = new DataTable("tblBudgetSummary");
                    var budgetListInfo = itemBriefBL.GetBudgetListInfo(parameters.ProjectId, parameters.ItemTypeId);

                    // Build the DataTable for budget summary
                    dtBudgetSummary.Columns.Add("Budget", typeof(decimal));
                    dtBudgetSummary.Columns.Add("Expended", typeof(decimal));
                    dtBudgetSummary.Columns.Add("Remaining", typeof(decimal));
                    dtBudgetSummary.Columns.Add("Balance", typeof(decimal));

                    DataRow dtRow = dtBudgetSummary.NewRow();
                    dtRow["Budget"] = (budgetListInfo.GetItemTypeTotalBudget == null ? 0 : budgetListInfo.GetItemTypeTotalBudget);
                    dtRow["Expended"] = (budgetListInfo.SumExpened == null ? 0 : budgetListInfo.SumExpened);
                    dtRow["Remaining"] = (budgetListInfo.SumRemaining == null ? 0 : budgetListInfo.SumRemaining);
                    dtRow["Balance"] = (budgetListInfo.SumBalance == null ? 0 : budgetListInfo.SumBalance);
                    dtBudgetSummary.Rows.Add(dtRow);

                    ReportDataSource budgetdetailsDataSource = new ReportDataSource("budgetdetailsDataSourceForbudgetSummary", dvItems.ToTable());
                    ReportDataSource companyDetailstDataSource = new ReportDataSource("companyDetailstDataSourceForbudgetSummary", details);
                    ReportDataSource budgetSummaryHeaderDataSource = new ReportDataSource("budgetSummaryHeaderDataSource", dtBudgetSummary);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(budgetdetailsDataSource);
                    localReport.DataSources.Add(companyDetailstDataSource);
                    localReport.DataSources.Add(budgetSummaryHeaderDataSource);
                    localReport.DataSources.Add(logo);
                    localReport.SetParameters(new ReportParameter("ShouldShowNoEstimatedCostIcon", itemBriefBL.HasEmptyEstimateCostInProject(parameters.ProjectId, parameters.ItemTypeId) ? "true" : "false"));
                    localReport.SetParameters(new ReportParameter("CultureInfo", parameters.CultureName));
                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the booking details report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateBookingDetailsReport(BookingDetailsReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    InventoryBL inventoryBL = new InventoryBL(dataContext);
                    CompanyBL companyBL = new CompanyBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    Data.Company company = companyBL.GetCompany(parameters.CompanyId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "BookingDetails.rdlc");

                    IEnumerable<CompanyBookingDetails> bookingDetails = inventoryBL.GetBookingDetails(parameters.BookingId, parameters.CompanyId, parameters.ItemTypeId, parameters.ShowMyBookingsOnly ,parameters.UserId);

                    if (bookingDetails != null)
                    {
                        var bookingDetailsHeader = bookingDetails.FirstOrDefault();
                        if (parameters.ContactPerson == null)
                        {
                            int maxContactBookingManagerId = bookingDetailsHeader.BookingDetailList.Max(bd => bd.ContactLocationManagerId);
                            int minContactBookingManagerId = bookingDetailsHeader.BookingDetailList.Min(bd => bd.ContactLocationManagerId);
                            int? contactBookingManagerId = maxContactBookingManagerId == minContactBookingManagerId ? (int?)minContactBookingManagerId : null;
                            if(contactBookingManagerId.HasValue)
                            {
                                parameters.ContactPerson = personalBL.GetUser(contactBookingManagerId.Value);
                            }
                            else
                            {
                                parameters.ContactPerson = inventoryBL.GetInventoryAdmin(parameters.CompanyId);
                            }
                        }

                        #region EXPORT SORT SETTINGS

                        var bookings = bookingDetails.FirstOrDefault().BookingDetailList;
                        var sortedBookings = bookings;

                        switch (parameters.SortExpression)
                        {
                            case "ItemTypeId ASC, ItemBriefName ASC":
                                sortedBookings = bookings.OrderBy(b => b.ItemBriefName).ToList();
                                break;

                            case "ItemTypeId ASC, ItemBriefName DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.ItemBriefName).ToList();
                                break;

                            case "ItemTypeId ASC, ItemName ASC":
                                sortedBookings = bookings.OrderBy(b => b.ItemName).ToList();
                                break;

                            case "ItemTypeId ASC, ItemName DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.ItemName).ToList();
                                break;

                            case "ItemTypeId ASC, Quantity ASC":
                                sortedBookings = bookings.OrderBy(b => b.Quantity).ToList();
                                break;

                            case "ItemTypeId ASC, Quantity DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.Quantity).ToList();
                                break;

                            case "ItemTypeId ASC, BookedBy ASC":
                                sortedBookings = bookings.OrderBy(b => b.BookedBy).ToList();
                                break;

                            case "ItemTypeId ASC, BookedBy DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.BookedBy).ToList();
                                break;

                            case "ItemTypeId ASC, CurrentStatus ASC":
                                sortedBookings = bookings.OrderBy(b => b.CurrentStatus).ToList();
                                break;

                            case "ItemTypeId ASC, CurrentStatus DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.CurrentStatus).ToList();
                                break;

                            case "ItemTypeId ASC, FromDate ASC":
                                sortedBookings = bookings.OrderBy(b => b.FromDate).ToList();
                                break;

                            case "ItemTypeId ASC, FromDate DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.FromDate).ToList();
                                break;

                            case "ItemTypeId ASC, ToDate ASC":
                                sortedBookings = bookings.OrderBy(b => b.ToDate).ToList();
                                break;

                            case "ItemTypeId ASC, ToDate DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.ToDate).ToList();
                                break;

                            case "ItemTypeId ASC, IsPickedUpOrder ASC":
                                sortedBookings = bookings.OrderBy(b => b.IsPickedUpOrder).ToList();
                                break;

                            case "ItemTypeId ASC, IsPickedUpOrder DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.IsPickedUpOrder).ToList();
                                break;

                            case "ItemTypeId ASC, IsReturnedOrder ASC":
                                sortedBookings = bookings.OrderBy(b => b.IsReturnedOrder).ToList();
                                break;

                            case "ItemTypeId ASC, IsReturnedOrder DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.IsReturnedOrder).ToList();
                                break;

                            case "ItemTypeId ASC, IsActive ASC":
                                sortedBookings = bookings.OrderBy(b => b.IsActive).ToList();
                                break;

                            case "ItemTypeId ASC, IsActive DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.IsActive).ToList();
                                break;

                            case "ItemTypeId ASC, StatusSortOrder ASC":
                                sortedBookings = bookings.OrderBy(b => b.StatusSortOrder).ToList();
                                break;

                            case "ItemTypeId ASC, StatusSortOrder DESC":
                                sortedBookings = bookings.OrderByDescending(b => b.StatusSortOrder).ToList();
                                break;

                            default:
                                sortedBookings = bookings.OrderBy(b => b.StatusSortOrder).ThenBy(b => b.ConfirmedSortOrder).ToList();
                                break;
                        }

                        #endregion EXPORT SORT SETTINGS

                        
                        var bookingHeaderList = new List<BookingHeaderData>();

                        bookingHeaderList.Add(new BookingHeaderData
                        {
                            BookingName = parameters.BookingName,
                            BookingNumber = inventoryBL.GetCompanyBookingNumber(parameters.BookingId, parameters.CompanyId).BookingNumber.ToString(),
                            CompanyName = company.CompanyName,
                            InventoryManagerName = string.Concat(parameters.ContactPerson.FirstName, " ", parameters.ContactPerson.LastName),
                            MaxLastUpdatedDate = bookingDetailsHeader.MaxLastUpdatedDate
                        });

                        ReportDataSource reportDetail = new ReportDataSource("BookingDetails", sortedBookings);
                        ReportDataSource bookingHeader = new ReportDataSource("BookingDetailsHeader", bookingHeaderList);
                        localReport.DataSources.Add(reportDetail);
                        localReport.DataSources.Add(bookingHeader);
                    }
                    else
                    {
                        List<CompanyBookingDetails> cbList = new List<CompanyBookingDetails>();
                        ReportDataSource reportDetailEmpty = new ReportDataSource("BookingDetails", cbList);

                        var bookingHeaderList = new List<BookingHeaderData>();

                        bookingHeaderList.Add(new BookingHeaderData
                        {
                            BookingName = parameters.BookingName,
                            BookingNumber = inventoryBL.GetCompanyBookingNumber(parameters.BookingId, parameters.CompanyId).BookingNumber.ToString(),
                            CompanyName = company.CompanyName,
                            InventoryManagerName = string.Concat(parameters.ContactPerson.FirstName, " ", parameters.ContactPerson.LastName)
                        });

                        ReportDataSource bookingHeaderEmpty = new ReportDataSource("BookingDetailsHeader", bookingHeaderList);
                        localReport.DataSources.Add(reportDetailEmpty);
                        localReport.DataSources.Add(bookingHeaderEmpty);
                    }

                    localReport.SetParameters(new ReportParameter("UserName", (user.FirstName + " " + user.LastName)));

                    // localReport.SetParameters(new ReportParameter("IsProjectRelatedPage", DisplayMode == ViewMode.Project ? "true" : "false"));
                    localReport.SetParameters(new ReportParameter("DisplayMode", parameters.DisplayMode));
                    localReport.SetParameters(new ReportParameter("RelatedTable", parameters.RelatedTable));

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the inventory item list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateInventoryItemListReport(InventoryItemListReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    InventoryBL inventoryBL = new InventoryBL(dataContext);
                    CompanyBL companyBL = new CompanyBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    Data.Company company = companyBL.GetCompany(parameters.CompanyId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "InventoryItemList.rdlc");

                    string findName = parameters.FindByName;
                    if (string.IsNullOrEmpty(findName))
                    {
                        findName = null;
                    }

                    int totalCount = 0;
                    var items = inventoryBL.GetInventoryItems(parameters.UserId, parameters.SharedInventoryCompanyId, parameters.CompanyId,
                            findName, parameters.FindByItemTypeId, parameters.FindFromDate, parameters.FindToDate,parameters.LocationId, parameters.ItemVisibilityCodeId,
                            null, null, parameters.SortExpression, out totalCount).ToList<InventoryItemData>();
                    bool isSharedInventory = inventoryBL.IsCompanyInSharedInventory(parameters.CompanyId);                    

                    string reportTitle = isSharedInventory && parameters.SharedInventoryCompanyId <= 0 ? "All Shared Inventories" : string.Format("{0}'s Inventory", company.CompanyName);

                    var headerData = new List<ReportHeaderDetails>();
                    headerData.Add(new ReportHeaderDetails
                    {
                        CompanyName = string.Empty,
                        ProjectName = string.Empty,
                        UserName = (user.FirstName + " " + user.LastName),
                        ReportTitle = reportTitle
                    });

                    dynamic reportData = from sib in items
                                         select new
                                         {
                                             Name = sib.Item.Name,
                                             Description = sib.Item.Description,
                                             Qty = sib.Item.Quantity,
                                             AvailableQty = sib.AvailableQty,
                                             ItemType = sib.ItemTypeName,
                                             CompanyName = sib.CompanyName,
                                             Location = sib.LocationPath
                                         };

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", company.CompanyId) });

                    ReportDataSource reportHeader = new ReportDataSource("Header", headerData);
                    ReportDataSource reportDetail = new ReportDataSource("InventoryItemList", reportData);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDetail);
                    localReport.DataSources.Add(reportHeader);
                    localReport.DataSources.Add(logo);

                    string argSharedCompany = string.Empty;
                    Data.Company sharedCompany = companyBL.GetCompany(parameters.SharedInventoryCompanyId);
                    if (sharedCompany != null)
                    {
                        argSharedCompany = sharedCompany.CompanyName;
                    }

                    string argItemType = string.Empty;
                    Data.ItemType itemType = itemBriefBL.GetItemType(parameters.FindByItemTypeId);
                    if (itemType != null)
                    {
                        argItemType = itemType.Name;
                    }

                    localReport.SetParameters(new ReportParameter("FilterByName", parameters.FindByName));
                    localReport.SetParameters(new ReportParameter("AvailableFrom", Utils.FormatDate(parameters.FindFromDate)));
                    localReport.SetParameters(new ReportParameter("AvailableTo", Utils.FormatDate(parameters.FindToDate)));
                    localReport.SetParameters(new ReportParameter("FilterByInventory", argSharedCompany));
                    localReport.SetParameters(new ReportParameter("FilterByItemType", argItemType));
                    localReport.SetParameters(new ReportParameter("DisplayAvailableQty", parameters.HasNoDateConfigured ? "False" : "True"));
                    localReport.SetParameters(new ReportParameter("IsSharedInventory", isSharedInventory ? "True" : "False"));

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the task list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateTaskListReport(TaskListReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ReportBL reportBL = new ReportBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "TaskList.rdlc");

                    var tasksListToExport = itemBriefBL.GetShoppingListByTaskListId(parameters.TaskListId);
                    string sortExpression = parameters.SortExpression;

                    #region EXPORT SORT SETTINGS

                    var sortedItemBriefTasks = tasksListToExport;

                    // set sorting
                    switch (sortExpression)
                    {
                        case "SortOrder ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.Status).ToList<Data.DataTypes.ShoppingList>(); // Sorting is different is grid sorting and telerik control sorting
                            break;

                        case "SortOrder DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.Status).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefName ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.ItemName).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefName DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.ItemName).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.EstimatedCost ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.EstimatedCost).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.EstimatedCost DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.EstimatedCost).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.NetCost ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.NetCost).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.NetCost DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.NetCost).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.Tax ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.Tax).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "ItemBriefTask.Tax DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.Tax).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "Total ASC":
                            sortedItemBriefTasks = tasksListToExport.OrderBy(u1 => u1.Total).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        case "Total DESC":
                            sortedItemBriefTasks = tasksListToExport.OrderByDescending(u1 => u1.Total).ToList<Data.DataTypes.ShoppingList>();
                            break;

                        default:
                            break;
                    }

                    #endregion EXPORT SORT SETTINGS

                    var shoppingList = itemBriefBL.GetTaskListByTaskListId(parameters.TaskListId);
                    var details = reportBL.GetReportHeaderDetails(parameters.ProjectId, user.FirstName + " " + user.LastName, shoppingList.Name);

                    Data.Project project = projectBL.GetProject(parameters.ProjectId);

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", project.CompanyId) });

                    ReportDataSource reportDataSource = new ReportDataSource("TaskList", sortedItemBriefTasks);
                    ReportDataSource reportDataSource2 = new ReportDataSource("Header", details);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDataSource);
                    localReport.DataSources.Add(reportDataSource2);
                    localReport.DataSources.Add(logo);
                    int estimatedCostNullCount = sortedItemBriefTasks.Where(tl => tl.IsEstimatedCostNullForShoppingList == true).Count();
                    localReport.SetParameters(new ReportParameter("ShouldShowNoEstimatedCostIcon", (estimatedCostNullCount > 0 ? "true" : "false")));

                    localReport.SetParameters(new ReportParameter("CultureInfo", parameters.CultureName));

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the item brief specifications report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateItemBriefSpecificationsReport(ItemBriefSpecificationsReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    UtilityBL utilityBL = new UtilityBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    Data.Project project = projectBL.GetProject(parameters.ProjectId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ItemBriefSpecifications.rdlc");

                    var itemBrief = itemBriefBL.GetItemBrief(parameters.ItemBriefId);
                    if (itemBrief != null)
                    {
                        string itemBriefStatus = Utils.GetCodeByCodeId(itemBrief.ItemBriefStatusCodeId).Description;
                        string firstAppears = "Act: " + itemBrief.Act + "   Scene: " + itemBrief.Scene + "   Page: " + itemBrief.Page;

                        var itemBriefDetails = new List<dynamic>();
                        itemBriefDetails.Add(new
                        {
                            ItemBriefName = itemBrief.Name,
                            Quantity = itemBrief.Quantity.ToString(),
                            Status = itemBriefStatus,
                            DueDate = itemBrief.DueDate,
                            FirstAppears = firstAppears,
                            Category = itemBrief.Category,
                            Character = itemBrief.Character,
                            Preset = itemBrief.Preset,
                            Approver = itemBrief.Approver,
                            RehersalItem = itemBrief.RehearsalItem
                        });

                        string userName = (user.FirstName + " " + user.LastName);

                        var companyDetails = new List<dynamic>();
                        companyDetails.Add(new
                        {
                            CompanyName = project.Company.CompanyName,
                            ProjectName = project.ProjectName,
                            ItemType = itemBrief.ItemBriefTypes.FirstOrDefault().ItemType.Name,
                            UserName = userName
                        });

                        var itemBriefSpecifications = new List<dynamic>();
                        itemBriefSpecifications.Add(new
                        {
                            Description = itemBrief.Description,
                            Usage = itemBrief.Usage,
                            Brief = itemBrief.Brief,
                            Notes = itemBrief.Considerations
                        });

                        DocumentMedia image = utilityBL.GetDefaultImage(parameters.ItemBriefId, "ItemBrief");
                        var itemBriefImage = new List<dynamic>();
                        if (image != null)
                        {
                            itemBriefImage.Add(new { ItemBriefImage = image.DocumentMediaContent });
                        }
                        else
                        {
                            itemBriefImage.Add(new { ItemBriefImage = parameters.NoImagePDFBytes });
                        }

                        ReportDataSource companyDetailsDataSource = new ReportDataSource("CompanyDetailsItemSpcifications", companyDetails);
                        ReportDataSource itemBriefDetailsDataSource = new ReportDataSource("ItemBriefDetails", itemBriefDetails);
                        ReportDataSource itemBriefSpecificationsDataSet = new ReportDataSource("ItemBriefSpecifications", itemBriefSpecifications);
                        ReportDataSource itemBriefImageDataSet = new ReportDataSource("ItemBriefImage", itemBriefImage);

                        localReport.DataSources.Add(companyDetailsDataSource);
                        localReport.DataSources.Add(itemBriefDetailsDataSource);
                        localReport.DataSources.Add(itemBriefSpecificationsDataSet);
                        localReport.DataSources.Add(itemBriefImageDataSet);

                        return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                    }
                    else
                    {
                        fileNameExtension = encoding = mimeType = string.Empty;
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the inventory manage booking list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateInventoryManageBookingListReport(InventoryManageBookingListReportParameters parameters, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    InventoryBL inventoryBL = new InventoryBL(dataContext);
                    CompanyBL companyBL = new CompanyBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "InventoryManageBookingList.rdlc");

                    string searchText = parameters.SearchText;

                    IEnumerable<BookingInfo> bookings = null;
                    if (parameters.IsInventoryManagerMode)
                    {
                        bookings = inventoryBL.GetBookingInfo(parameters.CompanyId, null, searchText, parameters.BookingStatus, parameters.ShowArchived);
                    }
                    else
                    {
                        bookings = inventoryBL.GetBookingInfo(null, parameters.CreatedByUserId, searchText, parameters.BookingStatus, parameters.ShowArchived);
                    }

                    #region EXPORT SORT SETTINGS

                    var sortedBookings = bookings;

                    string sortExpression = parameters.SortExpression;

                    // set sorting
                    switch (sortExpression)
                    {
                        case "BookingNumber ASC":
                            sortedBookings = bookings.OrderBy(b => b.BookingNumber).ToList<BookingInfo>();
                            break;

                        case "BookingNumber DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.BookingNumber).ToList<BookingInfo>();
                            break;

                        case "CompanyName ASC":
                            sortedBookings = bookings.OrderBy(b => b.CompanyName).ToList<BookingInfo>();
                            break;

                        case "CompanyName DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.CompanyName).ToList<BookingInfo>();
                            break;

                        case "BookingCount ASC":
                            sortedBookings = bookings.OrderBy(b => b.BookingCount).ToList<BookingInfo>();
                            break;

                        case "BookingCount DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.BookingCount).ToList<BookingInfo>();
                            break;

                        case "FromDate ASC":
                            sortedBookings = bookings.OrderBy(b => b.FromDate).ToList<BookingInfo>();
                            break;

                        case "FromDate DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.FromDate).ToList<BookingInfo>();
                            break;

                        case "ToDate ASC":
                            sortedBookings = bookings.OrderBy(b => b.ToDate).ToList<BookingInfo>();
                            break;

                        case "ToDate DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.ToDate).ToList<BookingInfo>();
                            break;

                        case "StatusSortOrder ASC":
                            sortedBookings = bookings.OrderBy(b => b.StatusSortOrder).ToList<BookingInfo>();
                            break;

                        case "StatusSortOrder DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.StatusSortOrder).ToList<BookingInfo>();
                            break;

                        case "LastUpdatedDate ASC":
                            sortedBookings = bookings.OrderBy(b => b.LastUpdatedDate).ToList<BookingInfo>();
                            break;

                        case "LastUpdatedDate DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.LastUpdatedDate).ToList<BookingInfo>();
                            break;

                        case "BookingName ASC":
                            sortedBookings = bookings.OrderBy(b => b.BookingName).ToList<BookingInfo>();
                            break;

                        case "BookingName DESC":
                            sortedBookings = bookings.OrderByDescending(b => b.BookingName).ToList<BookingInfo>();
                            break;

                        default:
                            break;
                    }

                    #endregion EXPORT SORT SETTINGS

                    string reportTitle = string.Empty;

                    if (parameters.IsInventoryManagerMode)
                    {
                        Data.Company company = companyBL.GetCompany(parameters.CompanyId.Value);
                        if (company != null)
                        {
                            reportTitle = string.Format("{0}'s Bookings", company.CompanyName);
                        }
                    }
                    else
                    {
                        reportTitle = "My Bookings";
                    }

                    var headerData = new List<ReportHeaderDetails>();
                    headerData.Add(new ReportHeaderDetails
                    {
                        CompanyName = string.Empty,
                        ProjectName = string.Empty,
                        UserName = (user.FirstName + " " + user.LastName),
                        ReportTitle = reportTitle
                    });

                    ReportDataSource reportHeader = new ReportDataSource("Header", headerData);
                    ReportDataSource reportDetail = new ReportDataSource("InventoryManageBookings", sortedBookings);

                    localReport.DataSources.Add(reportDetail);
                    localReport.DataSources.Add(reportHeader);

                    string bookingStatus = parameters.BookingStatus.HasValue ? Utils.GetCodeByCodeId(parameters.BookingStatus.Value).Description : string.Empty;
                    localReport.SetParameters(new ReportParameter("FilterByName", searchText));
                    localReport.SetParameters(new ReportParameter("FilterByStatus", bookingStatus));
                    localReport.SetParameters(new ReportParameter("IsMyBooking", (!parameters.IsInventoryManagerMode).ToString().ToLower()));

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the itemized purchase report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateItemisedPurchaseReport(ItemisedPurchaseReportParameters parameters, ReportTypes exportType,
               out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ReportBL reportBL = new ReportBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    Data.Project project = projectBL.GetProject(parameters.ProjectId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ItemisedPurchase.rdlc");

                    int itemTypeId = parameters.ItemTypeId;
                    int itemBriefTaskCompletedStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");

                    var itemBriefTask = itemBriefBL.GetItemBriefTasks(parameters.ProjectId, itemTypeId).Select(
                                    t => new
                                    {
                                        CompletedDate = t.ItemBriefTask.CompletedDate,
                                        ItemName = t.ItemBriefName,
                                        TaskDescription = t.ItemBriefTask.Description,
                                        Vendor = t.ItemBriefTask.Vendor,
                                        NetCost = t.ItemBriefTask.NetCost,
                                        Tax = t.ItemBriefTask.Tax,
                                        Total = t.ItemBriefTask.TotalCost != null ? t.ItemBriefTask.TotalCost : 0
                                    });

                    string sortExpression = parameters.SortExpression;

                    #region EXPORT SORT SETTINGS

                    var sortedItemBriefTasks = itemBriefTask;

                    // set sorting
                    switch (sortExpression)
                    {
                        case "ItemBriefTask.CompletedDate ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.CompletedDate);
                            break;

                        case "ItemBriefTask.CompletedDate DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.CompletedDate);
                            break;

                        case "ItemBriefName ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.ItemName);
                            break;

                        case "ItemBriefName DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.ItemName);
                            break;

                        case "ItemBriefTask.Vendor ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.Vendor);
                            break;

                        case "ItemBriefTask.Vendor DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.Vendor);
                            break;

                        case "ItemBriefTask.NetCost ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.NetCost);
                            break;

                        case "ItemBriefTask.NetCost DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.NetCost);
                            break;

                        case "ItemBriefTask.Tax ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.Tax);
                            break;

                        case "ItemBriefTask.Tax DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.Tax);
                            break;

                        case "Total ASC":
                            sortedItemBriefTasks = itemBriefTask.OrderBy(u1 => u1.Total);
                            break;

                        case "Total DESC":
                            sortedItemBriefTasks = itemBriefTask.OrderByDescending(u1 => u1.Total);
                            break;

                        default:
                            break;
                    }

                    #endregion EXPORT SORT SETTINGS

                    var itemType = Utils.GetItemTypeById(itemTypeId);
                    string reportTitle = string.Empty;
                    if (itemTypeId != 0)
                    {
                        reportTitle = "Itemised Purchase Report - " + itemType.Name;
                    }
                    else
                    {
                        reportTitle = "Itemised Purchase Report";
                    }

                    var details = reportBL.GetReportHeaderDetails(parameters.ProjectId, (user.FirstName + " " + user.LastName), reportTitle);

                    int companyId = project.CompanyId;

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", project.CompanyId) });

                    ReportDataSource reportDataSource = new ReportDataSource("ItemisedPurchase", sortedItemBriefTasks);
                    ReportDataSource reportDataSource2 = new ReportDataSource("ItemisedPurchase2", details);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDataSource);
                    localReport.DataSources.Add(reportDataSource2);
                    localReport.DataSources.Add(logo);
                    localReport.SetParameters(new ReportParameter("CultureInfo", parameters.CultureName));

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the item booking list report.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns>Report byte array</returns>
        public static byte[] GenerateItemBookingListReport(ItemBookingListReportParameters parameters, ReportTypes exportType,
               out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    InventoryBL inventoryBL = new InventoryBL(dataContext);
                    ReportBL reportBL = new ReportBL(dataContext);
                    UtilityBL utilityBL = new UtilityBL(dataContext);
                    PersonalBL personalBL = new PersonalBL(dataContext);

                    User user = personalBL.GetUser(parameters.UserId);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ItemBookingList.rdlc");

                    var itemBookings = inventoryBL.GetBookingTabData(parameters.ItemId);

                    #region EXPORT SORT SETTINGS

                    string sortExpression = parameters.SortExpression;
                    var sortedItemBriefs = itemBookings;

                    // set sorting
                    switch (sortExpression)
                    {
                        case "Project ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.BookingName).ToList<ItemBookingData>();
                            break;

                        case "Project DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.BookingName).ToList<ItemBookingData>();
                            break;

                        case "ItemBrief ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.ItemBrief).ToList<ItemBookingData>();
                            break;

                        case "ItemBrief DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.ItemBrief).ToList<ItemBookingData>();
                            break;

                        case "BookedBy ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.BookedBy).ToList<ItemBookingData>();
                            break;

                        case "BookedBy DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.BookedBy).ToList<ItemBookingData>();
                            break;

                        case "StatusSortOrder ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.StatusSortOrder).ToList<ItemBookingData>();
                            break;

                        case "StatusSortOrder DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.StatusSortOrder).ToList<ItemBookingData>();
                            break;

                        case "FromDate ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.FromDate).ToList<ItemBookingData>();
                            break;

                        case "FromDate DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.FromDate).ToList<ItemBookingData>();
                            break;

                        case "ToDate ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.ToDate).ToList<ItemBookingData>();
                            break;

                        case "ToDate DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.ToDate).ToList<ItemBookingData>();
                            break;

                        case "BookedQuantity ASC":
                            sortedItemBriefs = itemBookings.OrderBy(ib => ib.BookedQuantity).ToList<ItemBookingData>();
                            break;

                        case "BookedQuantity DESC":
                            sortedItemBriefs = itemBookings.OrderByDescending(ib => ib.BookedQuantity).ToList<ItemBookingData>();
                            break;

                        default:
                            break;
                    }

                    #endregion EXPORT SORT SETTINGS

                    Data.Item item = inventoryBL.GetItem(parameters.ItemId);
                    string reportTitle = string.Format("{0} Bookings", item.Name);

                    var headerData = new List<ReportHeaderDetails>();
                    headerData.Add(new ReportHeaderDetails
                    {
                        CompanyName = item.Company.CompanyName,
                        ProjectName = string.Empty,
                        UserName = (user.FirstName + " " + user.LastName),
                        ReportTitle = reportTitle
                    });

                    var companyLogo = new List<dynamic>();
                    companyLogo.Add(new { Logo = utilityBL.GetThumbnailImage("Company", item.Company.CompanyId) });

                    ReportDataSource reportHeader = new ReportDataSource("Header", headerData);
                    ReportDataSource reportDetail = new ReportDataSource("ItemBookingList", sortedItemBriefs);
                    ReportDataSource logo = new ReportDataSource("CompanyLogo", companyLogo);

                    localReport.DataSources.Add(reportDetail);
                    localReport.DataSources.Add(reportHeader);
                    localReport.DataSources.Add(logo);

                    switch (exportType)
                    {
                        case ReportTypes.Pdf:
                            localReport.SetParameters(new ReportParameter("IsExcel", "false"));
                            break;

                        case ReportTypes.Excel:
                            localReport.SetParameters(new ReportParameter("IsExcel", "true"));
                            break;

                        default:
                            break;
                    }

                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        public static byte[] GenerateInventoryExport(int companyId, int itemTypeId,ReportTypes exportType,out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    InventoryBL inventoryBL = new InventoryBL(dataContext);
                    CompanyBL companyBL = new CompanyBL(dataContext);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ExportedInventoryItems.rdlc");
                    var exportInfor = inventoryBL.GetInventoryExportDetails(companyId, itemTypeId);
                    ReportDataSource reportDataSource = new ReportDataSource("InventoryExportInforDS", exportInfor);
                    localReport.DataSources.Add(reportDataSource);
                    localReport.SetParameters(new ReportParameter("CompanyName", string.Concat(companyBL.GetCompany(companyId).CompanyName,"'s Inventory")));
                    localReport.SetParameters(new ReportParameter("ItemTypeName", Utils.GetItemTypeById(itemTypeId).Name));
                    return GetByteArrayByLocalReport(localReport, exportType, true, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        public static byte[] GenerateItemBriefExport(int projectId, int itemTypeId, ReportTypes exportType, out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "ExportedItemBriefList.rdlc");
                    var exportInfor = itemBriefBL.GetItemBriefExportDetails(projectId, itemTypeId);
                    ReportDataSource reportDataSource = new ReportDataSource("ExportedItemBriefListDS", exportInfor);
                    localReport.DataSources.Add(reportDataSource);
                    localReport.SetParameters(new ReportParameter("ProjectName", projectBL.GetProject(projectId).ProjectName));
                    localReport.SetParameters(new ReportParameter("ItemTypeName", Utils.GetItemTypeById(itemTypeId).Name));
                    return GetByteArrayByLocalReport(localReport, exportType, true, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }
    }
}