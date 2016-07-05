using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;

namespace StageBitz.AdminWeb.Controls.Company
{
    public partial class CompanyPaymentPackageTestConfigurations : UserControlBase
    {
        public int CompanyId
        {
            get
            {
                if (ViewState["companyid"] == null)
                {
                    ViewState["companyid"] = 0;
                }

                return (int)ViewState["companyid"];
            }
            set
            {
                ViewState["companyid"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
                CompanyCurrentUsage companyCurrentUsage = this.GetBL<FinanceBL>().GetCompanyCurrentUsage(CompanyId, null);
                txtCurrentProjectCount.Value = companyCurrentUsage.ProjectCount;
                txtCurrentUserCount.Value = companyCurrentUsage.UserCount;
                txtInvCurrentCount.Value = companyCurrentUsage.InventoryCount;

                if (companyPaymentPackage != null)
                {
                    //This does not get NULL. 0 values as default
                    
                    CompanyPaymentPackage futurePackage = this.GetBL<FinanceBL>().GetLatestRequestForTheCompany(CompanyId);

                    ProjectPaymentPackageDetails projectPackageDetails = Utils.GetSystemProjectPackageDetailByPaymentPackageTypeId(companyPaymentPackage.ProjectPaymentPackageTypeId);
                    InventoryPaymentPackageDetails inventoryPaymentPackageDetails = Utils.GetSystemInventoryPackageDetailByPaymentPackageTypeId(companyPaymentPackage.InventoryPaymentPackageTypeId);

                    litActiveProjects.Text = string.Concat(projectPackageDetails.ProjectCount, projectPackageDetails.ProjectCount == 1 ? " Active Project" : " Active Projects");
                   

                    if (this.GetBL<FinanceBL>().IsEducationalCompany(companyPaymentPackage))
                    {
                        litActiveUsers.Text = "Unlimited Users";
                    }
                    else
                    {
                        litActiveUsers.Text = string.Concat(projectPackageDetails.HeadCount, projectPackageDetails.HeadCount == 1 ? " Active User" : " Active Users");
                    }

                    
                    litInventoryItems.Text = string.Concat(inventoryPaymentPackageDetails.ItemCount == null ? "Unlimited " : inventoryPaymentPackageDetails.ItemCount.ToString(), inventoryPaymentPackageDetails.ItemCount == 1 ? " Inventory Item" : " Inventory Items");
                    
                }
                else
                {
                    const string NoPaymentPackageMsg = "No Payment Package";
                    litActiveProjects.Text = NoPaymentPackageMsg;
                    litActiveUsers.Text = NoPaymentPackageMsg;
                    litInventoryItems.Text = NoPaymentPackageMsg;
                }
            }
        }

        protected void btnAddItems_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                DataContext.CreateDummyCompanyItems(CompanyId, (int)txtInvCurrentCount.Value);
                PageBase.ShowNotification("divMsgItems");
            }
        }

        protected void btnAddUsers_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                DataContext.CreateDummyProjectAndCompanyUsers(CompanyId, (int)txtCurrentUserCount.Value);
                PageBase.ShowNotification("divMsgUsers");
            }
        }

        protected void btnAddProjects_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                DataContext.CreateDummyProjects(CompanyId, (int)txtCurrentProjectCount.Value);
                PageBase.ShowNotification("divMsgProjects");
            }
        }
    }
}