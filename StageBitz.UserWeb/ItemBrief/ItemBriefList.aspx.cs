using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for item brief list.
    /// </summary>
    public partial class ItemBriefList : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    int ProjectId = 0;

                    if (Request["ProjectId"] != null)
                    {
                        int.TryParse(Request["ProjectId"], out ProjectId);
                    }

                    ViewState["ProjectId"] = ProjectId;
                }

                return (int)ViewState["ProjectId"];
            }
        }

        /// <summary>
        /// Gets the company identifier.
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
            private set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        private int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    int itemTypeId = 0;

                    if (Request["ItemTypeId"] != null)
                    {
                        int.TryParse(Request["ItemTypeId"], out itemTypeId);
                    }
                    else
                        itemTypeId = projectItemTypes.SelectedItemTypeId;

                    ViewState["ItemTypeId"] = itemTypeId;
                }

                return (int)ViewState["ItemTypeId"];
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        private string FindByName
        {
            get
            {
                if (ViewState["FindByName"] == null)
                {
                    ViewState["FindByName"] = string.Empty;
                }

                return ViewState["FindByName"].ToString();
            }
            set
            {
                ViewState["FindByName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the editing item brief last updated date.
        /// </summary>
        /// <value>
        /// The editing item brief last updated date.
        /// </value>
        private DateTime EditingItemBriefLastUpdatedDate
        {
            get
            {
                return (DateTime)ViewState["EditingItemBriefLastUpdatedDate"];
            }
            set
            {
                ViewState["EditingItemBriefLastUpdatedDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        private int SortOrder
        {
            get
            {
                int order = 0;
                if (ViewState["SortOrder"] != null)
                {
                    int.TryParse(ViewState["SortOrder"].ToString(), out order);
                }

                return order;
            }

            set
            {
                ViewState["SortOrder"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort by.
        /// </summary>
        /// <value>
        /// The sort by.
        /// </value>
        private string SortBy
        {
            get
            {
                string sortBy = string.Empty;
                if (ViewState["SortBy"] != null)
                {
                    sortBy = ViewState["SortBy"].ToString();
                }

                return sortBy;
            }

            set
            {
                ViewState["SortBy"] = value;
            }
        }

        #endregion Properties

        #region Events Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">
        /// Item type not found
        /// or
        /// Project not found
        /// or
        /// Permission denied for this project
        /// </exception>
        /// <exception cref="System.Exception">Item Type not found</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            projectItemTypes.ProjectID = ProjectId;
            if (!IsPostBack)
            {
                LoadItemBriefSorting();
                StageBitz.Data.Project project = GetBL<ProjectBL>().GetProject(ProjectId);

                #region concurrentScenarios

                projectWarningPopup.ProjectId = ProjectId;

                if (isItemRmoved())
                {
                    throw new ApplicationException("Item type not found");
                }

                #endregion concurrentScenarios

                #region Check access security

                if (project == null)
                {
                    throw new ApplicationException("Project not found");
                }

                if (!Support.CanAccessProject(project))
                {
                    if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                    {
                        StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                    }

                    throw new ApplicationException("Permission denied for this project ");
                }

                //Check the validity of the ItemType
                if (Utils.GetItemTypeById(ItemTypeId) == null)
                {
                    throw new Exception("Item Type not found");
                }

                #endregion Check access security

                this.CompanyId = project.CompanyId;
                warningDisplay.ProjectID = ProjectId;
                warningDisplay.LoadData();

                //Set the page read only status if the user is project "Observer"
                IsReadOnly = Support.IsReadOnlyRightsForProject(ProjectId);
                pnlAddItem.Visible = !IsReadOnly;

                displaySettings.Module = ListViewDisplaySettings.ViewSettingModule.ProjectItemBriefList;
                displaySettings.LoadControl();

                txtName.Focus();

                #region SET LINKS

                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}",
                    project.CompanyId, (int)BookingTypes.Project, ProjectId, ItemTypeId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                hyperLinkTaskManager.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}&ItemTypeId={1}", ProjectId, ItemTypeId));
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}&ItemTypeId={1}", ProjectId, ItemTypeId),
                    string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}&ItemTypeId={1}", ProjectId, ItemTypeId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();

                #endregion SET LINKS

                var itemType = Utils.GetItemTypeById(ItemTypeId);
                string searchDefaultText = string.Concat("Find ", itemType.Name, " Briefs...");
                wmeFindName.WatermarkText = searchDefaultText;
                DisplayTitle = string.Format("{0} Brief List", string.Concat(Utils.GetItemTypeById(ItemTypeId).Name));
                LoadBreadCrumbs(project);
                ImportbulkItemsControl.ProjectID = ProjectId;
                ImportbulkItemsControl.ItemTypeId = ItemTypeId;
                ImportbulkItemsControl.IsReadOnly = IsReadOnly;

                //Security - Hide editing deleting for Observers
                if (IsReadOnly)
                {
                    foreach (GridColumn col in gvItemList.MasterTableView.RenderColumns)
                    {
                        if (col.UniqueName == "EditCommandColumn")
                        {
                            col.Visible = false;
                        }
                    }
                }
            }

            projectItemTypes.InformParentToReload += delegate()
            {
                //Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri);
                ReloadByItemType();
            };

            ImportbulkItemsControl.InformItemListToLoad += delegate()//Subscribe to the Informparent to get updated
            {
                displaySettings.LoadControl();
            };
        }

        #region Grid View events

        /// <summary>
        /// Handles the SortCommand event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvItemList_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvItemList.MasterTableView.SortExpressions.Clear();
                gvItemList.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvItemList.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvItemList_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
            {
                gvItemList.DataSource = GetItemBriefList();
            }
        }

        /// <summary>
        /// Handles the UpdateCommand event of the gvItemList control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvItemList_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (Page.IsValid)
                {
                    //Get the GridEditableItem of the RadGrid
                    GridEditableItem editedItem = e.Item as GridEditableItem;
                    int itemBriefId = (int)editedItem.GetDataKeyValue("ItemBrief.ItemBriefId");// (int)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["ItemBrief.ItemBriefId"];
                    DateTime localLastUpdatedDate = (DateTime)editedItem.GetDataKeyValue("ItemBrief.LastUpdatedDate");

                    Data.ItemBrief itemBrief = GetBL<ItemBriefBL>().GetItemBrief(itemBriefId, localLastUpdatedDate);

                    if (itemBrief == null)
                    {
                        StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ItemBriefList, ProjectId));
                    }

                    TextBox tbItemBriefName = (TextBox)editedItem.FindControl("tbItemBriefName");

                    if (!IsValidtoSave(tbItemBriefName.Text, itemBriefId, false))
                    {
                        Label lblErrorMsgForDuplicateItemBriefs = (Label)editedItem.FindControl("lblErrorMsgForDuplicateItemBriefs");
                        lblErrorMsgForDuplicateItemBriefs.Visible = true;
                        lblErrorMsgForDuplicateItemBriefs.ToolTip = string.Format("'{0}' already exists in this Project.", tbItemBriefName.Text.Trim());

                        e.Canceled = true;
                        return;
                    }

                    TextBox tbDescription = (TextBox)editedItem.FindControl("tbDescription");
                    TextBox tbCategory = (TextBox)editedItem.FindControl("tbCategory");
                    TextBox tbAct = (TextBox)editedItem.FindControl("tbAct");
                    TextBox tbScene = (TextBox)editedItem.FindControl("tbScene");
                    TextBox tbPage = (TextBox)editedItem.FindControl("tbPage");
                    TextBox tbCharacter = (TextBox)editedItem.FindControl("tbCharacter");
                    TextBox tbPreset = (TextBox)editedItem.FindControl("tbPreset");
                    TextBox tbRehearsal = (TextBox)editedItem.FindControl("tbRehearsal");

                    RadNumericTextBox tbItemQuantity = (RadNumericTextBox)editedItem.FindControl("tbItemQuantity");

                    if (itemBrief != null)
                    {
                        StageBitz.Data.ItemBrief tmpItemBrief = new Data.ItemBrief();

                        #region Assign input values to the temporary item brief object

                        tmpItemBrief.Name = tbItemBriefName.Text.Trim();

                        if (tbItemQuantity.Value.HasValue)
                        {
                            tmpItemBrief.Quantity = (int)tbItemQuantity.Value;
                        }
                        else
                        {
                            tmpItemBrief.Quantity = null;
                        }

                        tmpItemBrief.Description = tbDescription.Text.Trim();
                        tmpItemBrief.Act = tbAct.Text.Trim();
                        tmpItemBrief.Scene = tbScene.Text.Trim();
                        tmpItemBrief.Page = tbPage.Text.Trim();
                        tmpItemBrief.Category = tbCategory.Text.Trim();
                        tmpItemBrief.Character = tbCharacter.Text.Trim();
                        tmpItemBrief.Preset = tbPreset.Text.Trim();
                        tmpItemBrief.RehearsalItem = tbRehearsal.Text.Trim();

                        //Added since the Grid does not contains these fields.
                        tmpItemBrief.Budget = itemBrief.Budget;
                        tmpItemBrief.DueDate = itemBrief.DueDate;

                        #endregion Assign input values to the temporary item brief object

                        //Compare and generate the edited field list.
                        string editedFieldList = this.GetBL<ItemBriefBL>().GenerateEditedFieldListForItemBrief(itemBrief, tmpItemBrief);

                        #region Generate 'Edit' Notification

                        if (editedFieldList != string.Empty)
                        {
                            Notification nf = new Notification();
                            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
                            nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "EDIT");
                            nf.RelatedId = itemBrief.ItemBriefId;
                            nf.ProjectId = ProjectId;
                            nf.Message = string.Format("{0} edited the Item Brief - {1}.", Support.UserFullName, editedFieldList);
                            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                            nf.CreatedDate = nf.LastUpdatedDate = Now;

                            DataContext.Notifications.AddObject(nf);
                        }

                        #endregion Generate 'Edit' Notification

                        #region Update the existing ItemBrief

                        itemBrief.Name = tbItemBriefName.Text.Trim();
                        itemBrief.Description = tbDescription.Text.Trim();
                        itemBrief.Category = tbCategory.Text.Trim();
                        itemBrief.Act = tbAct.Text.Trim();
                        itemBrief.Scene = tbScene.Text.Trim();
                        itemBrief.Page = tbPage.Text.Trim();
                        itemBrief.Character = tbCharacter.Text.Trim();
                        itemBrief.Preset = tbPreset.Text.Trim();
                        itemBrief.RehearsalItem = tbRehearsal.Text.Trim();

                        if (tbItemQuantity.Value != null)
                            itemBrief.Quantity = (int)tbItemQuantity.Value;
                        else
                            itemBrief.Quantity = null;

                        itemBrief.LastUpdatedDate = Now;
                        itemBrief.LastUpdatedByUserId = UserID;

                        #endregion Update the existing ItemBrief

                        GetBL<ItemBriefBL>().SaveChanges();
                    }

                    gvItemList.EditIndexes.Clear();
                    gvItemList.MasterTableView.IsItemInserted = false;
                    gvItemList.Rebind();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvItemList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvItemList_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                Data.ItemBrief itemBrief = ((dynamic)dataItem.DataItem).ItemBrief;

                TextBox tbItemBriefName = (TextBox)dataItem.FindControl("tbItemBriefName");
                tbItemBriefName.Text = itemBrief.Name;

                tbItemBriefName.Attributes.Add("OnKeyUp", "ClearItemBriefDuplicateErrorMessages('" + e.Item.ItemIndex + "');");

                TextBox tbDescription = (TextBox)dataItem.FindControl("tbDescription");
                tbDescription.Text = itemBrief.Description;
                TextBox tbCategory = (TextBox)dataItem.FindControl("tbCategory");
                tbCategory.Text = itemBrief.Category;
                TextBox tbAct = (TextBox)dataItem.FindControl("tbAct");
                tbAct.Text = itemBrief.Act;
                TextBox tbScene = (TextBox)dataItem.FindControl("tbScene");
                tbScene.Text = itemBrief.Scene;
                TextBox tbPage = (TextBox)dataItem.FindControl("tbPage");
                tbPage.Text = itemBrief.Page;
                RadNumericTextBox tbItemQuantity = (RadNumericTextBox)dataItem.FindControl("tbItemQuantity");
                tbItemQuantity.Value = itemBrief.Quantity;
                (e.Item as GridEditableItem)["Status"].Enabled = false;
            }
            else if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                Data.ItemBrief itemBrief = ((dynamic)dataItem.DataItem).ItemBrief;
                HtmlAnchor lnkItemBriefDetails = (HtmlAnchor)e.Item.FindControl("lnkItemBriefDetails");

                //Item link
                lnkItemBriefDetails.InnerText = Support.TruncateString(itemBrief.Name, 35);

                string sorting = GetSortParam();
                lnkItemBriefDetails.HRef = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&sort={1}",
                        itemBrief.ItemBriefId, sorting));

                tooltipManager.TargetControls.Add(lnkItemBriefDetails.ClientID, itemData.ThumbnailMediaId.ToString(), true);

                //Description
                dataItem["Description"].Text = Support.TruncateString(itemBrief.Description, 30);

                if (itemBrief.Description != null && itemBrief.Description.Length > 30)
                {
                    dataItem["Description"].ToolTip = itemBrief.Description;
                }

                //Category
                dataItem["Category"].Text = Support.TruncateString(itemBrief.Category, 15);

                if (itemBrief.Category != null && itemBrief.Category.Length > 15)
                {
                    dataItem["Category"].ToolTip = itemBrief.Category;
                }

                dataItem["Act"].Text = Support.TruncateString(itemBrief.Act, 4);
                if (itemBrief.Act != null && itemBrief.Act.Length > 4)
                {
                    dataItem["Act"].ToolTip = itemBrief.Act;
                }

                dataItem["Scene"].Text = Support.TruncateString(itemBrief.Scene, 4);
                if (itemBrief.Scene != null && itemBrief.Scene.Length > 4)
                {
                    dataItem["Scene"].ToolTip = itemBrief.Scene;
                }

                dataItem["Page"].Text = Support.TruncateString(itemBrief.Page, 4);
                if (itemBrief.Page != null && itemBrief.Page.Length > 4)
                {
                    dataItem["Page"].ToolTip = itemBrief.Page;
                }

                dataItem["Quantity"].Text = Support.TruncateString(itemBrief.Quantity.ToString(), 4);
                if (itemBrief.Quantity != null && itemBrief.Quantity.ToString().Length > 4)
                {
                    dataItem["Quantity"].ToolTip = itemBrief.Quantity.ToString();
                }

                HtmlImage imgNoEstimatedCost = (HtmlImage)dataItem.FindControl("imgNoEstimatedCost");
                imgNoEstimatedCost.Visible = this.GetBL<ItemBriefBL>().HasEmptyEstimateCostInItemBrief(itemBrief.ItemBriefId);

                dataItem["Character"].Text = Support.TruncateString(itemBrief.Character, 7);
                if (itemBrief.Character != null && itemBrief.Character.Length > 7)
                {
                    dataItem["Character"].ToolTip = itemBrief.Character;
                }

                dataItem["Preset"].Text = Support.TruncateString(itemBrief.Preset, 6);
                if (itemBrief.Preset != null && itemBrief.Preset.Length > 6)
                {
                    dataItem["Preset"].ToolTip = itemBrief.Preset;
                }

                dataItem["Rehearsal"].Text = Support.TruncateString(itemBrief.RehearsalItem, 7);
                if (itemBrief.RehearsalItem != null && itemBrief.RehearsalItem.Length > 7)
                {
                    dataItem["Rehearsal"].ToolTip = itemBrief.RehearsalItem;
                }
            }
        }

        /// <summary>
        /// Gets the sort parameter.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetSortParam()
        {
            string param = string.Concat(this.SortBy, "|",
                    this.SortOrder.ToString(CultureInfo.InvariantCulture), "|",
                    Support.EncodeToBase64String(this.FindByName));
            return Server.UrlEncode(param);
        }

        #endregion Grid View events

        /// <summary>
        /// Handles the ItemDataBound event of the lvItemThumbList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvItemThumbList_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            dynamic itemBrief = ((dynamic)e.Item.DataItem).ItemBrief;
            int thumbnailMediaId = ((dynamic)e.Item.DataItem).ThumbnailMediaId;

            HyperLink lnkItemBrief = (HyperLink)e.Item.FindControl("lnkItemBrief");
            ImageDisplay itemBriefThumbDisplay = (ImageDisplay)e.Item.FindControl("itemBriefThumbDisplay");

            lnkItemBrief.ToolTip = itemBrief.Name;

            string sorting = GetSortParam();
            lnkItemBrief.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&sort={1}",
                    itemBrief.ItemBriefId, sorting));
            itemBriefThumbDisplay.DocumentMediaId = thumbnailMediaId;
        }

        /// <summary>
        /// Handles the DisplayModeChanged event of the displaySettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void displaySettings_DisplayModeChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (isItemRmoved())
                {
                    popupItemBriefRemoved.ShowPopup();
                }
                else
                {
                    txtFindName.Text = string.Empty;
                    FindByName = string.Empty;
                    upnlFindItems.Update();

                    if (this.IsValid && !this.StopProcessing)
                    {
                        string itemName = txtName.Text.Trim();

                        Data.ItemBrief itemBrief = new Data.ItemBrief();

                        #region Fill data

                        itemBrief.ProjectId = ProjectId;
                        itemBrief.Name = itemName;

                        itemBrief.Description = txtDescription.Text.Trim();
                        itemBrief.Category = txtCategory.Text.Trim();
                        itemBrief.Act = txtAct.Text.Trim();
                        itemBrief.Scene = txtScene.Text.Trim();
                        itemBrief.Page = txtPage.Text.Trim();
                        itemBrief.Preset = txtPreset.Text.Trim();
                        itemBrief.Character = txtCharacter.Text.Trim();
                        itemBrief.RehearsalItem = txtRehearsal.Text.Trim();

                        if (txtQuantity.Value.HasValue)
                        {
                            itemBrief.Quantity = (int)txtQuantity.Value.Value;
                        }
                        else
                        {
                            itemBrief.Quantity = null;
                        }

                        itemBrief.Budget = 0;

                        itemBrief.ItemBriefStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

                        itemBrief.CreatedByUserId = itemBrief.LastUpdatedByUserId = UserID;
                        itemBrief.CreatedDate = itemBrief.LastUpdatedDate = Now;

                        #endregion Fill data

                        #region ItemBrief Types

                        Data.ItemBriefType itemBriefType = new Data.ItemBriefType();
                        itemBriefType.ItemTypeId = ItemTypeId;
                        itemBriefType.ItemBriefTypeCodeId = Support.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");
                        itemBriefType.CreatedByUserId = itemBriefType.LastUpdatedByUserId = UserID;
                        itemBriefType.CreatedDate = itemBriefType.LastUpdatedDate = Now;
                        itemBriefType.IsActive = true;
                        itemBrief.ItemBriefTypes.Add(itemBriefType);

                        #endregion ItemBrief Types

                        GetBL<ItemBriefBL>().AddItemBrief(itemBrief);

                        #region Generate 'Add' Notification

                        Notification nf = new Notification();
                        nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
                        nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "ADD");
                        nf.RelatedId = itemBrief.ItemBriefId;
                        nf.ProjectId = ProjectId;
                        nf.Message = string.Format("{0} added an Item Brief.", Support.UserFullName);
                        nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
                        nf.CreatedDate = nf.LastUpdatedDate = Now;

                        GetBL<NotificationBL>().AddNotification(nf);

                        #endregion Generate 'Add' Notification

                        LoadData();

                        txtName.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnItemAlreadyRemoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnItemAlreadyRemoved_Click(object sender, EventArgs e)
        {
            popupItemBriefRemoved.HidePopup();
            Response.Redirect(String.Format("~/Project/ProjectDashboard.aspx?projectid={0}", ProjectId));
        }

        /// <summary>
        /// Handles the ServerValidate event of the cusvalName control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cusvalName_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            #region Check for duplicate item name (case-insensitive)

            if (IsValidtoSave(args.Value, 0, true))
            {
                args.IsValid = true;
            }
            else
            {
                cusvalName.ErrorMessage = string.Format("'{0}' already exists in this Project.", txtName.Text.Trim());
                args.IsValid = false;
            }

            #endregion Check for duplicate item name (case-insensitive)
        }

        /// <summary>
        /// Handles the Click event of the btnFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFind_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                FindByName = txtFindName.Text.Trim().ToLower();
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the ibtnClearSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ibtnClearSearch_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                txtFindName.Text = string.Empty;
                FindByName = string.Empty;
                LoadData();
            }
        }

        /// <summary>
        /// Shows the thumbnail image of the item as a tooltip.
        /// </summary>
        protected void tooltipManager_AjaxUpdate(object sender, ToolTipUpdateEventArgs e)
        {
            int thumbnailMediaId = 0;
            int.TryParse(e.Value, out thumbnailMediaId);

            using (ImageDisplay image = new ImageDisplay())
            {
                image.DocumentMediaId = thumbnailMediaId;
                image.ShowImagePreview = false;

                e.UpdatePanel.ContentTemplateContainer.Controls.Add(image);
            }
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_ExcelExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (isItemRmoved())
                {
                    popupItemBriefRemoved.ShowPopup();
                }
                else
                {
                    ExportReport(ReportTypes.Excel);
                }
            }
        }

        /// <summary>
        /// Handles the PDFExportClick event of the exportData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportData_PDFExportClick(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (isItemRmoved())
                {
                    popupItemBriefRemoved.ShowPopup();
                }
                else
                {
                    ExportReport(ReportTypes.Pdf);
                }
            }
        }

        #endregion Events Handlers

        #region Private Methods

        /// <summary>
        /// Reloads the type of the by item.
        /// </summary>
        private void ReloadByItemType()
        {
            var nameValues = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            nameValues.Set("ItemTypeId", projectItemTypes.SelectedItemTypeId.ToString());

            string url = Request.Url.AbsolutePath;
            string updatedQueryString = "?" + nameValues.ToString();
            Response.Redirect(url + updatedQueryString);
        }

        /// <summary>
        /// Determines whether [is item rmoved].
        /// </summary>
        /// <returns></returns>
        private bool isItemRmoved()
        {
            var itemToRemove = (from pit in DataContext.ProjectItemTypes
                                where pit.ProjectId == ProjectId && pit.ItemTypeId == ItemTypeId
                                select pit).FirstOrDefault();
            if (itemToRemove == null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads the item brief sorting.
        /// </summary>
        private void LoadItemBriefSorting()
        {
            if (this.Request.QueryString["Sort"] != null && !string.IsNullOrEmpty(this.Request.QueryString["Sort"]))
            {
                string sortParam = this.Request.QueryString["Sort"];
                string[] sortParamArray = sortParam.Split('|');
                if (sortParamArray.Length == 3)
                {
                    if (sortParamArray[0] == "StatusSortOrder")
                    {
                        this.SortBy = sortParamArray[0];
                        int sortOrder = int.Parse(sortParamArray[1]);
                        this.SortOrder = sortOrder <= 2 && sortOrder >= 0 ? sortOrder : 1;
                        this.FindByName = Support.DecodeBase64String(sortParamArray[2]);

                        gvItemList.MasterTableView.SortExpressions[0].FieldName = this.SortBy;
                        gvItemList.MasterTableView.SortExpressions[0].SortOrder = (GridSortOrder)this.SortOrder;
                        txtFindName.Text = this.FindByName;
                    }
                    else
                    {
                        string[] sortByPropertyInfo = sortParamArray[0].Split('.');
                        string sortByProperty = sortByPropertyInfo.Length == 2 ? sortByPropertyInfo[1] : string.Empty;
                        if (!string.IsNullOrEmpty(sortByProperty))
                        {
                            Type itemBriefType = typeof(Data.ItemBrief);
                            PropertyInfo[] properties = itemBriefType.GetProperties();
                            foreach (PropertyInfo property in properties)
                            {
                                if (property.Name == sortByProperty)
                                {
                                    this.SortBy = sortParamArray[0];
                                    int sortOrder = int.Parse(sortParamArray[1]);
                                    this.SortOrder = sortOrder <= 2 && sortOrder > 0 ? sortOrder : 1;
                                    this.FindByName = Support.DecodeBase64String(sortParamArray[2]);

                                    gvItemList.MasterTableView.SortExpressions[0].FieldName = this.SortBy;
                                    gvItemList.MasterTableView.SortExpressions[0].SortOrder = (GridSortOrder)this.SortOrder;
                                    txtFindName.Text = this.FindByName;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [is validto save] [the specified item brief name].
        /// </summary>
        /// <param name="itemBriefName">Name of the item brief.</param>
        /// <param name="itemBriefID">The item brief identifier.</param>
        /// <param name="isAddNew">if set to <c>true</c> [is add new].</param>
        /// <returns></returns>
        private bool IsValidtoSave(string itemBriefName, int itemBriefID, bool isAddNew)
        {
            var existingItemNameList = (from ib in DataContext.ItemBriefs
                                        where ib.Name.Equals(itemBriefName.Trim(), StringComparison.InvariantCultureIgnoreCase) && ib.ProjectId == ProjectId
                                        select ib).ToList<Data.ItemBrief>();
            bool status = false;

            if (isAddNew)
            {
                status = existingItemNameList.Count() == 0 ? true : false;
            }
            else if ((existingItemNameList.Count() == 1 && existingItemNameList[0].ItemBriefId == itemBriefID) || existingItemNameList.Count() == 0)
            {
                status = true;
            }
            return status;
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="Project">The project.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Project Project)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            string companyUrl = Support.IsCompanyAdministrator(Project.Company.CompanyId) ?
                string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", Project.Company.CompanyId) : null;
            bc.AddLink(Project.Company.CompanyName, companyUrl);
            bc.AddLink(Project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", Project.ProjectId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            ClearInputFields();
            tooltipManager.TargetControls.Clear();

            List<dynamic> itemBriefs = GetItemBriefList();
            int itemCount = itemBriefs.Count();
            var itemType = DataContext.ItemTypes.Where(it => it.ItemTypeId == ItemTypeId).FirstOrDefault();

            ltrlItemListTitle.Text = string.Format("{0} {1} Briefs found", itemCount, itemType.Name);

            if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
            {
                gvItemList.DataSource = itemBriefs;
                gvItemList.DataBind();
                gvItemList.Visible = true;
                divThumbList.Visible = false;
            }
            else
            {
                lvItemThumbList.DataSource = itemBriefs.OrderBy(ib => ib.ItemBrief.Name);
                lvItemThumbList.DataBind();
                divThumbList.Visible = true;
                gvItemList.Visible = false;
            }

            upnlItemList.Update();
        }

        /// <summary>
        /// Gets the item brief list.
        /// </summary>
        /// <returns></returns>
        private List<dynamic> GetItemBriefList()
        {
            InitializeSortingSettings();

            string findName = FindByName;
            if (findName.Length == 0)
                findName = null;

            var items = GetBL<ItemBriefBL>().GetItemBriefList(findName, ProjectId, ItemTypeId);

            return items.ToList<dynamic>();
        }

        /// <summary>
        /// Initializes the sorting settings.
        /// </summary>
        private void InitializeSortingSettings()
        {
            if (gvItemList.MasterTableView.SortExpressions.Count != 0)
            {
                string name = gvItemList.MasterTableView.SortExpressions[0].FieldName;
                GridSortOrder order = gvItemList.MasterTableView.SortExpressions[0].SortOrder;
                this.SortBy = name;
                this.SortOrder = (int)order;
            }
        }

        /// <summary>
        /// Clears out all user inputs.
        /// </summary>
        private void ClearInputFields()
        {
            txtName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtCategory.Text = string.Empty;
            txtAct.Text = string.Empty;
            txtScene.Text = string.Empty;
            txtPage.Text = string.Empty;
            txtQuantity.Value = null;
            txtCharacter.Text = string.Empty;
            txtRehearsal.Text = string.Empty;
            txtPreset.Text = string.Empty;
        }

        #endregion Private Methods

        #region Report export

        /// <summary>
        /// Exports the report.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        private void ExportReport(ReportTypes exportType)
        {
            Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectId);
            if (project != null)
            {
                string fileTailName = string.Empty;
                int itemTypeId = projectItemTypes.SelectedItemTypeId;

                string sortExpression = "ItemBrief.Name ASC";
                if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ListView)
                {
                    sortExpression = gvItemList.MasterTableView.SortExpressions.GetSortString();
                }

                if (itemTypeId != 0)
                {
                    fileTailName = string.Format("_{0}BriefList", Utils.GetItemTypeById(itemTypeId).Name);
                }
                else
                {
                    fileTailName = "_ItemBriefList";
                }

                string fileName = project.ProjectName + fileTailName;

                string fileNameExtension;
                string encoding;
                string mimeType;

                ItemBriefListReportParameters parameters = new ItemBriefListReportParameters
                {
                    ItemTypeId = itemTypeId,
                    ProjectId = ProjectId,
                    SortExpression = sortExpression,
                    UserId = this.UserID
                };

                byte[] reportBytes = UserWebReportHandler.GenerateItemBriefListReport(parameters, exportType,
                        out fileNameExtension, out encoding, out mimeType, true);
                Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
            }
        }

        #endregion Report export
    }
}