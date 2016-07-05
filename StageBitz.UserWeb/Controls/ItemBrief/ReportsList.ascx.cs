using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.ItemBrief
{
    /// <summary>
    /// User  control for ReportsList
    /// </summary>
    public partial class ReportsList : UserControlBase
    {
        #region properties

        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        /// <value>
        /// The left margin.
        /// </value>
        public int LeftMargin
        {
            get
            {
                if (ViewState["LeftMargin"] == null)
                    return 360;
                else
                    return (int)ViewState["LeftMargin"];
            }
            set
            {
                ViewState["LeftMargin"] = value;
            }
        }

        #endregion properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            divReportsPopup.Style.Add("margin-left", LeftMargin.ToString() + "px");
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Shows the report links.
        /// </summary>
        /// <param name="itemisedPurchaseReportUrl">The itemised purchase report URL.</param>
        /// <param name="budgetSummaryReportUrl">The budget summary report URL.</param>
        /// <param name="projectId">The project identifier.</param>
        public void ShowReportLinks(string itemisedPurchaseReportUrl, string budgetSummaryReportUrl, int projectId)
        {
            if (Support.CanSeeBudgetSummary(UserID, projectId))
            {
                divReportList.Visible = true;
                hyperLinkItemisedPurchaseReport.NavigateUrl = itemisedPurchaseReportUrl;
                hyperLinkBudgetSummary.NavigateUrl = budgetSummaryReportUrl;
            }
            else
            {
                divReportList.Visible = false;
            }
        }

        /// <summary>
        /// Applies the report link style.
        /// </summary>
        /// <param name="reportName">Name of the report.</param>
        public void ApplyReportLinkStyle(string reportName)
        {
            if (reportName == "BudgetSummaryReport")
            {
                hyperLinkBudgetSummary.CssClass = "highlight";
            }
            else if (reportName == "ItemisedPurchaseReport")
            {
                hyperLinkItemisedPurchaseReport.CssClass = "highlight";
            }

            divReportList.Attributes.Add("class", "boldLink");
        }

        #endregion Public Methods
    }
}