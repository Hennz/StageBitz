using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Web page for add new company.
    /// </summary>
    public partial class AddNewCompany : PageBase
    {
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
                //LoadBreadCrumbs();
                lblTitle.Text = "Create New Company";
                StringBuilder textBuilder = new StringBuilder();
                textBuilder.AppendFormat("<h2>{0}</h2> <br/>{1}", "Need a new Plan? Have a Promotional Code?", "Just choose the Project and Inventory level that suits you best!");
                lblMsg.Text = textBuilder.ToString();
            }

            paymentPackageSelector.InformParentToUpdate +=
                delegate(int projectPaymentTypeId, int inventoryPaymentPackageTypeId, int paymentDurationTypeCodeId, bool hasAllpackagesSelected)//Subsucribe to the Informparent to update
                {
                    paymentPackageSummary.ProjectPaymentPackageDetailId = projectPaymentTypeId;
                    paymentPackageSummary.InventoryPaymentPackageDetailId = inventoryPaymentPackageTypeId;
                    paymentPackageSummary.PaymentDurationTypeCodeId = paymentDurationTypeCodeId;
                    paymentPackageSummary.UpdatePaymentSummary();
                };

            paymentPackageSelector.InformParentToUpdate +=
                delegate(int projectPaymentTypeId, int inventoryPaymentPackageTypeId, int paymentDurationTypeCodeId, bool hasAllpackagesSelected)//Subsucribe to the Informparent to update
                {
                    paymentPackageSummary.ProjectPaymentPackageDetailId = projectPaymentTypeId;
                    paymentPackageSummary.InventoryPaymentPackageDetailId = inventoryPaymentPackageTypeId;
                    paymentPackageSummary.PaymentDurationTypeCodeId = paymentDurationTypeCodeId;
                    paymentPackageSummary.UpdatePaymentSummary();
                    Control container = wizard.FindControl("StepNavigationTemplateContainerID") as Control;
                    Button StepNextButton = container.FindControl("StepNextButton") as Button;
                    StepNextButton.Enabled = hasAllpackagesSelected;
                    StepNextButton.ToolTip = hasAllpackagesSelected ? "" : "Please select an option for both Project and Inventory";

                    Control finishContainer = wizard.FindControl("FinishNavigationTemplateContainerID") as Control;
                    Button FinishButton = finishContainer.FindControl("FinishButton") as Button;

                    FinishButton.Enabled = hasAllpackagesSelected;
                    FinishButton.ToolTip = hasAllpackagesSelected ? "" : "Please select an option for both Project and Inventory";

                    upnlWizard.Update();
                };

            paymentValidation.InformDiscountChanged += delegate(DiscountCodeUsage discountCodeUsage)
            {
                paymentPackageSummary.DiscountCodeUsage = discountCodeUsage;
                paymentPackageSummary.UpdateDiscountCode(discountCodeUsage);
                paymentPackageSummary.UpdatePaymentSummary();
            };
        }

        /// <summary>
        /// Handles the Click event of the FinishButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FinishButton_Click(object sender, EventArgs e)
        {
            PricePlanDetails pricePlanDetails = new PricePlanDetails
            {
                CompanyId = 0,
                DiscountCodeUsage = paymentPackageSummary.GetDiscountCodeUsage(),
                InventoryPaymentPackageTypeId = paymentPackageSelector.InventoryPaymentPackageTypeId,
                ProjectPaymentPackageTypeId = paymentPackageSelector.ProjectPaymentPackageTypeId,
                PaymentDurationCodeId = paymentPackageSelector.PaymentDurationTypeCodeId,
                TotalAmount = paymentPackageSummary.TotalAmount,
                TotalAmountForPeriod = paymentPackageSummary.TotalAmountForPeriod,
                TotalAmountWithEducationalPackage = paymentPackageSummary.TotalAmountWithEducationalPackage,
                DiscountCode = paymentPackageSummary.GetDiscountCode(),
                IsEducationalPackage = paymentPackageSummary.HasEducationalPackage()
            };

            paymentPackageSummary.UpdatePaymentSummary();//We need to call this. If not Summary panel does not load properly on postbacks
            paymentValidation.ValidatePackageConfigurations(pricePlanDetails);
        }

        /// <summary>
        /// Handles the Click event of the StartNextButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void StartNextButton_Click(object sender, EventArgs e)
        {
            if (this.IsValid)
            {
                TemplatedWizardStep step = (TemplatedWizardStep)wizard.WizardSteps[0];

                UserWeb.Controls.Company.CompanyDetails companyDetails = (UserWeb.Controls.Company.CompanyDetails)step.ContentTemplateContainer.FindControl("ucCompanyDetails");
                Data.Company company = companyDetails.SaveCompanyDetails(false, false);
                paymentValidation.NewCompany = company;
                UserWeb.Controls.Company.CompanyHeaderDetails companyHeaderDetails = (UserWeb.Controls.Company.CompanyHeaderDetails)step.ContentTemplateContainer.FindControl("companyHeaderDetails");
                paymentValidation.Media = companyHeaderDetails.GetMedia();
                paymentPackageSummary.UpdatePaymentSummary();
                lblTitle.Text = "Select Pricing Plans";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmYes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmYes_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        /// <summary>
        /// Handles the Click event of the FinishPreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FinishPreviousButton_Click(object sender, EventArgs e)
        {
            lblTitle.Text = "Create New Company";
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            popupCancelMsg.ShowPopup();
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.AddLink("Create new Company", null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}