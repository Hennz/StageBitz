using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for export files.
    /// </summary>
    public partial class ExportFiles : UserControlBase
    {
        #region Properties and Fields

        /// <summary>
        /// The queued status code id var.
        /// </summary>
        public int queuedStatusCodeId = StageBitz.Common.Utils.GetCodeByValue("ExportFileStatus", "QUEUED").CodeId;

        /// <summary>
        /// The completed status code id var.
        /// </summary>
        public int completedStatusCodeId = StageBitz.Common.Utils.GetCodeByValue("ExportFileStatus", "COMPLETED").CodeId;

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        #endregion Properties and Fields

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvExportFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvExportFiles_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
        {
            GridDataItem dataItem = (GridDataItem)e.Item;
            HiddenField hdnRelatedTable = dataItem.FindControl("hdnRelatedTable") as HiddenField;
            string relatedTable = string.Empty;
            if (hdnRelatedTable != null)
            {
                relatedTable = hdnRelatedTable.Value;
            }

            HiddenField hdnRelatedId = dataItem.FindControl("hdnRelatedId") as HiddenField;
            int relatedId = 0;
            if (hdnRelatedId != null)
            {
                relatedId = int.Parse(hdnRelatedId.Value);
            }
            string fileName = string.Empty;
            switch (e.CommandName)
            {
                case "GenerateExportFile":
                    this.GetBL<CompanyBL>().GenerateExportFiles(relatedTable, relatedId, UserID);
                    LoadData();
                    break;

                case "DownLoad":
                    HiddenField hdnExportFileId = dataItem.FindControl("hdnExportFileId") as HiddenField;
                    int exportFileId = 0;
                    if (hdnExportFileId != null)
                    {
                        exportFileId = int.Parse(hdnExportFileId.Value);
                    }
                    ExportFile exportFile = GetBL<CompanyBL>().GetExportedFile(exportFileId);

                    string path = this.GetBL<CompanyBL>().GetExportFileLocation(exportFile.RelatedTable, exportFile.RelatedId, true, out fileName);
                    Utils.DownLoadFile(path, fileName, GlobalConstants.FileContentTypes.ZipFile);
                    break;

                case "Delete":
                    hdnRelatedId.Value = relatedId.ToString();
                    hdnRelatedTable.Value = relatedTable.ToString();
                    btnRemoveFile.CommandArgument = string.Concat(relatedTable.ToString(), ",", relatedId.ToString());
                    popDeleteConfirm.ShowPopup();
                    LoadData();
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveFile_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            char[] characterToSplit = new char[] { ',' };
            string[] idList = button.CommandArgument.Split(characterToSplit);
            if (idList.Length > 1)
            {
                this.GetBL<CompanyBL>().DeleteExportedFile(idList[0], int.Parse(idList[1]), UserID);
            }
            popDeleteConfirm.HidePopup();
            LoadData();
        }

        /// <summary>
        /// Handles the Tick event of the Timer1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvExportFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvExportFiles_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;

                Page page = HttpContext.Current.Handler as Page;
                LinkButton lb = e.Item.FindControl("lnkDownload") as LinkButton;

                if (lb != null)
                    ScriptManager.GetCurrent(page).RegisterPostBackControl(lb);

                dataItem["EntityName"].Text = Support.TruncateString(itemData.EntityName, 20);
                if (!string.IsNullOrEmpty(itemData.EntityName) && itemData.EntityName.Length > 20)
                {
                    dataItem["EntityName"].ToolTip = itemData.EntityName;
                }
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            var allExporFiles = this.GetBL<CompanyBL>().GetExportFileDetails(CompanyId);
            gvExportFiles.DataSource = allExporFiles;
            gvExportFiles.DataBind();
            litDays.Text = StageBitz.Common.Utils.GetSystemValue("ExportedFilesRemainingDays");
        }

        #endregion Private Methods
    }
}