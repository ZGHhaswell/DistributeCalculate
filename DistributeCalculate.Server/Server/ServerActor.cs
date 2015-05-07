using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributeCalculate.Contract.Data;
using DistributeCalculate.Contract.StateMachine;
using DistributeCalculate.Server.Service;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Server.Server
{
    public class ServerActor
    {
        /// <summary>
        /// 服务器Input
        /// </summary>
        public ActorInput ActorInput { get; private set; }

        public IStateMachine<ServerActor> CurrentStateMachine { get;private set; }

        public void Start()
        {
            if (OpenService())
            {
                EnterNextState(new ServerOpenState());
            }
        }

        public void EnterNextState(IStateMachine<ServerActor> nextStateMachine)
        {
            if (CurrentStateMachine != null)
            {
                CurrentStateMachine.Exit(this);
            }
            if (nextStateMachine != null)
            {
                CurrentStateMachine = nextStateMachine;
                CurrentStateMachine.Enter(this);
                CurrentStateMachine.Execute(this);
            }
        }

        private bool OpenService()
        {
            WcfDefault.ApplicationStart(new string[] { }, new WcfTrc.PluginFile(), /*ExitHandler=*/true);
            ClientExecuteMessage.AddKnownTypes();

            ActorInput = new ActorInput("DistributeService", ReponseClientMessage);
            ActorInput.IsMultithreaded = true; // we have no message queue in a console application

            ActorInput.LinkInputToNetwork("DistributeService", 40001, false);

            return ActorInput.TryConnect();
        }


        public void ReponseClientMessage(WcfReqIdent req)
        {
            IWcfMessage response = new WcfErrorMessage(WcfErrorMessage.Code.AppRequestNotAcceptedByService, "");
            if (req.Message is ClientExecuteMessage)
            {
                var clientExecuteMessage = req.Message as ClientExecuteMessage;
                if (clientExecuteMessage.ExecuteType == ExecuteType.Join)
                {
                    response = new ClientExecuteBackMessage(executeType: ExecuteType.JoinCompleted);
                }
            }

            req.SendResponse(response);
        }
    }
}
