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
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Personal;
using System.Data;
using StageBitz.Reports.AdminWeb.Helper;
using StageBitz.Common.Enum;

namespace StageBitz.AdminWeb.User
{
    public partial class Users : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();
            }
        }

        protected void gvUsers_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvUsers.MasterTableView.SortExpressions.Clear();
                gvUsers.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvUsers.Rebind();
            }
        }


        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Users", null);
            breadCrumbs.LoadControl();
        }

        protected void gvUsers_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic user = (dynamic)dataItem.DataItem;

                HyperLink lnkFirstName = (HyperLink)e.Item.FindControl("lnkFirstName");
                lnkFirstName.Text = Support.TruncateString(user.FirstName, 20);
                lnkFirstName.NavigateUrl = string.Format("~/User/UserDetails.aspx?ViewUserId={0}", user.UserId);
                if (user.FirstName.Length > 20)
                {
                    lnkFirstName.ToolTip = user.FirstName;
                }


                Literal litStatus = (Literal)e.Item.FindControl("litStatus");
                litStatus.Text = user.Status;
                if (user.IsStageBitzAdmin)
                {
                    Image imgAdmin = (Image)e.Item.FindControl("imgAdmin");
                    imgAdmin.Visible = true;
                }

                HyperLink lnkLastName = (HyperLink)e.Item.FindControl("lnkLastName");
                lnkLastName.Text = Support.TruncateString(user.LastName, 20);
                lnkLastName.NavigateUrl = string.Format("~/User/UserDetails.aspx?ViewUserId={0}", user.UserId);
                if (user.LastName.Length > 20)
                {
                    lnkLastName.ToolTip = user.LastName;
                }


                dataItem["City"].Text = Support.TruncateString(user.City, 10);
                if (user.City != null && user.City.Length > 10)
                {
                    dataItem["City"].ToolTip = user.City;
                }

                dataItem["State"].Text = Support.TruncateString(user.State, 10);
                if (user.State != null && user.State.Length > 10)
                {
                    dataItem["State"].ToolTip = user.State;
                }

                dataItem["Country"].Text = Support.TruncateString(user.Country, 15);
                if (user.Country != null && user.Country.Length > 20)
                {
                    dataItem["Country"].ToolTip = user.Country;
                }

                dataItem["LastLogIn"].Text = Support.FormatDate(user.LastLogIn);
                dataItem["RegisteredDate"].Text = Support.FormatDate(user.RegisteredDate);
            }
        }

        protected void gvUsers_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            List<UserListForAdmin> userList = this.GetBL<PersonalBL>().GetAllUsers();
            gvUsers.DataSource = userList;
        }

        public DataTable GetUserList()
        {
            List<UserListForAdmin> userList = this.GetBL<PersonalBL>().GetAllUsers();
            using (DataTable dtUsers = new DataTable("tblItems"))
            {
                dtUsers.Columns.Add("FirstName");
                dtUsers.Columns.Add("LastName");
                dtUsers.Columns.Add("City");
                dtUsers.Columns.Add("Country");
                dtUsers.Columns.Add("LastLogIn", typeof(DateTime));
                dtUsers.Columns.Add("RegisteredDate", typeof(DateTime));
                dtUsers.Columns.Add("Email");
                dtUsers.Columns.Add("State");
                dtUsers.Columns.Add("Status");


                foreach (UserListForAdmin userListForAdmin in userList)
                {
                    DataRow dtRow = dtUsers.NewRow();
                    dtRow["FirstName"] = userListForAdmin.FirstName;
                    dtRow["LastName"] = userListForAdmin.LastName;
                    dtRow["City"] = userListForAdmin.City;
                    dtRow["Country"] = userListForAdmin.Country;
                    dtRow["LastLogIn"] = userListForAdmin.LastLogIn == null ? (object)DBNull.Value : userListForAdmin.LastLogIn.Value;
                    dtRow["RegisteredDate"] = userListForAdmin.RegisteredDate == null ? (object)DBNull.Value : userListForAdmin.RegisteredDate.Value;
                    dtRow["Email"] = userListForAdmin.Email;
                    dtRow["State"] = userListForAdmin.State;
                    dtRow["Status"] = userListForAdmin.Status;
                    dtUsers.Rows.Add(dtRow);
                }
                return dtUsers;
            }
        }

        protected void btnExporttoExcel_Click(object sender, EventArgs e)
        {
            string sortExpression = gvUsers.MasterTableView.SortExpressions.GetSortString();
            DataView dvUsers = GetUserList().DefaultView;
            //set sorting
            if (!string.IsNullOrEmpty(sortExpression))
            {
                dvUsers.Sort = sortExpression;
            }

            string fileNameExtension;
            string encoding;
            string mimeType;
            string fileName = "UserList";

            byte[] reportBytes = AdminWebReportHandler.GenerateUserListReport(dvUsers, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }
    }
}