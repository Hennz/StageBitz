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


namespace StageBitz.AdminWeb.User
{
    public partial class UserDetails : PageBase
    {

        private int ViewUserId
        {
            get
            {
                if (ViewState["ViewUserId"] == null)
                {
                    int userId = 0;

                    if (Request["ViewUserId"] != null)
                    {
                        int.TryParse(Request["ViewUserId"], out userId);
                    }

                    ViewState["ViewUserId"] = userId;
                }

                return (int)ViewState["ViewUserId"];
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
                LoadUserCompanies();
                LoadBreadCrumbs();
            }
        }


        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Users", "~/User/Users.aspx");
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        private void LoadData()
        {
            var user = (from u in DataContext.Users
                        join c in DataContext.Countries on u.CountryId equals c.CountryId
                        where u.UserId == ViewUserId
                        select new { user = u, Country = c.CountryName });

            ucUserActivity.ViewUserId = ViewUserId;

            foreach (var selectedUser in user)
            {
                Data.User currentuser = selectedUser.user;

                //Basic details
                ltrlFirstName.Text = Support.TruncateString(currentuser.FirstName, 50);
                ltrlLastName.Text = Support.TruncateString(currentuser.LastName, 50);

                ltrlLoginName.Text = Support.TruncateString(currentuser.LoginName, 50);

                ltrlStatus.Text = currentuser.IsActive ? "Active" : "Pending Activation";

                ltrlLastLogInDate.Text = Support.FormatDate(currentuser.LastLoggedInDate);

                //Contacts
                lblEmail1.Text = Support.TruncateString(currentuser.Email1,32);
                lblEmail1.ToolTip = currentuser.Email2;

                lblEmail2.Text = Support.TruncateString(currentuser.Email2,32);
                lblEmail2.ToolTip = currentuser.Email2;

                lblPhone1.Text = Support.TruncateString(currentuser.Phone1,32);
                lblPhone1.ToolTip = currentuser.Phone1;
                
                lblPhone2.Text = Support.TruncateString(currentuser.Phone2,32);
                lblPhone2.ToolTip = currentuser.Phone2;

                lblAddressLine1.Text = Support.TruncateString(currentuser.AddressLine1,32);
                lblAddressLine1.ToolTip = currentuser.AddressLine1;

                lblAddressLine2.Text = Support.TruncateString(currentuser.AddressLine2,32);
                lblAddressLine2.ToolTip = currentuser.AddressLine2;

                lblCity.Text = Support.TruncateString(currentuser.City,32);
                lblCity.ToolTip = currentuser.City;

                lblState.Text = Support.TruncateString(currentuser.State,32);
                lblState.ToolTip = currentuser.State;

                lblPostalCode.Text =  Support.TruncateString(currentuser.PostCode,32);
                lblPostalCode.ToolTip = currentuser.PostCode;

                lblCountry.Text = Support.TruncateString(selectedUser.Country,32);
                lblCountry.ToolTip = selectedUser.Country;
            }
        }

        private void LoadUserCompanies()
        {
            var userCompanies = (from cu in DataContext.CompanyUsers
                                join c in DataContext.Companies on cu.CompanyId equals c.CompanyId
                                 from cur in
                                     (
                                         from curTemp in DataContext.CompanyUserRoles
                                         join cut in DataContext.Codes on curTemp.CompanyUserTypeCodeId equals cut.CodeId
                                         where cu.CompanyUserId == curTemp.CompanyUserId && curTemp.IsActive
                                         orderby cut.SortOrder
                                         select curTemp).Take(1)
                                join code in DataContext.Codes on cur.CompanyUserTypeCodeId equals code.CodeId
                                where cu.UserId == ViewUserId && cur.IsActive
                                select new { CompanyID = c.CompanyId, CompanyName = c.CompanyName, Permission = code.Description }).ToList();

            //Get Companies of the projects that this user works for.
            var projectCompanies =  (from pu in DataContext.ProjectUsers
                                    join p in DataContext.Projects on pu.ProjectId equals p.ProjectId
                                    where pu.UserId == ViewUserId
                                    select new { CompanyID = p.CompanyId, CompanyName = p.Company.CompanyName, Permission = "" }).Distinct();
            var list = userCompanies.ToList();

            foreach (var pc in projectCompanies)
            {
                var NotAuthorizedCompany = userCompanies.Select(uc => uc.CompanyID == pc.CompanyID);
                
                //If the company we are looking for is not in the list.
                if (!NotAuthorizedCompany.Contains(true))
                {

                    list.Add(pc);
                }
            }

            lvUserCompanies.DataSource = list.ToList();
            lvUserCompanies.DataBind();
        }


        protected void lvUserCompanies_OnItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic companyDetails = e.Item.DataItem as dynamic;

                GridView gvProjects = (GridView)e.Item.FindControl("gvProjects");
                Label lblRole = (Label)e.Item.FindControl("lblRole");

                if (companyDetails.Permission != string.Empty)
                    lblRole.Text = "(" + companyDetails.Permission + ")";

                LinkButton lnkCompanyName = (LinkButton)e.Item.FindControl("lnkCompanyName");

                lnkCompanyName.Text = Support.TruncateString(companyDetails.CompanyName, 50);
                if (companyDetails.CompanyName.Length > 50)
                {
                    lnkCompanyName.ToolTip = companyDetails.CompanyName;
                }

                Image imgCompAdmin = (Image)e.Item.FindControl("imgCompAdmin");
                if (imgCompAdmin != null)
                {
                    int companyId = companyDetails.CompanyID;

                    var companyUserRoles = from cur in DataContext.CompanyUserRoles
                                           join cu in DataContext.CompanyUsers on cur.CompanyUserId equals cu.CompanyUserId
                                           where cu.UserId == ViewUserId && cu.CompanyId == companyId && cur.IsActive && cu.IsActive
                                           select cur;
                    int inventoryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");
                    var companyInventoryManage = companyUserRoles.Where(cur => cur.CompanyUserTypeCodeId == inventoryAdminCodeId && cur.IsActive).FirstOrDefault();

                    if (companyUserRoles.Count() > 1 && companyInventoryManage != null)
                    {
                        imgCompAdmin.Visible = true;
                    }
                }

                lnkCompanyName.PostBackUrl = string.Format("~/Company/CompanyDetails.aspx?CompanyID={0}", companyDetails.CompanyID);


                int companyID = companyDetails.CompanyID;

                var userProjects =  from p in DataContext.Projects
                                    join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                                    join code in DataContext.Codes on pu.ProjectUserTypeCodeId equals code.CodeId
                                    where pu.UserId == ViewUserId && p.CompanyId == companyID && pu.IsActive == true
                                    orderby p.ProjectName
                                    select new { Name = p.ProjectName ,Role = "("+code.Description+ ")" };

                gvProjects.DataSource = userProjects;
                gvProjects.DataBind();
            }
        }

        protected void gvProjects_OnDataBound(Object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                dynamic project = e.Row.DataItem as dynamic;

                Label lblName = (Label)e.Row.FindControl("lblName");
                Label lblRole = (Label)e.Row.FindControl("lblRole");
                lblRole.Text = project.Role;

                lblName.Text = Support.TruncateString(project.Name, 50);
                if (project.Name.Length > 50)
                {
                    lblName.ToolTip = project.Name;
                }
            }
        }
    }
}