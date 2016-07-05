using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Company
{
    public partial class CompanyFinancialDetails : PageBase
    {
        #region Enums

        /// <summary>
        /// Enum for project view modes.
        /// </summary>
        private enum ProjectsViewMode
        {
            ActiveProjects,
            ClosedProjects,
            AllProjects,
        }

        #endregion Enums

        #region Global variables

        /// <summary>
        /// Global vars.
        /// </summary>
        private int freeTrialCodeId, activeCodeId, gracePeriodCodeId, paymentFailed, suspendCodeId, closedCodeId;

        /// <summary>
        /// The company credit card token.
        /// </summary>
        private CreditCardToken companyCreditCardToken = null;

        /// <summary>
        /// The project status list.
        /// </summary>
        private List<int> projectStatusList = new List<int>();

        #endregion Global variables

        #region Properties

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        private int CompanyId
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
        /// Gets or sets the user opt-in project identifier.
        /// </summary>
        /// <value>
        /// The user opt-in project identifier.
        /// </value>
        private int UserOptInProjectId
        {
            get
            {
                if (ViewState["UserOptInProjectId"] == null)
                {
                    ViewState["UserOptInProjectId"] = 0;
                }
                return (int)ViewState["UserOptInProjectId"];
            }
            set
            {
                ViewState["UserOptInProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company has suspended by sb admin.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company has suspended by sb admin; otherwise, <c>false</c>.
        /// </value>
        private bool HasCompanySuspendedBySBAdmin
        {
            get
            {
                if (ViewState["HasCompanySuspendedBySBAdmin"] == null)
                {
                    ViewState["HasCompanySuspendedBySBAdmin"] = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId);
                }
                return (bool)ViewState["HasCompanySuspendedBySBAdmin"];
            }
            set
            {
                ViewState["HasCompanySuspendedBySBAdmin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company has payment failed invoices.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company has payment failed invoices; otherwise, <c>false</c>.
        /// </value>
        private bool IsPaymentFailedInvoicesExistForCompany
        {
            get
            {
                if (ViewState["IsPaymentFailedInvoicesExistForCompany"] == null)
                {
                    ViewState["IsPaymentFailedInvoicesExistForCompany"] = ProjectFinanceHandler.IsPaymentFailedInvoicesExistForCompany(CompanyId);
                }
                return (bool)ViewState["IsPaymentFailedInvoicesExistForCompany"];
            }
            set
            {
                ViewState["IsPaymentFailedInvoicesExistForCompany"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this company is payment failed company.
        /// </summary>
        /// <value>
        /// <c>true</c> if this company is payment failed company; otherwise, <c>false</c>.
        /// </value>
        private bool IsPaymentFailedCompany
        {
            get
            {
                if (ViewState["IsPaymentFailedCompany"] == null)
                {
                    ViewState["IsPaymentFailedCompany"] = GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);
                }
                return (bool)ViewState["IsPaymentFailedCompany"];
            }
            set
            {
                ViewState["IsPaymentFailedCompany"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the projects display mode.
        /// </summary>
        /// <value>
        /// The projects display mode.
        /// </value>
        private ProjectsViewMode ProjectsDisplayMode
        {
            get
            {
                if (ViewState["ProjectsDisplayMode"] == null)
                {
                    ViewState["ProjectsDisplayMode"] = default(ProjectsViewMode);
                }

                return (ProjectsViewMode)ViewState["ProjectsDisplayMode"];
            }
            set
            {
                ViewState["ProjectsDisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">You do not have administrator rights to view this page.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check for permissions
            if (!IsPostBack)
            {
                if (!Support.IsCompanyAdministrator(CompanyId))
                {
                    throw new ApplicationException("You do not have administrator rights to view this page.");
                }

                string companyName = Support.GetCompanyNameById(CompanyId);

                lnkCreateNewProject.CompanyId = this.CompanyId;
                lnkCreateNewProject.LoadData();
                lnkFinancialHistory.HRef = string.Format("~/Company/CompanyFinanceHistory.aspx?companyid={0}", CompanyId);
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);

                sbPackageLimitsValidation.CompanyId = CompanyId;
                sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.ReactivateProjects;
                sbPackageLimitsValidation.LoadData();

                bool isReadOnly = HasCompanySuspendedBySBAdmin || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);
                spnCreateNewProject.Visible = !isReadOnly;
                LoadBreadCrumbs(companyName);

                gvProjects.DataBind();

                CompanyPaymentPackage currentPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                if (currentPaymentPackage != null)
                {
                    int anualDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL");
                    int monthlyDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "MONTHLY");
                    DateTime newPackageStartDate = Utils.Today;
                    string period = string.Empty;
                    int durationDifference = this.GetBL<FinanceBL>().GetDurationDifference(currentPaymentPackage.StartDate, Utils.Today, currentPaymentPackage.PaymentDurationCodeId);

                    if (currentPaymentPackage.PaymentDurationCodeId == anualDurationCodeId)
                    {
                        period = "Yearly";
                        newPackageStartDate = currentPaymentPackage.EndDate != null ? (DateTime)currentPaymentPackage.EndDate : (DateTime)currentPaymentPackage.StartDate.AddYears(durationDifference);
                    }
                    else
                    {
                        period = "Monthly";
                        newPackageStartDate = currentPaymentPackage.EndDate != null ? (DateTime)currentPaymentPackage.EndDate : (DateTime)currentPaymentPackage.StartDate.AddMonths(durationDifference);
                    }
                    CompanyBL companyBL = new CompanyBL(DataContext);
                    if (companyBL.IsFreeTrialStatusIncludedFortheDay(CompanyId, Utils.Today))
                    {
                        newPackageStartDate = currentPaymentPackage.StartDate;
                    }

                    if (newPackageStartDate == Utils.Today)
                    {
                        if (currentPaymentPackage.PaymentDurationCodeId == anualDurationCodeId)
                        {
                            newPackageStartDate = newPackageStartDate.AddYears(1);
                        }
                        else
                        {
                            newPackageStartDate = newPackageStartDate.AddMonths(1);
                        }
                    }
                    lblRenewsOn.Text = Utils.FormatDate(newPackageStartDate);
                    lblSubscriptionPeriod.Text = period;
                    divSubscriptionPeriod.Visible = true;
                    divRenewsOn.Visible = true;
                }
                else
                {
                    divSubscriptionPeriod.Visible = false;
                    divRenewsOn.Visible = false;
                }
                setUpCreditCardDetails.CompanyId = CompanyId;
                setUpCreditCardDetails.RelatedTable = "Company";
                setUpCreditCardDetails.DisplayMode = SetupCreditCardDetails.ViewMode.CompanyBilling;
                Data.CreditCardToken creditCardToken = this.GetBL<FinanceBL>().GetCreditCardToken(CompanyId);
                SetUISettings(creditCardToken);
                planMonitor.CompanyId = CompanyId;

                sbCompanyWarningDisplay.CompanyID = CompanyId;
                sbCompanyWarningDisplay.LoadData();
                sbFutureRequestNotificationMessage.CompanyId = CompanyId;
                sbFutureRequestNotificationMessage.LoadData();

                ConfigureCompanyStatus();
            }
        }

        /// <summary>
        /// Updates the project statuses after payment set up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void UpdateProjectStatusesAfterPaymentSetUp(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                SetUISettings(this.GetBL<FinanceBL>().GetCreditCardToken(CompanyId));
                RefreshProjectList();
                sbCompanyWarningDisplay.LoadData();
            }
        }

        #region Project List Events

        /// <summary>
        /// Handles the NeedDataSource event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            if (!StopProcessing)
            {
                #region Fill global variables required for data binding

                freeTrialCodeId = Support.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId;
                activeCodeId = Support.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;
                gracePeriodCodeId = Support.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;
                paymentFailed = Support.GetCodeByValue("ProjectStatus", "PAYMENTFAILED").CodeId;
                suspendCodeId = Support.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId;
                closedCodeId = Support.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
                companyCreditCardToken = FinanceSupport.GetCreditCardToken("Company", CompanyId);

                #endregion Fill global variables required for data binding

                switch (ProjectsDisplayMode)
                {
                    case ProjectsViewMode.ActiveProjects:
                        projectStatusList.Add(freeTrialCodeId);
                        projectStatusList.Add(activeCodeId);
                        projectStatusList.Add(gracePeriodCodeId);
                        projectStatusList.Add(paymentFailed);
                        projectStatusList.Add(suspendCodeId);
                        break;

                    case ProjectsViewMode.ClosedProjects:
                        projectStatusList.Add(closedCodeId);
                        break;

                    case ProjectsViewMode.AllProjects:
                        projectStatusList.Add(freeTrialCodeId);
                        projectStatusList.Add(activeCodeId);
                        projectStatusList.Add(gracePeriodCodeId);
                        projectStatusList.Add(paymentFailed);
                        projectStatusList.Add(suspendCodeId);
                        projectStatusList.Add(closedCodeId);
                        break;
                }
                List<CompanyProjectDetails> projects = this.GetBL<FinanceBL>().GetCompanyProjectDetails(CompanyId, projectStatusList);

                gvProjects.DataSource = projects;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                dynamic project = e.Item.DataItem as dynamic;
                int projectStatusCodeId = project.ProjectStatusCodeId;

                #region Project Name

                HtmlAnchor lnkProject = (HtmlAnchor)e.Item.FindControl("lnkProject");

                lnkProject.HRef = string.Format("~/Project/ProjectTeam.aspx?projectid={0}", project.ProjectId);
                lnkProject.InnerText = Support.TruncateString(project.ProjectName, 20);

                if (project.ProjectName.Length > 20)
                {
                    lnkProject.Title = project.ProjectName;
                }

                #endregion Project Name

                #region Payment Status

                MultiView multStatus = (MultiView)e.Item.FindControl("multStatus");
                //bool shouldDisplayLastPayment = false;

                if (project.ProjectStatusCodeId == activeCodeId)
                {
                    multStatus.ActiveViewIndex = 1;

                    //shouldDisplayLastPayment = true;
                }
                else if (projectStatusCodeId == gracePeriodCodeId || projectStatusCodeId == paymentFailed
                    || ((projectStatusCodeId == suspendCodeId || projectStatusCodeId == closedCodeId) && IsPaymentFailedInvoicesExistForCompany))
                {
                    multStatus.ActiveViewIndex = 2;

                    View activeView = multStatus.GetActiveView();

                    Literal ltrlExpiresOn = (Literal)activeView.FindControl("ltrlExpiresOn");

                    if (projectStatusCodeId == paymentFailed
                        || ((projectStatusCodeId == suspendCodeId || projectStatusCodeId == closedCodeId) && IsPaymentFailedInvoicesExistForCompany))
                    {
                        plcPayNow.Visible = true;

                        if (projectStatusCodeId == paymentFailed)
                            ltrlExpiresOn.Text = "Payment Failed | <strong>Project read-only</strong>";
                        else
                            ltrlExpiresOn.Text = "Payment Failed | " + project.ProjectStatus;
                    }
                    else if (projectStatusCodeId == gracePeriodCodeId)
                    {
                        ltrlExpiresOn.Text = string.Format("Payment Failed");
                    }
                }
                else if (projectStatusCodeId == freeTrialCodeId || projectStatusCodeId == suspendCodeId || projectStatusCodeId == closedCodeId)
                {
                    multStatus.ActiveViewIndex = 0;
                    Literal ltrlStatus = (Literal)multStatus.GetActiveView().FindControl("ltrlStatus");
                    ltrlStatus.Text = project.ProjectStatus;
                }

                #endregion Payment Status

                #region Suspend/ReActivate

                LinkButton lnkReActivate = (LinkButton)e.Item.FindControl("lnkReActivate");
                LinkButton lnkSuspend = (LinkButton)e.Item.FindControl("lnkSuspend");

                bool isProjectSuspended = (projectStatusCodeId == suspendCodeId);
                bool isProjectClosed = (projectStatusCodeId == closedCodeId);
                bool isFreeTrial = (projectStatusCodeId == freeTrialCodeId);

                //Check if there are any pending invoices.If so disable the ReActivate button. disable the reactivate for suspended company
                if (lnkReActivate != null)
                {
                    lnkReActivate.Visible =
                        !IsPaymentFailedCompany && isProjectSuspended && !isProjectClosed && !HasCompanySuspendedBySBAdmin;
                    Label lblDiabledReActivate = (Label)e.Item.FindControl("lblDiabledReActivate");

                    if (lblDiabledReActivate != null)
                    {
                        lblDiabledReActivate.Visible =
                            (IsPaymentFailedCompany && isProjectSuspended && !isProjectClosed)
                            || (HasCompanySuspendedBySBAdmin && !isProjectClosed);

                        if (HasCompanySuspendedBySBAdmin)
                        {
                            lblDiabledReActivate.ToolTip = "StageBitz Admin Suspended the Company";
                        }
                        else if (IsPaymentFailedCompany)
                        {
                            lblDiabledReActivate.ToolTip = "Pending Invoice(s) exist";
                        }
                    }
                }

                Label lblDisabledSuspend = (Label)e.Item.FindControl("lblDisabledSuspend");
                if (lnkSuspend != null && lblDisabledSuspend != null)
                {
                    lnkSuspend.Visible = !isProjectSuspended && !isProjectClosed && !isFreeTrial;
                    lblDisabledSuspend.Visible = isFreeTrial;
                    lblDisabledSuspend.ToolTip = "Projects in Free Trial cannot be suspended.";
                }

                #endregion Suspend/ReActivate
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.Item is GridDataItem)
                {
                    //Pay now for this project
                    GridDataItem dataItem = (GridDataItem)e.Item;
                    int projectId = (int)dataItem.GetDataKeyValue("ProjectId");
                    Data.Project project = DataContext.Projects.Where(p => p.ProjectId == projectId).SingleOrDefault();

                    switch (e.CommandName)
                    {
                        case "Suspend":
                            popupSuspendReActivate.Title = "Confirm Suspend";
                            divMsgSuspendReActivate.InnerText = "When you suspend a Project, it will become read-only for all users. " +
                                    "This means you will not be able to add or edit any information, but you will still be able to view all " +
                                    "elements of your Project. Suspended Projects are not included in your active Project count.";
                            popupSuspendReActivate.ShowPopup();
                            btnSuspendReActivate.CommandArgument = projectId.ToString();
                            break;

                        case "ReActivate":
                            sbPackageLimitsValidation.ProjectId = projectId;
                            if (sbPackageLimitsValidation.Validate())
                            {
                                //When ReActiving, we need to consider Free trial projects.
                                int prevoiusStatusCodeId = ProjectStatusHandler.GetPreviuosProjectStatusFromHistory(projectId);

                                popupSuspendReActivate.Title = "Confirm Reactivate";
                                divMsgSuspendReActivate.InnerText = "When you reactivate a Project, all users will have the same access to the Project " +
                                        "that they had before it was suspended. The Project will also be included in the active Project count.";
                                popupSuspendReActivate.ShowPopup();
                                btnSuspendReActivate.CommandArgument = projectId.ToString();

                                if (prevoiusStatusCodeId == Utils.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId)
                                {
                                    freeTrialCodeId = Support.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId;
                                    UserOptInProjectId = projectId;
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the user opt in.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void UpdateUserOptIn(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Data.Project project = DataContext.Projects.Where(p => p.ProjectId == UserOptInProjectId).SingleOrDefault();
                if (project != null)
                {
                    project.ProjectTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;//This is a safeside. If d project is in Free trial, we show this message.
                    project.LastUpdatedByUserId = UserID;
                    int statusCodeId = ProjectStatusHandler.GetPreviuosProjectStatusFromHistory(project.ProjectId);

                    //If today is greater than Project Free trial expired day then activate the project.
                    if (project.ExpirationDate != null && (project.ExpirationDate > Today || project.ExpirationDate == Today && !Support.HasConsideringDayEnded(Now)))
                    {
                        project.ProjectStatusCodeId = ProjectStatusHandler.GetPreviuosProjectStatusFromHistory(project.ProjectId);
                    }
                    else
                    {
                        project.ProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;
                        project.ExpirationDate = null;
                    }

                    //ProjectUsageHandler.UpdateFutureDailyUsageProjectState(project, true, UserID, statusCodeId, Today);
                    project.LastUpdatedDate = Now;
                    DataContext.SaveChanges();
                    SetUISettings(this.GetBL<FinanceBL>().GetCreditCardToken(CompanyId));
                    RefreshProjectList();
                }
            }
        }

        /// <summary>
        /// Suspends the re activate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void SuspendReActivate(object sender, CommandEventArgs e)
        {
            if (!StopProcessing)
            {
                popupSuspendReActivate.HidePopup();
                int projectId;
                int.TryParse(e.CommandArgument.ToString(), out projectId);
                Data.Project project = GetBL<ProjectBL>().GetProject(projectId);
                suspendCodeId = Support.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId;
                freeTrialCodeId = Support.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId;
                gracePeriodCodeId = Support.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;
                bool isReActivation = project.ProjectStatusCodeId == suspendCodeId;
                int statusToConsider;

                activeCodeId = Support.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;
                int compnayGracePeriodCodeId = Support.GetCodeByValue("CompanyStatus", "GRACEPERIOD").CodeId;
                Data.Company company = GetBL<CompanyBL>().GetCompany(this.CompanyId);

                if (project != null)
                {
                    //This is a Reactivation.
                    if (isReActivation)
                    {
                        int previousStatusCodeId = ProjectStatusHandler.GetPreviuosProjectStatusFromHistory(projectId);
                        //We have to move the current status to where it was(Active or FreeTrial).
                        //Make sure whether Free trial period has expired.
                        if ((previousStatusCodeId == freeTrialCodeId || previousStatusCodeId == gracePeriodCodeId) && project.ExpirationDate != null && (project.ExpirationDate > Today || project.ExpirationDate == Today && !Support.HasConsideringDayEnded(Now)))
                        {
                            //Still on the Free Trial
                            project.ProjectStatusCodeId = previousStatusCodeId;
                        }
                        else if (company.CompanyStatusCodeId == compnayGracePeriodCodeId && (previousStatusCodeId == activeCodeId || previousStatusCodeId == freeTrialCodeId))
                        {
                            project.ProjectStatusCodeId = gracePeriodCodeId;
                            project.ExpirationDate = company.ExpirationDate;
                        }
                        else if (!this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId))//This checks for safeside.
                        {
                            //Just an Activation.
                            previousStatusCodeId = activeCodeId;
                            project.ProjectStatusCodeId = previousStatusCodeId;
                            project.ExpirationDate = null;
                        }
                        else
                        {
                            //Just a safe action.
                            gvProjects.Rebind();
                            return;
                        }
                        statusToConsider = previousStatusCodeId;
                    }
                    else
                    {
                        statusToConsider = suspendCodeId;
                        project.ProjectStatusCodeId = suspendCodeId;
                    }

                    //ProjectUsageHandler.UpdateFutureDailyUsageProjectState(project, isReActivation, UserID, statusToConsider, Today);
                    project.LastUpdatedByUserId = UserID;
                    project.LastUpdatedDate = Now;
                    DataContext.SaveChanges();
                }

                gvProjects.Rebind();
                planMonitor.LoadData();
                upnlPlanMonitor.Update();
            }
        }

        /// <summary>
        /// Makes the payment.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void MakePayment(object sender, CommandEventArgs e)
        {
            if (!StopProcessing)
            {
                popupMakePayment.HidePopup();
                //Make the payment
                bool isPaymentSuccess = ProjectFinanceHandler.ProcessInvoicesAndPayments(CompanyId, Today, false, UserID);
                if (!isPaymentSuccess)
                {
                    popupNotification.Title = "Payment failed";
                    ltrlNotification.Text = "Payment failed. Please verify the payment details and retry.";
                    popupNotification.ShowPopup();
                    return;
                }
                //Refresh the UI
                Response.Redirect(Request.Url.ToString());
            }
        }

        /// <summary>
        /// Handles the SortCommand event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvProjects.MasterTableView.SortExpressions.Clear();
                gvProjects.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvProjects.Rebind();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDisplayProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDisplayProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                switch (ddlDisplayProjects.SelectedIndex)
                {
                    case 0:
                        ProjectsDisplayMode = ProjectsViewMode.ActiveProjects;
                        break;

                    case 1:
                        ProjectsDisplayMode = ProjectsViewMode.ClosedProjects;
                        break;

                    case 2:
                        ProjectsDisplayMode = ProjectsViewMode.AllProjects;
                        break;
                }

                RefreshProjectList();
            }
        }

        #endregion Project List Events

        #region Inventory List Events

        /// <summary>
        /// Handles the NeedDataSource event of the gvInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvInventory_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            List<CompanyInventoryDetails> inventoryDetails = this.GetBL<CompanyBL>().GetCompanyInventoryDetails(CompanyId);
            gvInventory.DataSource = inventoryDetails;
        }

        #endregion Inventory List Events

        /// <summary>
        /// Handles the Click event of the lnkbtnPayNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkbtnPayNow_Click(object sender, EventArgs e)
        {
            CreditCardToken companyCreditCardToken = FinanceSupport.GetCreditCardToken("Company", CompanyId);

            //Check whether payments are setup and display message if required.
            if (companyCreditCardToken == null)
            {
                popupNotification.Title = "Set up Payments";
                ltrlNotification.Text = "Please set up payment details for the Company in order to make payments.";
                popupNotification.ShowPopup();
                return;
            }

            popupMakePayment.ShowPopup();
        }

        #endregion Events

        #region Support Methods

        /// <summary>
        /// Refreshes the project list.
        /// </summary>
        private void RefreshProjectList()
        {
            gvProjects.Rebind();
            upnlProjects.Update();
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        private void LoadBreadCrumbs(string companyName)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.AddLink(companyName, string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", CompanyId));
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        /// <summary>
        /// Sets the UI settings.
        /// </summary>
        /// <param name="creditCardToken">The credit card token.</param>
        private void SetUISettings(CreditCardToken creditCardToken)
        {
            setUpCreditCardDetails.SetUISettings(creditCardToken);
        }

        /// <summary>
        /// Configures the company status.
        /// </summary>
        private void ConfigureCompanyStatus()
        {
            Data.Company company = this.GetBL<CompanyBL>().GetCompany(CompanyId);
            int companyStatusCodeId = company.CompanyStatusCodeId;
            int companyGracePeriodCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "GRACEPERIOD");

            if (this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId))
            {
                string msgCompanySuspended = "<strong>Company activity is suspended by StageBitz Admin</strong>";
                multStatusCompany.ActiveViewIndex = 0;
                ltrlStatus.Text = msgCompanySuspended;

                if (this.GetBL<CompanyBL>().IsCompanyInPaymentFailedGracePeriod(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId))
                {
                    multStatusCompany.ActiveViewIndex = 2;
                    View activeView = multStatusCompany.GetActiveView();
                    Literal ltrlExpiresOn = (Literal)activeView.FindControl("ltrlExpiresOn");
                    ltrlExpiresOn.Text = msgCompanySuspended + " | Payment Failed";
                    imgPaymentFailed.ToolTip = ProjectFinanceHandler.GetCompanyPaymentFailureDetails(CompanyId);
                }
            }
            else if (IsPaymentFailedInvoicesExistForCompany)
            {
                multStatusCompany.ActiveViewIndex = 2;
                View activeView = multStatusCompany.GetActiveView();

                Literal ltrlExpiresOn = (Literal)activeView.FindControl("ltrlExpiresOn");
                Image imgPaymentFailed = (Image)activeView.FindControl("imgPaymentFailed");
                imgPaymentFailed.ToolTip = ProjectFinanceHandler.GetCompanyPaymentFailureDetails(CompanyId);

                if (company.CompanyStatusCodeId == companyGracePeriodCodeId && IsPaymentFailedInvoicesExistForCompany)
                {
                    ltrlExpiresOn.Text = string.Format("Payment Failed (Company will become read-only on {0})", Support.FormatDate(company.ExpirationDate));
                }
                else if (IsPaymentFailedCompany)
                {
                    if (companyStatusCodeId == paymentFailed)
                        ltrlExpiresOn.Text = "<strong>Company activity is suspended</strong>";
                    else
                        ltrlExpiresOn.Text = Utils.GetCodeDescription(companyStatusCodeId);
                }
            }
        }

        #endregion Support Methods
    }
}