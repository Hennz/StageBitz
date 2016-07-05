using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for company details.
    /// </summary>
    public partial class CompanyDetails : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            CompanyDetail,
            PaymentDetail,
            CreateNewCompany
        }

        #endregion Enums

        #region properties

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
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public Data.Company Company
        {
            get
            {
                if (ViewState["Company"] == null)
                {
                    ViewState["Company"] = new Data.Company();
                }
                return (Data.Company)ViewState["Company"];
            }

            set
            {
                ViewState["Company"] = value;
            }
        }

        #endregion properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (DisplayMode == ViewMode.CompanyDetail || DisplayMode == ViewMode.CreateNewCompany)
                {
                    divLeftSection.Style.Add("width", "900px");
                    divSeperator.Visible = false;
                    divRightSection.Style.Add("width", "900px");
                    trPersonalPhone.Visible = false;
                }
                else
                {
                    divLeftSection.Style.Add("float", "left");
                    divLeftSection.Style.Add("width", "400px");
                    divSeperator.Visible = true;
                    divRightSection.Style.Add("float", "left");
                    divRightSection.Style.Add("width", "400px");
                    trPersonalPhone.Visible = true;
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Initializes the controls.
        /// </summary>
        /// <param name="isReadOnly">if set to <c>true</c> [is read only].</param>
        public void InitializeControls(bool isReadOnly)
        {
            txtCompanyName.ReadOnly = isReadOnly;
            txtAddressLine1.ReadOnly = isReadOnly;
            txtAddressLine2.ReadOnly = isReadOnly;
            txtCity.ReadOnly = isReadOnly;
            txtState.ReadOnly = isReadOnly;
            txtPostalCode.ReadOnly = isReadOnly;
            txtCompanyPhone.ReadOnly = isReadOnly;
            txtWebsite.ReadOnly = isReadOnly;
            ucCountryList.IsReadOnly = isReadOnly;

            if (DisplayMode == ViewMode.PaymentDetail)
            {
                txtPersonalPhone.ReadOnly = isReadOnly;
            }
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="company">The company.</param>
        public void LoadData(Data.Company company)
        {
            this.Company = company;

            if (DisplayMode == ViewMode.PaymentDetail)
            {
                Data.User user = this.GetBL<PersonalBL>().GetUser(UserID);
                txtPersonalPhone.Text = user != null ? user.Phone1 : "";
            }

            txtCompanyName.Text = company.CompanyName;
            txtAddressLine1.Text = company.AddressLine1;
            txtAddressLine2.Text = company.AddressLine2;
            txtCity.Text = company.City;
            txtState.Text = company.State;
            txtPostalCode.Text = company.PostCode;
            txtCompanyPhone.Text = company.Phone;
            txtWebsite.Text = company.Website;

            if (company.CountryId.HasValue)
                ucCountryList.SelectCountryId(company.CountryId.Value);
        }

        /// <summary>
        /// Sets the validation group.
        /// </summary>
        /// <param name="validationGroup">The validation group.</param>
        public void SetValidationGroup(string validationGroup)
        {
            rfvCompanyName.ValidationGroup = validationGroup;
            rfvAddress1.ValidationGroup = validationGroup;
            rfvCity.ValidationGroup = validationGroup;
            rfvState.ValidationGroup = validationGroup;
            rfvPostalCode.ValidationGroup = validationGroup;
            rfvCompanyPhone.ValidationGroup = validationGroup;
            revWebSite.ValidationGroup = validationGroup;
            revCompanyPhone.ValidationGroup = validationGroup;
            revPersonalPhone.ValidationGroup = validationGroup;
            ucCountryList.ValidationGroup = validationGroup;

            if (DisplayMode == ViewMode.PaymentDetail)
            {
                rfvPersonalPhone.ValidationGroup = validationGroup;
            }
        }

        /// <summary>
        /// Saves the company details.
        /// </summary>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        /// <param name="shouldIncludeToContext">if set to <c>true</c> [should include to context].</param>
        /// <returns></returns>
        public Data.Company SaveCompanyDetails(bool commit, bool shouldIncludeToContext = true)
        {
            int companyPrimaryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int inventoryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");

            //If the company already exists
            if (this.Company.CompanyId > 0)
            {
                //Get the existing company objetc
                this.Company = this.GetBL<CompanyBL>().GetCompany(this.Company.CompanyId);
            }

            this.Company.CompanyName = txtCompanyName.Text.Trim();
            this.Company.AddressLine1 = txtAddressLine1.Text.Trim();
            this.Company.AddressLine2 = txtAddressLine2.Text.Trim();
            this.Company.City = txtCity.Text.Trim();
            this.Company.State = txtState.Text.Trim();
            this.Company.PostCode = txtPostalCode.Text.Trim();
            this.Company.Phone = txtCompanyPhone.Text.Trim();
            this.Company.Website = txtWebsite.Text.Trim();

            // Only in the very first time.
            if (this.Company.CompanyId == 0)
            {
                this.Company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
                //this.Company.IsCompanyVisibleForSearchInInventory = true;
            }

            this.Company.CountryId = ucCountryList.CountryID;
            this.Company.LastUpdatedByUserId = UserID;
            this.Company.LastUpdatedDate = Now;
            this.Company.IsActive = true;

            if (this.Company.CompanyId == 0)
            {
                this.Company.CreatedByUserId = UserID;
                this.Company.CreatedDate = Now;
                if (shouldIncludeToContext)
                    DataContext.Companies.AddObject(this.Company);

                //Should added only once.
                if (Company.CompanyUsers != null && Company.CompanyUsers.Count == 0)
                {
                    //Save companyusers record
                    CompanyUser companyUser = new CompanyUser();

                    companyUser.UserId = UserID;
                    //companyUser.CompanyUserTypeCodeId = companyPrimaryAdminCodeID;

                    companyUser.CreatedDate = Now;
                    companyUser.CreatedByUserId = UserID;
                    companyUser.LastUpdatedByUserId = UserID;
                    companyUser.LastUpdatedDate = Now;
                    companyUser.IsActive = true;

                    companyUser.CompanyUserRoles.Add(
                        new CompanyUserRole
                        {
                            IsActive = true,
                            CompanyUserTypeCodeId = companyPrimaryAdminCodeID,
                            CreatedByUserId = UserID,
                            CreatedDate = Now,
                            LastUpdatedByUserId = UserID,
                            LastUpdatedDate = Now
                        }
                    );

                    companyUser.CompanyUserRoles.Add(
                        new CompanyUserRole
                        {
                            IsActive = true,
                            CompanyUserTypeCodeId = inventoryAdminCodeID,
                            CreatedByUserId = UserID,
                            CreatedDate = Now,
                            LastUpdatedByUserId = UserID,
                            LastUpdatedDate = Now
                        }
                    );

                    this.Company.CompanyUsers.Add(companyUser);
                }
            }

            if (DisplayMode == ViewMode.PaymentDetail)
            {
                SavePersonalPhone();
            }

            if (commit)
            {
                this.GetBL<CompanyBL>().SaveChanges();
            }
            return Company;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Saves the personal phone.
        /// </summary>
        private void SavePersonalPhone()
        {
            Data.User user = this.GetBL<PersonalBL>().GetUser(UserID);
            user.Phone1 = txtPersonalPhone.Text.Trim();
        }

        #endregion Private Methods
    }
}