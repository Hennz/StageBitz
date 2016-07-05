using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.UserWeb.Common.Helpers;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for Item Brief Type Summary.
    /// </summary>
    public partial class ItemBriefTypeSummary : UserControlBase
    {
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
        /// Gets the row count.
        /// </summary>
        /// <value>
        /// The row count.
        /// </value>
        public int RowCount
        {
            get;
            private set;
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            var itemBriefTypes = GetBL<ItemBriefBL>().GetItemBriefTypeSummary(ProjectID);
            rptItemBriefTypes.DataSource = itemBriefTypes;
            rptItemBriefTypes.DataBind();
            this.RowCount = itemBriefTypes.Count();
        }

        #endregion Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the ItemDataBound event of the rptItemBriefTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptItemBriefTypes_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                ItemTypeSummary itemTypeSummary = (ItemTypeSummary)e.Item.DataItem;
                Label lblItemTypeName = (Label)e.Item.FindControl("lblItemTypeName");
                Label lblItemTypeSummary = (Label)e.Item.FindControl("lblItemTypeSummary");
                if (lblItemTypeName != null && lblItemTypeSummary != null)
                {
                    lblItemTypeName.Text = itemTypeSummary.ItemTypeName;
                    lblItemTypeSummary.Text = GetItemTypeSummaryText(itemTypeSummary);
                }
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Gets the item type summary text.
        /// </summary>
        /// <param name="itemTypeSummary">The item type summary.</param>
        /// <returns></returns>
        private string GetItemTypeSummaryText(ItemTypeSummary itemTypeSummary)
        {
            return string.Format("{0} Completed, {1} In Progress, {2} Not Started", itemTypeSummary.CompletedItemCount, itemTypeSummary.InProgressItemCount, itemTypeSummary.NotStartedItemCount);
        }

        #endregion Private Methods
    }
}