using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Web page for company finance history.
    /// </summary>
    public partial class CompanyFinanceHistory : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        private int CompanyId
        {
            get
            {
                if (ViewState["companyid"] == null)
                {
                    int companyId = 0;

                    if (Request["companyid"] != null)
                    {
                        int.TryParse(Request["companyid"], out companyId);
                    }

                    ViewState["companyid"] = companyId;
                }

                return (int)ViewState["companyid"];
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lnkCompanyBilling.HRef = string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId);
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);
                lnkCreateNewProject.CompanyId = this.CompanyId;
                lnkCreateNewProject.LoadData();
                if (this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId))
                {
                    spanCreateNewProject.Visible = false;
                }

                string companyName = Support.GetCompanyNameById(CompanyId);
                LoadBreadCrumbs(companyName);

                transactionSearch.RelatedId = CompanyId;
                planMonitor.CompanyId = CompanyId;
                transactionSearch.LoadData();
                sbCompanyWarningDisplay.CompanyID = CompanyId;
                sbCompanyWarningDisplay.LoadData();
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        private void LoadBreadCrumbs(string companyName)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.AddLink(companyName, string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", CompanyId));
            bc.AddLink("Billing", string.Format("~/Company/CompanyFinancialDetails.aspx?CompanyId={0}", CompanyId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}