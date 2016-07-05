using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Text;
using System.Web.Configuration;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for payment package summary.
    /// </summary>
    public partial class PaymentPackageSummary : UserControlBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether this package is educational package.
        /// </summary>
        /// <value>
        /// <c>true</c> if this package is educational package; otherwise, <c>false</c>.
        /// </value>
        public bool IsEducationalPackage
        {
            get
            {
                if (ViewState["IsEducationalPackage"] == null)
                {
                    ViewState["IsEducationalPackage"] = false;
                }

                return (bool)ViewState["IsEducationalPackage"];
            }
            set
            {
                ViewState["IsEducationalPackage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the culture.
        /// </summary>
        /// <value>
        /// The name of the culture.
        /// </value>
        public String CultureName
        {
            get
            {
                if (ViewState["CultureName"] == null)
                {
                    ViewState["CultureName"] = Now;
                }

                return (string)ViewState["CultureName"];
            }
            set
            {
                ViewState["CultureName"] = value;
            }
        }

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
        /// Gets or sets the project payment package detail identifier.
        /// </summary>
        /// <value>
        /// The project payment package detail identifier.
        /// </value>
        public int ProjectPaymentPackageDetailId
        {
            get
            {
                if (ViewState["projectPaymentPackageDetailId"] == null)
                {
                    ViewState["projectPaymentPackageDetailId"] = 0;
                }

                return (int)ViewState["projectPaymentPackageDetailId"];
            }
            set
            {
                ViewState["projectPaymentPackageDetailId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the inventory payment package detail identifier.
        /// </summary>
        /// <value>
        /// The inventory payment package detail identifier.
        /// </value>
        public int InventoryPaymentPackageDetailId
        {
            get
            {
                if (ViewState["inventoryPaymentPackageDetailId"] == null)
                {
                    ViewState["inventoryPaymentPackageDetailId"] = 0;
                }

                return (int)ViewState["inventoryPaymentPackageDetailId"];
            }
            set
            {
                ViewState["inventoryPaymentPackageDetailId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment duration type code identifier.
        /// </summary>
        /// <value>
        /// The payment duration type code identifier.
        /// </value>
        public int PaymentDurationTypeCodeId
        {
            get
            {
                if (ViewState["paymentDurationTypeCodeId"] == null)
                {
                    ViewState["paymentDurationTypeCodeId"] = 0;
                }

                return (int)ViewState["paymentDurationTypeCodeId"];
            }
            set
            {
                ViewState["paymentDurationTypeCodeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount
        {
            get
            {
                if (ViewState["TotalAmount"] == null)
                {
                    ViewState["TotalAmount"] = decimal.Zero;
                }

                return (decimal)ViewState["TotalAmount"];
            }
            set
            {
                ViewState["TotalAmount"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the total amount with educational package.
        /// </summary>
        /// <value>
        /// The total amount with educational package.
        /// </value>
        public decimal TotalAmountWithEducationalPackage
        {
            get
            {
                if (ViewState["TotalAmountWithEducationalPackage"] == null)
                {
                    ViewState["TotalAmountWithEducationalPackage"] = 0;
                }

                return (decimal)ViewState["TotalAmountWithEducationalPackage"];
            }
            set
            {
                ViewState["TotalAmountWithEducationalPackage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the total amount for period.
        /// </summary>
        /// <value>
        /// The total amount for period.
        /// </value>
        public decimal TotalAmountForPeriod
        {
            get
            {
                if (ViewState["TotalAmountForPeriod"] == null)
                {
                    ViewState["TotalAmountForPeriod"] = 0;
                }

                return (decimal)ViewState["TotalAmountForPeriod"];
            }
            set
            {
                ViewState["TotalAmountForPeriod"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the discount code usage.
        /// </summary>
        /// <value>
        /// The discount code usage.
        /// </value>
        public DiscountCodeUsage DiscountCodeUsage
        {
            get
            {
                if (ViewState["DiscountCodeUsage"] == null)
                {
                    return null;
                }
                return (DiscountCodeUsage)ViewState["DiscountCodeUsage"];
            }
            set
            {
                ViewState["DiscountCodeUsage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
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

        #endregion Public Properties

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
                if (Request.Browser.MSDomVersion.Major == 0) // Non IE Browser?. (Firefox has an issue of caching the controller values on page refresh. These lines were added to fix it.
                {
                    Response.Cache.SetNoStore(); // No client side cashing for non IE browsers
                }

                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                CultureName = globalizationSection.Culture;

                decimal discount = decimal.Parse(Utils.GetSystemValue("EducationalDiscount"));

                litEduDiscount.Text = String.Format("{0:0%}", discount / 100);
                setUpDiscountCode.CompanyId = CompanyId;
                setUpDiscountCode.LoadDiscountCodes();

                if (IsReadOnly)
                {
                    setUpDiscountCode.SetDisable();
                    chkIsCollege.Enabled = false;
                }

                chkIsCollege.Checked = IsEducationalPackage;
            }

            setUpDiscountCode.InformParentToUpdateDiscountUsage += delegate()//Subsucribe to the InformParentToUpdateDiscountUsage to update
            {
                UpdatePaymentSummary();
            };
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Determines whether company has educational package.
        /// </summary>
        /// <returns></returns>
        public bool HasEducationalPackage()
        {
            return chkIsCollege.Checked;
        }

        /// <summary>
        /// Updates the payment summary.
        /// </summary>
        public void UpdatePaymentSummary()
        {
            int anualPaymentPackageDurationCodeId = Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId;

            ProjectPaymentPackageDetails projectPaymentPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(ProjectPaymentPackageDetailId);
            StringBuilder textCharhgesProjBuilder = new StringBuilder();

            if (projectPaymentPackageDetails != null)
            {
                litProjectLevelPackage.Text = projectPaymentPackageDetails.PackageDisplayName;
                textCharhgesProjBuilder.AppendFormat("<b>{0}</b><I>/{1}</I>", Support.FormatCurrency(this.GetBL<FinanceBL>().CalculatethePackageAmountByPeriod(Utils.GetCodeByValue("PaymentPackageType", "PROJECT").CodeId, ProjectPaymentPackageDetailId, PaymentDurationTypeCodeId), CultureName), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                litProjectLevelAmount.Text = textCharhgesProjBuilder.ToString();
            }
            else
            {
                if (CompanyId > 0 && GetBL<CompanyBL>().IsFreeTrialCompany(CompanyId))
                    litProjectLevelPackage.Text = Utils.GetSystemValue("DefaultPackageName");
                else
                    litProjectLevelPackage.Text = "N/A";

                textCharhgesProjBuilder.AppendFormat("<b>{0}</b><I>/{1}</I>", Support.FormatCurrency(0, CultureName), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                litProjectLevelAmount.Text = textCharhgesProjBuilder.ToString();
            }

            InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(InventoryPaymentPackageDetailId);
            StringBuilder textCharhgesInvBuilder = new StringBuilder();
            if (inventoryPaymentPackageDetails != null)
            {
                litInventoryLevelPackage.Text = inventoryPaymentPackageDetails.PackageDisplayName;
                textCharhgesInvBuilder.AppendFormat("<b>{0}</b><I>/{1}</I>", Support.FormatCurrency(this.GetBL<FinanceBL>().CalculatethePackageAmountByPeriod(Utils.GetCodeByValue("PaymentPackageType", "INVENTORY").CodeId, InventoryPaymentPackageDetailId, PaymentDurationTypeCodeId), CultureName), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                litInventoryLevelAmount.Text = textCharhgesInvBuilder.ToString();
            }
            else
            {
                if (CompanyId > 0 && GetBL<CompanyBL>().IsFreeTrialCompany(CompanyId))
                    litInventoryLevelPackage.Text = Utils.GetSystemValue("DefaultPackageName");
                else
                    litInventoryLevelPackage.Text = "N/A";

                textCharhgesInvBuilder.AppendFormat("<b>{0}</b><I>/{1}</I>", Support.FormatCurrency(0, CultureName), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                litInventoryLevelAmount.Text = textCharhgesInvBuilder.ToString();
            }

            //Total Amount With No Discount or Educational Package offer
            //decimal totalAmountForthePeriod = this.GetBL<FinanceBL>().CalculateALLPackageAmountsByPeriod(projectPaymentPackageDetailId, inventoryPaymentPackageDetailId, paymentDurationTypeCodeId);
            TotalAmountForPeriod = this.GetBL<FinanceBL>().CalculateALLPackageAmountsByPeriod(ProjectPaymentPackageDetailId, InventoryPaymentPackageDetailId, PaymentDurationTypeCodeId);
            //Just initialization
            decimal totalAmountWithPromotionalCode = TotalAmountForPeriod;

            setUpDiscountCode.LoadDiscountCodes();
            //Check if there is a Promotional Code. This is to assign to the hidden field
            DiscountCodeUsage = GetDiscountCodeUsage();
            if (DiscountCodeUsage != null)
            {
                DateTime packageStartDate = Utils.Today;//For Normal company, the package start date should be Today.

                if (this.GetBL<CompanyBL>().IsFreeTrialCompany(CompanyId))
                {
                    //For Free Trial Companies, Start Date should be FT end Date + 1
                    packageStartDate = this.GetBL<CompanyBL>().GetFreeTrialProjectEndDate(CompanyId).AddDays(1);
                }
                else
                {
                    packageStartDate = Utils.Today;
                }
                DateTime endDate = PaymentDurationTypeCodeId == anualPaymentPackageDurationCodeId ? packageStartDate.AddYears(1) : packageStartDate.AddMonths(1);
                totalAmountWithPromotionalCode = this.GetBL<FinanceBL>().GetDiscountedAmount(CompanyId, TotalAmountForPeriod, PaymentDurationTypeCodeId, DiscountCodeUsage, packageStartDate, endDate);
                litDiscountMsg.Text = "Promotional Code: ";
                lblDiscountedAmountText.Text = string.Format("(Including promotional discount of {0}) </br>", Support.FormatCurrency(TotalAmountForPeriod - totalAmountWithPromotionalCode, CultureName));

                lblTotalAmountAfterPromotion.Text = string.Concat(Support.FormatCurrency(TotalAmountForPeriod, CultureName), "/", Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description, " after discount expires on ", Support.FormatDate(DiscountCodeUsage.EndDate));
            }
            else
            {
                litDiscountMsg.Text = " Do you have a Promotional Code?";
                lblTotalAmountAfterPromotion.Text = string.Empty;
                lblDiscountedAmountText.Text = string.Empty;
            }
            //Assign to the hidden field
            hdnTotalAfterDiscount.Text = string.Concat(Support.FormatCurrency(totalAmountWithPromotionalCode, CultureName), "/", Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);

            //If there is an Educational discount, what would be the final amount
            decimal totalAmountWithEducationalPackage = TotalAmountForPeriod;
            decimal educationalDiscount = decimal.Parse(Utils.GetSystemValue("EducationalDiscount"));
            totalAmountWithEducationalPackage = TotalAmountForPeriod * (100 - educationalDiscount) / 100;

            TotalAmountWithEducationalPackage = totalAmountWithEducationalPackage;

            //Assign to the public variables
            TotalAmount = TotalAmountForPeriod;

            //If there is a promotional code or educational discount then get the total out of it.
            if (chkIsCollege.Checked)
            {
                TotalAmountForPeriod = totalAmountWithEducationalPackage;
                lblTotalAmountAfterPromotion.Style.Add("Display", "None"); //Hide the Promotional Text
                lblDiscountedAmountText.Style.Add("Display", "None");
                trPromotionalCode.Style.Add("Display", "None"); //Hide "Do you have a Promotional Code?"
                divEducationalDiscount.Style.Add("Display", " ");
                //Assign to the public variables
            }
            else
            {
                if (DiscountCodeUsage != null)
                {
                    lblTotalAmountAfterPromotion.Style.Add("Display", " ");
                    lblDiscountedAmountText.Style.Add("Display", " ");
                }
                else
                {
                    lblTotalAmountAfterPromotion.Style.Add("Display", "None");
                    lblDiscountedAmountText.Style.Add("Display", "None");
                }

                trPromotionalCode.Style.Add("Display", " ");
                TotalAmountForPeriod = totalAmountWithPromotionalCode;
                divEducationalDiscount.Style.Add("Display", "None");//Hide the Educational Discount text
            }

            hdnTotalAfterEducationalPackage.Text = string.Concat(Support.FormatCurrency(totalAmountWithEducationalPackage, CultureName), "/", Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
            lblTotaltoPay.Text = string.Concat(Support.FormatCurrency(TotalAmountForPeriod, CultureName), "/", Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);

            upnlSummary.Update();
        }

        /// <summary>
        /// Updates the discount code.
        /// </summary>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        public void UpdateDiscountCode(DiscountCodeUsage discountCodeUsage)
        {
            setUpDiscountCode.DiscountCodeUsage = discountCodeUsage;
            setUpDiscountCode.DiscountCode = discountCodeUsage != null ? discountCodeUsage.DiscountCode : null;
            setUpDiscountCode.LoadDiscountCodes();
        }

        /// <summary>
        /// Sets the read only discount code.
        /// </summary>
        public void SetReadOnlyDiscountCode()
        {
            divCollegeSelection.Visible = false;
            divEducationalDiscount.Visible = false;
            trPromotionalCode.Visible = false;
            setUpDiscountCode.SetReadOnly();
        }

        /// <summary>
        /// Gets the discount code.
        /// </summary>
        /// <returns></returns>
        public DiscountCode GetDiscountCode()
        {
            return setUpDiscountCode.DiscountCode;
        }

        /// <summary>
        /// Gets the discount code usage.
        /// </summary>
        /// <returns></returns>
        public DiscountCodeUsage GetDiscountCodeUsage()
        {
            return setUpDiscountCode.DiscountCodeUsage;
        }

        #endregion Methods
    }
}