using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// Delegate for inform parent to reload
    /// </summary>
    public delegate void InformParentToReload();

    /// <summary>
    /// User control for Project ItemTypes
    /// </summary>
    public partial class ProjectItemTypes : UserControlBase
    {
        /// <summary>
        /// The inform parent to reload
        /// </summary>
        public InformParentToReload InformParentToReload;

        #region Properties

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
        /// Gets or sets a value indicating whether [should show all item type text].
        /// </summary>
        /// <value>
        /// <c>true</c> if [should show all item type text]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldShowAllItemTypeText
        {
            get
            {
                if (ViewState["ShouldShowAllItemTypeText"] == null)
                {
                    ViewState["ShouldShowAllItemTypeText"] = false;
                }

                return (bool)ViewState["ShouldShowAllItemTypeText"];
            }
            set
            {
                ViewState["ShouldShowAllItemTypeText"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected item type identifier.
        /// </summary>
        /// <value>
        /// The selected item type identifier.
        /// </value>
        public int SelectedItemTypeId
        {
            get
            {
                if (ViewState["SelectedItemTypeId"] == null || (int)ViewState["SelectedItemTypeId"] == 0)
                {
                    int itemTypeId = 0;

                    if (Request["itemtypeid"] != null)
                    {
                        int.TryParse(Request["itemtypeid"], out itemTypeId);
                    }
                    else
                    {
                        //Get the Database saved value. (i.e the last visited ItemType)
                        UserLastVisitedtItemType userLastVisitedtItemType = (from uli in DataContext.UserLastVisitedtItemTypes
                                                                             where uli.UserId == UserID && uli.ProjectId == ProjectID
                                                                             select uli).FirstOrDefault();
                        if (userLastVisitedtItemType != null)
                        {
                            itemTypeId = userLastVisitedtItemType.ItemTypeId;
                        }
                    }

                    ViewState["SelectedItemTypeId"] = itemTypeId;
                }

                return (int)ViewState["SelectedItemTypeId"];
            }
            set
            {
                ViewState["SelectedItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last visited item type identifier.
        /// </summary>
        /// <value>
        /// The last visited item type identifier.
        /// </value>
        private int LastVisitedItemTypeId
        {
            get
            {
                if (this.ViewState["LastVisitedItemTypeId"] == null)
                {
                    if (this.SelectedItemTypeId > 0)
                    {
                        //this.ViewState["LastVisitedItemTypeId"] = itemType != null ? itemType.ItemTypeId : 0;
                    }
                    else
                    {
                        this.ViewState["LastVisitedItemTypeId"] = 0;
                    }
                }

                return (int)this.ViewState["LastVisitedItemTypeId"];
            }

            set
            {
                this.ViewState["LastVisitedItemTypeId"] = value;
            }
        }

        #endregion Properties

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
                LoadItemTypes();
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
                SelectedItemTypeId = int.Parse(ddItemTypes.SelectedValue);
                if (InformParentToReload != null)
                {
                    InformParentToReload();
                }
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the item types.
        /// </summary>
        /// <exception cref="System.Exception">Item Type not found</exception>
        private void LoadItemTypes()
        {
            //Get the Database saved value. (i.e the last visited ItemType)
            var userLastVisitedtItemTypeObj = GetBL<Logic.Business.Inventory.InventoryBL>().GetUserLastVisitedItemType(ProjectID, UserID);

            var itemTypeList = GetBL<Logic.Business.Inventory.InventoryBL>().GetItemTypes(ProjectID);

            //When there is no clue (Neither DB vaue nor URL)
            if (SelectedItemTypeId == 0 && userLastVisitedtItemTypeObj == null)
            {
                //If There is no ItemType is being added, Then we can not proceed
                if (itemTypeList.Count() == 0)
                {
                    divItemTypePanel.Visible = false;
                    return;
                }
                SelectedItemTypeId = itemTypeList.First().ItemTypeId;
            }
            else if (SelectedItemTypeId == 0)
            {
                SelectedItemTypeId = userLastVisitedtItemTypeObj.ItemTypeId;
            }

            if (SelectedItemTypeId != -1)
            {
                if (userLastVisitedtItemTypeObj == null)
                {
                    userLastVisitedtItemTypeObj = new UserLastVisitedtItemType();
                    userLastVisitedtItemTypeObj.ProjectId = ProjectID;
                    userLastVisitedtItemTypeObj.UserId = UserID;
                    userLastVisitedtItemTypeObj.ItemTypeId = SelectedItemTypeId;
                    userLastVisitedtItemTypeObj.CreatedByUserId = userLastVisitedtItemTypeObj.LastUpdatedByUserId = UserID;
                    userLastVisitedtItemTypeObj.CreatedDate = userLastVisitedtItemTypeObj.LastUpdatedDate = Now;
                    DataContext.UserLastVisitedtItemTypes.AddObject(userLastVisitedtItemTypeObj);
                    DataContext.SaveChanges();
                }
                else if (userLastVisitedtItemTypeObj.ItemTypeId != SelectedItemTypeId)
                {
                    userLastVisitedtItemTypeObj.ItemTypeId = SelectedItemTypeId;
                    userLastVisitedtItemTypeObj.LastUpdatedDate = Now;
                    userLastVisitedtItemTypeObj.LastUpdatedByUserId = UserID;
                    DataContext.SaveChanges();
                }
            }

            var itemType = (from it in DataContext.ItemTypes
                            where it.ItemTypeId == SelectedItemTypeId
                            select it);

            //Check the validity of the ItemType
            if (GetBL<Logic.Business.Inventory.InventoryBL>().GetProjectItemType(ProjectID, SelectedItemTypeId) == null)
            {
                if (!(SelectedItemTypeId == -1 && ShouldShowAllItemTypeText))
                    throw new Exception("Item Type not found");
            }

            //This decides whether
            ShouldShowAllItemTypeText = (GetBL<Logic.Business.Inventory.InventoryBL>().GetItemTypes(ProjectID).Count() > 1 && ShouldShowAllItemTypeText);
            string allitemTypeItemId = "-1";
            if (SelectedItemTypeId == -1 && ShouldShowAllItemTypeText)
                this.ddItemTypes.Items.Add(new ListItem("All Item Types", allitemTypeItemId));
            else
                this.ddItemTypes.Items.Add(new ListItem(itemType.FirstOrDefault().Name, SelectedItemTypeId.ToString(CultureInfo.InvariantCulture)));

            ddItemTypes.SelectedIndex = 0;
            if (itemTypeList.Count() > 1)
                this.ddItemTypes.AddItemGroup("Change to:");

            var listToBind = itemTypeList.Except(itemType).ToList<Data.ItemType>();

            foreach (var it in listToBind.OrderBy(it => it.Name))
            {
                this.ddItemTypes.Items.Add(new ListItem(it.Name, it.ItemTypeId.ToString(CultureInfo.InvariantCulture)));
            }
            if (ddItemTypes.Items.FindByValue(allitemTypeItemId) == null && ShouldShowAllItemTypeText)
                this.ddItemTypes.Items.Add(new ListItem("All Item Types", allitemTypeItemId.ToString(CultureInfo.InvariantCulture)));
        }

        #endregion Private Methods
    }
}