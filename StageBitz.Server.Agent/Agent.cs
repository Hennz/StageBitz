using StageBitz.Common;
using StageBitz.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace StageBitz.Server.Agent
{
    /// <summary>
    /// StageBitz server agent.
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// Process starts here.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            try
            {
                //set the culture
                CultureInfo cultureInfo = new CultureInfo("en-AU");
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                bool ownsMutex = true;
                string MutexName = "StageBitzAgent";

                #region Check mutex and run the agent activities

                using (Mutex mutex = new Mutex(true, MutexName, out ownsMutex))
                {
                    if (!ownsMutex)
                    {
                        AgentErrorLog.WriteToErrorLog("StageBitz Agent could not run as previous instance is already running.");
                    }
                    else
                    {
                        try
                        {
                            RunAgent();
                        }
                        catch (Exception ex)
                        {
                            AgentErrorLog.HandleException(ex);
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }

                #endregion Check mutex and run the agent activities
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
            }
        }

        /// <summary>
        /// Update agent execution flag.
        /// </summary>
        /// <param name="isExecuted">The is executed.</param>
        private static void UpDateAgentExecutionFlag(string isExecuted)
        {
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    SystemValue sysValue = dataContext.SystemValues.Where(sv => sv.Name == "IsAgentRunning").FirstOrDefault();
                    sysValue.Value = isExecuted;
                    dataContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
            }
        }

        /// <summary>
        /// Runs the agent.
        /// </summary>
        private static void RunAgent()
        {
            UpDateAgentExecutionFlag("true");
            new RegularTaskHandler().Run();
            new DailyTaskHandler().Run();
            new MonthlyTaskHandler().Run();
            UpDateAgentExecutionFlag("false");
        }
    }
}