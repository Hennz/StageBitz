using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Data;
using Telerik.Web.UI;
using System.Web.UI.HtmlControls;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;

namespace StageBitz.AdminWeb.Company
{
    public partial class Companies : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();
            }
        }


        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Companies", null);
            breadCrumbs.LoadControl();
        }

        protected void gvCompanies_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvCompanies.MasterTableView.SortExpressions.Clear();
                gvCompanies.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvCompanies.Rebind();
            }
        }


        protected void gvCompanies_ItemDataBound(object sender, GridItemEventArgs e)
        {            
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic company = (dynamic)dataItem.DataItem;
                
                HyperLink lnkName = (HyperLink)e.Item.FindControl("lnkName");
                lnkName.Text = Support.TruncateString(company.Name, 35);
                if (company.Name.Length > 35)
                {
                    lnkName.ToolTip = company.Name;
                }
                lnkName.NavigateUrl = string.Format("~/Company/CompanyDetails.aspx?CompanyID={0}", company.CompanyId);

                HyperLink lnkPAdmin = (HyperLink)e.Item.FindControl("lnkPAdmin");
                lnkPAdmin.Text = Support.TruncateString(company.PAdmin, 20);
                if (company.Name.Length > 20)
                {
                    lnkPAdmin.ToolTip = company.PAdmin;
                }
                lnkPAdmin.NavigateUrl = string.Format("~/User/UserDetails.aspx?ViewUserID={0}", company.PAdminID);

                dataItem["Country"].Text = Support.TruncateString(company.Country, 20);
                if (company.Country != null && company.Country.Length > 20)
                {
                    dataItem["Country"].ToolTip = company.Country;
                }

                dataItem["City"].Text = Support.TruncateString(company.City, 20);
                if (company.City != null && company.City.Length > 20)
                {
                    dataItem["City"].ToolTip = company.City;
                }

                dataItem["State"].Text = Support.TruncateString(company.State, 20);
                if (company.State != null && company.State.Length > 20)
                {
                    dataItem["State"].ToolTip = company.State;
                }

                dataItem["CreatedDate"].Text = Support.FormatDate(DateTime.Parse(dataItem["CreatedDate"].Text));
            }
        }

        protected void gvCompanies_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            int companyPrimaryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");

            var companyList =   from c in DataContext.Companies
                                join cu in DataContext.CompanyUsers on c.CompanyId equals cu.CompanyId
                                join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                where cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                orderby c.CompanyName
                                select new
                                {
                                    CompanyId = c.CompanyId,
                                    Name = c.CompanyName,
                                    PAdmin = (cu.User.FirstName + " " + cu.User.LastName),
                                    PAdminID = cu.UserId,
                                    Country = c.Country != null ? c.Country.CountryName : string.Empty,
                                    City = c.City,
                                    State = c.State,
                                    CreatedDate = c.CreatedDate,
                                    projectCount = c.Projects.Count
                                };

            gvCompanies.DataSource = companyList;
        }
    }
}
