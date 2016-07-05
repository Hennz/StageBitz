using Microsoft.Reporting.WebForms;
using StageBitz.Common.Enum;

namespace StageBitz.Reports
{
    /// <summary>
    /// Base class for report handlers.
    /// </summary>
    public class ReportHandlerBase
    {
        /// <summary>
        /// Gets the byte array for given local report.
        /// </summary>
        /// <param name="localReport">The local report.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="isLandscape">if set to <c>true</c> [is landscape].</param>
        /// <param name="fileNameExtension">The file name extension.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <returns></returns>
        internal static byte[] GetByteArrayByLocalReport(LocalReport localReport, ReportTypes exportType, bool isLandscape, out string fileNameExtension, out string encoding, out string mimeType)
        {
            string reportType = string.Empty;
            string deviceInfo = null;

            Warning[] warnings;
            string[] streams;

            switch (exportType)
            {
                case ReportTypes.Pdf: //Report properties are overridden by deviceinfo, this must be done.
                    if (isLandscape)
                    {
                        deviceInfo =
                            "<DeviceInfo>" +
                            "  <OutputFormat>PDF</OutputFormat>" +
                            "  <PageWidth>11in</PageWidth>" +
                            "  <PageHeight>8.5in</PageHeight>" +
                            "  <MarginTop>0.5in</MarginTop>" +
                            "  <MarginLeft>0.5in</MarginLeft>" +
                            "  <MarginRight>0.5in</MarginRight>" +
                            "  <MarginBottom>0.5in</MarginBottom>" +
                            "</DeviceInfo>";
                    }
                    else
                    {
                        deviceInfo =
                            "<DeviceInfo>" +
                            "  <OutputFormat>PDF</OutputFormat>" +
                            "  <PageWidth>8.5in</PageWidth>" +
                            "  <PageHeight>11in</PageHeight>" +
                            "  <MarginTop>0.5in</MarginTop>" +
                            "  <MarginLeft>0.5in</MarginLeft>" +
                            "  <MarginRight>0.5in</MarginRight>" +
                            "  <MarginBottom>0.5in</MarginBottom>" +
                            "</DeviceInfo>";
                    }

                    reportType = "PDF";
                    break;

                case ReportTypes.Excel:
                    reportType = "Excel";
                    break;

                default:
                    break;
            }

            return localReport.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
        }
    }
}