using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.Data;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;

namespace StageBitz.AdminWeb
{
    public partial class _Default : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int normalUserTypeCodeId = Utils.GetCodeIdByCodeValue("UserAccountType", "USER");

                //Load statistics
                var activeUserCount = DataContext.Users.Count(u => u.IsActive == true);
                var inactiveUserCount = DataContext.Users.Count(u => u.IsActive == false);
                var companyCount = DataContext.Companies.Count();
                var projectCount = DataContext.Projects.Count();

                ltrlUserStats.Text = string.Format("<br /><strong>{0}</strong> Active User{1}<br /><strong>{2}</strong> Pending Activation", activeUserCount, (activeUserCount == 1) ? string.Empty : "s", inactiveUserCount);
                ltrlCompanyStats.Text = string.Format("<br /><strong>{0}</strong> Compan{1}<br /><strong>{2}</strong> Project{3}", companyCount, (companyCount == 1) ? "y" : "ies", projectCount, (projectCount == 1) ? string.Empty : "s");
            }
        }
    }
}
