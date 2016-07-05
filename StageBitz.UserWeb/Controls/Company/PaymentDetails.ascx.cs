using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// Delegate for term accepted event.
    /// </summary>
    /// <param name="enabled">if set to <c>true</c> [enabled].</param>
    /// <param name="toolTip">The tool tip.</param>
    public delegate void TermsAccepted(bool enabled, string toolTip);

    /// <summary>
    /// Delegate for authorization enabled event.
    /// </summary>
    /// <param name="enabled">if set to <c>true</c> [enabled].</param>
    public delegate void AuthorizationEnabled(bool enabled);

    /// <summary>
    /// Payment details control for pricing plans.
    /// </summary>
    public partial class PaymentDetails : UserControlBase
    {
        #region Classes/Enums/Events

        /// <summary>
        /// Occurs when [terms check boxes checked].
        /// </summary>
        public event TermsAccepted TermsAccepted;

        public event AuthorizationEnabled AuthorizationEnabled;

        /// <summary>
        /// Display modes of the Payment Details control.
        /// </summary>
        public enum DisplayMode
        {
            CreditCard = 1,
            Invoice = 2
        }

        #endregion Classes/Enums/Events

        #region Private Fields

        /// <summary>
        /// The term accept tool tip
        /// </summary>
        ///
        private const string TermAcceptToolTip = "Please accept the Terms and Conditions.";

        /// <summary>
        /// The term and price accept tool tip
        /// </summary>
        private const string TermAndPriceAcceptToolTip = "Please authorize the amount and accept the Terms and Conditions.";

        /// <summary>
        /// The credit card payment code
        /// </summary>
        private Code CreditCardPaymentCode = Support.GetCodeByValue("PaymentMethod", "CREDITCARD");

        /// <summary>
        /// The invoice payment code
        /// </summary>
        private Code InvoicePaymentCode = Support.GetCodeByValue("PaymentMethod", "INVOICE");

        /// <summary>
        /// The monthly payment code
        /// </summary>
        private Code MonthlyPaymentCode = Support.GetCodeByValue("PaymentPackageDuration", "MONTHLY");

        /// <summary>
        /// The anual payment code
        /// </summary>
        private Code AnualPaymentCode = Support.GetCodeByValue("PaymentPackageDuration", "ANUAL");

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// Gets or sets the company id.
        /// </summary>
        /// <value>
        /// The company id.
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
        /// Gets or sets the new company.
        /// </summary>
        /// <value>
        /// The new company.
        /// </value>
        public Data.Company NewCompany
        {
            get
            {
                if (ViewState["NewCompany"] == null)
                {
                    ViewState["NewCompany"] = new Data.Company();
                }
                return (Data.Company)ViewState["NewCompany"];
            }
            set
            {
                ViewState["NewCompany"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the media.
        /// </summary>
        /// <value>
        /// The media.
        /// </value>
        public Data.DocumentMedia Media
        {
            get
            {
                if (ViewState["DocumentMedia"] == null)
                {
                    return null;
                }
                return (Data.DocumentMedia)ViewState["DocumentMedia"];
            }
            set
            {
                ViewState["DocumentMedia"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public DisplayMode Mode
        {
            get
            {
                if (ViewState["Mode"] == null)
                {
                    ViewState["Mode"] = DisplayMode.CreditCard;
                }

                return (DisplayMode)ViewState["Mode"];
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the price plan details data object.
        /// </summary>
        /// <value>
        /// The price plan details.
        /// </value>
        public PricePlanDetails PricePlanDetails
        {
            get
            {
                if (ViewState["PricePlanDetails"] == null)
                {
                    return null;
                }

                return (PricePlanDetails)ViewState["PricePlanDetails"];
            }
            set
            {
                ViewState["PricePlanDetails"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the original last updated date.
        /// </summary>
        /// <value>
        /// The original last updated date.
        /// </value>
        private DateTime? OriginalLastUpdatedDate
        {
            get
            {
                if (ViewState["LastUpdatedDate"] == null)
                {
                    return null;
                }

                return (DateTime)ViewState["LastUpdatedDate"];
            }
            set
            {
                ViewState["LastUpdatedDate"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            pnlPaymentType.Height = Unit.Empty;
            base.OnLoad(e);
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
                string termsandConditionURL = Utils.GetSystemValue("TermsAndConditionPageUrl");
                string termsandConditionURLText = string.Format("<a href=\"{0}\" target=\"_blank\" \">terms and conditions.</a>", termsandConditionURL);
                chkAcceptTerms.Text = "I have read and accept the " + termsandConditionURLText;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the rbtnViaCreditCard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rbtnViaCreditCard_CheckedChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Mode = DisplayMode.CreditCard;
                InitializeUI(true);

                if (TermsAccepted != null)
                {
                    TermsAccepted(false, TermAndPriceAcceptToolTip);
                }
                PricePlanDetails.PaymentMethodCodeId = CreditCardPaymentCode.CodeId;
                bool isPackageChanged = GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
                int codeId = CreditCardPaymentCode.CodeId;
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                if (isPackageChanged)
                {
                    lblAcceptPricing.Visible = false;
                    chkAcceptPricing.Visible = true;
                    chkAcceptPricing.Text = this.GetBL<FinanceBL>().GetPaymentFinalAgreementText(PricePlanDetails, codeId, globalizationSection.Culture);
                    chkAcceptTerms.Visible = true;
                    AuthorizationEnabled(true);
                }
                else
                {
                    lblAcceptPricing.Text = this.GetBL<FinanceBL>().GetTextForAlreadyConfiguredPackage();
                    chkAcceptPricing.Visible = false;
                    //chkAcceptPricing.Text = this.GetBL<FinanceBL>().GetPaymentFinalAgreementText(PricePlanDetails.IsEducationalPackage ? PricePlanDetails.TotalAmountWithEducationalPackage : PricePlanDetails.TotalAmount, CompanyId, PricePlanDetails.IsEducationalPackage, codeId, PricePlanDetails.PaymentDurationCodeId, PricePlanDetails.DiscountCodeUsage);
                    chkAcceptTerms.Visible = false;
                    AuthorizationEnabled(false);
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the rbtnViaInvoice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rbtnViaInvoice_CheckedChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Mode = DisplayMode.Invoice;
                InitializeUI(true);

                if (TermsAccepted != null)
                {
                    TermsAccepted(false, TermAcceptToolTip);
                }

                int codeId = InvoicePaymentCode.CodeId;

                PricePlanDetails.PaymentMethodCodeId = InvoicePaymentCode.CodeId;
                bool isPackageChanged = GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                if (isPackageChanged)
                {
                    lblAcceptPricing.Visible = true;
                    lblAcceptPricing.Text = this.GetBL<FinanceBL>().GetPaymentFinalAgreementText(PricePlanDetails, codeId, globalizationSection.Culture);
                    chkAcceptPricing.Visible = false;
                    chkAcceptTerms.Visible = true;
                    AuthorizationEnabled(true);
                }
                else
                {
                    lblAcceptPricing.Text = this.GetBL<FinanceBL>().GetTextForAlreadyConfiguredPackage();
                    chkAcceptPricing.Visible = false;
                    chkAcceptTerms.Visible = false;
                    AuthorizationEnabled(false);
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkAcceptTerms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkAcceptTerms_CheckedChanged(object sender, EventArgs e)
        {
            if (TermsAccepted != null)
            {
                string toolTip = chkAcceptPricing.Visible ? TermAndPriceAcceptToolTip : TermAcceptToolTip;
                TermsAccepted(chkAcceptTerms.Checked && (!chkAcceptPricing.Visible || chkAcceptPricing.Checked), toolTip);
            }
        }

        /// <summary>
        /// Pricings the plan setup credit card details_ credid card details visibility changed.
        /// </summary>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        protected void pricingPlanSetupCreditCardDetails_CredidCardDetailsVisibilityChanged(bool isVisible)
        {
            if (!PageBase.StopProcessing)
            {
                int width = (int)pnlPaymentType.Width.Value;
                int height = 0;

                if (isVisible)
                {
                    pnlPaymentType.Height = 20;
                    height = 180;
                }
                else
                {
                    pnlPaymentType.Height = 180;
                    height = 20;
                }

                AnimatePaymentTypeDiv(width, height);
                upnlPaymentDetails.Update();
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Initializes the control.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="validationGroup">The validation group.</param>
        public void InitializeControl(PricePlanDetails pricePlanDetails, string validationGroup)
        {
            if (!PageBase.StopProcessing)
            {
                this.CompanyId = pricePlanDetails.CompanyId;
                this.PricePlanDetails = pricePlanDetails;
                Mode = DisplayMode.CreditCard;

                LoadPaymentDetails();
                InitializeUI(validationGroup);
            }
        }

        /// <summary>
        /// Saves the payment details.
        /// </summary>
        /// <returns></returns>
        public bool SavePaymentDetails()
        {
            bool hasSaved = false;
            if (Page.IsValid && ValidateCreditCardDetails())
            {
                CompanyPaymentPackage oldPackage = GetCompanyPaymentPackageForUpdating();
                bool isInventoryPackageOrDurationChanged = GetBL<FinanceBL>().HasInventryProjectOrDurationChanged(PricePlanDetails, oldPackage);

                SaveCompanyDetails();
                if (CompanyId > 0) // Create New Company wizard has handled after the company got crated.
                {
                    SaveCreditCardDetails();
                }

                //Save Discount if applies
                SaveCompanyPackage(oldPackage);
                SaveDiscountCode();
                if (CompanyId > 0)
                {
                    if (chkAcceptTerms.Visible)  //Mail should be send only there is a change. Since we are displaying terms and conditions only there is a change, mail should be sent if that is visible
                        SendStageBitzAdminEmail(CompanyId, isInventoryPackageOrDurationChanged);
                }

                this.GetBL<CompanyBL>().SaveChanges();

                //Save the Company Profile image and Credit card detatils for New company wizard
                if (CompanyId == 0)
                {
                    if (Media != null)
                    {
                        Media.RelatedId = NewCompany.CompanyId;
                        DataContext.DocumentMedias.AddObject(Media);
                    }
                    if (Mode == DisplayMode.CreditCard)
                    {
                        //Get the latest CompanyId and assign to the Paymentsetup control
                        pricingPlanSetupCreditCardDetails.CompanyId = NewCompany.CompanyId;
                        SaveCreditCardDetails();
                    }

                    if (chkAcceptTerms.Visible)
                        SendStageBitzAdminEmail(NewCompany.CompanyId, isInventoryPackageOrDurationChanged);

                    this.GetBL<CompanyBL>().SaveChanges();
                }

                hasSaved = true;
            }

            return hasSaved;
        }

        /// <summary>
        /// Gets the latest company ID.
        /// </summary>
        /// <returns></returns>
        public int GetLatestCompanyID()
        {
            return NewCompany.CompanyId;
        }

        /// <summary>
        /// Resets the controls.
        /// </summary>
        public void ResetControls()
        {
            pricingPlanCompanyDetails.Visible = false;
            pricingPlanSetupCreditCardDetails.Visible = false;
            pricingPlanSetupCreditCardDetails.ClearValues();
            trEducational.Visible = false;
            txtPosition.Text = string.Empty;
            upnlPaymentDetails.Update();
        }

        /// <summary>
        /// Updates the payment details.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        /// <param name="validationGroup">The validation group.</param>
        public void UpdatePaymentDetails(PricePlanDetails pricePlanDetails, string validationGroup)
        {
            chkAcceptTerms.Checked = false;
            this.PricePlanDetails = pricePlanDetails;
            int paymentmethodCodeId = 0;
            var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
            if (Mode == DisplayMode.CreditCard)
            {
                paymentmethodCodeId = CreditCardPaymentCode.CodeId;
                lblAcceptPricing.Visible = false;
                chkAcceptPricing.Visible = true;
                chkAcceptPricing.Checked = false;
            }
            else if (Mode == DisplayMode.Invoice)
            {
                paymentmethodCodeId = InvoicePaymentCode.CodeId;
                lblAcceptPricing.Visible = true;
                chkAcceptPricing.Visible = false;
            }
            string paymentTermsandCondition = this.GetBL<FinanceBL>().GetPaymentFinalAgreementText(PricePlanDetails, paymentmethodCodeId, globalizationSection.Culture);
            chkAcceptPricing.Text = lblAcceptPricing.Text = paymentTermsandCondition;
            upnlPaymentDetails.Update();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the payment details.
        /// </summary>
        private void LoadPaymentDetails()
        {
            chkAcceptPricing.Checked = false;
            chkAcceptTerms.Checked = false;
            rbtnViaCreditCard.Checked = true;
            rbtnViaInvoice.Checked = false;

            if (TermsAccepted != null)
            {
                TermsAccepted(false, TermAndPriceAcceptToolTip);
            }

            if (CompanyId > 0)
            {
                var companyPaymentPackage = GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                if (companyPaymentPackage != null)
                {
                    OriginalLastUpdatedDate = companyPaymentPackage.LastUpdateDate.Value;
                    if (PricePlanDetails.PaymentDurationCodeId == AnualPaymentCode.CodeId)
                    {
                        if (companyPaymentPackage.PaymentMethodCodeId == CreditCardPaymentCode.CodeId)
                        {
                            Mode = DisplayMode.CreditCard;
                            rbtnViaCreditCard.Checked = true;
                            rbtnViaInvoice.Checked = false;
                            if (TermsAccepted != null)
                            {
                                TermsAccepted(false, TermAndPriceAcceptToolTip);
                            }
                        }
                        else if (companyPaymentPackage.PaymentMethodCodeId == InvoicePaymentCode.CodeId)
                        {
                            Mode = DisplayMode.Invoice;
                            rbtnViaCreditCard.Checked = false;
                            rbtnViaInvoice.Checked = true;
                            if (TermsAccepted != null)
                            {
                                TermsAccepted(false, TermAcceptToolTip);
                            }
                        }
                    }
                    else if (PricePlanDetails.PaymentDurationCodeId == MonthlyPaymentCode.CodeId)
                    {
                        Mode = DisplayMode.CreditCard;
                        rbtnViaCreditCard.Checked = true;
                        rbtnViaInvoice.Checked = false;
                        if (TermsAccepted != null)
                        {
                            TermsAccepted(false, TermAndPriceAcceptToolTip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        /// <param name="validationGroup">The validation group.</param>
        /// <exception cref="System.ApplicationException">Invalid payment duration.</exception>
        private void InitializeUI(string validationGroup)
        {
            pricingPlanSetupCreditCardDetails.SetValidationGroup(validationGroup);
            pricingPlanCompanyDetails.SetValidationGroup(validationGroup);
            rfvPosition.ValidationGroup = validationGroup;

            if (PricePlanDetails.PaymentDurationCodeId == MonthlyPaymentCode.CodeId)
            {
                rbtnViaInvoice.Visible = false;
                lblPaymentDurationType.Text = "Monthly";
            }
            else if (PricePlanDetails.PaymentDurationCodeId == AnualPaymentCode.CodeId)
            {
                rbtnViaInvoice.Visible = true;
                lblPaymentDurationType.Text = "Yearly";
            }
            else
            {
                throw new ApplicationException("Invalid payment duration.");
            }

            trEducational.Visible = PricePlanDetails.IsEducationalPackage;

            pnlPaymentType.Height = 20;
            InitializeUI(false);
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        /// <param name="animate">if set to <c>true</c> [animate].</param>
        private void InitializeUI(bool animate)
        {
            bool ShowPriceTermsChecks = this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
            int width = (int)pnlPaymentType.Width.Value;
            int height = 0;
            var creditCardToken = this.GetBL<FinanceBL>().GetCreditCardToken(CompanyId);
            chkAcceptPricing.Checked = false;
            chkAcceptTerms.Checked = false;
            var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
            switch (Mode)
            {
                case DisplayMode.CreditCard:
                    pricingPlanSetupCreditCardDetails.DisplayMode = Common.SetupCreditCardDetails.ViewMode.PricingPlan;
                    pricingPlanSetupCreditCardDetails.CompanyId = this.CompanyId;
                    pricingPlanSetupCreditCardDetails.RelatedTable = "Company";
                    pricingPlanSetupCreditCardDetails.SetUISettings(creditCardToken);
                    pricingPlanSetupCreditCardDetails.Visible = true;
                    pricingPlanCompanyDetails.Visible = false;
                    PricePlanDetails.PaymentMethodCodeId = CreditCardPaymentCode.CodeId;
                    ShowPriceTermsChecks = this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
                    if (ShowPriceTermsChecks)
                    {
                        lblAcceptPricing.Visible = false;
                        chkAcceptPricing.Visible = true;
                        chkAcceptTerms.Visible = true;
                        AuthorizationEnabled(ShowPriceTermsChecks);
                    }
                    else
                    {
                        lblAcceptPricing.Text = this.GetBL<FinanceBL>().GetTextForAlreadyConfiguredPackage();
                        chkAcceptPricing.Visible = false;
                        chkAcceptTerms.Visible = false;
                        AuthorizationEnabled(ShowPriceTermsChecks);
                    }

                    if (animate)
                    {
                        pnlPaymentType.Height = 150;
                        height = creditCardToken != null ? 15 : 160;
                    }
                    else
                    {
                        pnlPaymentType.Height = creditCardToken != null ? 20 : 150;
                    }
                    break;

                case DisplayMode.Invoice:
                    pricingPlanCompanyDetails.InitializeControls(false);
                    pricingPlanCompanyDetails.LoadData(CompanyId > 0 ? GetBL<CompanyBL>().GetCompany(CompanyId) : NewCompany);
                    pricingPlanCompanyDetails.Visible = true;
                    pricingPlanSetupCreditCardDetails.Visible = false;
                    PricePlanDetails.PaymentMethodCodeId = InvoicePaymentCode.CodeId;
                    ShowPriceTermsChecks = this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
                    if (ShowPriceTermsChecks)
                    {
                        lblAcceptPricing.Visible = true;
                        chkAcceptPricing.Visible = false;
                        chkAcceptTerms.Visible = true;
                        AuthorizationEnabled(ShowPriceTermsChecks);
                    }
                    else
                    {
                        lblAcceptPricing.Text = this.GetBL<FinanceBL>().GetTextForAlreadyConfiguredPackage();
                        chkAcceptPricing.Visible = false;
                        chkAcceptTerms.Visible = false;
                        AuthorizationEnabled(ShowPriceTermsChecks);
                    }

                    if (animate)
                    {
                        pnlPaymentType.Height = creditCardToken != null ? 15 : 160;
                        height = 150;
                    }
                    else
                    {
                        pnlPaymentType.Height = Unit.Empty;
                    }
                    break;
            }

            if (animate)
            {
                AnimatePaymentTypeDiv(width, height);
            }

            int paymentmethodCodeId = 0;
            if (Mode == DisplayMode.CreditCard)
            {
                paymentmethodCodeId = CreditCardPaymentCode.CodeId;
            }
            else if (Mode == DisplayMode.Invoice)
            {
                paymentmethodCodeId = InvoicePaymentCode.CodeId;
            }

            //Display Terms and conditions
            if (ShowPriceTermsChecks)
            {
                string paymentTermsandCondition = this.GetBL<FinanceBL>().GetPaymentFinalAgreementText(PricePlanDetails, paymentmethodCodeId, globalizationSection.Culture);

                chkAcceptPricing.Text = lblAcceptPricing.Text = paymentTermsandCondition;
            }

            upnlPaymentDetails.Update();
        }

        /// <summary>
        /// Gets the company payment package for updating.
        /// </summary>
        /// <returns></returns>
        private CompanyPaymentPackage GetCompanyPaymentPackageForUpdating()
        {
            if (CompanyId > 0 && OriginalLastUpdatedDate.HasValue)
            {
                var companyPaymentPackage = GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrialForConcurrency(CompanyId, OriginalLastUpdatedDate.Value);
                if (companyPaymentPackage == null)
                {
                    StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.CompanyPaymentPackage, CompanyId));
                }

                return companyPaymentPackage;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sends the stage bitz admin email.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        private void SendStageBitzAdminEmail(int companyId, bool IsInventryProjectOrDurationChanged)
        {
            var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
            string cultureName = globalizationSection.Culture;
            string formattedAuthText = GetFormattedString();
            this.GetBL<FinanceBL>().SendStageBitzAdminEmail(companyId, this.UserID, PricePlanDetails, cultureName, PricePlanDetails.IsEducationalPackage ? txtPosition.Text : string.Empty, formattedAuthText, IsInventryProjectOrDurationChanged);
        }

        /// <summary>
        /// Gets the formatted string.
        /// </summary>
        /// <returns></returns>
        private string GetFormattedString()
        {
            StringBuilder authTextBuilder = new StringBuilder();
            authTextBuilder.Append(lblAcceptPricing.Text);

            if (PricePlanDetails.PaymentMethodCodeId == Utils.GetCodeIdByCodeValue("PaymentMethod", "CREDITCARD"))
            {
                authTextBuilder.Replace("I understand that:", "They are paying by credit card...");
            }
            else
            {
                authTextBuilder.Replace("I understand that:", "They have requested an invoice...");
            }
            authTextBuilder.Replace("<b>", "");
            authTextBuilder.Replace("</b>", "");
            //authTextBuilder.Replace("You will not be charged anything for the remaining {0} days of your Free Trial and will have full access to the options you selected on the previous page.\n", "");
            authTextBuilder.Replace("<li>This includes my Educational discount.</li>", "");
            authTextBuilder.Replace("<li>In the meantime, my company will have full access to the options I selected on the previous page.</li>", "");
            authTextBuilder.Replace("I selected", "they selected");
            authTextBuilder.Replace("I", "They");
            authTextBuilder.Replace("my", "their");
            authTextBuilder.Replace("I will be charged", "they will be charged");
            authTextBuilder.Replace("My", "Their");
            return authTextBuilder.ToString();
        }

        /// <summary>
        /// Saves the company package.
        /// </summary>
        /// <param name="oldCompanyPaymentPackage">The old company payment package.</param>
        private void SaveCompanyPackage(CompanyPaymentPackage oldCompanyPaymentPackage)
        {
            PricePlanDetails.CompanyId = this.CompanyId;
            if (Mode == DisplayMode.CreditCard)
            {
                PricePlanDetails.Position = string.Empty;
                PricePlanDetails.PaymentMethodCodeId = CreditCardPaymentCode.CodeId;
            }
            else if (Mode == DisplayMode.Invoice)
            {
                PricePlanDetails.Position = txtPosition.Text.Trim();
                PricePlanDetails.PaymentMethodCodeId = InvoicePaymentCode.CodeId;
            }

            this.GetBL<FinanceBL>().SaveCompanyPackage(UserID, oldCompanyPaymentPackage, PricePlanDetails, false);
        }

        /// <summary>
        /// Validates the credit card details.
        /// </summary>
        /// <returns></returns>
        private bool ValidateCreditCardDetails()
        {
            bool valid = true;
            if (Mode == DisplayMode.CreditCard && pricingPlanSetupCreditCardDetails.Visible)
            {
                valid = pricingPlanSetupCreditCardDetails.ValidateCardDetails();
            }

            return valid;
        }

        /// <summary>
        /// Saves the credit card details.
        /// </summary>
        private void SaveCreditCardDetails()
        {
            if (Mode == DisplayMode.CreditCard && pricingPlanSetupCreditCardDetails.Visible)
            {
                pricingPlanSetupCreditCardDetails.SaveCreditCardDetails(false);
            }
        }

        /// <summary>
        /// Saves the discount code.
        /// </summary>
        private void SaveDiscountCode()
        {
            //At the time when the Admin applies the Discount code, it might have been deleted, or someone else might have used it.
            if (PricePlanDetails.DiscountCode != null)
            {
                string errorMsg = string.Empty;

                bool isValid = GetBL<FinanceBL>().IsDiscountCodeValidToUse(PricePlanDetails.DiscountCode.Code, CompanyId, out errorMsg, PricePlanDetails.DiscountCode);
                //First check whether the applied Discount is valid. Because it can be either deleted, some one else being used, etc..

                if (isValid)
                    this.GetBL<FinanceBL>().SaveDiscountCodeUsageToCompany(PricePlanDetails.DiscountCode, UserID, CompanyId);
            }
        }

        /// <summary>
        /// Saves the company details.
        /// </summary>
        private void SaveCompanyDetails()
        {
            //There is a possibility for the user to edit Company details, So read them from scracth (New Company Wizard)
            if ((Mode == DisplayMode.Invoice && pricingPlanCompanyDetails.Visible && CompanyId > 0) || CompanyId == 0 && rbtnViaInvoice.Checked)
            {
                //Set Company to NULL. Because we should Recreate this object when saving(Read the UI and create the Object to get latest value).
                //This hapens only when accesss from Wizard
                if (CompanyId == 0)
                    pricingPlanCompanyDetails.Company = null;

                NewCompany = pricingPlanCompanyDetails.SaveCompanyDetails(false);
            }
            else if (CompanyId == 0 && rbtnViaCreditCard.Checked)
            {
                //Just Add the allready created object to the Context
                DataContext.Companies.AddObject(NewCompany);
            }
        }

        /// <summary>
        /// Animates the payment type div.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private void AnimatePaymentTypeDiv(int width, int height)
        {
            string script = string.Format("AnimateDiv('{0}', {1}, {2});", pnlPaymentType.ClientID, width, height);
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "AnimatePaymentType", script, true);
        }

        #endregion Private Methods
    }
}