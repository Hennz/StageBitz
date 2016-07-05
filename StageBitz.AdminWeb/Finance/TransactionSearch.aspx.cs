using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.AdminWeb.Controls.Common;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.Finance
{
    public partial class TransactionSearch : PageBase
    {
    
        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();
            }
        }
        #endregion

        #region Support Methods

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Finance", null);
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        #endregion
    }
}