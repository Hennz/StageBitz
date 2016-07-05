using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Notification;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Delegate for inform item list to load
    /// </summary>
    public delegate void InformItemListToLoad();

    /// <summary>
    /// User control to import bulk item briefs
    /// </summary>
    public partial class ImportBulkItemBriefs : UserControlBase
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
        /// The duplicate item list var
        /// </summary>
        private ArrayList duplicateItemList;

        /// <summary>
        /// The reader var
        /// </summary>
        private CSVReader reader;

        /// <summary>
        /// The dt CSV data table  var
        /// </summary>
        private DataTable dtCSVData;

        #endregion Private variables

        #region Event

        /// <summary>
        /// The inform item list to load
        /// </summary>
        public InformItemListToLoad InformItemListToLoad;

        #endregion Event

        #region Public properties

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }
                return (int)ViewState["ProjectID"];
            }
            set
            {
                ViewState["ProjectID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    ViewState["ItemTypeId"] = 0;
                }
                return (int)ViewState["ItemTypeId"];
            }
            set
            {
                ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control instance is read only; otherwise, <c>false</c>.
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
        /// The colcategory const
        /// </summary>
        private const string colcategory = "Category";

        /// <summary>
        /// The colact const
        /// </summary>
        private const string colact = "Act";

        /// <summary>
        /// The colscene const
        /// </summary>
        private const string colscene = "Scene";

        /// <summary>
        /// The colpage const
        /// </summary>
        private const string colpage = "Page";

        /// <summary>
        /// The colpreset const
        /// </summary>
        private const string colpreset = "Preset";

        /// <summary>
        /// The colquantity const
        /// </summary>
        private const string colquantity = "Quantity";

        /// <summary>
        /// The colcharacter const
        /// </summary>
        private const string colcharacter = "Character";

        /// <summary>
        /// The colrehearsal const
        /// </summary>
        private const string colrehearsal = "Rehearsal";

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
            btnImportItemBrief.Visible = !IsReadOnly;
            if (ItemTypeId != 0)
                lithelpTip.Text = "Select 'Confirm' to create your " + Utils.GetItemTypeById(ItemTypeId).Name + " Briefs.";
            else
                lithelpTip.Text = "Select 'Confirm' to create.";

            ItemType itemType = Utils.GetItemTypeById(ItemTypeId);
            if (itemType != null)
                popupImportItems.Title = string.Concat("Import ", itemType.Name, " Briefs from list");
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
                var itemToRemove = (from pit in DataContext.ProjectItemTypes
                                    where pit.ProjectId == ProjectID && pit.ItemTypeId == ItemTypeId
                                    select pit).FirstOrDefault();
                if (itemToRemove == null)
                {
                    popupSuccess.HidePopup();
                    Response.Redirect(String.Format("~/Project/ProjectDashboard.aspx?projectid={0}", ProjectID));
                }
                else
                {
                    CheckDuplicatesinCSVFileWithExistingData();

                    foreach (GridDataItem gvRow in gvItems.Items)
                    {
                        if (!duplicateItemList.Contains(gvRow[colitemName].Text))
                        {
                            StageBitz.Data.ItemBrief itemBrief = new Data.ItemBrief();

                            #region Set Defaults

                            itemBrief.Description = string.Empty;
                            itemBrief.Category = string.Empty;
                            itemBrief.Act = string.Empty;
                            itemBrief.Scene = string.Empty;
                            itemBrief.Page = string.Empty;

                            #endregion Set Defaults

                            itemBrief.Name = gvRow[colitemName].Text;
                            if (whiteSpace != gvRow[coldescription].Text)
                                itemBrief.Description = gvRow[coldescription].Text;

                            if (whiteSpace != gvRow[colcategory].Text)
                                itemBrief.Category = gvRow[colcategory].Text;

                            if (whiteSpace != gvRow[colact].Text)
                                itemBrief.Act = gvRow[colact].Text;

                            if (whiteSpace != gvRow[colscene].Text)
                                itemBrief.Scene = gvRow[colscene].Text;

                            if (whiteSpace != gvRow[colpage].Text)
                                itemBrief.Page = gvRow[colpage].Text;

                            if (whiteSpace != gvRow[colpreset].Text)
                                itemBrief.Preset = gvRow[colpreset].Text;

                            if (whiteSpace != gvRow[colcharacter].Text)
                                itemBrief.Character = gvRow[colcharacter].Text;

                            if (whiteSpace != gvRow[colrehearsal].Text)
                                itemBrief.RehearsalItem = gvRow[colrehearsal].Text;

                            int quantity = 0;
                            int.TryParse(gvRow[colquantity].Text, out quantity);
                            if (quantity == 0)
                                itemBrief.Quantity = null;
                            else
                                itemBrief.Quantity = int.Parse(gvRow[colquantity].Text);

                            itemBrief.Budget = 0;
                            itemBrief.ProjectId = ProjectID;
                            itemBrief.ItemBriefStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");
                            itemBrief.CreatedByUserId = UserID;
                            itemBrief.LastUpdatedByUserId = UserID;
                            itemBrief.CreatedDate = Now;
                            itemBrief.LastUpdatedDate = Now;

                            #region ItemBrief Types

                            Data.ItemBriefType itemBriefType = new Data.ItemBriefType();
                            itemBriefType.ItemTypeId = ItemTypeId;
                            itemBriefType.ItemBriefTypeCodeId = Support.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");
                            itemBriefType.CreatedByUserId = itemBriefType.LastUpdatedByUserId = UserID;
                            itemBriefType.CreatedDate = itemBriefType.LastUpdatedDate = Now;
                            itemBriefType.IsActive = true;

                            #endregion ItemBrief Types

                            itemBrief.ItemBriefTypes.Add(itemBriefType);

                            GetBL<ItemBriefBL>().AddItemBrief(itemBrief);
                        }
                    }

                    StringBuilder finalErrorbuilder = new StringBuilder();

                    //(gvItems.Items.Count - duplicateItemList.Count) gives how many items being inserted.
                    int successcount = (gvItems.Items.Count - duplicateItemList.Count);
                    if (successcount == 0)
                        popupSuccess.Title = "Failed to Import";
                    if (successcount == 1)
                        finalErrorbuilder.AppendLine(string.Format("{0} Item has been successfully imported.<br />", successcount));
                    else if (successcount > 1)
                        finalErrorbuilder.AppendLine(string.Format("{0} Items have been successfully imported.<br />", successcount));

                    #region Generate 'Bulk Import' Notification

                    Notification nf = new Notification();
                    nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "ITEMLIST");
                    nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "ADD");
                    nf.RelatedId = 0;
                    nf.ProjectId = ProjectID;
                    nf.Message = string.Format("{0} imported {1} Item Brief{2}.", Support.UserFullName, successcount, (successcount == 1) ? string.Empty : "s");
                    nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                    nf.CreatedDate = nf.LastUpdatedDate = Now;

                    GetBL<NotificationBL>().AddNotification(nf);

                    #endregion Generate 'Bulk Import' Notification

                    popupSuccess.ShowPopup();

                    divSuccessMsg.InnerHtml = finalErrorbuilder.AppendLine(errorBuilder.ToString()).ToString();
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
                LoadItemBriefs();
                //Since divMsg is to show errors, it has to depend on errorBuilder
                divMsg.Visible = (errorBuilder.Length > 0);
                if (errorBuilder.Length > 0)
                    divMsg.InnerHtml = errorBuilder.ToString();
                duplicateItemList = new ArrayList();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnImportItemBrief control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportItemBrief_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupImportItems.ShowPopup();
                LoadItemBriefs();
                ConfigureUI(false);
            }
        }

        #endregion Event Handlers

        #region Private methods

        /// <summary>
        /// Configures the UI.
        /// </summary>
        /// <param name="isFileSelected">if set to <c>true</c> [is file selected].</param>
        private void ConfigureUI(bool isFileSelected)
        {
            lnkShowUpload.Visible = isFileSelected;
            divItemsGrid.Visible = isFileSelected;
            radUploader.Visible = !isFileSelected;
            divFile.Visible = !isFileSelected;
            btnConfirm.Visible = isFileSelected;
            divMsg.Visible = isFileSelected;
            if (!isFileSelected)
                divMsg.InnerHtml = string.Empty;
        }

        /// <summary>
        /// Loads the item briefs.
        /// </summary>
        private void LoadItemBriefs()
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
        /// Checks the duplicatesin CSV.
        /// </summary>
        /// <returns></returns>
        private string CheckDuplicatesinCSV()
        {
            duplicateItemList = new ArrayList();

            foreach (DataRow dtRow in dtCSVData.Rows)
            {
                if (errorCount > maximumerrorcount)
                {
                    break;
                }

                StringBuilder errorRowBuilder = new StringBuilder();

                //Get the duplicate rows.
                DataRow[] drDuplicateCollection = dtCSVData.Select("[Item Name] IS NOT NULL AND [Item Name] <> '' AND [Item Name]='" + dtRow[colitemName].ToString().Replace("'", "''") + "'");

                if (drDuplicateCollection.Length > 1 && !duplicateItemList.Contains(dtRow[colitemName])) //If more than the same item name exist and if it is should not have been checked before.
                {
                    //Add to the duplicate list
                    duplicateItemList.Add(dtRow[colitemName].ToString());

                    //Iterate through all the duplicate rows and build the error message.
                    foreach (DataRow dtRowNumber in drDuplicateCollection)
                    {
                        errorRowBuilder.Append(dtRowNumber[colrowNumber]);
                        errorRowBuilder.Append(",");
                    }

                    errorRowBuilder.Remove(errorRowBuilder.Length - 1, 1);//remove the last comma.

                    errorBuilder.Append(string.Format("Row {0}: Duplicate Items exist for 'Item Name' {1} in the CSV file.", errorRowBuilder.ToString(), dtRow[colitemName].ToString()));
                    errorBuilder.Append("<br/>");
                    errorCount++;
                }
            }
            return errorBuilder.ToString();
        }

        /// <summary>
        /// Determines whether the specified CSV line is empty data row.
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
                string preset = csvLine[dtCSVData.Columns[colpreset].Ordinal - 1].ToString();
                string character = csvLine[dtCSVData.Columns[colcharacter].Ordinal - 1].ToString();
                string rehearsal = csvLine[dtCSVData.Columns[colrehearsal].Ordinal - 1].ToString();
                string act = csvLine[dtCSVData.Columns[colact].Ordinal - 1].ToString();

                string scene = csvLine[dtCSVData.Columns[colscene].Ordinal - 1].ToString();
                string page = csvLine[dtCSVData.Columns[colpage].Ordinal - 1].ToString();
                string category = csvLine[dtCSVData.Columns[colcategory].Ordinal - 1].ToString();
                string description = csvLine[dtCSVData.Columns[coldescription].Ordinal - 1].ToString();

                if (itemName == string.Empty && quantity == string.Empty && preset == string.Empty && act == string.Empty
                        && scene == string.Empty && page == string.Empty && category == string.Empty && description == string.Empty
                        && character == string.Empty && rehearsal == string.Empty)
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

            int itemNameLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>("Name");
            int sceneLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colscene);
            int pageLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colpage);
            int presetLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colpreset);
            int actLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colact);
            int categoryLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colcategory);
            int characterLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colcharacter);
            int rehearsalLength = Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>("RehearsalItem");

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
                        //isReadytoInsert = false;
                    }
                    else if (itemNameLength < itemName.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", lineIndex, colitemName, itemNameLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
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
                            //isReadytoInsert = false;
                            errorCount++;
                        }
                        else
                        {
                            bool isConversionSuccess = int.TryParse(csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString(), out quantity);

                            if (!isConversionSuccess)
                            {
                                string error = string.Format("Row {0}: Incorrect data format for 'Quantity': {1}. Please provide a numeric value other than zero.<br />", (lineIndex), csvLine[dtCSVData.Columns[colquantity].Ordinal - 1].ToString());
                                errorBuilder.Append(error);
                                //isReadytoInsert = false;
                                errorCount++;
                            }
                            else if (quantity <= 0)
                            {
                                string error = string.Format("Row {0}: 'Quantity' cannot be zero.<br />", (lineIndex));
                                errorBuilder.Append(error);
                                //isReadytoInsert = false;
                                errorCount++;
                            }
                        }
                    }

                    #endregion quantity

                    #region description

                    string description = csvLine[dtCSVData.Columns["Description"].Ordinal - 1].ToString();

                    #endregion description

                    #region category

                    string category = csvLine[dtCSVData.Columns["Category"].Ordinal - 1].ToString();

                    if (categoryLength < category.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colcategory, categoryLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion category

                    #region act

                    string act = csvLine[dtCSVData.Columns["Act"].Ordinal - 1].ToString();

                    if (actLength < act.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colact, actLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion act

                    #region scene

                    string scene = csvLine[dtCSVData.Columns["Scene"].Ordinal - 1].ToString();

                    if (Support.GetDatabaseFieldLength<StageBitz.Data.ItemBrief>(colscene) < scene.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colscene, sceneLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion scene

                    #region page

                    string page = csvLine[dtCSVData.Columns["Page"].Ordinal - 1].ToString();

                    if (pageLength < page.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colpage, pageLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion page

                    #region preset

                    string preset = csvLine[dtCSVData.Columns["Preset"].Ordinal - 1].ToString();

                    if (presetLength < preset.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colpreset, presetLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion preset

                    #region Character

                    string character = csvLine[dtCSVData.Columns["Character"].Ordinal - 1].ToString();

                    if (characterLength < character.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colcharacter, characterLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion Character

                    #region Rehearsal

                    string rehearsal = csvLine[dtCSVData.Columns["Rehearsal"].Ordinal - 1].ToString();

                    if (rehearsalLength < rehearsal.Length)
                    {
                        string error = string.Format("Row {0}: \'{1}\' exceeds the maximum character length ({2}).<br />", (lineIndex), colrehearsal, rehearsalLength);
                        errorBuilder.Append(error);
                        //isReadytoInsert = false;
                        errorCount++;
                    }

                    #endregion Rehearsal

                    #endregion VALIDATE EACH DATA ITEM

                    #region CREATE NEW DATA ROW

                    //if (isReadytoInsert)
                    //{
                    DataRow newRow = dtCSVData.NewRow();
                    newRow[colrowNumber] = lineIndex;
                    newRow[colitemName] = itemName;
                    newRow[coldescription] = description;
                    newRow[colcategory] = category;
                    newRow[colact] = act;
                    newRow[colscene] = scene.ToString();
                    newRow[colpage] = page;
                    newRow[colpreset] = preset;
                    newRow[colrehearsal] = rehearsal;
                    newRow[colcharacter] = character;
                    newRow[colquantity] = (quantity == 0) ? string.Empty : quantity.ToString();
                    dtCSVData.Rows.Add(newRow);
                    //}

                    #endregion CREATE NEW DATA ROW

                    lineIndex++;
                }
            }
        }

        /// <summary>
        /// Checks the duplicatesin CSV file with existing data.
        /// </summary>
        private void CheckDuplicatesinCSVFileWithExistingData()
        {
            var eventList = (from ib in DataContext.ItemBriefs
                             where ib.ProjectId == ProjectID
                             select ib).ToList<StageBitz.Data.ItemBrief>();

            duplicateItemList = new ArrayList();

            foreach (StageBitz.Data.ItemBrief itemBrief in eventList)
            {
                if (errorCount > maximumerrorcount)
                {
                    break;
                }

                StringBuilder errorRowBuilder = new StringBuilder();

                if (dtCSVData != null)
                {
                    //Add all the duplicate rows to an array.
                    DataRow[] drDuplicateCollection = dtCSVData.Select("[Item Name]='" + itemBrief.Name.Replace("'", "''") + "'");
                    if (drDuplicateCollection.Length > 0 && !duplicateItemList.Contains(itemBrief.Name))
                    {
                        //Add to the duplicate list
                        duplicateItemList.Add(itemBrief.Name);

                        //Iterate through all the duplicate rows and build the error message.
                        foreach (DataRow dtRowNumber in drDuplicateCollection)
                        {
                            errorRowBuilder.Append(dtRowNumber[colrowNumber]);
                            errorRowBuilder.Append(",");
                        }

                        errorRowBuilder.Remove(errorRowBuilder.Length - 1, 1);//remove the last comma.
                        errorBuilder.Append(string.Format("Row {0}: \'{1}\'  already exists in this Project.", errorRowBuilder.ToString(), itemBrief.Name));
                        errorBuilder.Append("<br/>");
                        errorCount++;
                    }
                }
                else
                {
                    foreach (GridDataItem gvRow in gvItems.Items)
                    {
                        if (gvRow[colitemName].Text == itemBrief.Name && !duplicateItemList.Contains(gvRow[colitemName].Text))
                        {
                            errorRowBuilder.Append(gvRow.OwnerTableView.DataKeyValues[gvRow.ItemIndex]["RowNumber"]);
                            errorRowBuilder.Append(",");
                            duplicateItemList.Add(gvRow[colitemName].Text);
                        }
                    }
                    if (errorRowBuilder.Length > 0)
                    {
                        errorRowBuilder.Remove(errorRowBuilder.Length - 1, 1);//remove the last comma.
                        errorBuilder.Append(string.Format("Row {0}: \'{1}\'  already exists in this Project.", errorRowBuilder.ToString(), itemBrief.Name));
                        errorBuilder.Append("<br/>");
                        errorCount++;
                    }
                }
            }

            //To make sure empty templates can not be uploaded.
            if (dtCSVData != null && dtCSVData.Rows.Count == 0 && errorCount == 0)
            {
                errorBuilder.Append("Please import a valid CSV file.");
                errorCount++;
                return;
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
                        dtCSVData.Columns.Add(new DataColumn(colcategory));
                        dtCSVData.Columns.Add(new DataColumn(colcharacter));
                        dtCSVData.Columns.Add(new DataColumn(colpreset));
                        dtCSVData.Columns.Add(new DataColumn(colrehearsal));
                        dtCSVData.Columns.Add(new DataColumn(colact));
                        dtCSVData.Columns.Add(new DataColumn(colscene));
                        dtCSVData.Columns.Add(new DataColumn(colpage));

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
                        CheckDuplicatesinCSV();
                        CheckDuplicatesinCSVFileWithExistingData();

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

        #endregion Private methods
    }
}