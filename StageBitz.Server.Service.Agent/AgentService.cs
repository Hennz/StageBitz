using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace StageBitz.Server.Service.Agent
{
    public partial class AgentService : ServiceBase
    {
        StageBitz.Server.AgentController.AgentController agentController = null;

        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            agentController = new StageBitz.Server.AgentController.AgentController();
            agentController.Start();
        }

        protected override void OnStop()
        {
            agentController.Stop();
        }
    }
}
