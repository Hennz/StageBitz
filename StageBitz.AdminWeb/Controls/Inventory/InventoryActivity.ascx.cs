using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Common.Google;
using StageBitz.Data.DataTypes.Analytics;
using StageBitz.Logic.Business.Company;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.Controls.Inventory
{
    public partial class InventoryActivity : UserControlBase
    {
        #region Properties

        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        #endregion

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
                monthfilter.MaxDate = Utils.Now;
                monthfilter.SelectedDate = Utils.Now;
                LoadInventoryActivity(Utils.Now);
            }
        }

        protected void monthfilter_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
        {
            LoadInventoryActivity(monthfilter.SelectedDate.Value);
            upnlInventoryActivities.Update();
        }
        #endregion

        #region Private Methods
        private void LoadInventoryActivity(DateTime selectedDate)
        {
            DateTime startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime endDate = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));

            try
            {
                AnalyticsManager analyticsManager = new AnalyticsManager();
                InventoryPageViews pageViews = analyticsManager.GetInventoryPageViews(CompanyId, startDate, endDate);

                lblInventoryPageViews.Text = pageViews.PageViews.ToString(CultureInfo.InvariantCulture);
                lblInventoryUserViews.Text = pageViews.UserViews.ToString(CultureInfo.InvariantCulture);
                lblInventoryUserViews.ForeColor = lblInventoryPageViews.ForeColor = Color.Empty;
            }
            catch (StageBitzException ex)
            {
                lblInventoryPageViews.Text = "Error!";                
                lblInventoryUserViews.Text = "Error!";
                lblInventoryPageViews.ToolTip = lblInventoryUserViews.ToolTip = ex.InnerException.ToString();

                lblInventoryUserViews.ForeColor = lblInventoryPageViews.ForeColor = Color.Red;
            }

            gvInventoryActivity.DataSource = this.GetBL<CompanyBL>().GetInventoryActivityData(CompanyId, startDate,endDate);
            gvInventoryActivity.DataBind();
        }

        #endregion
    }
}