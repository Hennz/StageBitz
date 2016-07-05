using System;
using System.Linq;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;
using StageBitz.Logic.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Data;
using System.Collections.Generic;

namespace StageBitz.AdminWeb.Company
{
    public partial class CompanyDetails : PageBase
    {
        #region Properties

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

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
                transactionSearch.CompanyId = CompanyId;
                ucInventoryActivity.CompanyId = CompanyId;
                ucProjectActivity.CompanyId = CompanyId;
                uccompanyPaymentPackageTestConfigurations.CompanyId = CompanyId;
                uccompanyPaymentPackageTestConfigurations.Visible = Utils.IsDebugMode;
                LoadBreadCrumbs();
            }
        }

        protected void lvAdmins_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            dynamic admin = (dynamic)((ListViewDataItem)e.Item).DataItem;

            HyperLink lnkAdmin = (HyperLink)e.Item.FindControl("lnkAdmin");
            lnkAdmin.Text = Support.TruncateString(admin.FullName, 50);
            if (admin.FullName.Length > 50)
            {
                lnkAdmin.ToolTip = admin.FullName;
            }

            lnkAdmin.NavigateUrl = "~/User/UserDetails.aspx?ViewUserId=" + admin.UserId;

            Label lblRole = (Label)e.Item.FindControl("lblRole");
            if (lblRole != null)
                lblRole.Text = "(" + admin.Permission + ")";

            Image imgCompAdmin = (Image)e.Item.FindControl("imgCompAdmin");
            if (imgCompAdmin != null)
            {
                int userId = admin.UserId;
                List<CompanyUserRole> companyUserRoles = this.GetBL<CompanyBL>().GetCompanyUserRoles(userId, CompanyId);
                int inventoryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");
                var companyInventoryManage = companyUserRoles.Where(cur => cur.CompanyUserTypeCodeId == inventoryAdminCodeId && cur.IsActive).FirstOrDefault();

                if (companyUserRoles.Count() > 1 && companyInventoryManage != null)
                {
                    imgCompAdmin.Visible = true;
                }
            }
        }

        protected void chkSuspendCompany_CheckedChanged(object sender, EventArgs e)
        {
            divActivate.Visible = divSuspend.Visible = false;
            if (chkSuspendCompany.Checked)
            {
                divSuspend.Visible = true;
                btnConfirmSuspedReactivate.CommandArgument = "SUSPEND";
            }
            else
            {
                divActivate.Visible = true;
                btnConfirmSuspedReactivate.CommandArgument = "ACTIVATE";
            }
            popupConfirmSuspendReactivate.ShowPopup();
        }


        protected void btnConfirmSuspedReactivate_Click(object sender, EventArgs e)
        {
            if (btnConfirmSuspedReactivate.CommandArgument.Equals("SUSPEND"))
            {
                this.GetBL<CompanyBL>().SuspendCompanybySBAdmin(CompanyId, UserID);
            }
            else
            {
                this.GetBL<CompanyBL>().ReactivateCompanybySBAdmin(CompanyId,UserID);
            }
            popupConfirmSuspendReactivate.HidePopup();
            SetCompanySuspensionCheckBox();
        }

        protected void btnCancelSuspendReactivate_Click(object sender, EventArgs e)
        {
            popupConfirmSuspendReactivate.HidePopup();
            SetCompanySuspensionCheckBox();
        }
        #endregion

        private void LoadData()
        {
            StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == CompanyId).FirstOrDefault();

            #region Header Details

            Support.AssignTextToLabel(lblCompanyName, company.CompanyName, 80);

            if (company.CreatedByUserId == null)
            {
                lblCreatedBy.Text = "System";
            }
            else
            {
                StageBitz.Data.User user = DataContext.Users.Where(u => u.UserId == company.CreatedByUserId).FirstOrDefault();
                string userFullName = (user.FirstName + " " + user.LastName).Trim();

                Support.AssignTextToLabel(lblCreatedBy, userFullName, 80);
            }

            ltrlCreatedDate.Text = Support.FormatDate(company.CreatedDate);

            int invoiceFailedCodeId = Utils.GetCodeIdByCodeValue("InvoiceStatus", "FAILED");
            //Check if there any pending invoice exist
            int paymentFailedInvoiceCount = (from i in DataContext.Invoices
                                           join p in DataContext.Projects on i.RelatedID equals p.ProjectId
                                           where i.RelatedTableName == "Project" && i.InvoiceStatusCodeId == invoiceFailedCodeId && p.CompanyId == CompanyId
                                           select i).Count();

            if (paymentFailedInvoiceCount == 1)
                imgPaymentError.Attributes.Add("Title", "There is a project with a payment failure.");
            else if (paymentFailedInvoiceCount > 1)
                imgPaymentError.Attributes.Add("Title", "There are projects with payment failures.");
            else if (FinanceSupport.GetCreditCardToken("Company", CompanyId) == null)
                imgPaymentError.Attributes.Add("Title", "Credit card details not provided.");
            else
                imgPaymentError.Visible = false;
            #endregion

            #region Contact Details

            int truncateLength = 30;
            Support.AssignTextToLabel(lblAddressLine1, company.AddressLine1, truncateLength);
            Support.AssignTextToLabel(lblAddressLine2, company.AddressLine2, truncateLength);
            Support.AssignTextToLabel(lblCity, company.City, truncateLength);
            Support.AssignTextToLabel(lblState, company.State, truncateLength);
            Support.AssignTextToLabel(lblPostCode, company.PostCode, truncateLength);

            if (company.Country != null)
                Support.AssignTextToLabel(lblCountry, company.Country.CountryName, truncateLength);

            Support.AssignTextToLabel(lblPhone, company.Phone, truncateLength);
            Support.AssignTextToLabel(lblWebsite, company.Website, truncateLength);

            #endregion

            SetCompanySuspensionCheckBox();

            LoadCompanyAdministrators();
        }

        private void LoadCompanyAdministrators()
        {
            int primaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");

            var admins = from cu in DataContext.CompanyUsers
                         join u in DataContext.Users on cu.UserId equals u.UserId
                         from cur in
                             (
                                 from curTemp in DataContext.CompanyUserRoles
                                 join cut in DataContext.Codes on curTemp.CompanyUserTypeCodeId equals cut.CodeId
                                 where cu.CompanyUserId == curTemp.CompanyUserId && curTemp.IsActive
                                 orderby cut.SortOrder
                                 select curTemp).Take(1)
                         where cu.CompanyId == CompanyId && cur.IsActive
                         orderby cur.Code.SortOrder
                         select new { u.UserId, FullName = (u.FirstName + " " + u.LastName).Trim(), IsPrimaryAdmin = (cur.CompanyUserTypeCodeId == primaryAdminCodeId), Permission = cur.Code.Description };


            lvAdmins.DataSource = admins;
            lvAdmins.DataBind();
        }

        private void SetCompanySuspensionCheckBox()
        {
            chkSuspendCompany.Checked = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId);
        }

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Companies", "~/Company/Companies.aspx");
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }
    }
}