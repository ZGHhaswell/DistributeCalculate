using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributeCalculate.Client.ClientStepViews;
using DistributeCalculate.Contract.StateMachine;

namespace DistributeCalculate.Client.Client
{
    public class ClientJoinInState : IStateMachine<ClientActor>
    {
        public void Enter(ClientActor t)
        {
            
        }

        public void Execute(ClientActor t)
        {
            var clientJoinInWindow = new ClientJoinInWindow(t);
            t.SetWindow(clientJoinInWindow);
            clientJoinInWindow.ShowDialog();
        }

        public void Exit(ClientActor t)
        {
            
        }
    }
}
