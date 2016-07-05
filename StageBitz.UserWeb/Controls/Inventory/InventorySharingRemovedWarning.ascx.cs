using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for inventory sharing removed warnings.
    /// </summary>
    public partial class InventorySharingRemovedWarning : UserControlBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Shows the inventory sharing removed.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="removedCompanyId">The removed company identifier.</param>
        public void ShowInventorySharingRemoved(int companyId, int removedCompanyId)
        {
            btnDoneInventorySharingRemoved.CommandArgument = companyId.ToString();
            ltrRemovedCompanyName.Text = this.GetBL<CompanyBL>().GetCompany(removedCompanyId).CompanyName;
            popupInventorySharingRemoved.ShowPopup();
        }

        /// <summary>
        /// Handles the Click event of the btnDoneInventorySharingRemoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDoneInventorySharingRemoved_Click(object sender, EventArgs e)
        {
            int companyId = int.Parse(btnDoneInventorySharingRemoved.CommandArgument);
            Response.Redirect(string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", companyId));
        }
    }
}