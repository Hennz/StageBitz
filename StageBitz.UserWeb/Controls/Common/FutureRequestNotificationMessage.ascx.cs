using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Finance.Company;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Text;
using System.Web.Configuration;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// UserControl for Future Request Notification Message.
    /// </summary>
    public partial class FutureRequestNotificationMessage : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the price plan details.
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
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width
        {
            get
            {
                if (ViewState["Width"] == null)
                {
                    ViewState["Width"] = 800;
                }

                return (int)ViewState["Width"];
            }
            set
            {
                ViewState["Width"] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            if (CompanyId > 0)
            {
                Data.Company company = this.GetBL<CompanyBL>().GetCompany(CompanyId);
                CompanyStatusHandler.CompanyWarningInfo warningInfo = CompanyStatusHandler.GetCompanyWarningStatus(CompanyId, company.CompanyStatusCodeId, company.ExpirationDate);
                if (warningInfo.WarningStatus == CompanyStatusHandler.CompanyWarningStatus.NoWarning)
                {
                    var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                    int anualDurationCodeId = (int)Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
                    int monthlyDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
                    CompanyBL companyBL = new CompanyBL(DataContext);
                    CompanyPaymentPackage oldPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);

                    if (!((companyBL.IsFreeTrialStatusIncludedFortheDay(CompanyId, Utils.Today)) || oldPackage == null))  //free trial scenario and first time configuration. In this case no banners to display.
                    {
                        //comming from a popup. Refer PBI 14218.If user changes from yearly->monthly and downgrading or upgrading we should display banners in AC2,AC3.5. Else if it is a downgrade, display the banner in AC 3.4. If it is just an option change or upgrade with option change, then display banner in AC1.
                        if (PricePlanDetails != null)
                        {
                            divNotifyFutureRequest.Style.Add("Width", Width.ToString() + "px");

                            #region Calculations

                            int durationDifference = this.GetBL<FinanceBL>().GetDurationDifference(oldPackage.StartDate, Utils.Today, oldPackage.PaymentDurationCodeId);
                            DateTime endDate = Utils.Today;
                            DateTime newBillingDate = Utils.Today;
                            DateTime virtualBillingDate = Utils.Today;
                            string newOption = PricePlanDetails.PaymentDurationCodeId == (int)Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL") ? "yearly" : "monthly";
                            string currentOption = oldPackage.PaymentDurationCodeId == (int)Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL") ? "yearly" : "monthly";

                            //In order to find whether it is an upgrade or downgrade,
                            decimal newAmount = this.GetBL<FinanceBL>().CalculateALLPackageAmountsByPeriod(PricePlanDetails.ProjectPaymentPackageTypeId, PricePlanDetails.InventoryPaymentPackageTypeId, oldPackage.PaymentDurationCodeId);
                            decimal oldAmount = this.GetBL<FinanceBL>().CalculateALLPackageAmountsByPeriod(oldPackage.ProjectPaymentPackageTypeId, oldPackage.InventoryPaymentPackageTypeId, oldPackage.PaymentDurationCodeId);
                            int dayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));

                            if (oldPackage.PaymentDurationCodeId == anualDurationCodeId)
                            {
                                endDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddYears(durationDifference);

                                if (PricePlanDetails.PaymentDurationCodeId == monthlyDurationCodeId)
                                {
                                    virtualBillingDate = new DateTime(endDate.Year, endDate.Month, dayToRun);
                                    newBillingDate = virtualBillingDate > endDate ? virtualBillingDate : virtualBillingDate.AddMonths(1);
                                }
                            }
                            else
                            {
                                endDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddMonths(durationDifference);

                                if (PricePlanDetails.PaymentDurationCodeId == anualDurationCodeId)
                                {
                                    virtualBillingDate = new DateTime(endDate.Year, endDate.Month, dayToRun);
                                    newBillingDate = virtualBillingDate > endDate ? virtualBillingDate : virtualBillingDate.AddMonths(1);
                                }
                            }

                            #endregion Calculations

                            #region creating banners

                            if (endDate == Utils.Today)
                            {
                                pnlNotifyFutureRequest.Visible = false;
                            }
                            else if (PricePlanDetails.PaymentDurationCodeId != oldPackage.PaymentDurationCodeId) //Duration change
                            {
                                string displayText = string.Empty;
                                if (PricePlanDetails.PaymentDurationCodeId == monthlyDurationCodeId && newAmount != oldAmount)
                                {
                                    if (newAmount < oldAmount)  //downgrade yearly->monthly
                                    {
                                        displayText = string.Format("<b>A little note on downgrades…</b> We’re glad you’ve found the levels that are right for you in StageBitz, but we do need to let you know that we can’t offer refunds, as Yearly subscriptions are paid in advance. " +
                                            "You’re currently paid up until {0}. Your choice will be effective immediately so if you would like to continue to enjoy the higher subscription level until that date please downgrade later." +
                                        " After that, we’ll change your billing to the lower level you’ve chosen. Your {1} charges will start on {2}.", Utils.FormatDate(endDate), newOption, Utils.FormatDate(newBillingDate));
                                    }
                                    else if (newAmount > oldAmount)  //upgrade yearly->monthly
                                    {
                                        PaymentSummaryDetails paymentSummaryDetail = this.GetBL<FinanceBL>().GetPaymentSummaryDetailRecord(PricePlanDetails, true);
                                        int totalDays = (int)(endDate - Utils.Today).TotalDays;
                                        decimal prorataAmount = ProjectUsageHandler.GetProrataAmount(paymentSummaryDetail, oldPackage, this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId), newAmount, oldAmount, endDate, totalDays);

                                        StringBuilder upgradeText = new StringBuilder();
                                        upgradeText.Append(string.Format("You have selected to upgrade your plan and pay {0}. ", newOption));
                                        if (prorataAmount > 0)
                                        {
                                            upgradeText.Append(string.Format("You will be charged a pro-rata amount of {0} for your upgrade. ", Utils.FormatCurrency(prorataAmount, globalizationSection.Culture)));
                                        }
                                        upgradeText.Append(string.Format("Your {0} charges will start on {1}.", newOption, Utils.FormatDate(newBillingDate)));
                                        displayText = upgradeText.ToString();
                                    }
                                }
                                else
                                {
                                    if (newAmount < oldAmount)//even for monthly->yearly downgrade we should display downgrade banner
                                    {
                                        displayText = string.Format("<b>A little note on downgrades…</b> We’re glad you’ve found the levels that are right for you in StageBitz, but we do need to let you know that we can’t offer refunds, as subscriptions are paid in advance. " +
                                       "You’re currently paid up until {0}. Your choice will be effective immediately so if you would like to continue to enjoy the higher subscription level until that date please downgrade later.", Utils.FormatDate(endDate));
                                    }
                                    else//only an option change (yearly->monthly or monthly->yearly).Even if it is a monthly->yearly upgrade we display this banner.
                                    {
                                        displayText = string.Format("You are currently paying {0}, but you have opted to pay {1} as of {2}.",
                                            currentOption, newOption, Utils.FormatDate(endDate));
                                    }
                                }
                                divNotifyFutureRequest.InnerHtml = displayText;
                                pnlNotifyFutureRequest.Visible = true;
                            }
                            else
                            {
                                if (newAmount < oldAmount)  //downgrade only.
                                {
                                    string downgradeOnlyText = string.Format("<b>A little note on downgrades…</b> We’re glad you’ve found the levels that are right for you in StageBitz, but we do need to let you know that we can’t offer refunds, as subscriptions are paid in advance. " +
                                        "You’re currently paid up until {0}. Your choice will be effective immediately so if you would like to continue to enjoy the higher subscription level until that date please downgrade later.", Utils.FormatDate(endDate));
                                    divNotifyFutureRequest.InnerHtml = downgradeOnlyText;
                                    pnlNotifyFutureRequest.Visible = true;
                                }
                                else
                                {
                                    pnlNotifyFutureRequest.Visible = false;
                                }
                            }

                            #endregion creating banners
                        }
                        else  //Comming from company billing or price plan page
                        {
                            CompanyPaymentPackage futurePackage = this.GetBL<FinanceBL>().GetLatestRequestForTheCompany(CompanyId);
                            DateTime newDurationStartDate = Utils.Today;
                            if (futurePackage != null && oldPackage != null && futurePackage.PaymentDurationCodeId != oldPackage.PaymentDurationCodeId)
                            {
                                string currentPaymentDuration = string.Empty;
                                string newPaymentDuration = string.Empty;
                                int durationDifference = this.GetBL<FinanceBL>().GetDurationDifference(oldPackage.StartDate, Utils.Today, oldPackage.PaymentDurationCodeId);

                                if (oldPackage.PaymentDurationCodeId == anualDurationCodeId)
                                {
                                    currentPaymentDuration = "yearly";
                                    newPaymentDuration = "monthly";
                                    newDurationStartDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddMonths(durationDifference);
                                }
                                else
                                {
                                    currentPaymentDuration = "monthly";
                                    newPaymentDuration = "yearly";
                                    newDurationStartDate = oldPackage.EndDate != null ? (DateTime)oldPackage.EndDate : (DateTime)oldPackage.StartDate.AddYears(durationDifference);
                                }
                                string optionChangedText = string.Format("You are currently paying {0}, but you have opted to pay {1} as of {2}.", currentPaymentDuration, newPaymentDuration, Utils.FormatDate(newDurationStartDate));
                                divNotifyFutureRequest.InnerHtml = optionChangedText;
                                pnlNotifyFutureRequest.Visible = true;
                            }
                            else
                            {
                                pnlNotifyFutureRequest.Visible = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion Public Methods
    }
}