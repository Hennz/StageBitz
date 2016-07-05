using StageBitz.Logic.Business.ItemBrief;
using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.ItemBrief
{
    /// <summary>
    /// User control to show Budget lsit of project item briefs by item type
    /// </summary>
    public partial class BudgetList : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            BudgetSummary,
            ProjectDashboard,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeID
        {
            get
            {
                if (ViewState["ItemTypeID"] == null)
                {
                    ViewState["ItemTypeID"] = 0;
                }

                return (int)ViewState["ItemTypeID"];
            }
            set
            {
                ViewState["ItemTypeID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }

                return (int)ViewState["ProjectID"];
            }
            set
            {
                ViewState["ProjectID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the budget ListView mode.
        /// </summary>
        /// <value>
        /// The budget ListView mode.
        /// </value>
        public ViewMode BudgetListViewMode
        {
            get
            {
                if (ViewState["BudgetListViewMode"] == null)
                {
                    ViewState["BudgetListViewMode"] = default(ViewMode);
                }

                return (ViewMode)ViewState["BudgetListViewMode"];
            }
            set
            {
                ViewState["BudgetListViewMode"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadData();

            if (BudgetListViewMode == ViewMode.ProjectDashboard)
            {
                imgNoEstimatedCost.Attributes.Add("title", "At least one Item Brief has a Task without an estimated cost. To find which one(s), please look at the Budget Summary Report and select ‘All Item Types’ to display.");
            }
            else
            {
                imgNoEstimatedCost.Attributes.Add("title", "At least one Item Brief has a Task without an estimated cost.");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            var budgetListInfo = this.GetBL<ItemBriefBL>().GetBudgetListInfo(ProjectID, ItemTypeID);
            var itemBriefTasksBudget = budgetListInfo.ItemBriefTaskBudgetList;
            var totalBudget = budgetListInfo.GetItemTypeTotalBudget;

            Data.Project project = GetBL<Logic.Business.Project.ProjectBL>().GetProject(ProjectID);
            string cultureName = Support.GetCultureName(project.Country.CountryCode);

            ltrlBudget.Text = Support.FormatCurrency(totalBudget == null ? 0 : totalBudget, cultureName);
            imgNoEstimatedCost.Visible = this.GetBL<ItemBriefBL>().HasEmptyEstimateCostInProject(ProjectID, ItemTypeID);

            int completeStatusCodeID = Support.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED").CodeId;
            int inprogressStatusCodeID = Support.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;

            var sumExpened = budgetListInfo.SumExpened;
            ltrlExpendedAmount.Text = Support.FormatCurrency((sumExpened == null ? 0 : sumExpened), cultureName);

            var sumRemaining = budgetListInfo.SumRemaining;
            ltrlRemainingExpenses.Text = Support.FormatCurrency(sumRemaining == null ? 0 : sumRemaining, cultureName);

            var sumbalance = budgetListInfo.SumBalance;
            ltrlBalanceAmount.Text = Support.FormatCurrency(sumbalance == null ? 0 : sumbalance, cultureName);
        }

        #endregion Private Methods
    }
}