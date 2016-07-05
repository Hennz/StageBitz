using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Business.Utility;
using StageBitz.Reports.UserWeb.Helper;
using StageBitz.Reports.UserWeb.Parameters;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.ItemBrief
{
    /// <summary>
    /// Web page for item brief details.
    /// </summary>
    public partial class ItemBriefDetails : PageBase
    {
        /// <summary>
        /// The item brief type primary code identifier
        /// </summary>
        private int ItemBriefTypePrimaryCodeId = Support.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");

        #region Properties

        /// <summary>
        /// Gets the item brief identifier.
        /// </summary>
        /// <value>
        /// The item brief identifier.
        /// </value>
        public int ItemBriefId
        {
            get
            {
                if (ViewState["ItemBriefId"] == null)
                {
                    int itemBriefId = 0;

                    if (Request["ItemBriefId"] != null)
                    {
                        int.TryParse(Request["ItemBriefId"], out itemBriefId);
                    }

                    ViewState["ItemBriefId"] = itemBriefId;
                }

                return (int)ViewState["ItemBriefId"];
            }
        }

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
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            private set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        public string Currency
        {
            get
            {
                return (string)ViewState["Currency"];
            }
            private set
            {
                ViewState["Currency"] = value;
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
        /// Gets or sets a value indicating whether this page is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this page is read only; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether this instance is complete item tab dirty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is complete item tab dirty; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleteItemTabDirty
        {
            get
            {
                return completedItem.IsCompleteItemTabDirty;
            }
            set
            {
                completedItem.IsCompleteItemTabDirty = value;
            }
        }

        /// <summary>
        /// Gets the initial selected tab.
        /// </summary>
        /// <value>
        /// The initial selected tab.
        /// </value>
        private string InitialSelectedTab
        {
            get
            {
                if (Request["selectedTab"] != null)
                {
                    return Request["selectedTab"];
                }
                else
                {
                    return string.Empty;
                }
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
                string sortBy = string.Empty;
                if (ViewState["FindByName"] != null)
                {
                    sortBy = ViewState["FindByName"].ToString();
                }

                return sortBy;
            }

            set
            {
                ViewState["FindByName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort parameter.
        /// </summary>
        /// <value>
        /// The sort parameter.
        /// </value>
        private string SortParam
        {
            get
            {
                string sortParam = string.Empty;
                if (ViewState["SortParam"] != null)
                {
                    sortParam = ViewState["SortParam"].ToString();
                }

                return sortParam;
            }

            set
            {
                ViewState["SortParam"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the next item brief.
        /// </summary>
        /// <value>
        /// The next item brief.
        /// </value>
        private int NextItemBriefId
        {
            get
            {
                int itemBreif = 0;
                if (ViewState["NextItemBriefId"] != null)
                {
                    int.TryParse(ViewState["NextItemBriefId"].ToString(), out itemBreif);
                }

                return itemBreif;
            }

            set
            {
                ViewState["NextItemBriefId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the previous item brief.
        /// </summary>
        /// <value>
        /// The previous item brief.
        /// </value>
        private int PreviousItemBriefId
        {
            get
            {
                int itemBreif = 0;
                if (ViewState["PreviousItemBriefId"] != null)
                {
                    int.TryParse(ViewState["PreviousItemBriefId"].ToString(), out itemBreif);
                }

                return itemBreif;
            }

            set
            {
                ViewState["PreviousItemBriefId"] = value;
            }
        }

        /// <summary>
        /// Gets the item type id.
        /// </summary>
        /// <value>
        /// The item type id.
        /// </value>
        private int ItemTypeId
        {
            get
            {
                if (this.ViewState["ItemTypeId"] == null)
                {
                    if (this.ItemBriefId > 0)
                    {
                        var itemType = (from ibt in DataContext.ItemBriefTypes
                                        join pit in DataContext.ProjectItemTypes on ibt.ItemTypeId equals pit.ItemTypeId
                                        where ibt.ItemBriefId == this.ItemBriefId
                                            && ibt.ItemBriefTypeCodeId == ItemBriefTypePrimaryCodeId
                                            && pit.ProjectId == this.ProjectId
                                        select ibt.ItemType).FirstOrDefault();

                        this.ViewState["ItemTypeId"] = itemType != null ? itemType.ItemTypeId : 0;
                    }
                    else
                    {
                        this.ViewState["ItemTypeId"] = 0;
                    }
                }

                return (int)this.ViewState["ItemTypeId"];
            }

            set
            {
                this.ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets the last updated date of the original record which was used to load data on the page.
        /// Used for concurrency handling.
        /// </summary>
        private DateTime OriginalLastUpdatedDate
        {
            get
            {
                return (DateTime)ViewState["LastUpdatedDate"];
            }
            set
            {
                ViewState["LastUpdatedDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user has exclusive rights for project.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user has exclusive rights for project; otherwise, <c>false</c>.
        /// </value>
        private bool HasExclusiveRightsForProject
        {
            get
            {
                if (ViewState["HasExclusiveRightsForProject"] == null)
                {
                    ViewState["HasExclusiveRightsForProject"] = Support.HasExclusiveRightsForProject(this.ProjectId);
                }

                return (bool)ViewState["HasExclusiveRightsForProject"];
            }
            set
            {
                ViewState["HasExclusiveRightsForProject"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user can see budget summary.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user can see budget summary; otherwise, <c>false</c>.
        /// </value>
        private bool CanSeeBudgetSummary
        {
            get
            {
                if (ViewState["CanSeeBudgetSummary"] == null)
                {
                    ViewState["CanSeeBudgetSummary"] = Support.CanSeeBudgetSummary(this.UserID, this.ProjectId);
                }

                return (bool)ViewState["CanSeeBudgetSummary"];
            }
            set
            {
                ViewState["CanSeeBudgetSummary"] = value;
            }
        }

        #endregion Properties

        #region Events Hanlders

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            pinnedItems.UpdatePinnedItemsCount += delegate(int count)
            {
                UpdateTabHeadingsCount(3, count);
            };

            if (!IsPostBack)
            {
                LoadSorting();

                if (!LoadData())
                {
                    return;
                }

                projectWarningPopup.ProjectId = ProjectId;

                completedItem.ValidationGroup = btnSave.ValidationGroup;

                InitializeNavigation();
                if (this.CanSeeBudgetSummary)
                {
                    Loadbudget();
                }
                else
                {
                    divBudget.Visible = false;
                }

                documentPreview.IsReadOnly = documentPreview.IsReadOnly || IsReadOnly;

                ItemBriefTasks.ItemBriefID = ItemBriefId;
                ItemBriefTasks.ProjectId = ProjectId;

                completedItem.ItemBriefId = ItemBriefId;

                completedItem.IsItemBriefReadOnly = IsReadOnly;
                completedItem.ProjectId = ProjectId;

                reqName.ControlToValidate = itemBriefNameEdit.TextBox.ID;
                cusvalName.ControlToValidate = itemBriefNameEdit.TextBox.ID;

                pinnedItems.ItemBriefId = ItemBriefId;
                pinnedItems.ProjectId = ProjectId;
                pinnedItems.IsReadOnly = this.IsReadOnly;
                pinnedItems.LoadData();

                if (ItemTypeId != 0)
                    litSetImageInfo.Text = string.Format("Click an image to set it as {0} brief's preview image.", Utils.GetItemTypeById(ItemTypeId).Name);
                else
                    litSetImageInfo.Text = "Click an image to set it as preview image.";

                #region SET LINKS

                //Inside Load data, ProjectId is being set
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}&ItemTypeId={3}",
                    Support.GetCompanyByProjectId(ProjectId).CompanyId, (int)BookingTypes.Project, ProjectId, ItemTypeId);
                lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectId);
                hyperLinkTaskManager.NavigateUrl = ResolveUrl(string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectId));
                reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectId), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectId), ProjectId);
                projectUpdatesLink.ProjectID = ProjectId;
                projectUpdatesLink.LoadData();

                #endregion SET LINKS

                #region Set Tab

                string tabId = Request.QueryString["TabId"];
                if (tabId != null)
                {
                    int tabIndex = Convert.ToInt16(tabId);
                    itemBriefTabs.SelectedIndex = tabIndex;
                    itemBriefPages.SelectedIndex = tabIndex;
                }

                #endregion Set Tab
            }

            ItemBriefTasks.SetTasksGridWidth(880);

            ItemBriefTasks.UpdateTaskCount += delegate(int count)
            {
                UpdateTabHeadingsCount(1, count);
            };

            attachments.UpdateAttachmentsCount += delegate(int count)
            {
                UpdateTabHeadingsCount(2, count);
            };

            ItemBriefTasks.InformItemBriefDetail += delegate(int itembriefStatusCodeID, bool swtichToCompletedTab)//Subsucribe to the Informparent to get updated
            {
                DisplayItemBriefStatus(itembriefStatusCodeID);
                if (swtichToCompletedTab)
                {
                    SwtichToCompletedTab();
                }

                completedItem.LoadData();
            };

            ItemBriefTasks.InformItemBriefDetailToShowBudget += delegate()
            {
                Loadbudget();
            };

            attachments.InformParentToUpdateAboutAttachmentChanges += delegate()
            {
                DisplayItemThumbnail();
                // Update the Complete Item tab
                completedItem.LoadData();
                pinnedItems.LoadData();
            };

            attachments.InformParentToUpdateInItem += delegate()
            {
                // Update the Complete Item tab
                completedItem.LoadData();
                //Update Pinbloard tab
                pinnedItems.LoadData();
            };

            completedItem.InformItemBriefDetailToComplete += delegate(int itembriefStatusCodeID)
            {
                DisplayItemBriefStatus(itembriefStatusCodeID);
                //   attachments.DisplayMode = attachments.DisplayMode.ItemBrief;
                attachments.LoadData();
                pinnedItems.LoadData();
            };

            completedItem.InformItemBriefDetailToReloadAttachments += delegate()
            {
                // Load item brief attachment tab
                attachments.LoadData();
                DisplayItemThumbnail();
            };

            completedItem.InformItemBriefDetailToLoadPinTab += delegate()
            {
                // Load item brief attachment tab
                pinnedItems.LoadData();
            };

            pinnedItems.InformParentToUpdateCompleteItemTab += delegate()
            {
                completedItem.LoadData();
            };

            pinnedItems.InformItemBriefDetailToReloadCompleteItemTab += delegate()
            {
                completedItem.ResetProperties();
                // Update the Complete Item tab
                completedItem.LoadData();
            };

            //pinnedItems.InformItemBriefDetailToSaveCompleteItem += delegate()
            //{
            //    // Update the Complete Item tab
            //    //completedItem.SaveData();
            //    //DataContext.SaveChanges();
            //};

            pinnedItems.InformItemBriefDetailToReloadAttachmentsFromPinTab += delegate()
            {
                // Load item brief attachment tab
                attachments.LoadData();
            };

            pinnedItems.InformItemBriefDetailToGetCompleteItemDirtyStatus += delegate()
            {
                //Get the Complete Item status
                pinnedItems.IsCompleteItemTabDirty = IsCompleteItemTabDirty;
            };

            pinnedItems.InformItemBriefDetailToLoad += delegate(int itemBriefIdStatus)
            {
                DisplayItemBriefStatus(itemBriefIdStatus);
                DisplayItemThumbnail();
            };

            pinnedItems.InformParentToShowDelayedPopup += delegate(int itemId)
            {
                Data.Item item = GetBL<InventoryBL>().GetItem(itemId);
                if (item != null)
                {
                    Data.User locationManager = GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                    if (locationManager != null)
                    {
                        string inventoryManagerName = string.Concat(locationManager.FirstName, " ", locationManager.LastName);
                        lnkPopupPinnedDelayedContactIM.NavigateUrl = string.Concat("mailto:", locationManager.Email1);
                        lnkPopupPinnedDelayedContactIM.Text = inventoryManagerName;

                        popupPinnedDelayed.ShowPopup();
                    }
                }
            };

            if (InitialSelectedTab != string.Empty)
            {
                int tabIndex = itemBriefTabs.Tabs.IndexOf(itemBriefTabs.Tabs.FindTabByValue(InitialSelectedTab));
                if (tabIndex >= 0)
                {
                    itemBriefTabs.SelectedIndex = tabIndex;
                    itemBriefPages.SelectedIndex = tabIndex;
                }
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fileUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fileUpload_FileUploaded(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                DisplayItemThumbnail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmCompleteItemTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmCompleteItemTab_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                completedItem.IsCompleteItemTabDirty = false;// Reset the dirty flag
                completedItem.LoadData();
                popCompleteItemTabDirty.HidePopup();
                DisplayRemoveItemBriefPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmCompleteItemTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmCompleteItemTab_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                //completedItem.IsCompleteItemTabDirty = false;// Reset the dirty flag
                //popCompleteItemTabDirty.HidePopup();
                //completedItem.SaveData();
                //DataContext.SaveChanges();//To Commit the changes in Complete Item tab
                pinnedItems.LoadData();
                DisplayRemoveItemBriefPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCreatePDF control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreatePDF_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsPageDirty)
                {
                    popupConfirmCreatePDF.ShowPopup();
                }
                else
                {
                    ExportReport();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCreatePDFHiddenNoDirtyCheck control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreatePDFHiddenNoDirtyCheck_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                ExportReport();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmCreatePDF control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmCreatePDF_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                // SaveEnteredData();
                ExportReport();
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the cusvalName control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cusvalName_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            //Check for duplicate item name within the same project
            int existingItemCount = (from ib in DataContext.ItemBriefs
                                     where ib.Name.Equals(args.Value.Trim(), StringComparison.InvariantCultureIgnoreCase) && ib.ProjectId == ProjectId && ib.ItemBriefId != ItemBriefId
                                     select ib.ItemBriefId).Count();

            //args.IsValid = (existingItemCount == 0);
            if (existingItemCount == 0)
            {
                args.IsValid = true;
            }
            else
            {
                CustomValidator validator = source as CustomValidator;
                if (validator != null)
                {
                    validator.ErrorMessage = string.Format("'{0}' already exists in this Project.", args.Value.Trim());
                }

                args.IsValid = false;
                upnlCustomValidator.Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveConfirmComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveConfirmComplete_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                RemoveItemBrief();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemove_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (IsCompleteItemTabDirty)
                {
                    popCompleteItemTabDirty.ShowPopup();
                }
                else
                {
                    DisplayRemoveItemBriefPopup();
                }
            }
        }

        /// <summary>
        /// This will be fired when some attributes have been changed or image is deleted via the image preview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void documentPreview_DocumentChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                // Inform Attachment tab to Reload
                attachments.LoadData();

                DisplayItemThumbnail();

                // Inform Completed Item tab to be Reload
                completedItem.LoadData();

                // Load pinboard tab
                pinnedItems.LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkbtnChangePreviewImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkbtnChangePreviewImage_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                imagePickerDocumentList.LoadData();
                popupImagePicker.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the DocumentPicked event of the imagePickerDocumentList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Controls.Common.DocumentListDocumentPickedEventArgs"/> instance containing the event data.</param>
        protected void imagePickerDocumentList_DocumentPicked(object sender, Controls.Common.DocumentListDocumentPickedEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.DocumentMediaId > 0)
                {
                    DataContext.SetItemBriefDefaultImage(ItemBriefId, e.DocumentMediaId);
                }

                popupImagePicker.HidePopup();

                DisplayItemThumbnail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnHiddenUpdateTabs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHiddenUpdateTabs_Click(object sender, EventArgs e)
        {
            pinnedItems.LoadData();
            completedItem.LoadData();
        }

        /// <summary>
        /// Handles the Click event of the btnReloadConcurrencyBookingOverlap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReloadConcurrencyBookingOverlap_Click(object sender, EventArgs e)
        {
            string newQueryStrings = Utils.ReplaceQueryStringParam(Request.QueryString.ToString(), "TabId", "4");
            string url = Request.Url.AbsolutePath;
            string updatedQueryString = "?" + newQueryStrings;
            Response.Redirect(url + updatedQueryString);
        }

        #endregion Events Hanlders

        #region Methods

        /// <summary>
        /// Removes the item brief.
        /// </summary>
        private void RemoveItemBrief()
        {
            string itemBriefName = DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ItemBriefId).Select(ib => ib.Name).FirstOrDefault();

            //Remove the item and navigate to item list page.
            DataContext.DeleteItemBrief(ItemBriefId, UserID);

            #region Generate 'Delete' Notification

            Notification nf = new Notification();
            nf.ModuleTypeCodeId = Support.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
            nf.OperationTypeCodeId = Support.GetCodeIdByCodeValue("OperationType", "DELETE");
            nf.RelatedId = 0;
            nf.ProjectId = ProjectId;
            nf.Message = string.Format("{0} removed Item Brief - {1}.", Support.UserFullName, itemBriefName);
            nf.CreatedByUserId = nf.LastUpdatedByUserId = UserID;
            nf.CreatedDate = nf.LastUpdatedDate = Now;
            this.GetBL<NotificationBL>().AddNotification(nf);

            #endregion Generate 'Delete' Notification

            NavigateToProjectItemList();
        }

        /// <summary>
        /// Displays the remove item brief popup.
        /// </summary>
        private void DisplayRemoveItemBriefPopup()
        {
            int completeItemBriefStatusCodeId = Support.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            var itemBrief = DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ItemBriefId).FirstOrDefault();

            if (itemBrief == null)
            {
                return;
            }
            else
            {
                if (itemBrief.ItemBriefStatusCodeId != completeItemBriefStatusCodeId && !GetBL<ItemBriefBL>().IsItemBriefComplete(itemBrief))
                {
                    var itemBrieftype = GetBL<ItemBriefBL>().GetItemBriefType(ItemBriefId);
                    ltrItemType1.Text = itemBrieftype.ItemType.Name;
                    ltrItemType2.Text = itemBrieftype.ItemType.Name;
                    ltrItemType3.Text = itemBrieftype.ItemType.Name;
                    divNotCompleteItemBriefRemove.Visible = true;
                    divCompleteItemBriefRemove.Visible = false;
                    btnRemoveConfirmComplete.Visible = false;
                    btnRemoveCompleteItem.Visible = true;
                    btnRemoveConfirmNotComplete.Visible = true;
                    removeCancel.Value = "Cancel";
                }
                else
                {
                    divCompleteItemBriefRemove.Visible = true;
                    divNotCompleteItemBriefRemove.Visible = false;
                    btnRemoveConfirmComplete.Visible = true;
                    btnRemoveCompleteItem.Visible = false;
                    btnRemoveConfirmNotComplete.Visible = false;
                    removeCancel.Value = "No";
                }
                popupItemBriefRemoveConfirmation.ShowPopup();
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="itemBrief">The item brief.</param>
        /// <param name="itemType">Type of the item.</param>
        private void LoadBreadCrumbs(StageBitz.Data.ItemBrief itemBrief, ItemType itemType)
        {
            if (itemBrief != null && itemType != null)
            {
                BreadCrumbs bc = GetBreadCrumbsControl();
                string companyUrl = Support.IsCompanyAdministrator(itemBrief.Project.Company.CompanyId) ?
                        string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", itemBrief.Project.Company.CompanyId) : null;
                bc.AddLink(itemBrief.Project.Company.CompanyName, companyUrl);
                bc.AddLink(itemBrief.Project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?projectid={0}",
                        itemBrief.Project.ProjectId));
                bc.AddLink(string.Concat(itemType.Name, " List"),
                        string.Format("~/ItemBrief/ItemBriefList.aspx?projectid={0}&ItemTypeId={1}&Sort={2}",
                        itemBrief.Project.ProjectId, itemType.ItemTypeId, Server.UrlEncode(this.SortParam)));
                bc.AddLink(DisplayTitle, null);
                bc.LoadControl();
                //bc.UpdateBreadCrumb();
            }
        }

        /// <summary>
        /// Gets the irem brief from the DB for updating data, after checking for concurrency vialations.
        /// </summary>
        /// <returns></returns>
        private StageBitz.Data.ItemBrief GetItemBriefForUpdating()
        {
            StageBitz.Data.ItemBrief itemBrief = (from ib in DataContext.ItemBriefs
                                                  where ib.ItemBriefId == ItemBriefId && ib.LastUpdatedDate == OriginalLastUpdatedDate
                                                  select ib).FirstOrDefault();

            if (itemBrief == null)
            {
                StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ItemBriefDetails, ItemBriefId));
            }

            return itemBrief;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for item breif id  + ItemBriefId
        /// or
        /// Permission denied for Item Brief id  + ItemBriefId
        /// </exception>
        private bool LoadData()
        {
            StageBitz.Data.ItemBrief itemBrief = this.GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId);

            #region Validation

            if (itemBrief == null)
            {
                DisplayTitle = "Item Brief not found.";
                plcItemBriefNotAvailable.Visible = true;
                pnlItemBrief.Visible = false;
                plcHeaderLinks.Visible = false;
                plcNavigationButtons.Visible = false;
                return false;
            }

            if (!Support.CanAccessProject(itemBrief.Project))
            {
                if (GetBL<ProjectBL>().IsProjectClosed(itemBrief.Project.ProjectId))
                {
                    StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, ProjectId, "This Project Is Closed."));
                }

                throw new ApplicationException("Permission denied for item breif id " + ItemBriefId);
            }

            //Duplicate check

            #endregion Validation

            warningDisplay.ProjectID = itemBrief.ProjectId;
            warningDisplay.LoadData();

            ProjectId = itemBrief.ProjectId;
            this.CompanyId = itemBrief.Project.CompanyId;

            //Set the page read only status if the user is project "Observer"
            IsReadOnly = Support.IsReadOnlyRightsForProject(ProjectId);

            //Set last updated date for concurrency handling
            OriginalLastUpdatedDate = itemBrief.LastUpdatedDate.Value;

            DisplayItemBrief(itemBrief);
            LoadSource();
            attachments.Mode = ItemAttachments.DisplayMode.ItemBrief;
            attachments.RelatedId = ItemBriefId;
            attachments.IsReadOnly = IsReadOnly;
            attachments.ItemTypeId = this.ItemTypeId;
            attachments.ProjectId = ProjectId;
            attachments.LoadData();
            DisplayItemThumbnail();

            btnRemove.Enabled = !IsReadOnly;
            btnRemoveConfirmComplete.Enabled = !IsReadOnly;
            btnSave.Enabled = !IsReadOnly;

            if (this.ItemTypeId > 0)
            {
                LoadItemTypes();
                Data.ItemType itemType = this.GetBL<ItemBriefBL>().GetItemType(this.ItemTypeId);
                DisplayTitle = Support.TruncateString(itemType.Name, 50) + " Brief - " + Support.TruncateString(itemBrief.Name, 50);
                Page.Title = string.Concat("'", itemBrief.Name, "'");
                LoadBreadCrumbs(itemBrief, itemType);
            }
            else
            {
                throw new ApplicationException("Permission denied for Item Brief id " + ItemBriefId);
            }

            return true;
        }

        /// <summary>
        /// Loads the source.
        /// </summary>
        private void LoadSource()
        {
            // This code need to be replaced with dynamic filed.
            List<FieldOption> fieldOptions = (from fo in DataContext.FieldOptions
                                              join f in DataContext.Fields on fo.FieldId equals f.FieldId
                                              where f.DisplayName == "Source"
                                              select fo).ToList();

            ddlSource.DataSource = fieldOptions;
            ddlSource.DataValueField = "FieldOptionId";
            ddlSource.DataTextField = "OptionText";
            ddlSource.DataBind();

            ddlSource.Attributes.Add("data-field", fieldOptions[0].FieldId.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Loads the item types.
        /// </summary>
        private void LoadItemTypes()
        {
            //item type can be changed by the project admin and the company admin.
            divItemTypeSelect.Visible = HasExclusiveRightsForProject;
            divItemTypeStatic.Visible = !HasExclusiveRightsForProject;

            Data.ItemType itemType = this.GetBL<ItemBriefBL>().GetItemType(this.ItemTypeId);
            if (HasExclusiveRightsForProject)
            {
                this.ddItemTypes.Items.Clear();
                List<Data.ItemType> itemTypeList = this.GetBL<ItemBriefBL>().GetItemTypeList(this.ProjectId, itemType.ItemTypeId);
                this.ddItemTypes.Items.Add(new ListItem(itemType.Name, ItemTypeId.ToString(CultureInfo.InvariantCulture)));
                ddItemTypes.SelectedIndex = 0;
                if (itemTypeList.Count > 0)
                    this.ddItemTypes.AddItemGroup("Change to:");

                foreach (var it in itemTypeList.OrderBy(it => it.Name))
                {
                    this.ddItemTypes.Items.Add(new ListItem(it.Name, it.ItemTypeId.ToString(CultureInfo.InvariantCulture)));
                }
            }
            else if (itemType != null)
            {
                lblItemType.Text = itemType.Name;
            }
        }

        /// <summary>
        /// Loadbudgets this instance.
        /// </summary>
        private void Loadbudget()
        {
            List<ItemBriefTaskBudget> itemBriefTasksBudget = this.GetBL<ItemBriefBL>().GetItemBriefTasksBudget(ItemBriefId);
            int completeStatusCodeID = Support.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED").CodeId;
            int inprogressStatusCodeID = Support.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;

            Country country = Utils.GetCountryById(this.GetBL<ProjectBL>().GetProject(ProjectId).CountryId.Value);
            string cultureName = Support.GetCultureName(country.CountryCode);
            this.Currency = cultureName;

            txtBudget.Culture = new CultureInfo(cultureName);
            decimal? sumExpened = this.GetBL<ItemBriefBL>().GetSumExpend(ItemBriefId, completeStatusCodeID);
            divExpendedAmount.InnerHtml = Support.FormatCurrency(sumExpened == null ? 0 : sumExpened, cultureName);

            decimal? sumRemaining = this.GetBL<ItemBriefBL>().GetSumRemaining(ItemBriefId, inprogressStatusCodeID);
            litRemainingExpenses.Text = Support.FormatCurrency(sumRemaining == null ? 0 : sumRemaining, cultureName);
            imgNoEstimatedCost.Visible = this.GetBL<ItemBriefBL>().HasEmptyEstimateCostInItemBrief(ItemBriefId);

            decimal? balance = (decimal)txtBudget.Value.GetValueOrDefault() - ((sumExpened == null ? 0 : sumExpened) + (sumRemaining == null ? 0 : sumRemaining));
            divBalanceAmount.InnerHtml = Support.FormatCurrency(balance, cultureName);

            ScriptManager.RegisterStartupScript(this, GetType(), "RemainingExpensesScript", string.Format("var RemainingExpenses = {0};", (sumRemaining == null ? 0 : sumRemaining)), true);
            ScriptManager.RegisterStartupScript(this, GetType(), "ExpendedAmountScript", string.Format("var ExpendedAmount = {0};", (sumExpened == null ? 0 : sumExpened)), true);

            uppnlBudget.Update();
        }

        /// <summary>
        /// Displays the item brief status.
        /// </summary>
        /// <param name="itemBriefStatusCodeId">The item brief status code identifier.</param>
        private void DisplayItemBriefStatus(int itemBriefStatusCodeId)
        {
            StageBitz.Data.ItemBrief itemBrief = this.GetBL<ItemBriefBL>().GetItemBrief(ItemBriefId);
            //Set last updated date for concurrency handling
            OriginalLastUpdatedDate = itemBrief.LastUpdatedDate.Value;

            Code itemStatusCode = Support.GetCodeByCodeId(itemBriefStatusCodeId);
            lblItemStatus.Text = itemStatusCode.Description;
            ScriptManager.RegisterStartupScript(this, GetType(), "DisplayItemBriefStatus",
                string.Format("DisplayItemBriefStatus({0}, '{1}', '{2}');", itemStatusCode.CodeId, itemStatusCode.Value, itemStatusCode.Description), true);
        }

        /// <summary>
        /// Populates UI controls with item brief values from database
        /// </summary>
        private void DisplayItemBrief(StageBitz.Data.ItemBrief itemBrief)
        {
            #region Read Only

            itemBriefNameEdit.TextBox.ReadOnly = IsReadOnly;
            txtItemQuantity.ReadOnly = IsReadOnly;
            dtpkDueDate.Disabled = IsReadOnly;
            txtBudget.ReadOnly = IsReadOnly;
            txtDescription.ReadOnly = IsReadOnly;
            txtBrief.ReadOnly = IsReadOnly;
            txtAct.ReadOnly = IsReadOnly;
            txtScene.ReadOnly = IsReadOnly;
            txtPage.ReadOnly = IsReadOnly;
            txtCategory.ReadOnly = IsReadOnly;
            txtCharacter.ReadOnly = IsReadOnly;
            txtPreset.ReadOnly = IsReadOnly;
            txtApprover.ReadOnly = IsReadOnly;
            txtRehearsalItem.ReadOnly = IsReadOnly;
            txtUsage.ReadOnly = IsReadOnly;
            txtConsiderations.ReadOnly = IsReadOnly;
            ddItemTypes.Enabled = !IsReadOnly;

            #endregion Read Only
        }

        /// <summary>
        /// Sets up the thumbnail image display control with the default thumbnail
        /// image for this item brief.
        /// </summary>
        private void DisplayItemThumbnail()
        {
            DocumentMedia defaultImage = this.GetBL<UtilityBL>().GetDefaultImage(ItemBriefId, "ItemBrief");
            bool displayImagePicker = false;

            if (defaultImage != null)
            {
                thumbItemBrief.DocumentMediaId = defaultImage.DocumentMediaId;
                thumbItemBrief.ImageTitle = defaultImage.Name;

                displayImagePicker = true;

                // If there is a default image, there should be more than one image for Image picking to work.
                displayImagePicker = attachments.LoadedImageCount > 1;

                ItemBriefItemDocumentMedia itemBriefItemDocumentMedia = null;
                switch (defaultImage.RelatedTableName)
                {
                    case "ItemBrief":
                        itemBriefItemDocumentMedia = DataContext.ItemBriefItemDocumentMedias.Where(ibidm => ibidm.ItemBriefDocumentMediaId == defaultImage.DocumentMediaId).FirstOrDefault();
                        break;

                    case "Item":
                        itemBriefItemDocumentMedia = DataContext.ItemBriefItemDocumentMedias.Where(ibidm => ibidm.ItemDocumentMediaId == defaultImage.DocumentMediaId).FirstOrDefault();
                        break;
                }

                documentPreview.IsReadOnly = IsReadOnly || !(itemBriefItemDocumentMedia == null || (itemBriefItemDocumentMedia != null && itemBriefItemDocumentMedia.SourceTable == "ItemBrief"));
                documentPreview.InitializeUI();
            }
            else
            {
                thumbItemBrief.DocumentMediaId = 0;
                thumbItemBrief.ImageTitle = string.Empty;
                displayImagePicker = attachments.LoadedImageCount > 0;
            }

            if (displayImagePicker && !IsReadOnly)
            {
                trChangePreviewImage.Visible = true;
                imagePickerDocumentList.Visible = true;
                imagePickerDocumentList.RelatedTableName = "ItemBrief";
                imagePickerDocumentList.RelatedId = ItemBriefId;

                if (defaultImage != null)
                {
                    imagePickerDocumentList.ExcludedDocumentMediaIds = new int[] { defaultImage.DocumentMediaId };
                }
            }
            else
            {
                trChangePreviewImage.Visible = false;
                imagePickerDocumentList.Visible = false;
            }

            upnlItemBriefThumb.Update();
        }

        /// <summary>
        /// Navigates to project item list.
        /// </summary>
        private void NavigateToProjectItemList()
        {
            Response.Redirect(string.Format("~/ItemBrief/ItemBriefList.aspx?projectid={0}&ItemTypeId={1}&Sort={2}",
                        this.ProjectId, this.ItemTypeId, Server.UrlEncode(this.SortParam)));
        }

        /// <summary>
        /// Swtiches to completed tab.
        /// </summary>
        private void SwtichToCompletedTab()
        {
            completedItem.LoadData();
            itemBriefTabs.Tabs[2].Selected = true;
            itemBriefPages.SelectedIndex = 2;

            upTabs.Update();
            // upnlCompletedItem.Update();
        }

        /// <summary>
        /// Reloads the item detail page.
        /// </summary>
        private void ReloadItemDetailPage()
        {
            if (this.HasExclusiveRightsForProject)
            {
                this.ItemTypeId = int.Parse(ddItemTypes.SelectedValue);
            }

            LoadData();
            completedItem.IsCompleteItemTabDirty = false;
            this.IsPageDirty = false;
            DisplayItemThumbnail();
            // Update the Complete Item tab
            completedItem.LoadData();
            pinnedItems.LoadData();
            attachments.LoadData();
        }

        /// <summary>
        /// Updates the tab headings count.
        /// </summary>
        /// <param name="tabIndex">Index of the tab.</param>
        /// <param name="count">The count.</param>
        private void UpdateTabHeadingsCount(int tabIndex, int count)
        {
            switch (tabIndex)
            {
                case 1:
                    itemBriefTabs.Tabs[tabIndex].Text = string.Format("Tasks ({0})", count);
                    break;

                case 2:
                    itemBriefTabs.Tabs[tabIndex].Text = string.Format("Attachments ({0})", count);
                    break;

                case 3:
                    itemBriefTabs.Tabs[tabIndex].Text = string.Format("Pinboard ({0})", count);
                    break;
            }

            upTabs.Update();
        }

        /// <summary>
        /// Initializes the navigation.
        /// </summary>
        private void InitializeNavigation()
        {
            string findName = this.FindByName;
            if (findName.Length == 0)
            {
                findName = null;
            }

            List<Data.ItemBrief> items = this.GetBL<ItemBriefBL>().GetItemBriefDetails(ProjectId, ItemTypeId, findName);
            GridSortOrder order = (GridSortOrder)this.SortOrder;

            List<Data.ItemBrief> orderedItems = new List<Data.ItemBrief>();

            if (this.SortBy == "StatusSortOrder")
            {
                switch (order)
                {
                    case GridSortOrder.Ascending:
                        orderedItems = (from ib in items
                                        join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                                        orderby statusCode.SortOrder ascending
                                        select ib).ToList<Data.ItemBrief>();
                        break;

                    case GridSortOrder.Descending:
                        orderedItems = (from ib in items
                                        join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                                        orderby statusCode.SortOrder descending
                                        select ib).ToList<Data.ItemBrief>();
                        break;
                }

                //orderedItems = this.GetBL<ItemBriefBL>().GetOrderedItems(items, 0);
            }
            else
            {
                string[] sortByPropertyInfo = this.SortBy.Split('.');
                string sortByProperty = sortByPropertyInfo.Length == 2 ? sortByPropertyInfo[1] : string.Empty;
                if (!string.IsNullOrEmpty(sortByProperty))
                {
                    bool propertyFound = false;
                    Type itemBriefType = typeof(Data.ItemBrief);
                    PropertyInfo[] properties = itemBriefType.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.Name == sortByProperty)
                        {
                            switch (order)
                            {
                                case GridSortOrder.Ascending:
                                    orderedItems = items.OrderBy(i => property.GetValue(i, null)).ToList<Data.ItemBrief>();
                                    break;

                                case GridSortOrder.Descending:
                                    orderedItems = items.OrderByDescending(i => property.GetValue(i, null)).ToList<Data.ItemBrief>();
                                    break;
                            }

                            propertyFound = true;
                            break;
                        }
                    }

                    if (!propertyFound)
                    {
                        orderedItems = items.OrderBy(i => i.Name).ToList<Data.ItemBrief>();
                    }
                }
            }

            int currentItemBriefIndex = orderedItems.FindIndex(i => i.ItemBriefId == this.ItemBriefId);
            Data.ItemBrief previous = currentItemBriefIndex <= 0 ? null : orderedItems.ElementAt(currentItemBriefIndex - 1);
            Data.ItemBrief next = (currentItemBriefIndex + 1) == orderedItems.Count ? null : orderedItems.ElementAt(currentItemBriefIndex + 1);

            if (next == null)
            {
                lnkNext.Visible = false;
                imgbtnNextDisabled.Visible = true;
            }
            else
            {
                this.NextItemBriefId = next.ItemBriefId;
                lnkNext.ToolTip = string.Concat("Next: ", next.Name);
                lnkNext.NavigateUrl = string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&sort={1}",
                    this.NextItemBriefId, Server.UrlEncode(this.SortParam));
            }

            if (previous == null)
            {
                lnkPrevious.Visible = false;
                imgbtnPreviousDisabled.Visible = true;
            }
            else
            {
                this.PreviousItemBriefId = previous.ItemBriefId;
                lnkPrevious.ToolTip = string.Concat("Previous: ", previous.Name);
                lnkPrevious.NavigateUrl = string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&sort={1}",
                        this.PreviousItemBriefId, Server.UrlEncode(this.SortParam));
            }
        }

        /// <summary>
        /// Loads the sorting.
        /// </summary>
        private void LoadSorting()
        {
            int defaultSortOrder = (int)GridSortOrder.Ascending;

            if (this.Request.QueryString["Sort"] != null && !string.IsNullOrEmpty(this.Request.QueryString["Sort"]))
            {
                string sortParam = this.Request.QueryString["Sort"];
                string[] sortParamArray = sortParam.Split('|');

                if (sortParamArray.Length == 3)
                {
                    this.SortParam = sortParam;
                    this.SortBy = sortParamArray[0];
                    this.FindByName = Support.DecodeBase64String(sortParamArray[2]);

                    int sortOrder = 0;
                    if (int.TryParse(sortParamArray[1], out sortOrder))
                    {
                        if (sortOrder > 0 && sortOrder <= 2)
                        {
                            this.SortOrder = sortOrder;
                        }
                        else
                        {
                            this.SortOrder = defaultSortOrder;
                        }
                    }
                }
                else
                {
                    this.SortParam = string.Empty;
                    this.SortBy = "ItemBrief.Name";
                    this.SortOrder = defaultSortOrder;
                }
            }
            else
            {
                this.SortParam = string.Empty;
                this.SortBy = "ItemBrief.Name";
                this.SortOrder = defaultSortOrder;
            }
        }

        /// <summary>
        /// Exports the report.
        /// </summary>
        private void ExportReport()
        {
            var itemBrief = GetBL<ItemBriefBL>().GetItemBrief(this.ItemBriefId);
            if (itemBrief != null)
            {
                var ms = new MemoryStream();
                Bitmap bm = new Bitmap(Server.MapPath(@"~/Common/Images/NoImgPDF.png"));
                bm.Save(ms, ImageFormat.Jpeg);
                ms.Flush();
                byte[] noImgPDFBytes = ms.ToArray();

                ItemBriefSpecificationsReportParameters parameters = new ItemBriefSpecificationsReportParameters
                {
                    ItemBriefId = this.ItemBriefId,
                    NoImagePDFBytes = noImgPDFBytes,
                    ProjectId = this.ProjectId,
                    UserId = this.UserID
                };
                string fileName = string.Format("{0}_ItemBriefSpecifications", itemBrief.Name);

                string fileNameExtension;
                string encoding;
                string mimeType;

                byte[] reportBytes = UserWebReportHandler.GenerateItemBriefSpecificationsReport(parameters, ReportTypes.Pdf,
                        out fileNameExtension, out encoding, out mimeType);
                Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
            }
        }

        #endregion Methods
    }
}