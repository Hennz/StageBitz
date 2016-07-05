using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Common.Google;
using StageBitz.Data.DataTypes.Analytics;
using StageBitz.Logic.Business.Personal;
using System;
using System.Drawing;
using System.Globalization;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.Controls.Personal
{
    /// <summary>
    /// User Activity control for display stage bitz user activities form Google Analytics
    /// </summary>
    public partial class UserActivity : UserControlBase
    {
        #region Fields/ Properties

        /// <summary>
        /// The sort order
        /// </summary>
        private GridSortOrder sortOrder = GridSortOrder.Descending;

        /// <summary>
        /// The sort field
        /// </summary>
        private string sortField = "Date";

        /// <summary>
        /// The page index
        /// </summary>
        private int pageIndex = 0;

        /// <summary>
        /// Gets or sets the view user id.
        /// </summary>
        /// <value>
        /// The view user id.
        /// </value>
        public int ViewUserId
        {
            get
            {
                if (ViewState["ViewUserId"] == null)
                {
                    ViewState["ViewUserId"] = 0;
                }

                return (int)ViewState["ViewUserId"];
            }
            set
            {
                ViewState["ViewUserId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        private GridSortOrder SortOrder
        {
            get
            {
                return sortOrder;
            }
            set
            {
                sortOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        /// <value>
        /// The sort field.
        /// </value>
        private string SortField
        {
            get
            {
                return sortField;
            }
            set
            {
                sortField = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        private int PageIndex
        {
            get
            {
                return pageIndex;
            }
            set
            {
                pageIndex = value;
            }
        }

        #endregion Fields/ Properties

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                monthfilter.MaxDate = Utils.Now;
                monthfilter.SelectedDate = Utils.Now;
                LoadUserActivities(Utils.Now);
                gvUserActivity.DataBind();
                LoadUserActivitySummary(Utils.Now);
            }
        }

        protected void monthfilter_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
        {
            LoadUserActivities(monthfilter.SelectedDate.Value);
            LoadUserActivitySummary(monthfilter.SelectedDate.Value);
            gvUserActivity.Rebind();
            upnlProjectTeamActivity.Update();
        }

        protected void gvUserActivity_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                PageIndex = 0;
                SortOrder = e.NewSortOrder;
                SortField = e.SortExpression;
            }
        }

        protected void gvUserActivity_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            string sortBy = gvUserActivity.MasterTableView.SortExpressions[0].FieldName;
            GridSortOrder sortOrder = gvUserActivity.MasterTableView.SortExpressions[0].SortOrder;

            PageIndex = e.NewPageIndex;
            SortOrder = sortOrder;
            SortField = sortBy;
        }

        protected void gvUserActivity_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            string sortBy = gvUserActivity.MasterTableView.SortExpressions[0].FieldName;
            GridSortOrder sortOrder = gvUserActivity.MasterTableView.SortExpressions[0].SortOrder;

            PageIndex = 0;
            SortOrder = sortOrder;
            SortField = sortBy;
        }

        protected void gvUserActivity_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            LoadUserActivities(monthfilter.SelectedDate.Value);
            upnlProjectTeamActivity.Update();
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the user activities.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        private void LoadUserActivities(DateTime selectedDate)
        {
            DateTime startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime endDate = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));

            try
            {
                bool isAscending = SortOrder == GridSortOrder.Ascending ? true : false;
                int totalRecords;
                int pageSize = gvUserActivity.MasterTableView.PageSize;
                var userActivityList = GetBL<PersonalBL>().GetUserActivitiesFromGoogleAnalytics(ViewUserId, startDate, endDate, pageSize, PageIndex, SortField, isAscending, out totalRecords);
                gvUserActivity.VirtualItemCount = totalRecords;
                gvUserActivity.DataSource = userActivityList;
            }
            catch (StageBitzException ex)
            {
                gvUserActivity.DataSource = string.Empty;
                gvUserActivity.Rebind();

                GridNoRecordsItem norecordItem = (GridNoRecordsItem)gvUserActivity.MasterTableView.GetItems(GridItemType.NoRecordsItem)[0];
                Label lblNoData = (Label)norecordItem.FindControl("lblNoData");
                Label lblError = (Label)norecordItem.FindControl("lblError");

                lblNoData.Visible = false;
                lblError.Visible = true;
                lblError.Text = ex.InnerException.ToString();
            }
        }

        /// <summary>
        /// Loads the user activity summary.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        private void LoadUserActivitySummary(DateTime selectedDate)
        {
            DateTime startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime endDate = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));
            try
            {
                AnalyticsManager analyticsManager = new AnalyticsManager();
                UserActivitySummary projectActivitySummary = analyticsManager.GetUserActivitySummary(ViewUserId, startDate, endDate);

                lblAccessedProjects.Text = projectActivitySummary.ProjectCount.ToString(CultureInfo.InvariantCulture);
                lblAccessedCompanies.Text = projectActivitySummary.CompanyCount.ToString(CultureInfo.InvariantCulture);
                lblActiveDays.Text = projectActivitySummary.DaysCount.ToString(CultureInfo.InvariantCulture);
                lblActiveDays.ForeColor = lblAccessedCompanies.ForeColor = lblAccessedProjects.ForeColor = Color.Empty;
            }
            catch (StageBitzException ex)
            {
                lblActiveDays.Text = lblAccessedCompanies.Text = lblAccessedProjects.Text = "Error!";
                lblActiveDays.ToolTip = lblAccessedCompanies.ToolTip = lblAccessedProjects.ToolTip = ex.InnerException.ToString();
                lblActiveDays.ForeColor = lblAccessedCompanies.ForeColor = lblAccessedProjects.ForeColor = Color.Red;
            }
        }

        #endregion Private Methods
    }
}