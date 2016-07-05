using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StageBitz.Common;
using StageBitz.Logic.Finance.Project;

namespace StageBitz.Server.Agent
{
    /// <summary>
    /// Monthly task handler class.
    /// </summary>
    public class MonthlyTaskHandler : TaskHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyTaskHandler"/> class.
        /// </summary>
        public MonthlyTaskHandler()
            : base("MONTHLY")
        {
        }

        /// <summary>
        /// Shoulds run the task.
        /// </summary>
        /// <param name="lastRunDate">The last run date.</param>
        /// <returns></returns>
        protected override bool ShouldRunTask(DateTime lastRunDate)
        {
            int dayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));
            
            DateTime currentDate = Utils.Now;
            TimeSpan ts;
            TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts);

            DateTime dateToConsiderToGetMargin = new DateTime(Utils.Today.Year, Utils.Today.Month, dayToRun) + ts;
            DateTime leftMarginDate,rightMarginDate;

            if (currentDate >= dateToConsiderToGetMargin)
            {
                leftMarginDate = dateToConsiderToGetMargin;
                rightMarginDate = leftMarginDate.AddMonths(1);
            }
            else
            {
                rightMarginDate = dateToConsiderToGetMargin;
                leftMarginDate = rightMarginDate.AddMonths(-1);
            }

            //monthly agent should not run if the last run date falls into the current time range.
            return !(lastRunDate >= leftMarginDate && lastRunDate < rightMarginDate);
        }

        /// <summary>
        /// Performs business logic actions specified for this task handler.
        /// </summary>
        protected override void PerformActions()
        {
            int dayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));
            DateTime dateToConsider = new DateTime(Utils.Today.Year, Utils.Today.Month, dayToRun);
            TimeSpan ts;
            if (TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts))
            {
                if (Utils.Now < dateToConsider + ts)
                {
                    dateToConsider = dateToConsider.AddMonths(-1);
                }
            }
            ProjectFinanceHandler.ProcessInvoicesAndPayments(0, dateToConsider,true,0);
        }
    }
}
