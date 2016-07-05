using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Web page for company details.
    /// </summary>
    public partial class CompanyDetails : PageBase
    {
        #region Properties

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
                if (ViewState["companyId"] != null)
                {
                    return (int)ViewState["companyId"];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["companyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this page is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this page is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] != null)
                {
                    return (bool)ViewState["IsReadOnly"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
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
            if (!IsPostBack)
            {
                string companyName = string.Empty;

                if (Request.QueryString["companyid"] != null)
                {
                    CompanyId = Convert.ToInt32(Request.QueryString["companyid"].ToString());
                }
                //If a companyId is passed in the Edit mode
                if (CompanyId > 0)
                {
                    bool isAdmin = Support.IsCompanyAdministrator(CompanyId);
                    bool hasCompanySuspended = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);

                    if (!isAdmin)
                    {
                        throw new ApplicationException("You do not have administrator rights to view this page.");
                    }

                    IsReadOnly = Support.IsReadOnlyRightsForCompany(CompanyId);

                    CompanyHeaderDetails.IsReadOnly = IsReadOnly;
                    CompanyHeaderDetails.CompanyId = CompanyId;
                    IntializeControls();
                    companyName = Support.GetCompanyNameById(CompanyId);

                    sbCompanyWarningDisplay.CompanyID = CompanyId;
                    sbCompanyWarningDisplay.LoadData();

                    LoadData(CompanyId);

                    btnCancel.Visible = true;

                    DisplayTitle = string.Format("{0} Details", companyName);
                    btnSubmit.Text = "Done";

                    spnNewCompanyNavigation.Visible = !hasCompanySuspended && isAdmin;
                    lnkCreateNewProject.CompanyId = this.CompanyId;
                    lnkCreateNewProject.LoadData();
                    lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);
                }
                LoadBreadCrumbs(companyName);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!IsValid)
                {
                    return;
                }

                ucCompanyDetails.SaveCompanyDetails(true);

                if (CompanyId == 0)
                {
                    CompanyId = ucCompanyDetails.Company.CompanyId;
                    //Add media content.
                    DocumentMedia media = CompanyHeaderDetails.GetMedia();

                    if (media != null)
                    {
                        media.RelatedId = CompanyId;
                        DataContext.SaveChanges();
                    }

                    //Notification email to StageBitzAdmins.Get the System@StageBitz.com
                    StageBitz.Common.EmailSender.SendCompanyRegistrationMailToStageBitzAdmin(Support.GetSystemValue("AdminEmail"),
                            Support.GetAdminPortalCompanyDetailsLink(CompanyId), ucCompanyDetails.Company.CompanyName, Support.UserFullName);
                }
                Response.Redirect(string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", CompanyId));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect(string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", CompanyId));
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Intializes the controls.
        /// </summary>
        private void IntializeControls()
        {
            ucCompanyDetails.InitializeControls(IsReadOnly);
            CompanyHeaderDetails.IntializeHeaderControls();
            btnSubmit.Visible = !IsReadOnly;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        private void LoadData(int companyId)
        {
            Data.Company company = this.GetBL<CompanyBL>().GetCompany(CompanyId);
            ucCompanyDetails.LoadData(company);
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        private void LoadBreadCrumbs(string companyName)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            if (CompanyId == 0)
            {
                bc.AddLink("Create new Company", null);
            }
            else
            {
                bc.AddLink(companyName, string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", CompanyId));
                bc.AddLink(DisplayTitle, null);
            }

            bc.LoadControl();
        }

        #endregion Private Methods
    }
}