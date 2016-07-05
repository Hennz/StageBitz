using Microsoft.Reporting.WebForms;
using StageBitz.Common.Enum;
using StageBitz.Data;
using System.Data;

namespace StageBitz.Reports.AdminWeb.Helper
{
    /// <summary>
    /// Report handler lass for admin web site.
    /// </summary>
    public class AdminWebReportHandler : ReportHandlerBase
    {
        /// <summary>
        /// The admin web site report namespace.
        /// </summary>
        public const string ReportNamespace = "StageBitz.Reports.AdminWeb.";

        /// <summary>
        /// Generates the user list report.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns></returns>
        public static byte[] GenerateUserListReport(DataView dataView, ReportTypes exportType,
                out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ReportDataSource dataSource = new ReportDataSource("Users", dataView);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "UserList.rdlc");
                    localReport.DataSources.Add(dataSource);
                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }

        /// <summary>
        /// Generates the prcing plan history report.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <returns></returns>
        public static byte[] GeneratePrcingPlanHistoryReport(DataView dataView, ReportTypes exportType,
               out string fileNameExtension, out string encoding, out string mimeType, bool isLandscape = false)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                using (LocalReport localReport = new LocalReport())
                {
                    ReportDataSource dataSource = new ReportDataSource("PricingPlanHistory", dataView);
                    localReport.ReportEmbeddedResource = string.Concat(ReportNamespace, "PrcingPlanHistory.rdlc");
                    localReport.DataSources.Add(dataSource);
                    return GetByteArrayByLocalReport(localReport, exportType, isLandscape, out fileNameExtension, out encoding, out mimeType);
                }
            }
        }
    }
}