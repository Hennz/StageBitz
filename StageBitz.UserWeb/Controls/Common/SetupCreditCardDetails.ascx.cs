using FatZebra;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Finance.Company;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// Delegate for credid card details visibility changed event.
    /// </summary>
    /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
    public delegate void CredidCardDetailsVisibilityChanged(bool isVisible);

    /// <summary>
    /// User control for setup creditcard details.
    /// </summary>
    public partial class SetupCreditCardDetails : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            CompanyBilling,
            PricingPlan
        }

        #endregion Enums

        #region Events

        /// <summary>
        /// Occurs when [payment details updated].
        /// </summary>
        public event EventHandler PaymentDetailsUpdated;

        /// <summary>
        /// Occurs when [credid card details visibility changed].
        /// </summary>
        public event CredidCardDetailsVisibilityChanged CredidCardDetailsVisibilityChanged;

        #endregion Events

        /// <summary>
        /// The credit card number var.
        /// </summary>
        private string creditCardNumber = null;

        #region Public properties

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
                if (ViewState["RelatedId"] == null)
                {
                    ViewState["RelatedId"] = 0;
                }

                return (int)ViewState["RelatedId"];
            }
            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related table.
        /// </summary>
        /// <value>
        /// The related table.
        /// </value>
        public string RelatedTable
        {
            get
            {
                if (ViewState["RelatedTable"] == null)
                {
                    ViewState["RelatedTable"] = string.Empty;
                }

                return (string)ViewState["RelatedTable"];
            }
            set
            {
                ViewState["RelatedTable"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public ViewMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(ViewMode);
                }

                return (ViewMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion Public properties

        #region Private Properties

        /// <summary>
        /// Keep FatZibra responce object  in memory
        /// </summary>
        /// <returns>The <see cref="T:System.Web.HttpResponse" /> object associated with the <see cref="T:System.Web.UI.Page" /> that contains the <see cref="T:System.Web.UI.UserControl" /> instance.</returns>
        private Response Response
        {
            get;
            set;
        }

        #endregion Private Properties

        #region Event Hanlders

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string validationGroup = this.ID + "PaymentValGroup";
                if (DisplayMode == ViewMode.CompanyBilling)
                {
                    paymentSetupDetails.Visible = true;
                    pnlExpander.Visible = false;

                    //Check for current package
                    CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                    if (companyPaymentPackage == null)
                    {
                        lnkSetUp.Text = "Select Pricing plan";
                        lnkSetUp.PostBackUrl = string.Format("~/Company/CompanyPricingPlans.aspx?companyid={0}", CompanyId);
                    }
                    else if (companyPaymentPackage.PaymentMethodCodeId == Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE"))
                    {
                        litPackageStatus.Text = "You have paid by Invoice";
                        litPackageStatus.Visible = true;
                        divPaymentDetailsSet.Visible = litpipePaymentDetailsNotSet.Visible = litPaymentDetailsNotSet.Visible = lnkSetUp.Visible = lnkChange.Visible = false;
                    }
                }
                else if (DisplayMode == ViewMode.PricingPlan)
                {
                    paymentSetupDetails.Visible = true;
                    pnlPopup.Visible = false;

                    // stop firing validations when click on the hide link.
                    lnkSetUp.CausesValidation = false;
                    lnkChange.CausesValidation = false;
                }
                else
                {
                    paymentSetupDetails.Visible = false;
                }

                SetValidationGroup(validationGroup);
            }

            popupConfirmPaymentDetails.ID = this.ID + "popupConfirmPaymentDetails";
        }

        /// <summary>
        /// Shows the credit card details.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ShowCreditCardDetails(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (DisplayMode == SetupCreditCardDetails.ViewMode.PricingPlan)
                {
                    pnlExpander.Visible = !pnlExpander.Visible;

                    if (CredidCardDetailsVisibilityChanged != null)
                    {
                        CredidCardDetailsVisibilityChanged(pnlExpander.Visible);
                    }

                    LinkButton linkButton = (LinkButton)sender;
                    if (linkButton != null)
                    {
                        if (linkButton.ID == lnkSetUp.ID)
                        {
                            linkButton.Text = pnlExpander.Visible ? "Hide" : "Set Up";
                        }
                        else if (linkButton.ID == lnkChange.ID)
                        {
                            linkButton.Text = pnlExpander.Visible ? "Hide" : "Change";
                        }
                    }
                }
                else
                {
                    DisplayMode = SetupCreditCardDetails.ViewMode.CompanyBilling;
                    ShowCreditCardPopup();
                    pnlExpander.Visible = false;
                }

                upnlCreditCardDetails.Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing && Page.IsValid && PaymentDetailsUpdated != null)
            {
                SaveCreditCardDetails(true);
            }
        }

        #endregion Event Hanlders

        #region Public Methods

        /// <summary>
        /// Sets the validation group.
        /// </summary>
        /// <param name="validationGroup">The validation group.</param>
        public void SetValidationGroup(string validationGroup)
        {
            btnConfirm.ValidationGroup = validationGroup;
            ucCreditCardDetailsExpander.SetValidationGroup(validationGroup);
            ucCreditCardDetailsPopup.SetValidationGroup(validationGroup);
            upnlCreditCardDetails.Update();
        }

        /// <summary>
        /// Clears the values.
        /// </summary>
        public void ClearValues()
        {
            ucCreditCardDetailsExpander.ClearValues();
            ucCreditCardDetailsPopup.ClearValues();
            upnlCreditCardDetails.Update();
        }

        /// <summary>
        /// Saves the credit card details.
        /// </summary>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void SaveCreditCardDetails(bool commit)
        {
            switch (DisplayMode)
            {
                case ViewMode.CompanyBilling:
                    SaveCreaditCardToken(ucCreditCardDetailsPopup, commit);
                    if (PaymentDetailsUpdated != null)
                    {
                        PaymentDetailsUpdated(this, EventArgs.Empty);
                    }
                    break;

                case ViewMode.PricingPlan:
                    if (pnlExpander.Visible)
                    {
                        SaveCreaditCardToken(ucCreditCardDetailsExpander, commit);
                        if (PaymentDetailsUpdated != null)
                        {
                            PaymentDetailsUpdated(this, EventArgs.Empty);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Validates the card details of current credit card details control.
        /// </summary>
        /// <returns>Is valid.</returns>
        public bool ValidateCardDetails()
        {
            bool valid = false;
            switch (DisplayMode)
            {
                case ViewMode.CompanyBilling:
                    valid = ValidateCardDetails(ucCreditCardDetailsPopup);
                    break;

                case ViewMode.PricingPlan:
                    if (pnlExpander.Visible)
                    {
                        valid = ValidateCardDetails(ucCreditCardDetailsExpander);
                    }
                    else
                    {
                        valid = true;
                    }
                    break;
            }

            return valid;
        }

        /// <summary>
        /// Shows the credit card popup with messages.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="noOfPeople">The no of people.</param>
        public void ShowCreditCardPopupWithMessages(int companyId, int noOfPeople)
        {
            bool isPaymentDetailsSet = this.GetBL<FinanceBL>().GetCreditCardToken(companyId) == null ? false : true;
            switch (DisplayMode)
            {
                case ViewMode.CompanyBilling:
                    popupConfirmPaymentDetails.Title = "Set Up Payment Details";
                    break;
            }

            DiscountCodeUsage discountUsage = GetLatestDiscountUsageCode(companyId);
            bool isFullDiscount = false;

            if (discountUsage != null)
                isFullDiscount = discountUsage.DiscountCode.Discount == 100; //Check whether it is a 100% discount.

            ucCreditCardDetailsPopup.ShowCreditCardEditableLabels(!isPaymentDetailsSet && !isFullDiscount);

            imgCreditCard.Visible = !isPaymentDetailsSet;

            popupConfirmPaymentDetails.ShowPopup();
        }

        /// <summary>
        /// Shows the credit card popup.
        /// </summary>
        public void ShowCreditCardPopup()
        {
            ucCreditCardDetailsPopup.AddStylesToEditableLabels();
            popupConfirmPaymentDetails.Title = "Register Credit Card";
            ucCreditCardDetailsPopup.ClearValues();
            popupConfirmPaymentDetails.ShowPopup();
        }

        /// <summary>
        /// Sets the UI settings.
        /// </summary>
        /// <param name="creditCardToken">The credit card token.</param>
        public void SetUISettings(CreditCardToken creditCardToken)
        {
            if (creditCardToken != null)
            {
                lnkSetUp.Visible = false;
                divPaymentDetailsSet.Visible = true;
                if (creditCardToken != null && creditCardToken.LastFourDigitsCreditCardNumber != null)
                {
                    imgCreditCardNo.Attributes["title"] = "The last 4 digits of the credit card number you've used are XXXX-XXXX-XXXX-" + Utils.DecryptStringAES(creditCardToken.LastFourDigitsCreditCardNumber);
                    imgCreditCardNo.Visible = true;
                }
                else
                {
                    imgCreditCardNo.Visible = false;
                }

                lnkChange.Visible = true;
                spnSeperator.Visible = true;
                pnlExpander.Visible = false;

                if (DisplayMode == SetupCreditCardDetails.ViewMode.PricingPlan)
                {
                    lnkSetUp.Text = pnlExpander.Visible ? "Hide" : "Set Up";
                    lnkChange.Text = pnlExpander.Visible ? "Hide" : "Change";
                }
                litPaymentDetailsNotSet.Visible = false;
            }
            else
            {
                lnkSetUp.Visible = true;
                divPaymentDetailsSet.Visible = false;
                litPaymentDetailsNotSet.Visible = true;
                lnkChange.Visible = false;
                spnSeperator.Visible = false;
                pnlExpander.Visible = true;

                if (DisplayMode == SetupCreditCardDetails.ViewMode.PricingPlan)
                {
                    paymentSetupDetails.Visible = false;
                }
            }
        }

        #endregion Public Methods

        /// <summary>
        /// Gets the credit card token.
        /// </summary>
        /// <returns></returns>
        private CreditCardToken GetCreditCardToken()
        {
            return this.GetBL<FinanceBL>().GetCreditCardToken(RelatedTable, CompanyId);
        }

        /// <summary>
        /// Validates the card details for given creditcard details control.
        /// </summary>
        /// <param name="ucCreditCardDetails">The credit card details control.</param>
        /// <returns>Is valid credit card.</returns>
        private bool ValidateCardDetails(CreditCardDetails ucCreditCardDetails)
        {
            FatZebra.Gateway.Username = Utils.GetSystemValue("PaymentGatewayUsername");
            FatZebra.Gateway.Token = Utils.GetSystemValue("PaymentGatewayToken");
            FatZebra.Gateway.SandboxMode = Convert.ToBoolean(Utils.GetSystemValue("PaymentGatewaySandboxMode"));
            FatZebra.Gateway.TestMode = Convert.ToBoolean(Utils.GetSystemValue("PaymentGatewayTestMode"));

            //Get the first day of the month
            DateTime monthwithFirstday = new DateTime(ucCreditCardDetails.SelectedYear, ucCreditCardDetails.SelectedMonth, 1);
            //Add a month a reduce one date to get the last day of the month
            DateTime monthWithLastday = monthwithFirstday.AddMonths(1).AddDays(-1).Date;
            if (monthWithLastday < Today)
            {
                ucCreditCardDetails.SetNotificationDates();
                popupConfirmPaymentDetails.ShowPopup();
                Response = null;
                return false;
            }

            Response = CreditCard.Create(ucCreditCardDetails.CreditCardHolderName, ucCreditCardDetails.CreditCardNumber, monthWithLastday, ucCreditCardDetails.CVVNumber);
            return Response.Successful;
        }

        /// <summary>
        /// Saves the creadit fatzibra card token.
        /// </summary>
        /// <param name="ucCreditCardDetails">The credit card details control.</param>
        /// <param name="commit">if set to <c>true</c> [commit to database].</param>
        private void SaveCreaditCardToken(CreditCardDetails ucCreditCardDetails, bool commit)
        {
            // If credit card not validated
            if (Response == null)
            {
                ValidateCardDetails(ucCreditCardDetails);
            }

            if (Response != null && Response.Successful)
            {
                #region Update existing token details if available

                CreditCardToken creditCardToken = GetCreditCardToken();
                if (creditCardToken != null)
                {
                    creditCardToken.IsActive = false;
                    creditCardToken.LastUpdatedBy = UserID;
                    creditCardToken.LastUpdatedDate = Now;
                }

                #endregion Update existing token details if available

                #region Add New token details

                CreditCardToken newCreditCardToken = new CreditCardToken();
                newCreditCardToken.Token = Utils.EncryptStringAES(Response.Result.ID);
                newCreditCardToken.CreatedBy = UserID;
                newCreditCardToken.LastUpdatedBy = UserID;
                newCreditCardToken.LastUpdatedDate = Now;
                newCreditCardToken.CreatedDate = Now;
                newCreditCardToken.IsActive = true;
                newCreditCardToken.RelatedTableName = RelatedTable;
                newCreditCardToken.RelatedId = CompanyId;
                creditCardNumber = ucCreditCardDetails.CreditCardNumber;
                newCreditCardToken.LastFourDigitsCreditCardNumber = Utils.EncryptStringAES(creditCardNumber.Substring(creditCardNumber.Length - 4));
                DataContext.CreditCardTokens.AddObject(newCreditCardToken);

                #endregion Add New token details

                #region Check the company status and reactivate it

                var company = GetBL<CompanyBL>().GetCompany(CompanyId);
                if (company != null)
                {
                    CompanyStatusHandler.CompanyWarningInfo warningInfo = CompanyStatusHandler.GetCompanyWarningStatus(CompanyId, company.CompanyStatusCodeId, company.ExpirationDate);
                    if (warningInfo.WarningStatus == CompanyStatusHandler.CompanyWarningStatus.SuspendedForNoPaymentOptions)
                    {
                        CompanyPaymentPackage companyPaymentPackage = GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(company.CompanyId);
                        companyPaymentPackage.PaymentMethodCodeId = Support.GetCodeIdByCodeValue("PaymentMethod", "CREDITCARD");
                        company.CompanyStatusCodeId = Support.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");

                        //update if there is summary for the period
                        CompanyPaymentSummary companyPaymentSummary = this.GetBL<FinanceBL>().GetPaymentSummaryToShouldProcess(company.CompanyId);
                        if (companyPaymentSummary != null)
                        {
                            companyPaymentSummary.ShouldProcess = true;
                        }
                    }
                }

                #endregion Check the company status and reactivate it

                if (commit)
                {
                    DataContext.SaveChanges();
                }

                ucCreditCardDetails.ClearValues();

                if (DisplayMode == ViewMode.CompanyBilling)
                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "HideDiv", "HidePaymentSuccessMessage();", true);

                popupConfirmPaymentDetails.HidePopup();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (Response != null && Response.Errors.Count > 0)
                {
                    sb.Append(string.Join("<br />", Response.Errors));
                    ucCreditCardDetails.SetNotification(sb.ToString().Length == 0 ? "Failed to set up payment details. Please verify the payment details and retry" : sb.ToString());
                }

                popupConfirmPaymentDetails.ShowPopup();
            }
        }

        /// <summary>
        /// Gets the latest discount usage code.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private DiscountCodeUsage GetLatestDiscountUsageCode(int companyId) // this parameter is not used inside the method. Please make sure to refactor it later.
        {
            return this.GetBL<FinanceBL>().GetLatestDiscountUsageCode(CompanyId);
        }
    }
}