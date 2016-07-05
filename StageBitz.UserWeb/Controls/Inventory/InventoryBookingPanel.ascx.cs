using StageBitz.Common;
using StageBitz.Common.Constants;
using StageBitz.Common.Enum;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="shouldResetItemId">if set to <c>true</c> [should reset item identifier].</param>
    /// <param name="shouldResetPageDirty">if set to <c>true</c> [should reset page dirty].</param>
    public delegate void InformInventoryToUpdate(bool shouldResetItemId = true, bool shouldResetPageDirty = true);

    /// <summary>
    /// 
    /// </summary>
    public delegate void InformInventoryToUpdateFilterationChange();

    /// <summary>
    /// User Control for booking panel in company inventory.
    /// </summary>
    public partial class InventoryBookingPanel : UserControlBase
    {
        #region Delegates &  Events
        /// <summary>
        /// Delegate for show error message
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public delegate void InformCompanyInventoryToShowErrorPopup(ErrorCodes errorCode);

        /// <summary>
        /// The inform inventory to show error message.
        /// </summary>
        public InformCompanyInventoryToShowErrorPopup OnInformCompanyInventoryToShowErrorPopup;
        #endregion

        #region Constatnts

        /// <summary>
        /// The project booking prefix
        /// </summary>
        public const string ProjectBookingPrefix = InventoryBL.ProjectBookingPrefix;

        /// <summary>
        /// The non project booking prefix
        /// </summary>
        public const string NonProjectBookingPrefix = InventoryBL.NonProjectBookingPrefix;

        #endregion Constatnts

        #region Enums

        /// <summary>
        /// View mode of the InventoryBookingPanel control.
        /// </summary>
        public enum ViewMode
        {
            ItemDetail,
            CompanyInventory,
            WatchList
        }

        #endregion Enums

        #region Private Variables

        /// <summary>
        /// Has read only rights for project
        /// </summary>
        private bool isReadOnlyRightsForProject;

        /// <summary>
        /// Is project is closed
        /// </summary>
        private bool isProjectClosed;

        /// <summary>
        /// The item briefs.
        /// </summary>
        private List<ItemBookingAllDetails> itemBriefs = null;

        #endregion Private Variables

        #region Public Properties

        /// <summary>
        /// The inform inventory to update
        /// </summary>
        public InformInventoryToUpdate InformInventoryToUpdate;

        /// <summary>
        /// The inform inventory to update filteration change
        /// </summary>
        public InformInventoryToUpdateFilterationChange InformInventoryToUpdateFilterationChange;

        /// <summary>
        /// Gets or sets the display module.
        /// </summary>
        /// <value>
        /// The display module.
        /// </value>
        public ViewMode DisplayModule
        {
            get
            {
                if (ViewState["DisplayModule"] == null)
                {
                    ViewState["DisplayModule"] = default(ViewMode);
                }

                return (ViewMode)ViewState["DisplayModule"];
            }
            set
            {
                ViewState["DisplayModule"] = value;
            }
        }

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

        /// <summary>
        /// Gets or sets the height of the div inventory panel.
        /// </summary>
        /// <value>
        /// The height of the div inventory panel.
        /// </value>
        public int divInventoryPanelHeight
        {
            get
            {
                if (ViewState["pnlInventoryHeight"] == null)
                {
                    ViewState["pnlInventoryHeight"] = 455;
                }

                return (int)ViewState["pnlInventoryHeight"];
            }
            set
            {
                ViewState["pnlInventoryHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId
        {
            get
            {
                if (ViewState["RelatedId"] == null)
                {
                    int projectId = 0;

                    if (Request["RelatedId"] != null)
                    {
                        int.TryParse(Request["RelatedId"], out projectId);
                    }

                    ViewState["RelatedId"] = projectId;
                }

                return (int)ViewState["RelatedId"];
            }
            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related table.
        /// </summary>
        /// <value>
        /// The related table.
        /// </value>
        public BookingTypes RelatedTable
        {
            get
            {
                if (ViewState["RelatedTable"] == null)
                {
                    BookingTypes relatedTable = BookingTypes.None;
                    if (Request["RelatedTable"] != null)
                    {
                        int optionId;
                        if (int.TryParse(Request["RelatedTable"], out optionId))
                        {
                            relatedTable = (BookingTypes)optionId;
                        }
                    }

                    ViewState["RelatedTable"] = relatedTable;
                }

                return (BookingTypes)ViewState["RelatedTable"];
            }
            set
            {
                ViewState["RelatedTable"] = value;
            }
        }

        /// <summary>
        /// Gets the booking code.
        /// </summary>
        /// <value>
        /// The booking code.
        /// </value>
        public string BookingCode
        {
            get
            {
                if (RelatedTable == BookingTypes.NonProject)
                {
                    return string.Concat(NonProjectBookingPrefix, RelatedId.ToString(CultureInfo.InvariantCulture));
                }
                else if (RelatedTable == BookingTypes.Project)
                {
                    return string.Concat(ProjectBookingPrefix, RelatedId.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the booked quantity.
        /// </summary>
        /// <value>
        /// The booked quantity.
        /// </value>
        public int BookedQuantity
        {
            get
            {
                if (ViewState["BookedQuantity"] == null)
                {
                    return 1;
                }

                return (int)ViewState["BookedQuantity"];
            }
            set
            {
                ViewState["BookedQuantity"] = value;
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
                    int itemTypeId = 0;

                    if (Request["ItemTypeId"] != null)
                    {
                        int.TryParse(Request["ItemTypeId"], out itemTypeId);
                    }

                    ViewState["ItemTypeId"] = itemTypeId;
                }

                return (int)ViewState["ItemTypeId"];
            }
            set
            {
                ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item identifier.
        /// </summary>
        /// <value>
        /// The item identifier.
        /// </value>
        public int ItemId
        {
            get
            {
                if (ViewState["ItemId"] == null)
                {
                    int itemTypeId = 0;

                    if (Request["ItemId"] != null)
                    {
                        int.TryParse(Request["ItemId"], out itemTypeId);
                    }

                    ViewState["ItemId"] = itemTypeId;
                }

                return (int)ViewState["ItemId"];
            }
            set
            {
                ViewState["ItemId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected itrem brief identifier.
        /// </summary>
        /// <value>
        /// The selected itrem brief identifier.
        /// </value>
        public int SelectedItremBriefId
        {
            get
            {
                if (ViewState["SelectedItremBriefId"] == null)
                {
                    ViewState["SelectedItremBriefId"] = 0;
                }

                return (int)ViewState["SelectedItremBriefId"];
            }
            set
            {
                ViewState["SelectedItremBriefId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets from date.
        /// </summary>
        /// <value>
        /// From date.
        /// </value>
        public DateTime? FromDate
        {
            get
            {
                if (ViewState["FromDate"] == null)
                {
                    ViewState["FromDate"] = Utils.GetDatetime(dtpkPPFrom.Text, false);
                }
                return (DateTime?)ViewState["FromDate"];
            }
            set
            {
                ViewState["FromDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets to date.
        /// </summary>
        /// <value>
        /// To date.
        /// </value>
        public DateTime? ToDate
        {
            get
            {
                if (ViewState["ToDate"] == null)
                {
                    ViewState["ToDate"] = Utils.GetDatetime(dtpkPPTo.Text, false);
                }
                return (DateTime?)ViewState["ToDate"];
            }
            set
            {
                ViewState["ToDate"] = value;
            }
        }

        /// <summary>
        /// This is the Item that selected to PIN. In ItemDetail Mode, hiddenItemId is the ItemID in URL.
        /// </summary>
        /// <value>
        /// The hidden item identifier.
        /// </value>
        public int HiddenItemId
        {
            get
            {
                int itemId;
                int.TryParse(hdnItemId.Value, out itemId);
                return itemId;
            }
            set
            {
                hdnItemId.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the shared inventory company identifier.
        /// </summary>
        /// <value>
        /// The shared inventory company identifier.
        /// </value>
        public int SharedInventoryCompanyId
        {
            get
            {
                int companyId = 0;
                if (ViewState["SharedInventoryCompanyId"] != null)
                {
                    int.TryParse(ViewState["SharedInventoryCompanyId"].ToString(), out companyId);
                }
                return companyId;
            }
            set
            {
                ViewState["SharedInventoryCompanyId"] = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            if (DisplayModule == ViewMode.CompanyInventory || DisplayModule == ViewMode.ItemDetail)
            {
                HiddenItemId = this.ItemId;
            }
            else //when you go to watch list view there is no need of keeping the item selected. (it will enable the plus button otherwise)
            {
                HiddenItemId = 0;
                this.ItemId = 0;
            }

            if (hdnQuantityBooked.Value == string.Empty)
                hdnQuantityBooked.Value = BookedQuantity.ToString();

            //Get Available Quantity
            if (HiddenItemId > 0)
            {
                int availableQty = GetBL<InventoryBL>().GetAvailableItemQuantity(ItemId, FromDate, ToDate);
                ScriptManager.RegisterStartupScript(upnl, GetType(), "availableQty", "availableQty = " + availableQty + ";", true);
            }
            tooltipManager.TargetControls.Clear();

            dtpkPPFrom.Text = FromDate.HasValue ? Utils.FormatDate(FromDate) : string.Empty;
            dtpkPPTo.Text = ToDate.HasValue ? Utils.FormatDate(ToDate) : string.Empty;

            List<UserBookings> bookings = this.GetBL<InventoryBL>().GetBookingList(CompanyId, UserID);

            ddBookings.Items.Clear();
            ddBookings.Items.Add(new ListItem("Select a Project or Booking", "-1"));
            ddBookings.DataSource = bookings.OrderBy(b => b.Name);
            ddBookings.DataTextField = "Name";
            ddBookings.DataValueField = "BookingCode";
            ddBookings.DataBind();

            if (this.RelatedTable == BookingTypes.Project)
            {
                ShowBookingPanel(this.RelatedTable);

                Data.Project project = DataContext.Projects.Where(p => p.ProjectId == RelatedId).FirstOrDefault();
                //Check whether user change the ProjectID from the URL.
                if (project != null && Support.CanAccessProject(project) && !this.GetBL<ProjectBL>().IsProjectClosed(RelatedId)) // closed project is excluding
                {
                    ddBookings.SelectedValue = ProjectBookingPrefix + RelatedId.ToString();
                }
                else if (RelatedId == 0 || this.GetBL<ProjectBL>().IsProjectClosed(RelatedId))
                {
                    RelatedId = GetBookingId(ddBookings.SelectedValue, ProjectBookingPrefix);
                }

                //Load Item Types  by Project
                if (RelatedId > 0)
                {
                    LoadItemTypes();
                    LoadItemBriefs();
                }
            }
            else if (this.RelatedTable == BookingTypes.NonProject)
            {
                ShowBookingPanel(this.RelatedTable);

                Data.NonProjectBooking nonProjectBooking = DataContext.NonProjectBookings.Where(npb => npb.NonProjectBookingId == RelatedId).FirstOrDefault();
                //Check whether user change the ProjectID from the URL.
                if (nonProjectBooking != null) // closed project is excluding
                {
                    ddBookings.SelectedValue = NonProjectBookingPrefix + RelatedId.ToString();
                }
                else if (RelatedId == 0)
                {
                    RelatedId = GetBookingId(ddBookings.SelectedValue, NonProjectBookingPrefix);
                }

                if (RelatedId > 0)
                {
                    LoadNonProjectBookingItems();
                }
            }
            else
            {
                ShowBookingPanel(this.RelatedTable);
            }

            SetHiddenSuccessAddMessage();
        }

        /// <summary>
        /// Sets the hidden success add message.
        /// </summary>
        private void SetHiddenSuccessAddMessage()
        {
            switch (RelatedTable)
            {
                case BookingTypes.NonProject:
                    hdnSuccessAddMessage.Value = "Add Item to the selected booking";
                    break;

                case BookingTypes.Project:
                    if (ItemTypeId > 0)
                    {
                        hdnSuccessAddMessage.Value = string.Format("Add Item to the selected {0} brief", Utils.GetItemTypeById(ItemTypeId).Name);
                    }
                    break;

                default:
                    hdnSuccessAddMessage.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Loads the non project booking items.
        /// </summary>
        private void LoadNonProjectBookingItems()
        {
            List<CompanyItemBooking> nonProjectBookingItems = this.GetBL<InventoryBL>().GetItemBookingsByRelatedTable(GlobalConstants.RelatedTables.Bookings.NonProject, RelatedId);

            int pinnedCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int inuseCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;
            int inuseCompleteCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;
            int notApprovedCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "NOTAPPROVED").CodeId;

            int returnedCodeId = Utils.GetCodeByValue("InventoryStatusCode", "RETURNED").CodeId;
            int notPickedUpCodeId = Utils.GetCodeByValue("InventoryStatusCode", "NOTPICKEDUP").CodeId;

            const string AwaitingApproval = "Awaiting Approval";
            const string Confirmed = "Confirmed";
            const string NotApproved = "Not Approved";
            const string Returned = "Returned";

            List<string> allStatus = new List<string>();
            allStatus.Add(AwaitingApproval);
            allStatus.Add(Confirmed);
            allStatus.Add(NotApproved);
            allStatus.Add(Returned);

            var groupedNonProjectBookingItems = (from ib in nonProjectBookingItems
                                                 let Status = ib.ItemBooking.InventoryStatusCodeId == returnedCodeId ? Returned :
                                                                 ib.ItemBooking.ItemBookingStatusCodeId == notApprovedCodeId ? NotApproved :
                                                                 ((ib.ItemBooking.ItemBookingStatusCodeId == inuseCodeId || ib.ItemBooking.ItemBookingStatusCodeId == inuseCompleteCodeId) && ib.ItemBooking.IsActive) ? Confirmed :
                                                                 (ib.ItemBooking.ItemBookingStatusCodeId == pinnedCodeID && ib.ItemBooking.IsActive) ? AwaitingApproval : string.Empty
                                                 where Status != string.Empty
                                                 orderby ib.Company.CompanyName
                                                 group new { ib.Company, Status, ib.ItemBooking } by ib.Company.CompanyId into grp
                                                 select new
                                                 {
                                                     CompanyName = grp.First().Company.CompanyName,
                                                     CompanyBookingCount = grp.Count(),
                                                     StatusGroups = (from s in allStatus
                                                                     from g in grp.Where(gx => s == gx.Status).DefaultIfEmpty()
                                                                     group g by s into gup
                                                                     select new
                                                                     {
                                                                         Status = gup.Key,
                                                                         ItemCount = gup.Count(g => g != null),
                                                                         Items = gup.Where(g => g != null).Select(g => new
                                                                         {
                                                                             ItemName = g.ItemBooking.Item.Name,
                                                                             ItemId = g.ItemBooking.ItemId,
                                                                             CompanyId = g.Company.CompanyId,
                                                                             ItemBookingId = g.ItemBooking.ItemBookingId,
                                                                             CanDelete = g.ItemBooking.IsActive && g.ItemBooking.InventoryStatusCodeId == notPickedUpCodeId
                                                                         }).OrderBy(i => i.ItemName)
                                                                     })
                                                 }).ToList();

            if (groupedNonProjectBookingItems.Count > 0)
            {
                divNonProjectBookingGrid.Visible = true;
                divNoDataNonProjectBookings.Visible = false;
            }
            else
            {
                divNoDataNonProjectBookings.Visible = true;
                divNonProjectBookingGrid.Visible = false;
            }

            lvNonProjectBookings.DataSource = groupedNonProjectBookingItems;
            lvNonProjectBookings.DataBind();
        }

        /// <summary>
        /// Shows the booking panel.
        /// </summary>
        /// <param name="bookingType">Type of the booking.</param>
        private void ShowBookingPanel(BookingTypes bookingType)
        {
            if (bookingType == BookingTypes.Project)
            {
                divProjectBookings.Visible = true;
                divNonProjectBookings.Visible = false;
                divEmptyBookings.Visible = false;
                tblBookingInputs.Visible = true;
                trItemType.Visible = true;
            }
            else if (bookingType == BookingTypes.NonProject)
            {
                divNonProjectBookings.Visible = true;
                divProjectBookings.Visible = false;
                divEmptyBookings.Visible = false;
                tblBookingInputs.Visible = true;
                trItemType.Visible = false;
            }
            else
            {
                divEmptyBookings.Visible = true;
                divNonProjectBookings.Visible = false;
                divProjectBookings.Visible = false;
                tblBookingInputs.Visible = false;
            }
        }

        /// <summary>
        /// Updates the project panel.
        /// </summary>
        public void UpdateProjectPanel()
        {
            LoadData();
            upnl.Update();
        }

        /// <summary>
        /// Gets the booking identifier.
        /// </summary>
        /// <param name="bookingCode">The booking code.</param>
        /// <param name="bookingType">Type of the booking.</param>
        /// <returns></returns>
        public int GetBookingId(string bookingCode, out BookingTypes bookingType)
        {
            // NOTE : Any changes to this method should refelct same method in client side.

            if (bookingCode.Contains(NonProjectBookingPrefix))
            {
                bookingType = BookingTypes.NonProject;
                return GetBookingId(bookingCode, NonProjectBookingPrefix);
            }
            else if (bookingCode.Contains(ProjectBookingPrefix))
            {
                bookingType = BookingTypes.Project;
                return GetBookingId(bookingCode, ProjectBookingPrefix);
            }
            else
            {
                bookingType = BookingTypes.None;
                return 0;
            }
        }

        /// <summary>
        /// Gets the booking identifier.
        /// </summary>
        /// <param name="bookingCode">The booking code.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public int GetBookingId(string bookingCode, string prefix)
        {
            // NOTE : Any changes to this method should refelct same method in client side.

            string[] codeParts = bookingCode.Split(new string[] { prefix }, StringSplitOptions.None);
            if (codeParts.Length > 1)
            {
                int id;
                if (int.TryParse(codeParts[1], out id))
                {
                    return id;
                }
            }

            return 0;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the item types.
        /// </summary>
        private void LoadItemTypes()
        {
            if (RelatedId == 0)
            {
                int projectId = 0;
                int.TryParse(ddBookings.SelectedValue, out projectId);
                RelatedId = projectId;
            }

            List<Data.ItemType> itemTypes = this.GetBL<InventoryBL>().GetItemTypes(RelatedId);

            ddItemTypes.Enabled = !(itemTypes.Count == 0);
            ddItemTypes.DataSource = itemTypes;
            ddItemTypes.DataTextField = "Name";
            ddItemTypes.DataValueField = "ItemTypeId";
            ddItemTypes.DataBind();

            if (itemTypes.Contains(DataContext.ItemTypes.Where(it => it.ItemTypeId == ItemTypeId).FirstOrDefault()))
            {
                ddItemTypes.SelectedValue = ItemTypeId.ToString();
            }
            else
            {
                int itemTypeId = 0;
                int.TryParse(ddItemTypes.SelectedValue, out itemTypeId);
                ItemTypeId = itemTypeId;
            }
        }

        /// <summary>
        /// Loads the item briefs.
        /// </summary>
        private void LoadItemBriefs()
        {
            if (ItemTypeId == 0)
            {
                int itemTypeId = 0;
                int.TryParse(ddItemTypes.SelectedValue, out itemTypeId);
                ItemTypeId = itemTypeId;
            }

            if (RelatedId == 0)
            {
                int projectId = 0;
                int.TryParse(ddBookings.SelectedValue, out projectId);
                RelatedId = projectId;
            }

            itemBriefs = this.GetBL<InventoryBL>().GetItemBriefs(ItemTypeId, RelatedId).ToList();

            int pinnedCodeID = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int inuseCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;
            int inuseCompleteCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;
            int storedInInventoryCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "STOREDININVENTORY");
            int disposedOfCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "DISPOSEDOF");
            int completedStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");

            var nothingPinnedList = (from ibi in itemBriefs where !(ibi.ItemBrief.ItemBriefStatusCodeId == storedInInventoryCodeId || ibi.ItemBrief.ItemBriefStatusCodeId == disposedOfCodeId) && ibi.ItemBookingList.Count() == 0 select ibi.ItemBrief).ToList<Data.ItemBrief>();
            var awaitingDecision = (from ib in itemBriefs where ib.ItemBookingList.Where(ibi => ibi.ItemBookingStatusCodeId == pinnedCodeID).Count() > 0 select ib.ItemBrief).ToList<Data.ItemBrief>();
            var confirmedList = (from ib in itemBriefs where ib.ItemBookingList.Where(ibi => ibi.ItemBookingStatusCodeId == inuseCodeId || ibi.ItemBookingStatusCodeId == inuseCompleteCodeId).Count() > 0 && ib.ItemBrief.ItemBriefStatusCodeId != completedStatusCodeId select ib.ItemBrief).ToList<Data.ItemBrief>();
            var completeList = (from ib in itemBriefs where ib.ItemBrief.ItemBriefStatusCodeId == completedStatusCodeId select ib.ItemBrief).ToList<Data.ItemBrief>();
            var releasedtoInventoryList = (from ib in itemBriefs where ib.ItemBrief.ItemBriefStatusCodeId == storedInInventoryCodeId select ib.ItemBrief).ToList<Data.ItemBrief>();
            var disposedOfInventoryList = (from ib in itemBriefs where ib.ItemBrief.ItemBriefStatusCodeId == disposedOfCodeId select ib.ItemBrief).ToList<Data.ItemBrief>();

            var result = new List<ItemBriefDetailsForProjectPanel>()
            {
                new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Nothing Pinned",
                    GroupCount = nothingPinnedList.Count(),
                    itemBriefList = nothingPinnedList
                },
                new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Awaiting decision",
                    GroupCount = awaitingDecision.Count(),
                    itemBriefList = awaitingDecision
                },
                       new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Confirmed",
                    GroupCount = confirmedList.Count(),
                    itemBriefList = confirmedList
                },
                new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Completed",
                    GroupCount = completeList.Count(),
                    itemBriefList = completeList
                },
                   new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Released",
                    GroupCount = releasedtoInventoryList.Count(),
                    itemBriefList = releasedtoInventoryList
                },
                    new ItemBriefDetailsForProjectPanel()
                {
                    GroupName = "Disposed of",
                    GroupCount = disposedOfInventoryList.Count(),
                    itemBriefList = disposedOfInventoryList
                }
            };

            isReadOnlyRightsForProject = Support.IsReadOnlyRightsForProject(RelatedId);
            isProjectClosed = this.GetBL<ProjectBL>().IsProjectClosed(RelatedId);

            lvItemBriefGroup.DataSource = result;
            lvItemBriefGroup.DataBind();
        }

        /// <summary>
        /// Reloads the item details.
        /// </summary>
        private void ReloadItemDetails()
        {
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "ReloadItemDetails", "ReloadItemDetails();", true);
        }

        /// <summary>
        /// Reloads the item details for date filteration.
        /// </summary>
        private void ReloadItemDetailsForDateFilteration()
        {
            bool hasDateFilteration = FromDate.HasValue && ToDate.HasValue;
            ScriptManager.RegisterStartupScript(this.Page, GetType(),
                "ReloadItemDetailsForDateFilteration",
                string.Format("ReloadItemDetailsForDateFilteration('{0}','{1}','{2}','{3}');",
                    Support.FormatDate(FromDate), Support.FormatDate(ToDate),
                    hasDateFilteration,
                    ItemId > 0 && hasDateFilteration ? this.GetBL<InventoryBL>().GetAvailableItemQuantity(ItemId, FromDate, ToDate) : 0),
                    true);
        }

        /// <summary>
        /// Books the item to booking.
        /// </summary>
        private bool BookItemToBooking()
        {
            int itemId = 0;
            int quantityBooked = 0;
            if (int.TryParse(hdnItemId.Value, out itemId) && int.TryParse(hdnQuantityBooked.Value, out quantityBooked))
            {
                Data.Item item = GetBL<InventoryBL>().GetItem(itemId);
                int bookingId = 0;
                string relatedTable = string.Empty;
                bool shouldValidateSharedInventory = false;
                switch (RelatedTable)
                {
                    case BookingTypes.NonProject:
                        shouldValidateSharedInventory = true;
                        bookingId = RelatedId;
                        relatedTable = GlobalConstants.RelatedTables.Bookings.NonProject;
                        break;

                    case BookingTypes.Project:
                        bookingId = SelectedItremBriefId;
                        relatedTable = "ItemBrief";
                        break;
                }

                Data.Code uservisibilityCode = GetBL<InventoryBL>().GetUserInventoryVisibilityLevel(item.CompanyId.Value, this.UserID, item.LocationId, shouldValidateSharedInventory);
                if (item.Code.SortOrder < uservisibilityCode.SortOrder && OnInformCompanyInventoryToShowErrorPopup != null)
                {
                    OnInformCompanyInventoryToShowErrorPopup(ErrorCodes.ItemNotVisible);
                    return false;
                }

                if (quantityBooked > 0)
                {
                    int itemBookingId = DataContext.AddItemToBooking(bookingId, relatedTable,
                            itemId, Utils.GetDatetime(dtpkPPFrom.Text, false), Utils.GetDatetime(dtpkPPTo.Text, false), quantityBooked, UserID);

                    if (itemBookingId <= 0)
                    {
                        litpopupPinnedStatus.Text = "Failed Booking Item";
                        popupPinnedStatus.ShowPopup();
                    }
                    else
                    {
                        if (RelatedTable == BookingTypes.Project)
                        {
                            GetBL<NotificationBL>().GenerateNotificationsForBookings(this.UserID, SelectedItremBriefId, itemId, RelatedId, NotificationBL.BookingAction.Pin);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            ReloadData();
            return true;
        }

        /// <summary>
        /// Reloads the data.
        /// </summary>
        private void ReloadData()
        {
            if (InformInventoryToUpdate != null)
            {
                InformInventoryToUpdate();
                ReloadItemDetails();
            }

            LoadData();
        }

        /// <summary>
        /// Shows the project closed warning for pinning.
        /// </summary>
        private void ShowProjectClosedWarningForPinning()
        {
            Data.User userClosedProject = this.GetBL<ProjectBL>().GetProjectClosedBy(RelatedId);
            Data.Company company = Support.GetCompanyByProjectId(RelatedId);
            Data.User companyPrimaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(company.CompanyId);
            if (userClosedProject != null)
            {
                litPersonClosedProject.Text = userClosedProject.FirstName + " " + userClosedProject.LastName;
            }
            if (company != null)
            {
                litClosedProjectCompanyName.Text = company.CompanyName + "'s";
            }
            if (companyPrimaryAdmin != null)
            {
                hyperLinkClosedProjectCompanyAdming.Text = companyPrimaryAdmin.FirstName + " " + companyPrimaryAdmin.LastName;
                hyperLinkClosedProjectCompanyAdming.NavigateUrl = "mailto:" + companyPrimaryAdmin.Email1;
            }
            popupPinClosedProjectsItemBriefs.ShowPopup();
        }

        /// <summary>
        /// Initiates the pin to item brief.
        /// </summary>
        private void InitiatePinToItemBrief()
        {
            if (GetBL<InventoryBL>().GetInUseOrCompleteItemBooking(SelectedItremBriefId) != null)
            {
                popupConfirmationForAlreadyCompleted.ShowPopup();
                return;
            }
            else
            {
                if (!BookItemToBooking())
                {
                    return;
                }
            }

            //Reset the ItemId
            if (DisplayModule != ViewMode.ItemDetail)
                hdnItemId.Value = "0";
            RegisterCheckboxInitializeScript();
        }

        /// <summary>
        /// Initiates the book item to non project booking.
        /// </summary>
        private void InitiateBookItemToNonProjectBooking()
        {
            if (BookItemToBooking())
            {
                //Reset the ItemId
                if (DisplayModule != ViewMode.ItemDetail)
                    hdnItemId.Value = "0";
                RegisterCheckboxInitializeScript();
            }
        }

        /// <summary>
        /// Registers the checkbox initialize script.
        /// </summary>
        private void RegisterCheckboxInitializeScript()
        {
            //ScriptManager.RegisterStartupScript(upnl, GetType(), "ConfigureProjectPanel", "ConfigureProjectPanel(" + projectPanelToolTipResultForItem.CanPin.ToString().ToLower() + ",'" + projectPanelToolTipResultForItem.ToolTip + "');", true);
            int itemId = 0;
            int.TryParse(hdnItemId.Value, out itemId);
            InventoryBL inventoryBL = new InventoryBL(DataContext);
            int quantityBooked = 0;
            int.TryParse(hdnQuantityBooked.Value, out quantityBooked);

            DateTime? fromDate = Utils.GetDatetime(dtpkPPFrom.Text, false);
            DateTime? toDate = Utils.GetDatetime(dtpkPPTo.Text, false);

            BookingPanelToolTipResultForItem projectPanelToolTipResultForItem = null;
            if (RelatedTable == BookingTypes.Project)
            {
                projectPanelToolTipResultForItem = inventoryBL.GetBookingPanelItemBriefToolTipResultForItem(HiddenItemId, RelatedId, ItemTypeId, UserID, fromDate, toDate, quantityBooked);
            }
            else if (RelatedTable == BookingTypes.NonProject)
            {
                projectPanelToolTipResultForItem = inventoryBL.GetBookingPanelNonProjectBookingToolTipResultForItem(HiddenItemId, UserID, RelatedId, fromDate, toDate, quantityBooked);
            }

            if (projectPanelToolTipResultForItem != null)
            {
                ScriptManager.RegisterStartupScript(upnl, GetType(), "ConfigureProjectPanel", "ConfigureProjectPanel(" + projectPanelToolTipResultForItem.CanPin.ToString().ToLower() + ",'" + projectPanelToolTipResultForItem.ToolTip + "');", true);
                ScriptManager.RegisterStartupScript(upnl, GetType(), "InitializeFilterations", "InitializeFilterations();", true);
                ScriptManager.RegisterStartupScript(upnl, GetType(), "availableQty", "availableQty = " + projectPanelToolTipResultForItem.AvailableQuantity + ";", true);
            }
        }

        /// <summary>
        /// Clears the dates.
        /// </summary>
        private void ClearDates()
        {
            dtpkPPTo.Text = string.Empty;
            dtpkPPFrom.Text = string.Empty;
            FromDate = null;
            ToDate = null;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                divInventoryPanel.Style.Add("height", string.Concat(divInventoryPanelHeight.ToString(), "px"));

                string bookingPanelHeight = string.Concat((divInventoryPanelHeight - 180).ToString(), "px");
                divItemBriefs.Style.Add("height", bookingPanelHeight);
                divNonProjectBookingGrid.Style.Add("height", bookingPanelHeight);

                LoadData();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterCheckboxInitializeScript();
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDone_Click(object sender, EventArgs e)
        {
            popupPinClosedProjectsItemBriefs.HidePopup();
            if (Support.CanAccessInventory(CompanyId))
            {
                Page.Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
            }
            else
            {
                Page.Response.Redirect("~/Default.aspx");
            }
        }

        /// <summary>
        /// Handles the OnItemDataBound event of the lvItemBriefs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvItemBriefs_OnItemDataBound(object sender, ListViewItemEventArgs e)
        {
            ListViewItem dataItem = (ListViewItem)e.Item;
            dynamic itemBrief = ((dynamic)e.Item.DataItem);
            int itemBriefId = itemBrief.ItemBriefId;
            var itemSelected = DataContext.Items.Where(i => i.ItemId == HiddenItemId).FirstOrDefault();

            HtmlAnchor lnkItemBriefName = (HtmlAnchor)dataItem.FindControl("lnkItemBriefName");

            int storedInInventoryCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "STOREDININVENTORY");
            int disposedOfCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "DISPOSEDOF");

            ImageButton btnAdd = (ImageButton)dataItem.FindControl("btnAdd");
            ImageButton btnAddDisabled = (ImageButton)dataItem.FindControl("btnAddDisabled");

            if (itemBrief.ItemBriefStatusCodeId == storedInInventoryCodeId || itemBrief.ItemBriefStatusCodeId == disposedOfCodeId)
            {
                ImageButton btnReleasedToInventoryIB = (ImageButton)dataItem.FindControl("btnReleasedToInventoryIB");
                btnReleasedToInventoryIB.Visible = true;
                btnAdd.Visible = false;
                btnAddDisabled.Visible = false;
            }
            else
            {
                btnAdd.Attributes.Add("style", "display:none;");
                btnAddDisabled.Attributes.Add("style", string.Empty);
            }

            lnkItemBriefName.HRef = ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&TabId={1}", itemBriefId, 3));//set the tab to pinboard tabid is 3
            lnkItemBriefName.InnerText = Support.TruncateString(itemBrief.Name, 18);

            tooltipManager.TargetControls.Add(lnkItemBriefName.ClientID, itemBrief.ItemBriefId.ToString(), true);

            //Load available bookings
            var itemList = from ibi in
                               ((from ib in itemBriefs
                                 where ib.ItemBrief.ItemBriefId == itemBriefId
                                 select ib.ItemBookingList).FirstOrDefault())
                           where ibi.IsActive == true
                           select ibi.Item;

            if (itemList.Count() > 0)
            {
                GridView gvItems = (GridView)dataItem.FindControl("gvItems");
                gvItems.DataSource = itemList;
                gvItems.DataBind();
            }
        }

        /// <summary>
        /// Handles the OnRowDataBound event of the gvItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gvItems_OnRowDataBound(Object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                dynamic item = e.Row.DataItem as dynamic;
                int itemid = item.ItemId;

                Label lblName = (Label)e.Row.FindControl("lblName");

                lblName.Text = Support.TruncateString(item.Name, 18);

                if (item.Name.Length > 18)
                {
                    lblName.ToolTip = item.Name;
                }

                Image imgPin = (Image)e.Row.FindControl("imgPin");

                Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetItemBookingByItemID(itemid);

                if (itemBooking != null)
                    imgPin.ToolTip = Utils.GetCodeByCodeId(itemBooking.ItemBookingStatusCodeId).Description;
            }
        }

        /// <summary>
        /// Handles the OnItemCommand event of the lvItemBriefs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvItemBriefs_OnItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                bool IsItemDeleted = this.GetBL<InventoryBL>().IsItemDeleted(this.HiddenItemId);
                InventoryBL inventoryBL = new InventoryBL(DataContext);

                if (String.Equals(e.CommandName, "PinToItemBrief") && !Support.IsReadOnlyRightsForProject(RelatedId) && !IsItemDeleted)
                {
                    int quantityBooked = 0;
                    int.TryParse(hdnQuantityBooked.Value, out quantityBooked);

                    ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                    ListView lvItemBriefs = (ListView)(sender as ListView);
                    SelectedItremBriefId = (int)lvItemBriefs.DataKeys[dataItem.DisplayIndex].Value;

                    if (inventoryBL.IsItemAlreadySharedToBooking(HiddenItemId, SelectedItremBriefId, "ItemBrief"))//Check if the Item has already been Pinned to the same ItemBrief.
                    {
                        Data.Item item = GetBL<InventoryBL>().GetItem(HiddenItemId);
                        if (item != null)
                        {
                            var locationManager = GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                            string primaryAdminLink = string.Concat("<a href='mailto:", locationManager.Email1, "'>", locationManager.FirstName, " ", locationManager.LastName, "</a>");
                            litAlreadyPinned.Text = "This Item has already been booked to this Item Brief for a different period.</br> If you wish to change the booking period please contact the " + primaryAdminLink;

                            popUpAlreadyPinned.ShowPopup();
                        }
                    }
                    //Else check if it has sufficient quantity.
                    else if (DataContext.GetAvailableItemQuantity(HiddenItemId, Utils.GetDatetime(dtpkPPFrom.Text, false).Value, Utils.GetDatetime(dtpkPPTo.Text, false).Value, 0).FirstOrDefault().Value >= quantityBooked)
                    {
                        if (DisplayModule == ViewMode.ItemDetail)
                        {
                            if (this.PageBase.IsPageDirty)
                            {
                                popupConfirmItemDetailSave.ShowPopup();
                            }
                            else
                            {
                                InitiatePinToItemBrief();
                            }
                        }
                        else
                        {
                            InitiatePinToItemBrief();
                        }
                    }
                    else
                    {
                        popUpAlreadyPinned.Title = "Hold on... another booking has just been made";
                        litAlreadyPinned.Text = "Someone else has already booked at least one of the units for the Item you're trying to book.<br/> When you close this pop-up, the units available for the Item will be refreshed.";
                        popUpAlreadyPinned.ShowPopup();
                    }
                }
                else if (this.GetBL<ProjectBL>().IsProjectClosed(RelatedId))
                {
                    ShowProjectClosedWarningForPinning();
                }
                else if (IsItemDeleted)
                {
                    popupItemDeletedWarning.ShowItemDeleteMessagePopup(HiddenItemId, CompanyId);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmSave_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupConfirmItemDetailSave.HidePopup();

                if (RelatedTable == BookingTypes.NonProject)
                {
                    InitiateBookItemToNonProjectBooking();
                }
                else if (RelatedTable == BookingTypes.Project)
                {
                    InitiatePinToItemBrief();
                }

                ReloadItemDetails();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelSave_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupConfirmItemDetailSave.HidePopup();

                if (RelatedTable == BookingTypes.NonProject)
                {
                    InitiateBookItemToNonProjectBooking();
                }
                else if (RelatedTable == BookingTypes.Project)
                {
                    InitiatePinToItemBrief();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDoneItemDeletedPopup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDoneItemDeletedPopup_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", this.CompanyId));
        }

        /// <summary>
        /// Shows the thumbnail image of the item as a tooltip.
        /// </summary>
        protected void tooltipManager_AjaxUpdate(object sender, ToolTipUpdateEventArgs e)
        {
            int itemBriefId = 0;
            int.TryParse(e.Value, out itemBriefId);

            InventoryBusinessCard businessCard = (InventoryBusinessCard)LoadControl("~/Controls/Inventory/InventoryBusinessCard.ascx");

            businessCard.ItemBriefId = itemBriefId;
            businessCard.RelatedTable = "ItemBrief";
            businessCard.Visible = true;
            businessCard.LoadData();
            e.UpdatePanel.ContentTemplateContainer.Controls.Add(businessCard);
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing && !Support.IsReadOnlyRightsForProject(RelatedId))
            {
                BookItemToBooking();
                popupConfirmationForAlreadyCompleted.HidePopup();
            }
        }

        /// <summary>
        /// Handles the OnSelectedIndexChanged event of the ddBookings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddBookings_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                BookingTypes type;
                int bookingid = GetBookingId(ddBookings.SelectedValue, out type);
                RelatedId = bookingid;
                RelatedTable = type;
                switch (type)
                {
                    case BookingTypes.NonProject:
                        LoadNonProjectBookingItems();
                        break;

                    case BookingTypes.Project:
                        //Load the ItemTypes accordingly
                        LoadItemTypes();
                        LoadItemBriefs();
                        break;
                }

                SetHiddenSuccessAddMessage();
                ShowBookingPanel(type);
                ClearDates();

                if (InformInventoryToUpdate != null)
                {
                    InformInventoryToUpdate(false, false);
                }

                if (InformInventoryToUpdateFilterationChange != null)
                {
                    InformInventoryToUpdateFilterationChange();
                    ReloadItemDetailsForDateFilteration();
                }
            }
        }

        /// <summary>
        /// Handles the OnSelectedIndexChanged event of the ddItemTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddItemTypes_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int itemTypeId = 0;
                int.TryParse(ddItemTypes.SelectedValue, out itemTypeId);
                ItemTypeId = itemTypeId;

                LoadItemBriefs();
                ClearDates();
                if (InformInventoryToUpdate != null)
                {
                    InformInventoryToUpdate(false, false);
                }
                if (InformInventoryToUpdateFilterationChange != null)
                {
                    InformInventoryToUpdateFilterationChange();
                    ReloadItemDetailsForDateFilteration();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDoInventirySearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDoInventirySearch_Click(object sender, EventArgs e)
        {
            if (InformInventoryToUpdate != null)
            {
                FromDate = Utils.GetDatetime(dtpkPPFrom.Text, false).Value;
                ToDate = Utils.GetDatetime(dtpkPPTo.Text, false).Value;
                if (InformInventoryToUpdate != null)
                {
                    InformInventoryToUpdate(false, false);
                    ReloadItemDetailsForDateFilteration();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkClearbtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkClearbtn_Click(object sender, EventArgs e)
        {
            ClearDates();
            if (InformInventoryToUpdate != null)
            {
                InformInventoryToUpdate(false, false);
                ReloadItemDetailsForDateFilteration();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReloadData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReloadData_Click(object sender, EventArgs e)
        {
            if (DisplayModule != ViewMode.ItemDetail)
            {
                HiddenItemId = ItemId = 0;
            }

            popUpAlreadyPinned.HidePopup();
            ReloadData();
        }

        /// <summary>
        /// Handles the Click event of the btnCreateNewBooking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateNewBooking_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Data.NonProjectBooking nonProjectBooking = new Data.NonProjectBooking();
                nonProjectBooking.CreatedBy = UserID;
                nonProjectBooking.CreatedDate = Now;
                nonProjectBooking.IsActive = true;
                nonProjectBooking.LastUpdatedBy = UserID;
                nonProjectBooking.LastUpdatedDate = Now;
                nonProjectBooking.Name = txtNewBookingName.Text.Trim();

                GetBL<InventoryBL>().SaveNonProjectBooking(nonProjectBooking, true);

                this.RelatedTable = BookingTypes.NonProject;
                this.RelatedId = nonProjectBooking.NonProjectBookingId;

                popupCreateNewBooking.HidePopup();
                ReloadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMakeBooking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMakeBooking_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                bool IsItemDeleted = this.GetBL<InventoryBL>().IsItemDeleted(this.HiddenItemId);
                InventoryBL inventoryBL = new InventoryBL(DataContext);

                if (!IsItemDeleted)
                {
                    int quantityBooked = 0;
                    int.TryParse(hdnQuantityBooked.Value, out quantityBooked);

                    int bookingId = GetBookingId(ddBookings.SelectedValue, NonProjectBookingPrefix);

                    Data.Booking booking = inventoryBL.GetBooking(bookingId, GlobalConstants.RelatedTables.Bookings.NonProject);
                    if (booking != null && booking.IsArchived)
                    {
                        popupBookingArchived.ShowPopup();
                        this.RelatedId = 0;
                        this.RelatedTable = BookingTypes.None;
                    }
                    else if (inventoryBL.IsItemAlreadySharedToBooking(HiddenItemId, bookingId, GlobalConstants.RelatedTables.Bookings.NonProject))//Check if the Item has already been Pinned to the same booking.
                    {
                        popUpAlreadyPinned.Title = "This Item cannot be booked again.";
                        litAlreadyPinned.Text = "This Item has already been added to this booking.";
                        popUpAlreadyPinned.ShowPopup();
                    }
                    //Else check if it has sufficient quantity.
                    else if (DataContext.GetAvailableItemQuantity(HiddenItemId, Utils.GetDatetime(dtpkPPFrom.Text, false).Value, Utils.GetDatetime(dtpkPPTo.Text, false).Value, 0).FirstOrDefault().Value >= quantityBooked)
                    {
                        if (DisplayModule == ViewMode.ItemDetail)
                        {
                            if (this.PageBase.IsPageDirty)
                            {
                                popupConfirmItemDetailSave.ShowPopup();
                            }
                            else
                            {
                                InitiateBookItemToNonProjectBooking();
                            }
                        }
                        else
                        {
                            InitiateBookItemToNonProjectBooking();
                        }
                    }
                    else
                    {
                        popUpAlreadyPinned.Title = "Hold on... another booking has just been made";
                        litAlreadyPinned.Text = "Someone else has already booked at least one of the units for the Item you're trying to book.<br/> When you close this pop-up, the units available for the Item will be refreshed.";
                        popUpAlreadyPinned.ShowPopup();
                    }
                }
                else
                {
                    popupItemDeletedWarning.ShowItemDeleteMessagePopup(HiddenItemId, CompanyId);
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvMyBookingItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvMyBookingItems_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (e.CommandName == "DeleteItemBooking")
                {
                    popupDeleteBooking.ShowPopup();
                    hdnDeleteItemBooking.Value = e.CommandArgument.ToString();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReload_Click(object sender, EventArgs e)
        {
            popupBookingCannotDeleted.HidePopup();
            popupBookingArchived.HidePopup();
            ReloadData();
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmRemoveBooking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmRemoveBooking_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int itemBookingId;
                if (int.TryParse(hdnDeleteItemBooking.Value, out itemBookingId))
                {
                    popupDeleteBooking.HidePopup();
                    Data.ItemBooking itemBooking = GetBL<InventoryBL>().GetItemBooking(itemBookingId);
                    if (itemBooking != null && itemBooking.IsActive && itemBooking.InventoryStatusCodeId == Utils.GetCodeByValue("InventoryStatusCode", "NOTPICKEDUP").CodeId)
                    {
                        itemBooking.IsActive = false;
                        GetBL<InventoryBL>().SaveChanges();
                        ReloadData();
                    }
                    else
                    {
                        if (itemBooking.InventoryStatusCodeId == Utils.GetCodeByValue("InventoryStatusCode", "PICKEDUP").CodeId)
                        {
                            lblBookingCannotDeleted.Text = "This Item has been picked up and cannot be removed from the booking.";
                        }
                        else
                        {
                            lblBookingCannotDeleted.Text = "This Item cannot be removed.";
                        }

                        popupBookingCannotDeleted.ShowPopup();
                    }
                }
            }
        }

        #endregion Events
    }
}