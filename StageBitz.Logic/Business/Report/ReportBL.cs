using StageBitz.Data;
using StageBitz.Data.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Business.Report
{
    /// <summary>
    ///  Business layer for reports
    /// </summary>
    public class ReportBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public ReportBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the budget summary body details.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<BudgetSummaryBody> GetBudgetSummaryBodyDetails(int projectId, int itemTypeId)
        {
            return (from ib in DataContext.ItemBriefs
                    join ibtyp in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtyp.ItemBriefId
                    join it in DataContext.ItemTypes on ibtyp.ItemTypeId equals it.ItemTypeId
                    where ib.ProjectId == projectId && (itemTypeId == -1 || ibtyp.ItemTypeId == itemTypeId)
                    select new BudgetSummaryBody
                    {
                        ItemBriefId = ib.ItemBriefId,
                        ItemType = it.Name,
                        ItemName = ib.Name,
                        Budget = ib.Budget.Value
                    }).ToList<BudgetSummaryBody>();
        }

        /// <summary>
        /// Gets the report header details.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="reportTitle">The report title.</param>
        /// <returns></returns>
        public List<ReportHeaderDetails> GetReportHeaderDetails(int projectId, string userName, string reportTitle)
        {
            return (from p in DataContext.Projects
                    where p.ProjectId == projectId
                    select new ReportHeaderDetails
                    {
                        CompanyName = p.Company.CompanyName,
                        ProjectName = p.ProjectName,
                        UserName = userName,
                        ReportTitle = reportTitle
                    }).ToList<ReportHeaderDetails>();
        }
    }
}