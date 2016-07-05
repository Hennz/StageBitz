using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Server.AgentHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            AgentController.AgentController agentController = new AgentController.AgentController();
            agentController.Start();

            Console.WriteLine("Agent Controller Successfully started!");
            Console.ReadLine(); 
        }
    }
}
