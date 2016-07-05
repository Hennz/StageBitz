using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for project item briefs
    /// </summary>
    public partial class ProjectItemBrief : UserControlBase
    {
        #region public properties

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

        #endregion public properties

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
                //Handle security. Only Project Admin can add Itemtypes
                divItemType.Visible = Support.HasExclusiveRightsForProject(ProjectID);
                LoadItemBriefDetails();
                LoadItemTypes();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvProjectItemTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvProjectItemTypes_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic itemBriefType = e.Item.DataItem as dynamic;

                System.Web.UI.HtmlControls.HtmlAnchor linkItemBrief = (System.Web.UI.HtmlControls.HtmlAnchor)e.Item.FindControl("linkItemBrief");

                //Set Links
                if (linkItemBrief != null)
                {
                    linkItemBrief.HRef = ResolveUrl(string.Format("~/ItemBrief/ItemBriefList.aspx?ProjectId={0}&ItemTypeId={1}", ProjectID, itemBriefType.ItemTypeId));
                }

                Literal litItemTypeName = (Literal)e.Item.FindControl("litItemTypeName");
                litItemTypeName.Text = itemBriefType.ItemTypeName;
                Literal litItems = (Literal)e.Item.FindControl("litItems");
                Literal litCompleted = (Literal)e.Item.FindControl("litCompleted");
                Literal litInProgress = (Literal)e.Item.FindControl("litInProgress");
                Literal litNotstarted = (Literal)e.Item.FindControl("litNotstarted");
                LinkButton button = (LinkButton)e.Item.FindControl("btnRemoveItemType");

                //Validate data before assigning
                int itemCount = (itemBriefType.ItemCount != null) ? itemBriefType.ItemCount : 0;
                int completeItemCount = (itemBriefType.CompletedItemCount != null) ? itemBriefType.CompletedItemCount : 0;
                int inProgressItemCount = (itemBriefType.InProgressItemCount != null) ? itemBriefType.InProgressItemCount : 0;
                int notStartedItemCount = (itemBriefType.NotStartedItemCount != null) ? itemBriefType.NotStartedItemCount : 0;

                if (itemCount == 1)
                    litItems.Text = string.Format("{0} {1} Brief", itemCount, itemBriefType.ItemTypeName);
                else
                    litItems.Text = string.Format("{0} {1} Briefs", itemCount, itemBriefType.ItemTypeName);

                bool isItemRemovable = GetBL<ProjectBL>().CheckRemovabilityWithProjectStatus(ProjectID);

                button.CommandArgument = itemBriefType.ItemTypeId.ToString();
                if (!isItemRemovable || !Support.HasExclusiveRightsForProject(ProjectID))
                    button.Visible = false;
                else
                {
                    if (itemCount > 0)
                    {
                        button.Enabled = false;
                        button.CssClass = "btnRemoveItemTypesDisabled";
                        button.ToolTip = "You've already created Item Briefs for this Item Type. Please move these to another Item Type or delete them, then try again.";
                    }
                    else
                    {
                        button.Enabled = true;
                        button.CssClass = "btnRemoveItemTypes";
                    }
                }
                litCompleted.Text = (litCompleted != null) ? string.Format("{0} Completed", completeItemCount) : string.Empty;
                litInProgress.Text = (litInProgress != null) ? string.Format("{0} In Progress", inProgressItemCount) : string.Empty;
                litNotstarted.Text = (litNotstarted != null) ? string.Format("{0} Not Started", notStartedItemCount) : string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConcurrentConfirmation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConcurrentConfirmation_Click(object sender, EventArgs e)
        {
            LoadItemTypes();
            LoadItemBriefDetails();
            upnlItemTypes.Update();
            ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
            popupItemBriefConcurrentScenario.HidePopup();
        }

        /// <summary>
        /// Handles the Click event of the btnItemAlreadyHasBrief control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnItemAlreadyHasBrief_Click(object sender, EventArgs e)
        {
            LoadItemTypes();
            LoadItemBriefDetails();
            upnlItemTypes.Update();
            ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
            popupItemAlreadyHasBrief.HidePopup();
        }

        /// <summary>
        /// Handles the Click event of the btnItemDeleteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnItemDeleteCancel_Click(object sender, EventArgs e)
        {
            popupItemDeleteConfirmation.HidePopup();
            ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
        }

        /// <summary>
        /// Handles the Click event of the btnItemDeleteConfirmation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnItemDeleteConfirmation_Click(object sender, EventArgs e)
        {
            int itemTypeId = int.Parse(btnItemDeleteConfirmation.CommandArgument);
            var itemToRmove = (from pit in DataContext.ProjectItemTypes
                               where pit.ProjectId == ProjectID && pit.ItemTypeId == itemTypeId
                               select pit).FirstOrDefault();

            DataContext.ProjectItemTypes.DeleteObject(itemToRmove);
            DataContext.SaveChanges();
            LoadItemTypes();
            LoadItemBriefDetails();
            upnlItemTypes.Update();
            ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
            popupItemDeleteConfirmation.HidePopup();
        }

        /// <summary>
        /// Handles the Clicked event of the btnRemoveItemType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveItemType_Clicked(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int itemTypeId = int.Parse(btn.CommandArgument);
            ItemTypeSummary itemBriefTypes = GetBL<ItemBriefBL>().GetItemBriefCountByItemTypeId(itemTypeId, ProjectID);
            int itemCount = (itemBriefTypes != null) ? itemBriefTypes.ItemCount : 0;
            if (itemCount > 0)
            {
                ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
                popupItemAlreadyHasBrief.ShowPopup();
            }
            else
            {
                bool alreadyRemoved = CheckForConcurrency(itemTypeId);
                if (!alreadyRemoved)
                {
                    ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
                    btnItemDeleteConfirmation.CommandArgument = btn.CommandArgument;
                    popupItemDeleteConfirmation.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvItemTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvItemTypes_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (e.CommandName == "AddItemType")
                {
                    //Get the parameters about which control to add to the page
                    int itemTypeId;
                    int.TryParse(e.CommandArgument.ToString(), out itemTypeId);

                    //check whether ItemType is not already included
                    if (itemTypeId > 0 && DataContext.ProjectItemTypes.Where(pit => pit.ItemTypeId == itemTypeId && pit.ProjectId == ProjectID).FirstOrDefault() == null)
                    {
                        Data.ProjectItemType projectItemType = new Data.ProjectItemType();
                        projectItemType.ItemTypeId = itemTypeId;
                        projectItemType.ProjectId = ProjectID;
                        projectItemType.CreatedDate = projectItemType.LastUpdatedDate = Now;
                        projectItemType.CreatedByUserId = projectItemType.LastUpdatedByUserId = UserID;
                        projectItemType.IsActive = true;
                        DataContext.ProjectItemTypes.AddObject(projectItemType);
                        DataContext.SaveChanges();
                    }
                    LoadItemTypes();
                    LoadItemBriefDetails();
                    upnlItemTypes.Update();
                    ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
                }
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the item brief details for a Project.
        /// </summary>
        private void LoadItemBriefDetails()
        {
            // This needs to be change after introducing the security module. Then onwards ItemBriefs will be what a user is authorized to see.
            // But for now it will show ItemBriefs belong to a project.
            var itemBriefTypes = GetBL<ItemBriefBL>().GetItemBriefTypeSummary(ProjectID);

            lvProjectItemTypes.DataSource = itemBriefTypes;
            lvProjectItemTypes.DataBind();
            bool hasNoItemBriefs = (itemBriefTypes.Count() == 0);
            divNoItemTypes.Visible = hasNoItemBriefs;
            divListViewItemTypes.Visible = !hasNoItemBriefs;
        }

        /// <summary>
        /// Checks for concurrency.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        private bool CheckForConcurrency(int itemTypeId)
        {
            var itemToRemove = (from pit in DataContext.ProjectItemTypes
                                where pit.ProjectId == ProjectID && pit.ItemTypeId == itemTypeId
                                select pit).FirstOrDefault();
            if (itemToRemove == null)
            {
                //display popup for concurrent scenario(Item already deleted).add reloadings here
                ScriptManager.RegisterStartupScript(UpdatePanelItemTypesDropDown, GetType(), "slideUpItemTypes", "slideUpItemTypes();", true);
                popupItemBriefConcurrentScenario.ShowPopup();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads the Item types.
        /// </summary>
        private void LoadItemTypes()
        {
            //Get the ItemTypes excluding the ItemTypes already added.
            lvItemTypes.DataSource = DataContext.ItemTypes.Except(from pit in DataContext.ProjectItemTypes
                                                                  where pit.ProjectId == ProjectID
                                                                  select pit.ItemType).OrderBy(it => it.Name).ToList<Data.ItemType>();
            lvItemTypes.DataBind();
        }

        #endregion Private Methods
    }
}