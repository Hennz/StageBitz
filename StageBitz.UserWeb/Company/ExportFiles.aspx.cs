using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Export file web page.
    /// </summary>
    public partial class ExportFiles : PageBase
    {
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
        }

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
                bool isAdmin = Support.IsCompanyAdministrator(CompanyId);

                if (!(isAdmin))
                {
                    throw new ApplicationException("You do not have administrator rights to view this page.");
                }

                exportFilesList.CompanyId = CompanyId;
                string companyName = GetBL<CompanyBL>().GetCompany(CompanyId).CompanyName;
                DisplayTitle = string.Format("{0}'s Export Information", companyName);
                LoadBreadCrumbs(GetBL<CompanyBL>().GetCompany(CompanyId).CompanyName);
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);
                lnkCreateNewProject.CompanyId = CompanyId;
                lnkCreateNewProject.LoadData();
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        private void LoadBreadCrumbs(string companyName)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            bc.AddLink(companyName, string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", CompanyId));
            bc.AddLink("Export Information", null);

            bc.LoadControl();
        }
    }
}