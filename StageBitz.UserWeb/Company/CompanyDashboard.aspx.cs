using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.UserWeb.Company
{
    public partial class CompanyDashboard : PageBase
    {
        #region Properties

        /// <summary>
        /// The company name
        /// </summary>
        private string companyName = string.Empty;

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        private int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    int CompanyId = 0;

                    if (Request["CompanyId"] != null)
                    {
                        int.TryParse(Request["CompanyId"], out CompanyId);
                    }

                    ViewState["CompanyId"] = CompanyId;
                }
                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        /// <value>
        /// The name of the company.
        /// </value>
        public string CompanyName
        {
            get { return companyName; }
            set { companyName = value; }
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
                if (Request.QueryString["companyid"] != null)
                {
                    this.CompanyId = Convert.ToInt32(Request.QueryString["companyid"].ToString());
                }

                bool isAdmin = Support.IsCompanyAdministrator(CompanyId);

                if (!isAdmin)
                {
                    throw new ApplicationException("You do not have administrator rights to view this page.");
                }

                this.CompanyName = Support.GetCompanyNameById(CompanyId);
                DisplayTitle = string.Format("{0}'s Dashboard", Support.TruncateString(CompanyName, 30));

                #region Set Links

                #endregion Set Links

                companyProjectList.CompanyId = CompanyId;

                sbCompanyWarningDisplay.CompanyID = CompanyId;
                sbCompanyWarningDisplay.LoadData();

                LoadAdmin();
                LoadInventory();
                LoadBreadCrumbs();
                InitializeLinks();
                scheduleList.CompanyID = CompanyId;

                int profileImageId = (from m in DataContext.DocumentMedias
                                      where m.RelatedTableName == "Company" && m.RelatedId == CompanyId && m.SortOrder == 1
                                      select m.DocumentMediaId).FirstOrDefault();

                bool hasProfileImage = (profileImageId != 0);

                if (hasProfileImage)
                {
                    idCompanyLogo.DocumentMediaId = profileImageId;
                    idCompanyLogo.Visible = true;
                }
                else
                {
                    imgCompanies.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSearchInventory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearchInventory_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect(ResolveUrl(string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId)));
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Initializes the links.
        /// </summary>
        private void InitializeLinks()
        {
            if (this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId) || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId))
            {
                spanCreateNewProject.Visible = false;
            }

            linkCompanyDetails.HRef = string.Format("~/Company/CompanyDetails.aspx?companyid={0}", CompanyId);
            lnkCreateNewProject.CompanyId = this.CompanyId;
            lnkCreateNewProject.LoadData();
            lnkFinancialDetails.HRef = string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId);
            lnkManageAdministrators.HRef = string.Format("~/Company/CompanyAdministrator.aspx?companyid={0}", CompanyId);
            lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?Companyid={0}", CompanyId);
            lnkExportFiles.HRef = string.Format("~/Company/ExportFiles.aspx?companyid={0}", CompanyId);
        }

        /// <summary>
        /// Loads the admin.
        /// </summary>
        private void LoadAdmin()
        {
            List<Code> companyUserTypeCode = Support.GetCodesByCodeHeader("CompanyUserTypeCode");
            int companyAdminCodeID = companyUserTypeCode.Where(c => c.Value == "SECADMIN").FirstOrDefault().CodeId;
            int companyPrimaryAdminCodeID = companyUserTypeCode.Where(c => c.Value == "ADMIN").FirstOrDefault().CodeId;

            int noOfCompanyAdmins = (from cu in DataContext.CompanyUsers
                                     from cur in DataContext.CompanyUserRoles.Where(cur => cur.CompanyUserId == cu.CompanyUserId && cur.IsActive
                                        && (cur.CompanyUserTypeCodeId == companyAdminCodeID || cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID)).Take(1)
                                     where cu.CompanyId == CompanyId && cu.IsActive == true
                                     select cu).Count();

            var primaryAdmin = this.GetBL<CompanyBL>().GetCompanyPrimaryAdministrator(CompanyId);

            if (noOfCompanyAdmins > 1)
            {
                ltNoOfAdministrators.Text = "There are " + Convert.ToString(noOfCompanyAdmins) + " Team Members </br>";
            }

            ltPrimaryAdmin.Text = "Primary Administrator is " + primaryAdmin.FirstName + " " + primaryAdmin.LastName;
        }

        /// <summary>
        /// Loads the inventory.
        /// </summary>
        private void LoadInventory()
        {
            int noOfInventoryItems = (from i in DataContext.Items
                                      where i.CompanyId == CompanyId && i.IsActive && !i.IsHidden
                                      select i.ItemId).Count();
            if (noOfInventoryItems > 1)
            {
                ltNoOfInventoryItems.Text = "There are " + Convert.ToString(noOfInventoryItems) + " Items in <br />the Company Inventory";
            }
            else if (noOfInventoryItems == 1)
            {
                ltNoOfInventoryItems.Text = "There is " + Convert.ToString(noOfInventoryItems) + " Item in <br />the Company Inventory";
            }
            else
            {
                ltNoOfInventoryItems.Text = "There are no Items in <br />the Company Inventory";
                btnSearchInventory.Text = "Go to Inventory";
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == CompanyId).FirstOrDefault();
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}