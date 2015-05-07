using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributeCalculate.Contract.StateMachine;
using DistributeCalculate.Server.ServerStepViews;

namespace DistributeCalculate.Server.Server
{
    public class ServerOpenState : IStateMachine<ServerActor>
    {
        public void Enter(ServerActor t)
        {
            
        }

        public void Execute(ServerActor t)
        {


            var serverOpen = new ServerOpenWindow();
            serverOpen.ShowDialog();


        }

        public void Exit(ServerActor t)
        {
            
        }
    }
}
