using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User Control for Validate Pricing Plan Limits.
    /// </summary>
    public partial class PackageLimitsValidation : UserControlBase
    {
        private const int Z_Index = 1001;

        #region Enums

        /// <summary>
        /// Display mode of the control.
        /// </summary>
        public enum ViewMode
        {
            CreateProjects,
            ReactivateProjects,
            UserLimit,
            InventoryLimit
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
        /// Gets or sets the company id.
        /// </summary>
        /// <value>
        /// The company id.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }

            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project id. Used to get project id for user limit upgrade requests.
        /// </summary>
        /// <value>
        /// The project id.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }

            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the invited user id. Used to get the user id who is selected to invite
        /// </summary>
        /// <value>
        /// The selected user to invite user id.
        /// </value>
        public string InvitedUserEmail
        {
            get
            {
                if (ViewState["InvitedUserEmail"] == null)
                {
                    ViewState["InvitedUserEmail"] = string.Empty;
                }

                return (string)ViewState["InvitedUserEmail"];
            }
            set
            {
                ViewState["InvitedUserEmail"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default free trial user count.
        /// </summary>
        /// <value>
        /// The default free trial user count.
        /// </value>
        public int DefaultFreeTrialUserCount
        {
            get
            {
                if (ViewState["DefaultFreeTrialUserCount"] == null)
                {
                    ViewState["DefaultFreeTrialUserCount"] = int.Parse(Utils.GetSystemValue("DefaultProjectHeadCountForCompany"));
                }

                return (int)ViewState["DefaultFreeTrialUserCount"];
            }

            set
            {
                ViewState["DefaultFreeTrialUserCount"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default free trial project count.
        /// </summary>
        /// <value>
        /// The default free trial project count.
        /// </value>
        public int DefaultFreeTrialProjectCount
        {
            get
            {
                if (ViewState["DefaultFreeTrialProjectCount"] == null)
                {
                    ViewState["DefaultFreeTrialProjectCount"] = int.Parse(Utils.GetSystemValue("DefaultProjectCountForCompany"));
                }

                return (int)ViewState["DefaultFreeTrialProjectCount"];
            }

            set
            {
                ViewState["DefaultFreeTrialProjectCount"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default free trial inventory limit.
        /// </summary>
        /// <value>
        /// The default free trial inventory limit.
        /// </value>
        public int DefaultFreeTrialInventoryLimit
        {
            get
            {
                if (ViewState["DefaultFreeTrialInventoryLimit"] == null)
                {
                    ViewState["DefaultFreeTrialInventoryLimit"] = int.Parse(Utils.GetSystemValue("DefaultInventoryCountForCompany"));
                }

                return (int)ViewState["DefaultFreeTrialInventoryLimit"];
            }

            set
            {
                ViewState["DefaultFreeTrialInventoryLimit"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the primary company admin.
        /// </summary>
        /// <value>
        /// The primary company admin.
        /// </value>
        public string PrimaryCompanyAdmin
        {
            get
            {
                if (ViewState["PrimaryCompanyAdmin"] == null)
                {
                    var companyAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(this.CompanyId);
                    ViewState["PrimaryCompanyAdmin"] = companyAdmin != null ? string.Concat(companyAdmin.FirstName, " ", companyAdmin.LastName) : string.Empty;
                }

                return (string)ViewState["PrimaryCompanyAdmin"];
            }

            set
            {
                ViewState["PrimaryCompanyAdmin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is company admin.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is company admin; otherwise, <c>false</c>.
        /// </value>
        private bool IsCompanyAdmin
        {
            get
            {
                if (ViewState["IsCompanyAdmin"] == null)
                {
                    ViewState["IsCompanyAdmin"] = Support.IsCompanyAdministrator(this.CompanyId);
                }

                return (bool)ViewState["IsCompanyAdmin"];
            }

            set
            {
                ViewState["IsCompanyAdmin"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the btnSendEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Button button = sender as Button;
                if (button != null)
                {
                    string feedbackEmail = Support.GetSystemValue("FeedbackEmail");
                    User primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(this.CompanyId);
                    string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
                    string pricingPlanUrl = string.Format("{0}/Company/CompanyPricingPlans.aspx?companyId={1}", userWebUrl, this.CompanyId);

                    // Determine the email templates based on the command name.
                    switch (button.CommandName)
                    {
                        case "UserLimitProjectAdmin":

                            User projectAdmin = GetBL<PersonalBL>().GetUser(this.UserID);
                            Data.Project project = GetBL<ProjectBL>().GetProject(this.ProjectId);
                            string projectAdminName = string.Concat(projectAdmin.FirstName, " ", projectAdmin.LastName);

                            EmailSender.SendUserLimitUpgradeRequest(primaryAdmin.Email1, primaryAdmin.FirstName, projectAdminName, pricingPlanUrl, feedbackEmail, project.ProjectName);

                            break;

                        case "InventoryLimitInventoryManager":
                            User inventoryManager = GetBL<PersonalBL>().GetUser(this.UserID);
                            Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);
                            string inventoryManagerName = string.Concat(inventoryManager.FirstName, " ", inventoryManager.LastName);

                            EmailSender.SendInventoryLimitUpgradeRequest(primaryAdmin.Email1, primaryAdmin.FirstName, inventoryManagerName, pricingPlanUrl, feedbackEmail, company.CompanyName);
                            break;
                    }

                    FindParentPopup(button).HidePopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpgrade control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpgrade_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                Response.Redirect(string.Format("~/Company/CompanyPricingPlans.aspx?companyId={0}", CompanyId));
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Initializes the popups. (hide all unnecessary popups)
        /// </summary>
        private void InitializePopups()
        {
            bool isFreeTrailCompany = GetBL<CompanyBL>().IsFreeTrialCompany(this.CompanyId);

            switch (DisplayMode)
            {
                case ViewMode.CreateProjects:
                    popupCheckProjectsLimits.Visible = true;
                    popupCreateNewProjectsDuringFreeTrail.Visible = true;
                    popupProjectsNoPricePlan.Visible = true;
                    lblProjectLimit.Text = this.DefaultFreeTrialProjectCount == 1 ? "one Project" : string.Format(" {0} Projects", this.DefaultFreeTrialProjectCount);
                    popupCreateNewProjectsNoPricingPlan.Visible = true;
                    break;

                case ViewMode.ReactivateProjects:
                    popupCheckProjectsLimits.Visible = true;
                    popupProjectsNoPricePlan.Visible = true;
                    popupReActivateProjectsUserLimitExceeded.Visible = true;
                    break;

                case ViewMode.InventoryLimit:
                    popupFreeTrailInventoryLimitCompanyAdmin.Visible = true;
                    popupFreeTrailInventoryLimitInventoryManager.Visible = true;
                    popupInventoryLimitCompanyAdmin.Visible = true;
                    popupInventoryLimitInventoryManager.Visible = true;
                    break;

                case ViewMode.UserLimit:
                    popupFreeTrailUserLimitCompanyAdmin.Visible = true;
                    popupFreeTrailUserLimitProjectAdmin.Visible = true;
                    popupUserLimitCompanyAdmin.Visible = true;
                    popupUserLimitProjectAdmin.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// Finds the parent popup.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <returns></returns>
        private PopupBox FindParentPopup(Button button)
        {
            var curParent = button.Parent;
            while (curParent != null && !(curParent is PopupBox))
            {
                curParent = curParent.Parent;
            }

            return (PopupBox)(object)curParent;
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Validates this instance of User control.
        /// </summary>
        /// <param name="bulkUploadCount">The bulk upload count.</param>
        /// <returns></returns>
        public bool Validate(int? bulkUploadCount = null)
        {
            bool isValid = true;

            bool isFreeTrailCompany = GetBL<CompanyBL>().IsFreeTrialCompany(this.CompanyId);
            CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
            ProjectPaymentPackageDetails projectPaymentPackageDetails = null;
            InventoryPaymentPackageDetails inventoryPaymentPackageDetails = null;

            if (companyPaymentPackage != null)
            {
                projectPaymentPackageDetails =
                    Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(companyPaymentPackage.ProjectPaymentPackageTypeId);
                inventoryPaymentPackageDetails =
                    Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(companyPaymentPackage.InventoryPaymentPackageTypeId);
            }

            CompanyCurrentUsage companyCurrentUsage = GetBL<FinanceBL>().GetCompanyCurrentUsage(CompanyId, null);

            switch (DisplayMode)
            {
                case ViewMode.CreateProjects:
                    if (isFreeTrailCompany)
                    {
                        if (GetBL<FinanceBL>().HasExceededProjectLimit(isFreeTrailCompany, projectPaymentPackageDetails, companyCurrentUsage))
                        {
                            if (companyPaymentPackage != null)
                            {
                                popupCheckProjectsLimits.ShowPopup(Z_Index);
                            }
                            else
                            {
                                popupCreateNewProjectsDuringFreeTrail.ShowPopup(Z_Index);
                            }

                            isValid = false;
                        }
                    }
                    else
                    {
                        var discountUsage = this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId);
                        ProjectPaymentPackageDetails freeProjectPackage = Utils.GetFreeSystemProjectPackageDetail();

                        if (companyPaymentPackage != null && (companyPaymentPackage.PaymentMethodCodeId.HasValue ||
                                (projectPaymentPackageDetails != null && projectPaymentPackageDetails.ProjectPaymentPackageDetailId == freeProjectPackage.ProjectPaymentPackageDetailId)))
                        {
                            if (GetBL<FinanceBL>().HasExceededProjectLimit(isFreeTrailCompany, projectPaymentPackageDetails, companyCurrentUsage))
                            {
                                popupCheckProjectsLimits.ShowPopup(Z_Index);
                                isValid = false;
                            }
                        }
                        else if (companyPaymentPackage == null)
                        {
                            if (GetBL<CompanyBL>().IsFreeTrialEndedCompany(this.CompanyId))
                            {
                                lblProjectLimitFreeTrailEnd.Visible = true;
                                lblProjectLimitNoPackage.Visible = false;
                                popupCreateNewProjectsNoPricingPlan.ShowPopup(Z_Index);
                                isValid = false;
                            }
                            else
                            {
                                lblProjectLimitFreeTrailEnd.Visible = false;
                                lblProjectLimitNoPackage.Visible = true;
                                popupCreateNewProjectsNoPricingPlan.ShowPopup(Z_Index);
                                isValid = false;
                            }
                        }
                        else if (!(companyPaymentPackage != null
                           && (companyPaymentPackage.PaymentMethodCodeId.HasValue || discountUsage != null && discountUsage.DiscountCode.Discount == 100))
                           && (projectPaymentPackageDetails == null || projectPaymentPackageDetails.ProjectCount > 0))
                        {
                            lblReActivateProjectsNoPaymentDetails.Visible = true;
                            lblReActivateProjectsNoPricePlan.Visible = false;
                            lblReActivateProjectsNoPaymentDetails.Text = "To create a new Project you will need to setup your payment details.";
                            popupProjectsNoPricePlan.ShowPopup(Z_Index);
                            isValid = false;
                            break;
                        }
                    }

                    break;

                case ViewMode.ReactivateProjects:

                    List<int> mustIncludeProjectIds = new List<int>();
                    mustIncludeProjectIds.Add(this.ProjectId);
                    companyCurrentUsage = GetBL<FinanceBL>().GetCompanyCurrentUsage(CompanyId, mustIncludeProjectIds);

                    if (!isFreeTrailCompany)
                    {
                        var discountUsage = this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId);
                        if (!(companyPaymentPackage != null
                                && (companyPaymentPackage.PaymentMethodCodeId.HasValue || discountUsage != null && discountUsage.DiscountCode.Discount == 100))
                                && (projectPaymentPackageDetails == null || projectPaymentPackageDetails.ProjectCount > 0))
                        {
                            lblReActivateProjectsNoPaymentDetails.Visible = companyPaymentPackage != null && !companyPaymentPackage.PaymentMethodCodeId.HasValue;
                            lblReActivateProjectsNoPricePlan.Visible = companyPaymentPackage == null;

                            popupProjectsNoPricePlan.ShowPopup(Z_Index);
                            isValid = false;
                            break;
                        }
                    }

                    if (GetBL<FinanceBL>().HasExceededProjectLimit(isFreeTrailCompany, projectPaymentPackageDetails, companyCurrentUsage))
                    {
                        popupCheckProjectsLimits.ShowPopup(Z_Index);
                        isValid = false;
                    }
                    else
                    {
                        if (GetBL<FinanceBL>().HasExceededUserLimit(CompanyId, string.Empty, isFreeTrailCompany, companyPaymentPackage, projectPaymentPackageDetails, companyCurrentUsage, true))
                        {
                            lblPackageUserCount.Text = projectPaymentPackageDetails.HeadCount.ToString(CultureInfo.InvariantCulture);
                            lnkPricingPlan.NavigateUrl = string.Format("~/Company/CompanyPricingPlans.aspx?companyId={0}", CompanyId);
                            lnkManageProjectTeam.NavigateUrl = string.Format("~/Project/ProjectTeam.aspx?projectid={0}", this.ProjectId);
                            popupReActivateProjectsUserLimitExceeded.ShowPopup(Z_Index);
                            isValid = false;
                        }
                    }

                    break;

                case ViewMode.InventoryLimit:
                    if (isFreeTrailCompany)
                    {
                        if (GetBL<FinanceBL>().HasExceededInventoryLimit(isFreeTrailCompany, inventoryPaymentPackageDetails, companyCurrentUsage, bulkUploadCount))
                        {
                            if (companyPaymentPackage != null)
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupInventoryLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblInventoryLimitCompanyAdmin.Visible = true;
                                    lblInventoryLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupInventoryLimitInventoryManager.ShowPopup(Z_Index);
                                    lblInventoryLimitInventoryManager.Visible = true;
                                    lblInventoryLimitInventoryManagerFreeTrailEndNoPackage.Visible = false;
                                }
                            }
                            else
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupFreeTrailInventoryLimitCompanyAdmin.ShowPopup(Z_Index);
                                }
                                else
                                {
                                    popupFreeTrailInventoryLimitInventoryManager.ShowPopup(Z_Index);
                                }
                            }

                            isValid = false;
                        }
                    }
                    else
                    {
                        if (companyPaymentPackage != null)
                        {
                            if (GetBL<FinanceBL>().HasExceededInventoryLimit(isFreeTrailCompany, inventoryPaymentPackageDetails, companyCurrentUsage, bulkUploadCount))
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupInventoryLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblInventoryLimitCompanyAdmin.Visible = true;
                                    lblInventoryLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupInventoryLimitInventoryManager.ShowPopup(Z_Index);
                                    lblInventoryLimitInventoryManager.Visible = true;
                                    lblInventoryLimitInventoryManagerFreeTrailEndNoPackage.Visible = false;
                                }

                                isValid = false;
                            }
                        }
                        else
                        {
                            if (GetBL<CompanyBL>().IsFreeTrialEndedCompany(this.CompanyId))
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupInventoryLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblInventoryLimitCompanyAdmin.Visible = false;
                                    lblInventoryLimitCompanyAdminFreeTrailEndNoPackage.Visible = true;
                                    lblInventoryLimitCompanyAdminNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupInventoryLimitInventoryManager.ShowPopup(Z_Index);
                                    lblInventoryLimitInventoryManager.Visible = false;
                                    lblInventoryLimitInventoryManagerFreeTrailEndNoPackage.Visible = true;
                                    lblInventoryLimitInventoryManagerNoPackage.Visible = false;
                                }

                                isValid = false;
                            }
                            else
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupInventoryLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblInventoryLimitCompanyAdmin.Visible = false;
                                    lblInventoryLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                    lblInventoryLimitCompanyAdminNoPackage.Visible = true;
                                }
                                else
                                {
                                    popupInventoryLimitInventoryManager.ShowPopup(Z_Index);
                                    lblInventoryLimitInventoryManager.Visible = false;
                                    lblInventoryLimitInventoryManagerFreeTrailEndNoPackage.Visible = false;
                                    lblInventoryLimitInventoryManagerNoPackage.Visible = true;
                                }
                                isValid = false;
                            }
                        }
                    }
                    break;

                case ViewMode.UserLimit:
                    if (isFreeTrailCompany)
                    {
                        if (GetBL<FinanceBL>().HasExceededUserLimit(CompanyId, InvitedUserEmail, isFreeTrailCompany, companyPaymentPackage, projectPaymentPackageDetails, companyCurrentUsage))
                        {
                            if (companyPaymentPackage != null)
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupUserLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblUserLimitCompanyAdmin.Visible = true;
                                    lblUserLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupUserLimitProjectAdmin.ShowPopup(Z_Index);
                                    lblUserLimitProjectAdmin.Visible = true;
                                    lblUserLimitProjectAdminFreeTrailEndNoPackage.Visible = false;
                                }
                            }
                            else
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupFreeTrailUserLimitCompanyAdmin.ShowPopup(Z_Index);
                                }
                                else
                                {
                                    popupFreeTrailUserLimitProjectAdmin.ShowPopup(Z_Index);
                                }
                            }

                            isValid = false;
                        }
                    }
                    else
                    {
                        if (companyPaymentPackage != null)
                        {
                            if (GetBL<FinanceBL>().HasExceededUserLimit(CompanyId, InvitedUserEmail, isFreeTrailCompany, companyPaymentPackage, projectPaymentPackageDetails, companyCurrentUsage))
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupUserLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblUserLimitCompanyAdmin.Visible = true;
                                    lblUserLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupUserLimitProjectAdmin.ShowPopup(Z_Index);
                                    lblUserLimitProjectAdmin.Visible = true;
                                    lblUserLimitProjectAdminFreeTrailEndNoPackage.Visible = false;
                                }
                                isValid = false;
                            }
                        }
                        else
                        {
                            if (GetBL<CompanyBL>().IsFreeTrialEndedCompany(this.CompanyId))
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupUserLimitCompanyAdmin.ShowPopup(Z_Index);
                                    lblUserLimitCompanyAdmin.Visible = false;
                                    lblUserLimitCompanyAdminFreeTrailEndNoPackage.Visible = true;
                                    lblUserLimitCompanyAdminNoPackage.Visible = false;
                                }
                                else
                                {
                                    popupUserLimitProjectAdmin.ShowPopup(Z_Index);
                                    lblUserLimitProjectAdmin.Visible = false;
                                    lblUserLimitProjectAdminFreeTrailEndNoPackage.Visible = true;
                                    lblUserLimitProjectAdminNoPackage.Visible = false;
                                }

                                isValid = false;
                            }
                            else
                            {
                                if (IsCompanyAdmin)
                                {
                                    popupUserLimitCompanyAdmin.ShowPopup(Z_Index);

                                    lblUserLimitCompanyAdmin.Visible = false;
                                    lblUserLimitCompanyAdminFreeTrailEndNoPackage.Visible = false;
                                    lblUserLimitCompanyAdminNoPackage.Visible = true;
                                }
                                else
                                {
                                    popupUserLimitProjectAdmin.ShowPopup(Z_Index);
                                    lblUserLimitProjectAdmin.Visible = false;
                                    lblUserLimitProjectAdminFreeTrailEndNoPackage.Visible = false;
                                    lblUserLimitProjectAdminNoPackage.Visible = true;
                                }
                                isValid = false;
                            }
                        }
                    }

                    break;
            }

            return isValid;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            InitializePopups();
        }

        #endregion Public Methods
    }
}