using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Security.Permissions;

namespace StageBitz.Server.AgentController
{
    public sealed class AgentController
    {
        Timer timer = null;

        /// <summary>
        /// This is the first method that runs in Agent Controller.
        /// This initiates the timer.
        /// </summary>
        public void Start()
        {
            string state = "Timer elapsed";

            int timerInterval = 60000;//default is 60 seconds
            int.TryParse(ConfigurationManager.AppSettings["TimerInterval"], out timerInterval);

            //create the timer
            TimerCallback timerCallBack = new TimerCallback(TimerHandler);
            timer = new Timer(timerCallBack, state, 0, timerInterval);
        }

        public void Stop()
        {
            timer.Change(0, 0);
            timer.Dispose();
        }

        /// <summary>
        /// This is the timer call back method that is executed after every timer-ealpse
        /// </summary>
        [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        public void TimerHandler(object state)
        {
            #region Load file paths

            string agentExecutablePath = ConfigurationManager.AppSettings["AgentExecutablePath"];
            string agentExecutableName = ConfigurationManager.AppSettings["AgentExecutableName"];

            //Set the default file name and path if they are not configured

            if (string.IsNullOrEmpty(agentExecutableName))
            {
                agentExecutableName = "StageBitz.Server.Agent.exe";
            }

            if (string.IsNullOrEmpty(agentExecutablePath))
            {
                agentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            string agentFullPath = Path.Combine(agentExecutablePath, agentExecutableName);

            #endregion

            try
            {
                //Start the agent if it is not currently running
                Process[] runningAgents = Process.GetProcessesByName(agentFullPath);
                if (runningAgents.Length == 0)
                {
                    //kickstart the agent
                    ProcessStartInfo agentStartInfo = new ProcessStartInfo();
                    agentStartInfo.FileName = agentFullPath;
                    using (Process agent = new Process())
                    {
                        agent.StartInfo = agentStartInfo;

                        try
                        {
                            agent.Start();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Executing Agent process failed. <path " + agentFullPath + "> <msg " + ex.Message + ">");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while initializing StageBitz Agent : " + ex.Message);
            }
        }
    }
}
