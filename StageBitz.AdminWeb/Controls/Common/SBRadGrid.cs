using Telerik.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.AdminWeb.Controls.Common
{
    /// <summary>
    /// Base Rad Grid for StageBitz project.
    /// </summary>
    public class SBRadGrid : RadGrid
    {
        /// <summary>
        /// The show page size combo box view state key
        /// </summary>
        private const string ShowPageSizeComboBoxKey = "ShowPageSizeComboBox";

        /// <summary>
        /// Gets or sets a value indicating whether show or hide page size combo box.
        /// </summary>
        /// <value>
        /// <c>true</c> if [show page size combo box]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPageSizeComboBox
        {
            get
            {
                bool isShowComboBox = true;
                if (this.ViewState[ShowPageSizeComboBoxKey] != null)
                {
                    bool.TryParse(this.ViewState[ShowPageSizeComboBoxKey].ToString(), out isShowComboBox);
                }

                return isShowComboBox;
            }

            set
            {
                this.ViewState[ShowPageSizeComboBoxKey] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ItemDataBound" /> event.
        /// </summary>
        /// <param name="e">The <see cref="GridItemEventArgs" /> instance containing the event data.</param>
        protected override void OnItemDataBound(GridItemEventArgs e)
        {
            base.OnItemDataBound(e);
            if (e.Item is GridPagerItem)
            {
                RadComboBox pageSizeCombo = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                if (pageSizeCombo != null)
                {
                    if (this.ShowPageSizeComboBox)
                    {
                        pageSizeCombo.Items.Clear();
                        pageSizeCombo.Items.Add(new RadComboBoxItem("10"));
                        pageSizeCombo.FindItemByText("10").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                        pageSizeCombo.Items.Add(new RadComboBoxItem("20"));
                        pageSizeCombo.FindItemByText("20").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                        pageSizeCombo.Items.Add(new RadComboBoxItem("50"));
                        pageSizeCombo.FindItemByText("50").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                        pageSizeCombo.Items.Add(new RadComboBoxItem("100"));
                        pageSizeCombo.FindItemByText("100").Attributes.Add("ownerTableViewId", MasterTableView.ClientID);
                        pageSizeCombo.FindItemByText(e.Item.OwnerTableView.PageSize.ToString()).Selected = true;
                    }
                    else
                    {
                        pageSizeCombo.Visible = false;
                        Label changePageSizeLabel = (Label)e.Item.FindControl("ChangePageSizeLabel");
                        if (changePageSizeLabel != null)
                        {
                            changePageSizeLabel.Visible = false;
                        }
                    }
                }
            }
        }
    }
}