using System;
using System.Linq;
using StageBitz.Data;
using StageBitz.Common;

namespace StageBitz.Server.Agent
{
    /// <summary>
    /// Common base class for server agent task handler types.
    /// </summary>
    public abstract class TaskHandlerBase
    {
        private int TaskTypeCodeId
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHandlerBase"/> class.
        /// </summary>
        /// <param name="taskTypeCodeValue">The task type code value.</param>
        public TaskHandlerBase(string taskTypeCodeValue)
        {
            TaskTypeCodeId = Utils.GetCodeByValue("SystemTaskType", taskTypeCodeValue).CodeId;
        }

        /// <summary>
        /// Checks the task schedule to see whether the task should be run
        /// and performs the business logic actions if needed.
        /// </summary>
        public void Run()
        {
            if (ShouldRunTask())
            {
                try
                {
                    UpdateAgentStartDate();
                    PerformActions();
                }
                catch (Exception ex)
                {
                    AgentErrorLog.HandleException(ex);
                }
            }
        }

        /// <summary>
        /// Checks the task schedule to see whether this task handler should be run now.
        /// </summary>
        private bool ShouldRunTask()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                SystemTask task = dataContext.SystemTasks.Where(t => t.TaskTypeCodeId == TaskTypeCodeId).FirstOrDefault();

                if (task == null || !task.IsActive)
                {
                    return false;
                }
                else
                {
                    if (task.LastRunDate == null)
                    {
                        return true;
                    }

                    return ShouldRunTask(task.LastRunDate.Value);
                }
            }
        }

        protected abstract bool ShouldRunTask(DateTime lastRunDate);

        /// <summary>
        /// Updates the task schedule to to reflect the start date and time.
        /// </summary>
        private void UpdateAgentStartDate()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                SystemTask task = dataContext.SystemTasks.Where(t => t.TaskTypeCodeId == TaskTypeCodeId).FirstOrDefault();
                task.LastRunDate = Utils.Now;
                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the task schedule to to reflect the latest run date and time.
        /// </summary>
        private void UpdateLastRunDate()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                SystemTask task = dataContext.SystemTasks.Where(t => t.TaskTypeCodeId == TaskTypeCodeId).FirstOrDefault();
                task.LastRunDate = Utils.Now;
                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Performs business logic actions specified for this task handler.
        /// </summary>
        protected abstract void PerformActions();
    }
}
