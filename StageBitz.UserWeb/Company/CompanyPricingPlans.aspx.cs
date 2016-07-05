using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Text;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Company pricing plans page.
    /// </summary>
    public partial class CompanyPricingPlans : PageBase
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

        /// <summary>
        /// Gets or sets a value indicating whether this page is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this page is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] != null)
                {
                    return (bool)ViewState["IsReadOnly"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">You do not have administrator rights to view this page.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check for permissions
            if (!IsPostBack)
            {
                if (!Support.IsCompanyAdministrator(CompanyId))
                {
                    throw new ApplicationException("You do not have administrator rights to view this page.");
                }

                sbCompanyWarningDisplay.CompanyID = CompanyId;
                sbCompanyWarningDisplay.LoadData();
                sbFutureRequestNotificationMessage.CompanyId = CompanyId;
                sbFutureRequestNotificationMessage.LoadData();
                IsReadOnly = Support.IsReadOnlyRightsForPricingPlanPage(CompanyId);
                paymentPackageSummary.IsReadOnly = IsReadOnly;
                paymentPackageSelector.IsReadOnly = IsReadOnly;
                btnNext.Enabled = !IsReadOnly;

                string companyName = Support.GetCompanyNameById(CompanyId);
                DisplayTitle = string.Format("Pricing Plans");
                LoadBreadCrumbs(companyName);

                //Display the Header Display Text
                //Check whether a Company has a payment package
                CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                StringBuilder textBuilder = new StringBuilder();
                if (companyPaymentPackage == null)
                {
                    textBuilder.AppendFormat("<h2>{0}</h2> <br/>{1}", "Thanks for sticking around!", "To continue with StageBitz, please choose your Project level,<br/> and then your Inventory level.");
                }
                else
                {
                    textBuilder.AppendFormat("<h2>{0}</h2> <br/>{1}", "Need a new Plan? Have a Promotional Code?", "Just choose the Project and Inventory level that suits you best!");
                    paymentPackageSummary.IsEducationalPackage = this.GetBL<FinanceBL>().IsEducationalCompany(companyPaymentPackage);
                }

                if (GetBL<CompanyBL>().IsFreeTrialCompany(this.CompanyId))
                {
                    string freeTrailDurationText = string.Empty;
                    int freeTrailDays = int.Parse(Support.GetSystemValue("FreeTrialDays"));
                    if (freeTrailDays % 7 > 0)
                    {
                        freeTrailDurationText = string.Format("{0} days", freeTrailDays);
                    }
                    else
                    {
                        int freeTrailWeeks = freeTrailDays / 7;
                        freeTrailDurationText = string.Format("{0} weeks", freeTrailWeeks);
                    }

                    textBuilder.AppendFormat("<br /><br />{0}{1}{2}", "If you’re still in your Free Trial, you will not be charged until the <br/>end of the ", freeTrailDurationText, " trial period.");
                }

                lblMsg.Text = textBuilder.ToString();
                btnNext.Enabled = (paymentPackageSelector.InventoryPaymentPackageTypeId != 0 && paymentPackageSelector.ProjectPaymentPackageTypeId != 0) && !IsReadOnly;
                paymentPackageSummary.CompanyId = CompanyId;

                lnkCreateNewProject.CompanyId = this.CompanyId;
                lnkCreateNewProject.LoadData();
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);

                bool isReadOnly = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);
                spnCreateNewProject.Visible = !isReadOnly;
            }

            paymentPackageSelector.InformParentToUpdate += delegate(int projectPaymentTypeId, int inventoryPaymentPackageTypeId, int paymentDurationTypeCodeId, bool hasAllpackagesSelected)//Subsucribe to the Informparent to update
            {
                paymentPackageSummary.ProjectPaymentPackageDetailId = projectPaymentTypeId;
                paymentPackageSummary.InventoryPaymentPackageDetailId = inventoryPaymentPackageTypeId;
                paymentPackageSummary.PaymentDurationTypeCodeId = paymentDurationTypeCodeId;
                paymentPackageSummary.UpdatePaymentSummary();
                btnNext.Enabled = hasAllpackagesSelected && !IsReadOnly;
                btnNext.ToolTip = IsReadOnly || hasAllpackagesSelected ? string.Empty : "Please select an option for both Project and Inventory";
                upnlPaymentPackage.Update();
            };

            paymentValidation.InformParentToUpdateValidation += delegate()
            {
                //paymentPackageSelector.LoadData();
                Response.Redirect(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId));
            };

            paymentValidation.InformDiscountChanged += delegate(DiscountCodeUsage discountCodeUsage)
            {
                paymentPackageSummary.DiscountCodeUsage = discountCodeUsage;
                paymentPackageSummary.UpdateDiscountCode(discountCodeUsage);
                paymentPackageSummary.UpdatePaymentSummary();
            };
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click(object sender, EventArgs e)
        {
            PricePlanDetails pricePlanDetails = new PricePlanDetails
            {
                CompanyId = this.CompanyId,
                DiscountCodeUsage = paymentPackageSummary.DiscountCodeUsage,
                DiscountCode = paymentPackageSummary.DiscountCodeUsage != null ? paymentPackageSummary.DiscountCodeUsage.DiscountCode : null,
                InventoryPaymentPackageTypeId = paymentPackageSelector.InventoryPaymentPackageTypeId,
                ProjectPaymentPackageTypeId = paymentPackageSelector.ProjectPaymentPackageTypeId,
                PaymentDurationCodeId = paymentPackageSelector.PaymentDurationTypeCodeId,
                TotalAmount = paymentPackageSummary.TotalAmount,
                TotalAmountForPeriod = paymentPackageSummary.TotalAmountForPeriod,
                TotalAmountWithEducationalPackage = paymentPackageSummary.TotalAmountWithEducationalPackage,
                IsEducationalPackage = paymentPackageSummary.HasEducationalPackage()
            };

            paymentValidation.ResetPopupSelections();
            paymentValidation.ValidatePackageConfigurations(pricePlanDetails);
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId));
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
            bc.AddLink("Billing", string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}