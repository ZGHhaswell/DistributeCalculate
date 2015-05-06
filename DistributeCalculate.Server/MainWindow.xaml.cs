using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DistributeCalculate.Contract.Data;
using DistributeCalculate.Server.Service;
using Nito.Async;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Server
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //Action action = () =>
            //{
                WcfDefault.ApplicationStart(new string[] { }, new WcfTrc.PluginFile(), /*ExitHandler=*/true);
                WcfApplication.ApplicationExit += WcfApplicationOnApplicationExit;
                ClientExecuteMessage.AddKnownTypes();

                DistributeService test = new DistributeService(this.InfoTextBox);
                ActorInput service = new ActorInput("DistributeService", test.WcfRequest);
                service.IsMultithreaded = true; // we have no message queue in a console application

                service.LinkInputToNetwork("DistributeService", 40001);

                if (service.TryConnect())
                {
                    test.Info(service.Uri.ToString());
                }
            //};
            //action.BeginInvoke(null, null);




        }

        private void WcfApplicationOnApplicationExit(WcfApplication.CloseType closeType, ref bool goExit)
        {
            if (goExit)
            {
                ActorPort.DisconnectAll();  
            }

        }
    }
}
