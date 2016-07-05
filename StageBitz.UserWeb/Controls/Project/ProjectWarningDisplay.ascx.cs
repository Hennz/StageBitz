using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Project
{
    public partial class ProjectWarningDisplay : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for project permissions
        /// </summary>
        private enum ProjectPermission
        {
            CompanyAdministrator,
            ProjectAdministrator,
            Staff
        }

        /// <summary>
        /// Enum for notice types
        /// See attachment in PBI #8804
        /// </summary>
        private enum NoticeType
        {
            FreeTrialGraceCompanyAdmin = 0, //Radio button notification

            GracePeriodCompanyAdmin = 1, //Notice B
            GracePeriodProjectAdmin = 2, //Notice C

            PaymentFailedCompanyAdmin = 3, //Notice D
            PaymentFailedProjectStaff = 4, //Notice E

            SuspendedCompanyAdmin = 5,//Notice D1
            SuspendedProjectStaff = 6,//Notice D2

            ClosedProjectCompanyAdmin = 7 //Notice F
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        protected string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the remaining days.
        /// </summary>
        /// <value>
        /// The remaining days.
        /// </value>
        protected string RemainingDays { get; set; }

        /// <summary>
        /// Gets or sets the company financial URL.
        /// </summary>
        /// <value>
        /// The company financial URL.
        /// </value>
        protected string CompanyFinancialUrl { get; set; }

        /// <summary>
        /// Gets or sets the payment failed date.
        /// </summary>
        /// <value>
        /// The payment failed date.
        /// </value>
        protected DateTime? PaymentFailedDate { get; set; }

        /// <summary>
        /// Gets or sets the payment overdue days.
        /// </summary>
        /// <value>
        /// The payment overdue days.
        /// </value>
        protected string PaymentOverdueDays { get; set; }

        /// <summary>
        /// Gets or sets the name of the company admin.
        /// </summary>
        /// <value>
        /// The name of the company admin.
        /// </value>
        protected string CompanyAdminName { get; set; }

        /// <summary>
        /// Gets or sets the company admin email.
        /// </summary>
        /// <value>
        /// The company admin email.
        /// </value>
        protected string CompanyAdminEmail { get; set; }

        /// <summary>
        /// The support email
        /// </summary>
        protected string supportEmail = null;

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
                if (supportEmail == null)
                {
                    supportEmail = Support.GetSystemValue("FeedbackEmail");
                }

                return supportEmail;
            }
        }

        /// <summary>
        /// Gets or sets the warning status.
        /// </summary>
        /// <value>
        /// The warning status.
        /// </value>
        public ProjectStatusHandler.ProjectWarningStatus WarningStatus { get; set; }

        /// <summary>
        /// Gets a value indicating whether this control is project read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is project read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsProjectReadOnly
        {
            get
            {
                return (WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Suspended
                    || WarningStatus == ProjectStatusHandler.ProjectWarningStatus.PaymentFailed
                    || WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Closed);
            }
        }

        /// <summary>
        /// Gets the project closed by.
        /// </summary>
        /// <value>
        /// The project closed by.
        /// </value>
        public string ProjectClosedBy
        {
            get
            {
                User projectClosedBy = this.GetBL<ProjectBL>().GetProjectClosedBy(ProjectID);
                if (projectClosedBy != null)
                {
                    return projectClosedBy.FirstName + " " + projectClosedBy.LastName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }
                return (int)ViewState["ProjectID"];
            }
            set
            {
                ViewState["ProjectID"] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectID).FirstOrDefault();
            bool paymentsSpecified = (FinanceSupport.GetCreditCardToken("Company", project.CompanyId) != null);

            ProjectStatusHandler.ProjectWarningInfo warningInfo = ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == Support.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);
            StageBitz.Data.User companyAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(project.CompanyId);

            ProjectName = Support.TruncateString(project.ProjectName, 30);
            RemainingDays = string.Format("{0} day{1}", warningInfo.DaysToExpiration < 0 ? 0 : warningInfo.DaysToExpiration, warningInfo.DaysToExpiration == 1 ? string.Empty : "s");
            CompanyAdminName = Support.TruncateString((companyAdmin.FirstName + " " + companyAdmin.LastName).Trim(), 30);
            CompanyAdminEmail = companyAdmin.Email1;
            PaymentFailedDate = null;

            CompanyFinancialUrl = ResolveUrl(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", project.CompanyId));

            if (warningInfo.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.NoWarning)
            {
                noticesMultiView.Visible = false;
                return;
            }
            else
            {
                noticesMultiView.Visible = true;
            }

            #region Determine User Permission Type

            ProjectPermission userType = ProjectPermission.Staff;
            if (Support.IsCompanyAdministrator(project.CompanyId))
            {
                userType = ProjectPermission.CompanyAdministrator;
            }
            else if (Support.IsProjectAdministrator(ProjectID))
            {
                userType = ProjectPermission.ProjectAdministrator;
            }
            else
            {
                userType = ProjectPermission.Staff;
            }

            #endregion Determine User Permission Type

            switch (warningInfo.WarningStatus)
            {
                case ProjectStatusHandler.ProjectWarningStatus.FreeTrialGrace:
                    if (userType == ProjectPermission.CompanyAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.FreeTrialGraceCompanyAdmin;
                    }
                    break;

                case ProjectStatusHandler.ProjectWarningStatus.GracePeriod:
                    if (userType == ProjectPermission.CompanyAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.GracePeriodCompanyAdmin;
                        ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin.CompanyID = project.CompanyId;
                        ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.CompanyAdministrator,
                                CompanyPaymentFailedWarning.DisplayMode.PaymentFailedGracePeriod);
                    }
                    else if (userType == ProjectPermission.ProjectAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.GracePeriodProjectAdmin;
                        ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin.CompanyID = project.CompanyId;
                        ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.NonCompanyAdministrator,
                                CompanyPaymentFailedWarning.DisplayMode.PaymentFailedGracePeriod);
                    }
                    PaymentFailedDate = project.ExpirationDate.Value.AddDays(-7);
                    break;

                case ProjectStatusHandler.ProjectWarningStatus.PaymentFailed:
                    if (userType == ProjectPermission.CompanyAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.PaymentFailedCompanyAdmin;
                        ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin.CompanyID = project.CompanyId;
                        ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.CompanyAdministrator,
                                CompanyPaymentFailedWarning.DisplayMode.PaymentFailed);
                    }
                    else if (userType == ProjectPermission.ProjectAdministrator || userType == ProjectPermission.Staff)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.PaymentFailedProjectStaff;
                        ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin.CompanyID = project.CompanyId;
                        ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin.LoadData(CompanyPaymentFailedWarning.PermissionLevel.NonCompanyAdministrator,
                                CompanyPaymentFailedWarning.DisplayMode.PaymentFailed);
                    }

                    break;

                case ProjectStatusHandler.ProjectWarningStatus.Suspended:
                    if (userType == ProjectPermission.CompanyAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.SuspendedCompanyAdmin;
                    }
                    else if (userType == ProjectPermission.ProjectAdministrator || userType == ProjectPermission.Staff)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.SuspendedProjectStaff;
                    }
                    break;

                case ProjectStatusHandler.ProjectWarningStatus.Closed:
                    if (userType == ProjectPermission.CompanyAdministrator)
                    {
                        noticesMultiView.ActiveViewIndex = (int)NoticeType.ClosedProjectCompanyAdmin;
                    }
                    break;
            }

            //If the project's payment has failed, calculate the number of days the payment is due.
            if (PaymentFailedDate != null)
            {
                int paymentOverdueDays = (int)(Today - PaymentFailedDate.Value).TotalDays;
                PaymentOverdueDays = string.Format("{0} day{1}", paymentOverdueDays, paymentOverdueDays == 1 ? string.Empty : "s");
            }

            upnlProjectWarningDisplay.Update();
        }

        #endregion Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the user response.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HandleUserResponse(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (rdContinueProject.Checked)
                {
                    Data.Project project = DataContext.Projects.Where(p => p.ProjectId == ProjectID).SingleOrDefault();

                    if (rdContinueProject.Checked)
                    {
                        // redirect to pricing plan page with opt-in flag
                        string url = string.Format("~/Company/CompanyPricingPlans.aspx?companyid={0}", project.CompanyId);
                        Response.Redirect(url);
                    }
                }
                else
                {
                    popupRequiredNotification.ShowPopup();
                }
            }
        }

        #endregion Event Handlers
    }
}