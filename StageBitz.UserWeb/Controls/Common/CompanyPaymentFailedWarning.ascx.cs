using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Finance.Company;
using StageBitz.UserWeb.Common.Helpers;
using System.Globalization;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for Company payment failed warning.
    /// </summary>
    public partial class CompanyPaymentFailedWarning : UserControlBase
    {
        #region Properties

        /// <summary>
        /// The primary admin var.
        /// </summary>
        private Data.User _primaryAdmin;

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

        /// <summary>
        /// Gets or sets the payment failed date.
        /// </summary>
        /// <value>
        /// The payment failed date.
        /// </value>
        public string PaymentFailedDate
        {
            get
            {
                if (this.ViewState["PaymentFailedDate"] == null)
                {
                    if (Company.ExpirationDate.HasValue)
                    {
                        this.ViewState["PaymentFailedDate"] = Support.FormatDate(Company.ExpirationDate.Value.AddDays(-7));
                    }
                    else
                    {
                        this.ViewState["PaymentFailedDate"] = string.Empty;
                    }
                }

                return (string)this.ViewState["PaymentFailedDate"];
            }

            set
            {
                this.ViewState["PaymentFailedDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the primary company admin.
        /// </summary>
        /// <value>
        /// The primary company admin.
        /// </value>
        public string PrimaryCompanyAdminName
        {
            get
            {
                if (this.ViewState["PrimaryCompanyAdminName"] == null)
                {
                    string name = string.Empty;
                    if (_primaryAdmin == null)
                    {
                        _primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID);
                    }

                    if (_primaryAdmin != null)
                    {
                        name = Support.TruncateString((_primaryAdmin.FirstName + " " + _primaryAdmin.LastName).Trim(), 30);
                    }

                    this.ViewState["PrimaryCompanyAdminName"] = name;
                }

                return (string)this.ViewState["PrimaryCompanyAdminName"];
            }

            set
            {
                this.ViewState["PrimaryCompanyAdminName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the primary company admin.
        /// </summary>
        /// <value>
        /// The primary company admin.
        /// </value>
        public string PrimaryCompanyAdminEmail
        {
            get
            {
                if (this.ViewState["PrimaryCompanyAdminEmail"] == null)
                {
                    string email = string.Empty;
                    if (_primaryAdmin == null)
                    {
                        _primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyID);
                    }

                    if (_primaryAdmin != null)
                    {
                        email = _primaryAdmin.Email1;
                    }

                    this.ViewState["PrimaryCompanyAdminEmail"] = email;
                }

                return (string)this.ViewState["PrimaryCompanyAdminEmail"];
            }

            set
            {
                this.ViewState["PrimaryCompanyAdminEmail"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the remaining days.
        /// </summary>
        /// <value>
        /// The remaining days.
        /// </value>
        public string RemainingDays
        {
            get
            {
                if (this.ViewState["RemainingDays"] == null)
                {
                    CompanyStatusHandler.CompanyWarningInfo warningInfo = CompanyStatusHandler.GetCompanyWarningStatus(CompanyID, Company.CompanyStatusCodeId, Company.ExpirationDate);
                    this.ViewState["RemainingDays"] = warningInfo.DaysToExpiration.ToString(CultureInfo.InvariantCulture);
                }

                return (string)this.ViewState["RemainingDays"];
            }

            set
            {
                this.ViewState["RemainingDays"] = value;
            }
        }

        /// <summary>
        /// Gets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public Data.Company Company
        {
            get
            {
                if (this.ViewState["Company"] == null)
                {
                    this.ViewState["Company"] = this.GetBL<CompanyBL>().GetCompany(CompanyID);
                }

                return (Data.Company)this.ViewState["Company"];
            }
        }

        /// <summary>
        /// Gets the company name.
        /// </summary>
        /// <value>
        /// The company name.
        /// </value>
        public string CompanyName
        {
            get
            {
                if (this.ViewState["CompanyName"] == null && Company != null)
                {
                    this.ViewState["CompanyName"] = Company.CompanyName;
                }
                else
                {
                    this.ViewState["CompanyName"] = string.Empty;
                }

                return (string)this.ViewState["CompanyName"];
            }
        }

        /// <summary>
        /// Gets the support email.
        /// </summary>
        /// <value>
        /// The support email.
        /// </value>
        protected string SupportEmail
        {
            get
            {
                if (this.ViewState["SupportEmail"] == null)
                {
                    this.ViewState["SupportEmail"] = Support.GetSystemValue("FeedbackEmail");
                }

                return (string)this.ViewState["SupportEmail"];
            }
        }

        /// <summary>
        /// Gets the company financial URL.
        /// </summary>
        /// <value>
        /// The company financial URL.
        /// </value>
        protected string CompanyFinancialUrl
        {
            get
            {
                if (this.ViewState["CompanyFinancialUrl"] == null)
                {
                    this.ViewState["CompanyFinancialUrl"] = this.ResolveUrl(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyID));
                }

                return (string)this.ViewState["CompanyFinancialUrl"];
            }
        }

        #endregion Properties

        #region Enums

        public enum PermissionLevel
        {
            CompanyAdministrator,
            NonCompanyAdministrator
        }

        /// <summary>
        /// Enum for control display mode
        /// </summary>
        public enum DisplayMode
        {
            None,
            PaymentFailedGracePeriod,
            PaymentFailed
        }

        #endregion Enums

        #region Public Methods

        /// <summary>
        /// Enum for user permission level
        /// </summary>
        /// <param name="permissionLevel">The permission level.</param>
        /// <param name="displayMode">The display mode.</param>
        public void LoadData(PermissionLevel permissionLevel, DisplayMode displayMode)
        {
            HideAllBanners();

            if (Company != null)
            {
                switch (permissionLevel)
                {
                    case PermissionLevel.CompanyAdministrator:
                        switch (displayMode)
                        {
                            case DisplayMode.PaymentFailed:
                                pnlCompanyPaymentFailedCompanyAdmin.Visible = true;
                                break;

                            case DisplayMode.PaymentFailedGracePeriod:
                                pnlCompanyPaymentFailedGracePeriodCompanyAdmin.Visible = true;
                                break;
                        }
                        break;

                    case PermissionLevel.NonCompanyAdministrator:
                        switch (displayMode)
                        {
                            case DisplayMode.PaymentFailed:
                                pnlCompanyPaymentFailedNonCompanyAdmin.Visible = true;
                                break;

                            case DisplayMode.PaymentFailedGracePeriod:
                                pnlCompanyPaymentFailedGracePeriodNonCompanyAdmin.Visible = true;
                                break;
                        }
                        break;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Hides all banners.
        /// </summary>
        private void HideAllBanners()
        {
            pnlCompanyPaymentFailedCompanyAdmin.Visible = false;
            pnlCompanyPaymentFailedNonCompanyAdmin.Visible = false;

            pnlCompanyPaymentFailedGracePeriodCompanyAdmin.Visible = false;
            pnlCompanyPaymentFailedGracePeriodNonCompanyAdmin.Visible = false;
        }

        #endregion Private Methods
    }
}