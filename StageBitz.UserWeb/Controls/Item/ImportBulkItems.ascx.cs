using StageBitz.Common;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemTypes;
using StageBitz.Logic.Business.Location;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections;
using System.Data;
using System.Text;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Item
{
    /// <summary>
    /// Delegate for inform item list to load
    /// </summary>
    public delegate void InformItemListToLoad();

    /// <summary>
    /// Delegate for inform company inventory to show error popup
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    public delegate void InformCompanyInventoryToShowErrorPopup(ErrorCodes errorCode);

    public partial class ImportBulkItems : UserControlBase
    {
        #region Private variables

        /// <summary>
        /// The error count var
        /// </summary>
        private int errorCount = 0;

        /// <summary>
        /// The error builder var
        /// </summary>
        private StringBuilder errorBuilder = new StringBuilder();

        /// <summary>
        /// The reader var
        /// </summary>
        private CSVReader reader;

        /// <summary>
        /// The dt CSV data table var
        /// </summary>
        private DataTable dtCSVData;

        #endregion Private variables

        #region Events

        /// <summary>
        /// The inform item list to load
        /// </summary>
        public InformItemListToLoad InformItemListToLoad;

        /// <summary>
        /// The inform company inventory to show error popup
        /// </summary>
        public InformCompanyInventoryToShowErrorPopup InformCompanyInventoryToShowErrorPopup;

        #endregion Events

        #region Public properties

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyID
        {
            get
            {
                if (ViewState["CompanyID"] == null)
                {
                    ViewState["CompanyID"] = 0;
                }
                return (int)ViewState["CompanyID"];
            }
            set
            {
                ViewState["CompanyID"] = value;
            }
        }

        /// <summary>
        /// Gets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeId
        {
            get
            {
                int itemTypeId;
                if (int.TryParse(ddlItemType.SelectedValue, out itemTypeId))
                {
                    return itemTypeId;
                }
                else
                {
                    itemTypeId = 0;
                }

                return itemTypeId;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    ViewState["IsReadOnly"] = false;
                }
                return (bool)ViewState["IsReadOnly"];
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Public properties

        #region Constants

        /// <summary>
        /// The colrow number const
        /// </summary>
        private const string colrowNumber = "RowNumber";

        /// <summary>
        /// The colitem name const
        /// </summary>
        private const string colitemName = "Item Name";

        /// <summary>
        /// The coldescription const
        /// </summary>
        private const string coldescription = "Description";

        /// <summary>
        /// The colquantity const
        /// </summary>
        private const string colquantity = "Quantity";

        /// <summary>
        /// The white space const
        /// </summary>
        private const string whiteSpace = "&nbsp;";

        /// <summary>
        /// The maximumerrorcount const
        /// </summary>
        private const int maximumerrorcount = 9;

        #endregion Constants

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            btnImportItem.Visible = !IsReadOnly;

            if (!this.IsPostBack)
            {
                ddlItemType.DataSource = GetBL<ItemTypesBL>().GetAllItemTypeData();
                ddlItemType.DataTextField = "Name";
                ddlItemType.DataValueField = "Id";
                ddlItemType.DataBind();

                sbPackageLimitsValidation.CompanyId = CompanyID;
                sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.InventoryLimit;
                sbPackageLimitsValidation.LoadData();
            }
        }

        /// <summary>
        /// Closes the pop up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ClosePopUp(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupSuccess.HidePopup();
                popupImportItems.HidePopup();
                InformItemListToLoad();
            }
        }

        /// <summary>
        /// Confirms the import.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ConfirmImport(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (bulkImportItemsInvLocation.SelectedLocationId.HasValue)
                {
                    if (GetBL<LocationBL>().GetLocation(bulkImportItemsInvLocation.SelectedLocationId.Value) == null)
                    {
                        divMsg.InnerText = "Location has already been deleted.";
                        divMsg.Visible = true;
                        return;
                    }
                }

                if (this.GetBL<CompanyBL>().HasEditPermissionForInventoryStaff(this.CompanyID, UserID, bulkImportItemsInvLocation.SelectedLocationId))
                {
                    if (sbPackageLimitsValidation.Validate(gvItems.Items.Count))
                    {
                        foreach (GridDataItem gvRow in gvItems.Items)
                        {
                            StageBitz.Data.Item item = new Data.Item();

                            #region Set Defaults

                            item.Description = string.Empty;
                            item.LocationId = bulkImportItemsInvLocation.SelectedLocationId;

                            #endregion Set Defaults

                            item.Name = gvRow[colitemName].Text;
                            if (whiteSpace != gvRow[coldescription].Text)
                                item.Description = gvRow[coldescription].Text;

                            int quantity = 0;
                            if (int.TryParse(gvRow[colquantity].Text, out quantity))
                            {
                                item.Quantity = quantity;
                            }

                            item.ItemTypeId = ItemTypeId;
                            item.CompanyId = CompanyID;
                            item.IsManuallyAdded = true;
                            item.IsActive = true;
                            item.CreatedByUserId = UserID;
                            item.LastUpdatedByUserId = UserID;
                            item.CreatedDate = Now;
                            item.LastUpdatedDate = Now;
                            item.VisibilityLevelCodeId = Support.GetCodeIdByCodeValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");

                            GetBL<InventoryBL>().AddItem(item, true);
                        }

                        StringBuilder finalErrorbuilder = new StringBuilder();

                        //(gvItems.Items.Count - duplicateItemList.Count) gives how many items being inserted.
                        int successcount = gvItems.Items.Count;
                        if (successcount == 0)
                            popupSuccess.Title = "Failed to Import";
                        if (successcount == 1)
                            finalErrorbuilder.AppendLine(string.Format("{0} Item has been successfully imported.<br />", successcount));
                        else if (successcount > 1)
                            finalErrorbuilder.AppendLine(string.Format("{0} Items have been successfully imported.<br />", successcount));

                        popupSuccess.ShowPopup();

                        divSuccessMsg.InnerHtml = finalErrorbuilder.AppendLine(errorBuilder.ToString()).ToString();
                    }
                    else
                    {
                        popupImportItems.HidePopup();
                    }
                }
                else
                {
                    if (InformCompanyInventoryToShowErrorPopup != null)
                    {
                        popupImportItems.HidePopup();
                        InformCompanyInventoryToShowErrorPopup(ErrorCodes.NoEditPermissionForInventory);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkShowUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkShowUpload_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                ConfigureUI(false);
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the radUploader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploadedEventArgs"/> instance containing the event data.</param>
        protected void radUploader_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                ConfigureUI(true);
                ValidateCSVFormat(e);
                btnConfirm.Visible = !(errorCount > 0);
                LoadItems();
                //Since divMsg is to show errors, it has to depend on errorBuilder
                divMsg.Visible = (errorBuilder.Length > 0);
                if (errorBuilder.Length > 0)
                    divMsg.InnerHtml = errorBuilder.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnImportItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportItem_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupImportItems.ShowPopup();
                LoadItems();
                LoadLocationData();
                ConfigureUI(false);
            }
        }

        #endregion Event Handlers

        #region Private methods

        /// <summary>
        /// Loads the items.
        /// </summary>
        private void LoadItems()
        {
            if (errorCount > 0)
            {
                gvItems.DataSource = null;
                divItemsGrid.Visible = false;
            }
            else
            {
                gvItems.DataSource = dtCSVData;
                divItemsGrid.Visible = true;
            }
            gvItems.DataBind();
        }

        /// <summary>
        /// Determines whether [is empty data row] [the specified CSV line].
        /// </summary>
        /// <param name="csvLine">The CSV line.</param>
        /// <returns></returns>
        private bool IsEmptyDataRow(ArrayList csvLine)
        {
            //We have to check this situation because when there is one Row in the csvLine that can be an empty Row or else it'is in the wrong format
            if (csvLine.Count == 1 && csvLine[0].ToString() == string.Empty)
            {
                return true;
            }
            else if (csvLine.Count == dtCSVData.Columns.Count - 1)
            {
                string itemName = csvLine[dtCSVData.Columns[colitemName].Ordinal - 1].ToString();
                string quantity = csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString();
                string description = csvLine[dtCSVData.Columns[coldescription].Ordinal - 1].ToString();

                if (itemName == string.Empty && quantity == string.Empty && description == string.Empty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Validates the CSV file data and addto table.
        /// </summary>
        private void ValidateCSVFileDataAndAddtoTable()
        {
            ArrayList csvLine = new ArrayList();
            int lineIndex = 2;//Start by 2 to skip header.

            #region Database field length

            int itemNameLength = Support.GetDatabaseFieldLength<StageBitz.Data.Item>("Name");

            #endregion Database field length

            while ((csvLine = reader.GetCSVLine()) != null)
            {
                if (errorCount > maximumerrorcount)
                {
                    //if more than defined maximum number of errors found, we are not going to perform the validation to rest of the file.
                    return;
                }

                //We are not validating empty rows but increase Row number.
                if (IsEmptyDataRow(csvLine))
                {
                    lineIndex++;
                }
                else
                {
                    //bool isReadytoInsert = true;//This is to flag candidate items which are pending to get inserted to the table.

                    if (csvLine.Count != dtCSVData.Columns.Count - 1)
                    {
                        string error = string.Format("Row {0}: Invalid number of Items appearing in the Row.<br />", (lineIndex));
                        errorBuilder.Append(error);
                        errorCount++;
                        lineIndex++;
                        //isReadytoInsert = false;
                        continue;
                    }

                    #region VALIDATE EACH DATA ITEM

                    #region itemName

                    string itemName = csvLine[dtCSVData.Columns[colitemName].Ordinal - 1].ToString();
                    if (string.IsNullOrEmpty(itemName))
                    {
                        string error = string.Format("Row {0}: Information is missing from the spreadsheet for 'Item Name'.<br />", lineIndex);
                        errorBuilder.Append(error);
                        errorCount++;
                    }
                    else if (itemNameLength < itemName.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", lineIndex, colitemName, itemNameLength);
                        errorBuilder.Append(error);
                        errorCount++;
                    }

                    #endregion itemName

                    #region quantity

                    int quantity = 0;
                    if (!string.IsNullOrEmpty(csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString()))
                    {
                        if (csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString().Trim().Length > 8)
                        {
                            string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length (8).<br />", (lineIndex), colquantity);
                            errorBuilder.Append(error);
                            errorCount++;
                        }
                        else
                        {
                            bool isConversionSuccess = int.TryParse(csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString(), out quantity);

                            if (!isConversionSuccess)
                            {
                                string error = string.Format("Row {0}: Incorrect data format for 'Quantity': {1}. Please provide a numeric value.<br />", (lineIndex), csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString());
                                errorBuilder.Append(error);
                                errorCount++;
                            }
                            else if (quantity < 0)
                            {
                                string error = string.Format("Row {0}: 'Quantity' cannot be negative.<br />", (lineIndex));
                                errorBuilder.Append(error);
                                errorCount++;
                            }
                        }
                    }
                    else
                    {
                        string error = string.Format("Row {0}: 'Quantity' cannot be empty.<br />", (lineIndex));
                        errorBuilder.Append(error);
                        errorCount++;
                    }

                    #endregion quantity

                    #region description

                    string description = csvLine[dtCSVData.Columns[coldescription].Ordinal - 1].ToString();

                    #endregion description

                    #endregion VALIDATE EACH DATA ITEM

                    #region CREATE NEW DATA ROW

                    DataRow newRow = dtCSVData.NewRow();
                    newRow[colrowNumber] = lineIndex;
                    newRow[colitemName] = itemName;
                    newRow[coldescription] = description;
                    newRow[colquantity] = quantity.ToString();
                    dtCSVData.Rows.Add(newRow);

                    #endregion CREATE NEW DATA ROW

                    lineIndex++;
                }
            }
        }

        /// <summary>
        /// Validates the CSV format.
        /// </summary>
        /// <param name="e">The <see cref="FileUploadedEventArgs"/> instance containing the event data.</param>
        private void ValidateCSVFormat(FileUploadedEventArgs e)
        {
            if (!e.IsValid)
            {
                errorBuilder.Append("The selected file type is invalid. Please select a CSV file.");
                return;
            }

            //create object for CSVReader and pass the stream
            reader = new CSVReader(e.File.InputStream);

            //get headers reading by first Row
            ArrayList csvHeaders = new ArrayList();
            bool hasData = false;
            while ((csvHeaders = reader.GetCSVLine()) != null)
            {
                hasData = true;
                //CSV header is mandatory.If not we are not going to perform any validation
                if (csvHeaders.Count <= 0)
                {
                    errorBuilder.Append("CSV file header cannot be empty.");
                    return;
                }
                else if (csvHeaders.Count > 1)
                {
                    using (dtCSVData = new DataTable("tblCSVData"))
                    {
                        #region Data table

                        dtCSVData.Columns.Add(new DataColumn(colrowNumber));
                        dtCSVData.Columns.Add(new DataColumn(colitemName));
                        dtCSVData.Columns.Add(new DataColumn(coldescription));
                        dtCSVData.Columns.Add(new DataColumn(colquantity));

                        #endregion Data table

                        if (csvHeaders.Count > dtCSVData.Columns.Count - 1)
                        {
                            errorCount++;
                            errorBuilder.Append("Invalid number of Items appearing in the Header.<br />");
                        }

                        // header fields must be exist in the CSV file
                        foreach (var item in dtCSVData.Columns)
                        {
                            if (item.ToString() != colrowNumber)
                            {
                                if (!csvHeaders.Contains(item.ToString()))
                                {
                                    string error = string.Format("The column header \'{0}\' is missing in CSV file.<br />", item);
                                    errorCount++;
                                    errorBuilder.Append(error);
                                }
                                //Check if the column order is in correct order.dtCSVData.Columns.Count - 1 is to ignore Row number.
                                else if (csvHeaders.Count >= dtCSVData.Columns.Count - 1 && csvHeaders[dtCSVData.Columns.IndexOf(item.ToString()) - 1].ToString() != item.ToString())
                                {
                                    errorCount++;
                                    errorBuilder.Append("Column structure does not match the expected format.<br />");
                                    break;
                                }
                            }
                        }

                        if (errorBuilder.Length > 0)
                        {
                            //At this point, at least one column from the header is missing.So no point of validating the data.
                            return;
                        }

                        ValidateCSVFileDataAndAddtoTable();
                        //To make sure empty templates can not be uploaded.
                        if (dtCSVData != null && dtCSVData.Rows.Count == 0 && errorCount == 0)
                        {
                            errorBuilder.Append("Please import a valid CSV file.");
                            errorCount++;
                            return;
                        }

                        break; //this needs to be executed only one time.
                    }
                }
                else
                {
                    hasData = false;
                }
            }
            if (!hasData)
            {
                errorBuilder.Append("Invalid CSV file.");
                errorCount++;
                return;
            }
            radUploader.Visible = false;
            lnkShowUpload.Visible = true;
        }

        /// <summary>
        /// Loads the location data.
        /// </summary>
        private void LoadLocationData()
        {
            bulkImportItemsInvLocation.CompanyId = CompanyID;
            bulkImportItemsInvLocation.LoadData();
        }

        /// <summary>
        /// Configures the UI.
        /// </summary>
        /// <param name="isFileSelected">if set to <c>true</c> [is file selected].</param>
        private void ConfigureUI(bool isFileSelected)
        {
            ddlItemType.SelectedValue = string.Empty;
            lnkShowUpload.Visible = isFileSelected;
            divItemsGrid.Visible = isFileSelected;
            radUploader.Visible = !isFileSelected;
            divFile.Visible = !isFileSelected;
            btnConfirm.Visible = isFileSelected;
            divMsg.Visible = isFileSelected;
            if (!isFileSelected)
                divMsg.InnerHtml = string.Empty;
        }

        #endregion Private methods
    }
}