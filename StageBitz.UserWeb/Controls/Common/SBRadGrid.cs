using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// StageBitz specific radGrid. (changed the pagesizes)
    /// </summary>
    public class SBRadGrid : RadGrid
    {
        /// <summary>
        /// Raises the <see cref="E:ItemDataBound" /> event.
        /// </summary>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected override void OnItemDataBound(GridItemEventArgs e)
        {
            base.OnItemDataBound(e);
            if (e.Item is GridPagerItem)
            {
                RadComboBox PageSizeCombo = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                PageSizeCombo.Items.Clear();
                PageSizeCombo.Items.Add(new RadComboBoxItem("10"));
                PageSizeCombo.FindItemByText("10").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                PageSizeCombo.Items.Add(new RadComboBoxItem("20"));
                PageSizeCombo.FindItemByText("20").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                PageSizeCombo.Items.Add(new RadComboBoxItem("50"));
                PageSizeCombo.FindItemByText("50").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                PageSizeCombo.Items.Add(new RadComboBoxItem("100"));
                PageSizeCombo.FindItemByText("100").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                PageSizeCombo.FindItemByText(e.Item.OwnerTableView.PageSize.ToString()).Selected = true;
            }
        }
    }
}