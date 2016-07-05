using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Finance.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// Display warning banner when company is suspended by Stagebitz admin.
    /// </summary>
    public partial class CompanyWarningDisplay : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for notice type.
        /// See attachment in PBI #8804
        /// </summary>
        private enum NoticeType
        {
            SBAdminSuspendedCompanyAdmin = 0,
            SBAdminSuspendedNonCompanyAdmin = 1,

            FreeTrailEndNoPaymentPackage = 2,

            GracePeriodCompanyAdmin = 3,
            GracePeriodNonCompanyAdmin = 4,

            PaymentFailedCompanyAdmin = 5,
            PaymentFailedNonCompanyAdmin = 6,

            SuspendedForNoPaymentOptions = 7
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the company ID.
        /// </summary>
        /// <value>
        /// The company ID.
        /// </value>
        public int CompanyID
        {
            get
            {
                if (this.ViewState["CompanyID"] == null)
                {
                    this.ViewState["CompanyID"] = 0;
                }
                return (int)this.ViewState["CompanyID"];
            }
            set
            {
                this.ViewState["CompanyID"] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            var company = GetBL<CompanyBL>().GetCompany(this.CompanyID);
            if (company == null)
            {
                return;
            }

            bool isCompanyAdmin = Support.IsCompanyAdministrator(CompanyID);
            User primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(this.CompanyID);
            string primaryAdminName = string.Concat(primaryAdmin.FirstName, " ", primaryAdmin.LastName);
            string primaryAdminEmailUrl = string.Concat("mailto:", primaryAdmin.Email1);

            CompanyStatusHandler.CompanyWarningInfo warningInfo = CompanyStatusHandler.GetCompanyWarningStatus(CompanyID, company.CompanyStatusCodeId, company.ExpirationDate);
            if (warningInfo.WarningStatus == CompanyStatusHandler.CompanyWarningStatus.NoWarning)
            {
                noticesMultiView.Visible = false;
            }
            else
            {
                noticesMultiView.Visible = true;

                switch (warningInfo.WarningStatus)
                {
                    case CompanyStatusHandler.CompanyWarningStatus.SBAdminSuspended:
                        if (isCompanyAdmin)
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.SBAdminSuspendedCompanyAdmin;
                            lblCompanyNameSBAdminSuspendCA.Text = company.CompanyName;

                            string feedbackEmail = Support.GetSystemValue("FeedbackEmail");
                            string feedbackEmailUrl = string.Concat("mailto:", feedbackEmail);
                            lnkContactSBSupport.Text = feedbackEmail;
                            lnkContactSBSupport.NavigateUrl = feedbackEmailUrl;
                        }
                        else
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.SBAdminSuspendedNonCompanyAdmin;
                            lblCompanyNameSBAdminSuspendNonCA.Text = company.CompanyName;

                            if (primaryAdmin != null)
                            {
                                lnkPrimaryComapnyAdmin.Text = primaryAdminName;
                                lnkPrimaryComapnyAdmin.NavigateUrl = primaryAdminEmailUrl;
                            }
                        }
                        break;

                    case CompanyStatusHandler.CompanyWarningStatus.PaymentFailed:
                        if (isCompanyAdmin)
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.PaymentFailedCompanyAdmin;
                            ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin.CompanyID = CompanyID;
                            ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.CompanyAdministrator,
                                    CompanyPaymentFailedWarning.DisplayMode.PaymentFailed);
                        }
                        else
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.PaymentFailedNonCompanyAdmin;
                            ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin.CompanyID = CompanyID;
                            ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.NonCompanyAdministrator,
                                    CompanyPaymentFailedWarning.DisplayMode.PaymentFailed);
                        }
                        break;

                    case CompanyStatusHandler.CompanyWarningStatus.PaymentFailedGracePeriod:
                        if (isCompanyAdmin)
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.GracePeriodCompanyAdmin;
                            ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin.CompanyID = CompanyID;
                            ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.CompanyAdministrator,
                                    CompanyPaymentFailedWarning.DisplayMode.PaymentFailedGracePeriod);
                        }
                        else
                        {
                            noticesMultiView.ActiveViewIndex = (int)NoticeType.GracePeriodNonCompanyAdmin;
                            ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin.CompanyID = CompanyID;
                            ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.NonCompanyAdministrator,
                                    CompanyPaymentFailedWarning.DisplayMode.PaymentFailedGracePeriod);
                        }
                        break;

                    case CompanyStatusHandler.CompanyWarningStatus.FreeTrailEndNoPaymentPackage:
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.FreeTrailEndNoPaymentPackage;
                        if (isCompanyAdmin)
                        {
                            divFreeTrialEndedCA.Visible = true;
                            divFreeTrialEndedNonCA.Visible = false;

                            lnkPricingPlanPage.NavigateUrl = string.Format("~/Company/CompanyPricingPlans.aspx?companyId={0}", this.CompanyID);
                        }
                        else
                        {
                            divFreeTrialEndedCA.Visible = false;
                            divFreeTrialEndedNonCA.Visible = true;

                            lblFreeTrialEndedCompanyName.Text = company.CompanyName;
                            lnkFreeTrialEndedContactPrimaryComapnyAdmin.Text = primaryAdminName;
                            lnkFreeTrialEndedContactPrimaryComapnyAdmin.NavigateUrl = primaryAdminEmailUrl;
                        }
                        break;

                    case CompanyStatusHandler.CompanyWarningStatus.SuspendedForNoPaymentOptions:
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.SuspendedForNoPaymentOptions;
                        if (isCompanyAdmin)
                        {
                            divNopamentOptionCA.Visible = true;
                            divNopamentOptionNonCA.Visible = false;
                            lnkNopamentOptionPricingPlanPage.NavigateUrl = string.Format("~/Company/CompanyPricingPlans.aspx?companyId={0}", this.CompanyID);
                        }
                        else
                        {
                            divNopamentOptionCA.Visible = false;
                            divNopamentOptionNonCA.Visible = true;
                            lblNoPaymentOptionCompanyName.Text = company.CompanyName;

                            if (primaryAdmin != null)
                            {
                                lnkNopamentOptionContactPrimaryComapnyAdmin.Text = primaryAdminName;
                                lnkNopamentOptionContactPrimaryComapnyAdmin.NavigateUrl = primaryAdminEmailUrl;
                            }
                        }

                        break;
                }
            }

            upnlCompanyWarningDisplay.Update();
        }

        #endregion Public Methods
    }
}