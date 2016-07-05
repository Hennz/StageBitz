using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Common.Google;
using StageBitz.Data.DataTypes.Analytics;
using StageBitz.Logic.Business.Project;
using System;
using System.Drawing;
using System.Globalization;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.Controls.Project
{
    /// <summary>
    /// Project team Activity control for display stage bitz user activities form Google Analytics
    /// </summary>
    public partial class ProjectActivity : UserControlBase
    {
        #region Properties/ Fields

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
        /// Gets or sets the company id.
        /// </summary>
        /// <value>
        /// The company id.
        /// </value>
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

        #endregion Properties/ Fields

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                monthfilter.MaxDate = Utils.Now;
                monthfilter.SelectedDate = Utils.Now;
                LoadProjectTeamActivities(Utils.Now);
                gvProjectTeamActivity.DataBind();
                LoadProjectTeamActivitySummary(Utils.Now);
            }
        }

        protected void monthfilter_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
        {
            LoadProjectTeamActivities(monthfilter.SelectedDate.Value);
            LoadProjectTeamActivitySummary(monthfilter.SelectedDate.Value);
            gvProjectTeamActivity.Rebind();
            upnlProjectTeamActivity.Update();
        }

        protected void gvProjectTeamActivity_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                PageIndex = 0;
                SortOrder = e.NewSortOrder;
                SortField = e.SortExpression;
            }
        }

        protected void gvProjectTeamActivity_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            string sortBy = gvProjectTeamActivity.MasterTableView.SortExpressions[0].FieldName;
            GridSortOrder sortOrder = gvProjectTeamActivity.MasterTableView.SortExpressions[0].SortOrder;

            PageIndex = e.NewPageIndex;
            SortOrder = sortOrder;
            SortField = sortBy;
        }

        protected void gvProjectTeamActivity_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            string sortBy = gvProjectTeamActivity.MasterTableView.SortExpressions[0].FieldName;
            GridSortOrder sortOrder = gvProjectTeamActivity.MasterTableView.SortExpressions[0].SortOrder;

            PageIndex = 0;
            SortOrder = sortOrder;
            SortField = sortBy;
        }

        protected void gvProjectTeamActivity_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            gvProjectTeamActivity.MasterTableView.CurrentPageIndex = PageIndex;
            LoadProjectTeamActivities(monthfilter.SelectedDate.Value);
            upnlProjectTeamActivity.Update();
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Loads the project team activities.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        private void LoadProjectTeamActivities(DateTime selectedDate)
        {
            DateTime startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime endDate = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));

            try
            {
                bool isAscending = SortOrder == GridSortOrder.Ascending ? true : false;
                int totalRecords;
                int pageSize = gvProjectTeamActivity.MasterTableView.PageSize;
                var projectTeamActivityList = GetBL<ProjectBL>().GetProjectTeamActivitiesFromGoogleAnalytics(CompanyId, startDate, endDate, pageSize, PageIndex, SortField, isAscending, out totalRecords);
                gvProjectTeamActivity.VirtualItemCount = totalRecords;
                gvProjectTeamActivity.DataSource = projectTeamActivityList;
            }
            catch (StageBitzException ex)
            {
                gvProjectTeamActivity.DataSource = string.Empty;
                gvProjectTeamActivity.Rebind();

                GridNoRecordsItem norecordItem = (GridNoRecordsItem)gvProjectTeamActivity.MasterTableView.GetItems(GridItemType.NoRecordsItem)[0];
                Label lblNoData = (Label)norecordItem.FindControl("lblNoData");
                Label lblError = (Label)norecordItem.FindControl("lblError");

                lblNoData.Visible = false;
                lblError.Visible = true;
                lblError.Text = ex.InnerException.ToString();
            }
        }

        /// <summary>
        /// Loads the project team activity summary.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        private void LoadProjectTeamActivitySummary(DateTime selectedDate)
        {
            DateTime startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime endDate = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));
            try
            {
                AnalyticsManager analyticsManager = new AnalyticsManager();
                ProjectTeamActivitySummary projectActivitySummary = analyticsManager.GetProjectTeamActivitySummary(CompanyId, startDate, endDate);

                lblAccessedProjects.Text = projectActivitySummary.ProjectCount.ToString(CultureInfo.InvariantCulture);
                lblActiveTeamMembers.Text = projectActivitySummary.UserCount.ToString(CultureInfo.InvariantCulture);
                lblActiveDays.Text = projectActivitySummary.DaysCount.ToString(CultureInfo.InvariantCulture);
                lblActiveDays.ForeColor = lblActiveTeamMembers.ForeColor = lblAccessedProjects.ForeColor = Color.Empty;
            }
            catch (StageBitzException ex)
            {
                lblActiveDays.Text = lblActiveTeamMembers.Text = lblAccessedProjects.Text = "Error!";
                lblActiveDays.ToolTip = lblActiveTeamMembers.ToolTip = lblAccessedProjects.ToolTip = ex.InnerException.ToString();
                lblActiveDays.ForeColor = lblActiveTeamMembers.ForeColor = lblAccessedProjects.ForeColor = Color.Red;
            }
        }

        #endregion Private Methods
    }
}