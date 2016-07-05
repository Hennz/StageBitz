using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for billing plan monitor
    /// </summary>
    public partial class PlanMonitor : UserControlBase
    {
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
                if (ViewState["companyid"] == null)
                {
                    ViewState["companyid"] = 0;
                }

                return (int)ViewState["companyid"];
            }
            set
            {
                ViewState["companyid"] = value;
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnViewPricingPlan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewPricingPlan_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("~/Company/CompanyPricingPlans.aspx?companyid={0}", CompanyId));
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
            if (companyPaymentPackage != null)
            {
                //This does not get NULL. 0 values as default
                CompanyCurrentUsage companyCurrentUsage = this.GetBL<FinanceBL>().GetCompanyCurrentUsage(CompanyId, null);
                CompanyPaymentPackage futurePackage = this.GetBL<FinanceBL>().GetLatestRequestForTheCompany(CompanyId);

                ProjectPaymentPackageDetails projectPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(companyPaymentPackage.ProjectPaymentPackageTypeId);
                InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(companyPaymentPackage.InventoryPaymentPackageTypeId);

                litActiveProjects.Text = string.Concat(projectPackageDetails.ProjectCount, projectPackageDetails.ProjectCount == 1 ? " Active Project" : " Active Projects");
                litCurrentProjectCount.Text = companyCurrentUsage.ProjectCount.ToString();

                if (this.GetBL<FinanceBL>().IsEducationalCompany(companyPaymentPackage))
                {
                    litActiveUsers.Text = "Unlimited Users";
                }
                else
                {
                    litActiveUsers.Text = string.Concat(projectPackageDetails.HeadCount, projectPackageDetails.HeadCount == 1 ? " Active User" : " Active Users");
                }

                litCurrentUserCount.Text = companyCurrentUsage.UserCount.ToString();
                litInventoryItems.Text = string.Concat(inventoryPaymentPackageDetails.ItemCount == null ? "Unlimited " : inventoryPaymentPackageDetails.ItemCount.ToString(), inventoryPaymentPackageDetails.ItemCount == 1 ? " Inventory Item" : " Inventory Items");
                litInvCurrentCount.Text = companyCurrentUsage.InventoryCount.ToString();
                divDefaultText.Visible = false;
            }
            else
            {
                //Show the default message
                tblPlanMonitor.Visible = false;
                if (this.GetBL<CompanyBL>().IsFreeTrialCompany(this.CompanyId))
                {
                    paraFreeTrailIndication.Visible = true;
                }
            }
            if (Request.Browser.Type.Contains("Firefox"))  //firefox has a margin issue
            {
                btnViewPricingPlan.Style.Add("margin", "0px");
            }
        }
    }
}