using System;
using System.Web;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.AdminWeb.Controls.Common;
using System.Web.UI;

namespace StageBitz.AdminWeb
{
    public partial class Content : MasterBase
    {
        #region Properties

        public BreadCrumbs BreadCrumbs
        {
            get
            {
                return breadCrumbs;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ltrlPageTitle.Text = ((PageBase)Page).DisplayTitle;

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    divUserInfo.Visible = true;
                    ltrlUser.Text = Support.TruncateString((Support.UserFirstName + " " + Support.UserLastName).Trim(), 25);
                }
                else
                {
                    divUserInfo.Visible = false;
                }
            }
        }
    }
}