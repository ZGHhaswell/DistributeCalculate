using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DistributeCalculate.Contract.Data;
using DistributeCalculate.Contract.StateMachine;
using Nito.Async;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Client.Client
{
    public class ClientActor
    {
        /// <summary>
        /// 客户端Input
        /// </summary>
        public ActorInput ActorInput { get; private set; }

        public ActorOutput ActorOutput { get; private set; }

        public Window CurrentWindow { get; private set; }

        public void SetWindow(Window window)
        {
            CurrentWindow = window;
        }

        public IStateMachine<ClientActor> CurrentStateMachine { get; private set; }

        public void Start()
        {
            OpenClient();

            EnterNextState(new ClientJoinInState());
            
        }

        public void EnterNextState(IStateMachine<ClientActor> nextStateMachine)
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

        public ActionThread ActionThread { get; set; }

        private void OpenClient()
        {
            WcfDefault.ApplicationStart(new string[] { }, new WcfTrc.PluginFile(), /*ExitHandler=*/true);
            ClientExecuteMessage.AddKnownTypes();

            string host = "192.168.1.108:40001";
            string serviceUri = "http://" + host + "/AsyncWcfLib/DistributeService";
            //string serviceUri = "net.tcp://zgh-pc:40001/AsyncWcfLib/DistributeService";
            ActorInput = new ActorInput("ClientInput", ClientInputHandle);
            ActorOutput = new ActorOutput("ClientOutput", ClientOutputHandle);
            ActorOutput.LinkOutputToRemoteService(new Uri(serviceUri));
            ActorOutput.TraceSend = true;

            ActionThread = new ActionThread();
            ActionThread.Start();
            ActionThread.Do(OnStartup);
        }

        private void OnStartup()
        {
            ActorInput.Open();
            ActorOutput.TryConnect();
        }

        private void ClientInputHandle(WcfReqIdent id)
        {
            if (id.Message is ClientExecuteMessage)
            {
                WcfState s = ActorOutput.OutputState;
                if (s == WcfState.Disconnected || s == WcfState.Faulted)
                {
                    OnStartup();
                }
                else if (s == WcfState.Connecting)
                {
                    
                }
                else
                {
                    ActorOutput.SendOut(id.Message);
                }
            }
        }

        private void ClientOutputHandle(WcfReqIdent id)
        {
            if (id.Message is ClientExecuteBackMessage)
            {
                var clientExecuteBackMessage = id.Message as ClientExecuteBackMessage;
                if (clientExecuteBackMessage.ExecuteType == ExecuteType.JoinCompleted)
                {
                    if (CurrentWindow != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentWindow.Close();
                        });
                        
                        if (ActionThread != null)
                        {
                            ActionThread.Dispose();
                        }
                    }
                }
            }
        }
    }
}
