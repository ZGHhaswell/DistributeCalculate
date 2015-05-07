using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DistributeCalculate.Contract.Data;
using Nito.Async;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ActorInput _actorInput;

        private ActorOutput _actorOutput;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            WcfDefault.ApplicationStart(new string[]{}, new WcfTrc.PluginFile(), /*ExitHandler=*/true);
            WcfApplication.ApplicationExit += WcfApplicationOnApplicationExit;
            ClientExecuteMessage.AddKnownTypes();

            string host = "192.168.1.108:40001";
            string serviceUri = "http://" + host + "/AsyncWcfLib/DistributeService";
            //string serviceUri = "net.tcp://zgh-pc:40001/AsyncWcfLib/DistributeService";
            _actorInput = new ActorInput("ClientInput", InputRequestHandler);
            _actorOutput = new ActorOutput("ClientOutput", OutputRequestHandler);
            _actorOutput.LinkOutputToRemoteService(new Uri(serviceUri));
            _actorOutput.TraceSend = true;

            actionThread = new ActionThread();
            actionThread.Start();
            actionThread.Do(OnStartup);

        }

        private ActionThread actionThread;

        private void Info(string info)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var oriInfo = this.InfoTextBox.Text;
                this.InfoTextBox.Text = string.Format("{0}{1}{2}", oriInfo, Environment.NewLine, info);
            });
            
        }

        private void OnStartup()
        {
            Info("连接服务器");
            _actorInput.Open();
            _actorOutput.TryConnect();
        }

        private void InputRequestHandler(WcfReqIdent id)
        {
            if (id.Message is ClientExecuteMessage)
            {
                WcfState s = _actorOutput.OutputState;
                if (s == WcfState.Disconnected || s == WcfState.Faulted)
                {
                  OnStartup();
                }
                else if (s == WcfState.Connecting)
                {
                    Info("Connecting");
                }
                else
                {
                    _actorOutput.SendOut(id.Message);
                    return;
                }
            }
        }

        private void OutputRequestHandler(WcfReqIdent id)
        {
            if (id.Message is ClientExecuteBackMessage)
            {
                var clientExecuteBackMessage = id.Message as ClientExecuteBackMessage;
                if (clientExecuteBackMessage.ExecuteType == ExecuteType.JoinCompleted)
                {
                    Info("JoinCompleted");
                }
            }
        }

        private void WcfApplicationOnApplicationExit(WcfApplication.CloseType closeType, ref bool goExit)
        {
            
            if(goExit)
                _actorOutput.Disconnect();
        }

        private void ButtonBase_OnClick1(object sender, RoutedEventArgs e)
        {
            
            _actorInput.PostInput(new ClientExecuteMessage(executeType:ExecuteType.Join));
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (actionThread != null)
            {
                actionThread.Dispose();
            }
            
            base.OnClosing(e);
        }
    }
}
