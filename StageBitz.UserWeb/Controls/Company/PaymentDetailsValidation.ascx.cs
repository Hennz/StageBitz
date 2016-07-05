using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// Delegate for inform parent to update validation.
    /// </summary>
    public delegate void InformParentToUpdateValidation();

    /// <summary>
    /// Delegate for inform parent about discount change.
    /// </summary>
    /// <param name="discountCodeUsage">The discount code usage.</param>
    public delegate void InformDiscountChanged(DiscountCodeUsage discountCodeUsage);

    /// <summary>
    /// User control for payment details validation.
    /// </summary>
    public partial class PaymentDetailsValidation : UserControlBase
    {
        #region Events

        /// <summary>
        /// The inform parent to update validation
        /// </summary>
        public InformParentToUpdateValidation InformParentToUpdateValidation;

        /// <summary>
        /// The inform discount changed
        /// </summary>
        public InformDiscountChanged InformDiscountChanged;

        #endregion Events

        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            PricingPlanPage,
            PaymentDetailsPopUp,
            HundredPercentDiscount
        }

        #endregion Enums

        #region Properties

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

        /// <summary>
        /// Gets the company identifier.
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
        /// Gets or sets the price plan details.
        /// </summary>
        /// <value>
        /// The price plan details.
        /// </value>
        private PricePlanDetails PricePlanDetails
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

        /// <summary>
        /// Gets or sets the discount code usage.
        /// </summary>
        /// <value>
        /// The discount code usage.
        /// </value>
        private DiscountCodeUsage DiscountCodeUsage
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
        /// Gets or sets a value indicating whether this company has responded to free trail no projects popup.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company has responded to free trail no projects popup; otherwise, <c>false</c>.
        /// </value>
        private bool HasRespondedToFreeTrailNoProjectsPopup
        {
            get
            {
                if (ViewState["HasRespondedToFreeTrailNoProjectsPopup"] == null)
                {
                    return false;
                }
                return (bool)ViewState["HasRespondedToFreeTrailNoProjectsPopup"];
            }
            set
            {
                ViewState["HasRespondedToFreeTrailNoProjectsPopup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company has responded to no package changes popup.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company has responded to no package changes popup; otherwise, <c>false</c>.
        /// </value>
        private bool HasRespondedToNoPackageChangesPopup
        {
            get
            {
                if (ViewState["HasRespondedToNoPackageChangesPopup"] == null)
                {
                    return false;
                }
                return (bool)ViewState["HasRespondedToNoPackageChangesPopup"];
            }
            set
            {
                ViewState["HasRespondedToNoPackageChangesPopup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [confirm enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [confirm enabled]; otherwise, <c>false</c>.
        /// </value>
        private bool confirmEnabled { get; set; }

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
                string termsandConditionURL = Utils.GetSystemValue("TermsAndConditionPageUrl");
                string termsandConditionURLText = string.Format("<a href=\"{0}\" target=\"_blank\" \">terms and conditions.</a>", termsandConditionURL);
                chkAcceptTermsFreeInventory.Text = "I have read and accept the " + termsandConditionURLText;
                chkAcceptTermsDiscount.Text = "I have read and accept the " + termsandConditionURLText;
            }
        }

        #region AddDetails

        /// <summary>
        /// Handles the Click event of the btnConfirmPaymentDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmPaymentDetails_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (!IsCompanyValidToSave())
                {
                    popupPaymentDetails.HidePopup();
                    return;
                }

                this.DisplayMode = ViewMode.PaymentDetailsPopUp;    //concurrent discount chaged popups come on top of payment details popup. view mode set accordingly.
                if (HandleConcurrentDiscountChanges())
                {
                    this.PricePlanDetails.DiscountCodeUsage = this.DiscountCodeUsage;
                    paymentDetails.PricePlanDetails.DiscountCode = (this.DiscountCodeUsage == null ? null : this.DiscountCodeUsage.DiscountCode);
                }
                else
                {
                    //This handles the Discount Save
                    if (paymentDetails.SavePaymentDetails())
                    {
                        popupPaymentDetails.HidePopup();
                        btnChangesSaved.CommandArgument = "PaymentDetails";
                        popupSaveChangesConfirm.ShowPopup();
                    }
                }
            }
        }

        /// <summary>
        /// Payments the details_ terms accepted.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="toolTip">The tool tip.</param>
        protected void paymentDetails_TermsAccepted(bool enabled, string toolTip)
        {
            if (!PageBase.StopProcessing)
            {
                btnConfirmPaymentDetails.Enabled = enabled;
                btnConfirmPaymentDetails.ToolTip = enabled ? string.Empty : toolTip;
                upnlPopUps.Update();
            }
        }

        /// <summary>
        /// Payments the details_ authorization enabled.
        /// </summary>
        /// <param name="isAuthorizationEnabled">if set to <c>true</c> [is authorization enabled].</param>
        protected void paymentDetails_AuthorizationEnabled(bool isAuthorizationEnabled)
        {
            if (!PageBase.StopProcessing)
            {
                confirmEnabled = !isAuthorizationEnabled;
                if (!isAuthorizationEnabled)
                {
                    btnConfirmPaymentDetails.ToolTip = string.Empty;
                }
                btnConfirmPaymentDetails.Enabled = !isAuthorizationEnabled;
                upnlPopUps.Update();
            }
        }

        #endregion AddDetails

        #region FreeInventory

        /// <summary>
        /// Handles the CheckedChanged event of the chkAcceptTermsFreeInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkAcceptTermsFreeInventory_CheckedChanged(object sender, EventArgs e)
        {
            btnConfirmFreeInventory.Enabled = chkAcceptTermsFreeInventory.Checked;
            btnConfirmFreeInventory.ToolTip = chkAcceptTermsFreeInventory.Checked ? string.Empty : "Please accept the Terms and Conditions";
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmFreeInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmFreeInventory_Click(object sender, EventArgs e)
        {
            popupFreeInventoryOnly.HidePopup();

            if (!IsCompanyValidToSave())
            {
                return;
            }

            CompanyPaymentPackage oldCompanyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
            bool isInventoryPackageOrDurationChanged = GetBL<FinanceBL>().HasInventryProjectOrDurationChanged(PricePlanDetails, oldCompanyPaymentPackage);
            //Handling concurrency (another admin changed the package concurrently)
            if (oldCompanyPaymentPackage != null && oldCompanyPaymentPackage.LastUpdateDate > OriginalLastUpdatedDate)
            {
                StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.CompanyPaymentPackage, CompanyId));
            }
            else
            {
                if (CompanyId == 0)
                {
                    //Add to the Context
                    DataContext.Companies.AddObject(NewCompany);
                }
                this.GetBL<FinanceBL>().SaveCompanyPackage(UserID, oldCompanyPaymentPackage, PricePlanDetails, false);
                //First check whether the applied Discount is valid. Because it can be either deleted, some one else being used, etc..
                if (PricePlanDetails.DiscountCode != null)
                {
                    string errorMsg = string.Empty;
                    bool isValid = GetBL<FinanceBL>().IsDiscountCodeValidToUse(PricePlanDetails.DiscountCode.Code, CompanyId, out errorMsg, PricePlanDetails.DiscountCode == null ? null : PricePlanDetails.DiscountCode);
                    if (isValid)
                        SaveDiscountCode();
                }
                this.GetBL<CompanyBL>().SaveChanges();
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                string cultureName = globalizationSection.Culture;
                string authText = string.Format("They have selected {0} Inventory level and {1} Project level.", Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(PricePlanDetails.InventoryPaymentPackageTypeId).PackageDisplayName, Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(PricePlanDetails.ProjectPaymentPackageTypeId).PackageDisplayName);

                if (chkAcceptTermsFreeInventory.Visible)
                {
                    this.GetBL<FinanceBL>().SendStageBitzAdminEmail(CompanyId == 0 ? NewCompany.CompanyId : CompanyId, UserID, PricePlanDetails, cultureName, string.Empty, authText, isInventoryPackageOrDurationChanged);
                }
                //Save the Company Profile image
                if (CompanyId == 0 && Media != null)
                {
                    Media.RelatedId = NewCompany.CompanyId;
                    DataContext.DocumentMedias.AddObject(Media);
                    this.GetBL<CompanyBL>().SaveChanges();
                }
            }
            btnChangesSaved.CommandArgument = "FreeInventory";
            popupSaveChangesConfirm.ShowPopup();
        }

        #endregion FreeInventory

        #region HundredPercentDisount

        /// <summary>
        /// Handles the CheckedChanged event of the chkAcceptTermsDiscount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkAcceptTermsDiscount_CheckedChanged(object sender, EventArgs e)
        {
            btnConfirmHundredPercentDiscount.Enabled = chkAcceptTermsDiscount.Checked;
            btnConfirmHundredPercentDiscount.ToolTip = chkAcceptTermsDiscount.Checked ? string.Empty : "Please accept the Terms and Conditions";
        }

        /// <summary>
        /// Handles the Click event of the btnAddDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddDetails_Click(object sender, EventArgs e)
        {
            popupHundredPercentDiscount.HidePopup();

            if (IsCompanyValidToSave())
            {
                ShowAdddetailsPaymentpopUp();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmHundredPercentDiscount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmHundredPercentDiscount_Click(object sender, EventArgs e)
        {
            if (!IsCompanyValidToSave())
            {
                popupHundredPercentDiscount.HidePopup();
                return;
            }

            this.DisplayMode = ViewMode.HundredPercentDiscount;    //concurrent discount chaged popups come on top of payment details popup. view mode set accordingly.
            if (HandleConcurrentDiscountChanges())
            {
                this.PricePlanDetails.DiscountCodeUsage = this.DiscountCodeUsage;
                this.PricePlanDetails.DiscountCode = (this.DiscountCodeUsage != null ? this.DiscountCodeUsage.DiscountCode : null);
            }
            else
            {
                CompanyPaymentPackage oldCompanyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                bool isInventoryPackageOrDurationChanged = GetBL<FinanceBL>().HasInventryProjectOrDurationChanged(PricePlanDetails, oldCompanyPaymentPackage);
                //Handling concurrency (another admin changed the package concurrently)
                if (oldCompanyPaymentPackage != null && oldCompanyPaymentPackage.LastUpdateDate > OriginalLastUpdatedDate)
                {
                    StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.CompanyPaymentPackage, CompanyId));
                }
                else
                {
                    if (CompanyId == 0)
                    {
                        DataContext.Companies.AddObject(NewCompany);
                    }

                    this.GetBL<FinanceBL>().SaveCompanyPackage(UserID, oldCompanyPaymentPackage, PricePlanDetails, false);
                    //First check whether the applied Discount is valid. Because it can be either deleted, some one else being used, etc..
                    string errorMsg = string.Empty;
                    bool isValid = GetBL<FinanceBL>().IsDiscountCodeValidToUse(PricePlanDetails.DiscountCodeUsage.DiscountCode.Code, CompanyId, out errorMsg, PricePlanDetails.DiscountCodeUsage.DiscountCode);
                    if (isValid)
                        SaveDiscountCode();
                    this.GetBL<CompanyBL>().SaveChanges();

                    var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                    string cultureName = globalizationSection.Culture;
                    string authText = string.Concat(String.Format("{0:P}", PricePlanDetails.DiscountCodeUsage.DiscountCode.Discount / 100), " discount code is applied.");

                    if (chkAcceptTermsDiscount.Visible)
                    {
                        this.GetBL<FinanceBL>().SendStageBitzAdminEmail(CompanyId == 0 ? NewCompany.CompanyId : CompanyId, UserID, PricePlanDetails, cultureName, string.Empty, authText, isInventoryPackageOrDurationChanged);
                    }

                    //Save the Company Profile image. First you need to save the Company to get the CompanyId.
                    if (CompanyId == 0 && Media != null)
                    {
                        Media.RelatedId = NewCompany.CompanyId;
                        DataContext.DocumentMedias.AddObject(Media);
                        this.GetBL<CompanyBL>().SaveChanges();
                    }
                }
                popupHundredPercentDiscount.HidePopup();
                btnChangesSaved.CommandArgument = "HundredDiscount";
                popupSaveChangesConfirm.ShowPopup();
            }
        }

        #endregion HundredPercentDisount

        #region DiscountChanged

        /// <summary>
        /// Handles the Click event of the btnRespondDiscountChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRespondDiscountChanged_Click(object sender, EventArgs e)
        {
            popupConcurrentDiscountChanged.HidePopup();
            if (InformDiscountChanged != null)// will refresh the summary panel of prcing plan page
            {
                InformDiscountChanged(DiscountCodeUsage);
            }
            //if the newly added discount code is 100%
            if (this.DiscountCodeUsage != null && this.DiscountCodeUsage.DiscountCode.Discount == 100 && paymentPackageSummary.TotalAmountForPeriod == 0)
            {
                if (this.DisplayMode == ViewMode.PaymentDetailsPopUp) // if the concurrent popups appeard on top of payment details popup.
                {
                    popupPaymentDetails.HidePopup();
                }
                ShowHundredPercentDiscountPopUp(this.DiscountCodeUsage);
            }
            if (this.DisplayMode == ViewMode.PaymentDetailsPopUp)  //will refresh the authorization text on payment details popup
            {
                PricePlanDetails.TotalAmount = paymentPackageSummary.TotalAmount;
                btnConfirmPaymentDetails.Enabled = false;
                paymentDetails.UpdatePaymentDetails(PricePlanDetails, btnConfirmPaymentDetails.ValidationGroup);
            }
            else if (this.DisplayMode == ViewMode.HundredPercentDiscount) // if the concurrent popups appeard on top of hundred percent discount popup.
            {
                popupHundredPercentDiscount.HidePopup();
            }
        }

        #endregion DiscountChanged

        /// <summary>
        /// Handles the Click event of the btnChangesSaved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnChangesSaved_Click(object sender, EventArgs e)
        {
            popupSaveChangesConfirm.HidePopup();
            if (btnChangesSaved.CommandArgument.Equals("PaymentDetails"))
            {
                //payment details
                if (CompanyId == 0) //Newly created Company needs to Navigate to Dashboard
                    Response.Redirect(string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", paymentDetails.GetLatestCompanyID()));
                else if (InformParentToUpdateValidation != null)
                {
                    InformParentToUpdateValidation();
                }
            }
            else if (btnChangesSaved.CommandArgument.Equals("FreeInventory"))
            {
                //free
                if (CompanyId == 0) //Newly created Company needs to Navigate to Dashboard
                    Response.Redirect(string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", NewCompany.CompanyId));
                else if (InformParentToUpdateValidation != null)
                {
                    InformParentToUpdateValidation();
                }
            }
            else if (btnChangesSaved.CommandArgument.Equals("HundredDiscount"))
            {
                if (CompanyId == 0) //Newly created Company needs to Navigate to Dashboard
                    Response.Redirect(string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", NewCompany.CompanyId));
                else if (InformParentToUpdateValidation != null)
                {
                    InformParentToUpdateValidation();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCloseConfirmAccept control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCloseConfirmAccept_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupCloseConfirm.HidePopup();
                if (btnCloseConfirmAccept.CommandArgument.Equals("AddDetail"))
                {
                    paymentDetails.ResetControls();
                    popupPaymentDetails.HidePopup();
                }
                else if (btnCloseConfirmAccept.CommandArgument.Equals("FreeInventory"))
                {
                    popupFreeInventoryOnly.HidePopup();
                }
                else if (btnCloseConfirmAccept.CommandArgument.Equals("HundredDiscount"))
                {
                    popupHundredPercentDiscount.HidePopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClose_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                btnCloseConfirmAccept.CommandArgument = btnClose.CommandArgument;
                popupCloseConfirm.ShowPopup(1001);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnContinueNoProjectsPackageDuringFreeTrail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnContinueNoProjectsPackageDuringFreeTrail_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                this.HasRespondedToFreeTrailNoProjectsPopup = true;
                ValidatePackageConfigurations(PricePlanDetails);
                popupNoProjectsPackageSelectionDuringFreeTrail.HidePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNoPackageChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNoPackageChanges_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                this.HasRespondedToNoPackageChangesPopup = true;
                ValidatePackageConfigurations(PricePlanDetails);
                popupNoPackageChanges.HidePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReloadPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReloadPage_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Response.Redirect(Request.Url.ToString());
            }
        }

        #endregion Events

        #region PrivateMethods

        /// <summary>
        /// Saves the discount code.
        /// </summary>
        private void SaveDiscountCode()
        {
            if (PricePlanDetails.DiscountCode != null)
            {
                this.GetBL<FinanceBL>().SaveDiscountCodeUsageToCompany(PricePlanDetails.DiscountCode, UserID, CompanyId);
            }
        }

        /// <summary>
        /// Shows the popup for company already choose free inventory only.
        /// </summary>
        /// <param name="itemCount">The item count.</param>
        private void ShowFreeInventoryOnlyPopUp(int itemCount)
        {
            FutureRequestNotificationMessageForFreeInventory.CompanyId = CompanyId;
            FutureRequestNotificationMessageForFreeInventory.PricePlanDetails = PricePlanDetails;
            FutureRequestNotificationMessageForFreeInventory.Width = 520;
            FutureRequestNotificationMessageForFreeInventory.LoadData();
            bool isPlanChanged = this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
            ltrMaxItemsForFreeInventory.Text = itemCount.ToString();
            chkAcceptTermsFreeInventory.Visible = isPlanChanged;
            if (isPlanChanged)
            {
                chkAcceptTermsFreeInventory.Checked = false;
            }
            else
            {
                btnConfirmFreeInventory.ToolTip = "";
            }
            btnConfirmFreeInventory.Enabled = !isPlanChanged;
            btnClose.CommandArgument = "FreeInventory";
            popupFreeInventoryOnly.ShowPopup();
        }

        /// <summary>
        /// Shows the hundred percent discount pop up.
        /// </summary>
        /// <param name="disountCodeUsage">The disount code usage.</param>
        private void ShowHundredPercentDiscountPopUp(DiscountCodeUsage disountCodeUsage)
        {
            FutureRequestNotificationMessageForHundredPercent.CompanyId = CompanyId;
            FutureRequestNotificationMessageForHundredPercent.PricePlanDetails = PricePlanDetails;
            FutureRequestNotificationMessageForHundredPercent.Width = 510;
            FutureRequestNotificationMessageForHundredPercent.LoadData();
            bool isPlanChanged = this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(CompanyId, PricePlanDetails);
            ltrDiscountExpiryDate.Text = Utils.FormatDate(disountCodeUsage.EndDate);
            chkAcceptTermsDiscount.Visible = isPlanChanged;
            if (isPlanChanged)
            {
                chkAcceptTermsDiscount.Checked = false;
            }
            else
            {
                btnConfirmHundredPercentDiscount.ToolTip = "";
            }
            btnConfirmHundredPercentDiscount.Enabled = !isPlanChanged;
            btnClose.CommandArgument = "HundredDiscount";
            popupHundredPercentDiscount.ShowPopup();
        }

        /// <summary>
        /// Shows the adddetails paymentpop up.
        /// </summary>
        private void ShowAdddetailsPaymentpopUp()
        {
            FutureRequestNotificationForPaymentDetails.CompanyId = CompanyId;
            FutureRequestNotificationForPaymentDetails.PricePlanDetails = PricePlanDetails;
            FutureRequestNotificationForPaymentDetails.Width = 850;
            FutureRequestNotificationForPaymentDetails.LoadData();
            paymentDetails.InitializeControl(PricePlanDetails, btnConfirmPaymentDetails.ValidationGroup);
            btnConfirmPaymentDetails.Enabled = confirmEnabled;
            btnClose.CommandArgument = "AddDetail";
            popupPaymentDetails.ShowPopup();
        }

        /// <summary>
        /// Shows the discount changed.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="error">The error.</param>
        private void ShowDiscountChanged(string action, string error = " ")
        {
            divDiscountAdded.Visible = divDiscountRemoved.Visible = divDiscountAddedBy.Visible = divGeneralError.Visible = false;
            string sbAdminMail = Support.GetSystemValue("AdminEmail");
            paymentPackageSummary.CompanyId = CompanyId;
            paymentPackageSummary.ProjectPaymentPackageDetailId = PricePlanDetails.ProjectPaymentPackageTypeId;
            paymentPackageSummary.InventoryPaymentPackageDetailId = PricePlanDetails.InventoryPaymentPackageTypeId;
            paymentPackageSummary.PaymentDurationTypeCodeId = PricePlanDetails.PaymentDurationCodeId;
            paymentPackageSummary.UpdateDiscountCode(this.DiscountCodeUsage);
            paymentPackageSummary.UpdatePaymentSummary();
            paymentPackageSummary.SetReadOnlyDiscountCode();
            switch (action)
            {
                case "Added":
                    divDiscountAdded.Visible = true;
                    divDiscountAddedBy.Visible = true;
                    ltrDiscountAddedBy.Text = this.DiscountCodeUsage.IsAdminApplied ? "StageBitz has given you a Promotional Code." : "A Promotional Code has been applied by another Company Admin.";
                    popupConcurrentDiscountChanged.Title = this.DiscountCodeUsage.IsAdminApplied ? "You have been given a Promotional Code" : "Promotional Code Applied";
                    ltrDiscountValue.Text = (int)this.DiscountCodeUsage.DiscountCode.Discount + "%";
                    ltrChangedDiscountExpiryDate.Text = Utils.FormatDate(this.DiscountCodeUsage.EndDate);
                    break;

                case "Removed":
                    divDiscountRemoved.Visible = true;
                    lnkEmail.HRef = "mailto:" + sbAdminMail;
                    popupConcurrentDiscountChanged.Title = "The Promotional Code you have entered has been removed";
                    break;

                case "Changed":
                    divDiscountAdded.Visible = true;
                    popupConcurrentDiscountChanged.Title = "The Promotional Code has been changed";
                    ltrDiscountValue.Text = (int)this.DiscountCodeUsage.DiscountCode.Discount + "%";
                    ltrChangedDiscountExpiryDate.Text = Utils.FormatDate(this.DiscountCodeUsage.EndDate);
                    break;

                case "Expired":
                    divDiscountRemoved.Visible = true;
                    lnkEmail.HRef = "mailto:" + sbAdminMail;
                    popupConcurrentDiscountChanged.Title = "The Promotional Code you have entered has expired";
                    break;

                default:
                    divGeneralError.Visible = true;
                    popupConcurrentDiscountChanged.Title = "Error";
                    litGeneralError.Text = error;
                    break;
            }
            if (this.DisplayMode == ViewMode.PricingPlanPage)
            {
                popupConcurrentDiscountChanged.ShowPopup();
            }
            else
            {
                popupConcurrentDiscountChanged.ShowPopup(1001);
            }
        }

        /// <summary>
        /// Shows the payment failed concurrency popup.
        /// </summary>
        private void ShowPaymentFailedConcurrencyPopup()
        {
            lnkBillingPage.NavigateUrl = string.Format("~/Company/CompanyFinancialDetails.aspx?CompanyId={0}", this.CompanyId);
            spanPaymentFailed.Visible = true;
            spanSBAdminSuspened.Visible = false;
            popupConcurrency.ShowPopup();
        }

        /// <summary>
        /// Shows the sb admin suspended popup.
        /// </summary>
        private void ShowSBAdminSuspendedPopup()
        {
            string feedBackEmail = Utils.GetSystemValue("FeedbackEmail");
            lnkContactSBSupport.NavigateUrl = string.Concat("mailto:", feedBackEmail);
            lnkContactSBSupport.Text = feedBackEmail;
            spanPaymentFailed.Visible = false;
            spanSBAdminSuspened.Visible = true;
            popupConcurrency.ShowPopup();
        }

        /// <summary>
        /// Determines whether [is company in valid status]. If company status been changed to read only. if so show validation popups.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is company in valid status]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCompanyValidToSave()
        {
            if (CompanyId > 0 && Support.IsReadOnlyRightsForPricingPlanPage(CompanyId))
            {
                if (this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId))
                {
                    ShowSBAdminSuspendedPopup();
                }
                else
                {
                    ShowPaymentFailedConcurrencyPopup();
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the concurrent discount changes.
        /// </summary>
        /// <returns></returns>
        private bool HandleConcurrentDiscountChanges()
        {
            bool changed = false;
            if (!PricePlanDetails.IsEducationalPackage)
            {
                if (CompanyId > 0)
                {
                    this.DiscountCodeUsage = GetBL<FinanceBL>().GetDiscountCodeUsageByDate(Utils.Today, CompanyId);

                    //New Discount Added concurrently
                    if (PricePlanDetails.DiscountCodeUsage == null && this.DiscountCodeUsage != null)
                    {
                        changed = true;
                        ShowDiscountChanged("Added");
                    }//Discount Code has expired
                    else if (PricePlanDetails.DiscountCodeUsage != null && PricePlanDetails.DiscountCodeUsage.EndDate < Utils.Today)
                    {
                        changed = true;
                        ShowDiscountChanged("Expired");
                    }
                    else if (PricePlanDetails.DiscountCodeUsage != null)
                    {
                        //If there was a previous discount usage and now it's inactive means it is removed
                        List<DiscountCodeUsage> discountCodeUsages = GetBL<FinanceBL>().GetDiscountCodeUsages(PricePlanDetails.DiscountCodeUsage.DiscountCode.DiscountCodeID, true);
                        if (discountCodeUsages.Where(dcu => dcu.DiscountCodeUsageId == PricePlanDetails.DiscountCodeUsage.DiscountCodeUsageId && dcu.IsActive == false).FirstOrDefault() != null)
                        {
                            changed = true;
                            if (this.DiscountCodeUsage != null)
                            {
                                ShowDiscountChanged("Changed");
                            }
                            else
                            {
                                ShowDiscountChanged("Removed");
                            }
                        }
                    }
                }
                else
                {
                    if (DiscountCodeUsage != null && DiscountCodeUsage.DiscountCode != null)
                    {
                        string errormsg = string.Empty;

                        bool isValid = GetBL<FinanceBL>().IsDiscountCodeValidToUse(DiscountCodeUsage.DiscountCode.Code, CompanyId, out errormsg, DiscountCodeUsage.DiscountCode);

                        if (!isValid)
                        {
                            DiscountCode discountCode = GetBL<FinanceBL>().GetDiscountCodeByCodeId(DiscountCodeUsage.DiscountCode.DiscountCodeID);

                            //Make sure that the Discount has not been deleted and instance count has not been reached
                            if (discountCode != null && !GetBL<FinanceBL>().HasDiscountLimitReached(discountCode) && !GetBL<FinanceBL>().IsDiscountCodeExpired(discountCode))
                            {
                                DiscountCodeUsage.DiscountCode = discountCode; //Assign the new Discount code
                                DiscountCodeUsage = GetBL<FinanceBL>().SaveDiscountCodeUsageToCompany(DiscountCodeUsage.DiscountCode, UserID, CompanyId);//Build the new usage (Since duration can be get changed)
                            }
                            else
                                DiscountCodeUsage = null;

                            paymentPackageSummary.UpdateDiscountCode(DiscountCodeUsage);

                            ShowDiscountChanged("", errormsg);
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// Shows the popup for no projects package selection during free trail.
        /// </summary>
        private void ShowPopupForNoProjectsPackageSelectionDuringFreeTrail()
        {
            var freeTrailProject = GetBL<ProjectBL>().GetFreeTrialProjectsNotInClosedStatus(this.CompanyId).FirstOrDefault();
            if (freeTrailProject != null)
            {
                lblFreeTrailProject.Text = freeTrailProject.ProjectName;
                lblFreeTrailProjectEndDate.Text = Support.FormatDate(freeTrailProject.ExpirationDate);
                popupNoProjectsPackageSelectionDuringFreeTrail.ShowPopup();
            }
        }

        /// <summary>
        /// Shows the popup no package changes.
        /// </summary>
        private void ShowPopupNoPackageChanges()
        {
            popupNoPackageChanges.ShowPopup();
        }

        #endregion PrivateMethods

        #region Public Methods

        /// <summary>
        /// Resets the popup selections.
        /// </summary>
        public void ResetPopupSelections()
        {
            HasRespondedToNoPackageChangesPopup = false;
            HasRespondedToFreeTrailNoProjectsPopup = false;
        }

        /// <summary>
        /// Validates the package configurations.
        /// </summary>
        /// <param name="pricePlanDetails">The price plan details.</param>
        public void ValidatePackageConfigurations(PricePlanDetails pricePlanDetails)
        {
            PricePlanDetails = pricePlanDetails;

            if (!IsCompanyValidToSave())
            {
                return;
            }

            if (PricePlanDetails.ProjectPaymentPackageTypeId > 0 && PricePlanDetails.InventoryPaymentPackageTypeId > 0 && PricePlanDetails.PaymentDurationCodeId > 0)
            {
                ProjectPaymentPackageDetails projectPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(PricePlanDetails.ProjectPaymentPackageTypeId);
                InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(PricePlanDetails.InventoryPaymentPackageTypeId);
                //Get the current usage of the company. If exceeds the package show the error message
                string errorOnUsage = this.GetBL<FinanceBL>().GettheValidityOfSelectedPlan(CompanyId, pricePlanDetails, projectPackageDetails, inventoryPaymentPackageDetails);

                this.DisplayMode = ViewMode.PricingPlanPage; //concurrent discount chaged popups come on top of pricing plan page. view mode set accordingly.
                if (errorOnUsage.Length > 0)
                {
                    //Show the error message.
                    litError.Text = errorOnUsage;
                    popupError.ShowPopup();
                }
                else
                {
                    //Assign the InMemory Company object
                    if (CompanyId == 0)
                    {
                        paymentDetails.NewCompany = NewCompany;
                        paymentDetails.Media = Media;
                    }

                    CompanyPaymentPackage oldCompanyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);

                    DateTime packageStartDate = Utils.Today;//For Normal company, the package start date should be Today.
                    //Find the package start date
                    if (this.GetBL<CompanyBL>().IsFreeTrialCompany(CompanyId))
                    {
                        //For Free Trial Companies, Start Date should be FT end Date + 1
                        packageStartDate = this.GetBL<CompanyBL>().GetFreeTrialProjectEndDate(CompanyId).AddDays(1);
                    }
                    else
                    {
                        if (oldCompanyPaymentPackage != null)
                        {
                            if (oldCompanyPaymentPackage.PaymentDurationCodeId == Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL"))
                            {
                                if (oldCompanyPaymentPackage.ProjectPaymentPackageTypeId != PricePlanDetails.ProjectPaymentPackageTypeId || oldCompanyPaymentPackage.InventoryPaymentPackageTypeId != PricePlanDetails.InventoryPaymentPackageTypeId)
                                {
                                    packageStartDate = Utils.Today;
                                }
                                else
                                {
                                    if (oldCompanyPaymentPackage.EndDate != null)
                                        packageStartDate = oldCompanyPaymentPackage.EndDate.Value.AddDays(1);
                                    else
                                        packageStartDate = oldCompanyPaymentPackage.StartDate.AddYears(1).AddDays(1);
                                }
                            }
                            else if (oldCompanyPaymentPackage.EndDate != null)
                                packageStartDate = oldCompanyPaymentPackage.EndDate.Value;
                        }
                    }
                    PricePlanDetails.PackageStartDate = packageStartDate;

                    OriginalLastUpdatedDate = oldCompanyPaymentPackage != null ? oldCompanyPaymentPackage.LastUpdateDate : null;

                    // No projects selected during free trial period
                    if (!HasRespondedToFreeTrailNoProjectsPopup && this.CompanyId > 0 && projectPackageDetails.Amount == 0 && GetBL<CompanyBL>().IsFreeTrialCompany(this.CompanyId))
                    {
                        ShowPopupForNoProjectsPackageSelectionDuringFreeTrail();
                    }
                    // free inventory scenario
                    else if ((projectPackageDetails.Amount + inventoryPaymentPackageDetails.Amount) == 0)
                    {
                        ShowFreeInventoryOnlyPopUp(inventoryPaymentPackageDetails.ItemCount.Value);
                    }
                    else if (HandleConcurrentDiscountChanges())
                    {
                        this.PricePlanDetails.DiscountCodeUsage = this.DiscountCodeUsage;
                        this.PricePlanDetails.DiscountCode = (this.DiscountCodeUsage != null ? this.DiscountCodeUsage.DiscountCode : null);
                    }
                    else if (!HasRespondedToNoPackageChangesPopup && !this.GetBL<FinanceBL>().IsCompanyPaymentPackageChanged(this.CompanyId, pricePlanDetails))
                    {
                        ShowPopupNoPackageChanges();
                    }
                    else if (!PricePlanDetails.IsEducationalPackage && (PricePlanDetails.DiscountCodeUsage != null &&
                        PricePlanDetails.DiscountCodeUsage.DiscountCode.Discount == 100 && PricePlanDetails.TotalAmountForPeriod == 0))
                    {
                        ShowHundredPercentDiscountPopUp(PricePlanDetails.DiscountCodeUsage);
                    }
                    else
                    {
                        ShowAdddetailsPaymentpopUp();
                    }
                }
            }
        }

        #endregion Public Methods
    }
}