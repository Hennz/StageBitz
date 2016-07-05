using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for export data.
    /// </summary>
    public partial class ExportData : UserControlBase
    {
        #region Events

        /// <summary>
        /// Occurs when [PDF export click].
        /// </summary>
        public event EventHandler PDFExportClick;

        /// <summary>
        /// Occurs when [excel export click].
        /// </summary>
        public event EventHandler ExcelExportClick;

        #endregion Events

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the Click event of the btnCreatePDF control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreatePDF_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                //Raise the event to notify parent that pdf export event has occured
                if (PDFExportClick != null)
                {
                    PDFExportClick(sender, e);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendToExcel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendToExcel_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (ExcelExportClick != null)
                {
                    ExcelExportClick(sender, e);
                }
            }
        }

        #endregion Event Handlers
    }
}