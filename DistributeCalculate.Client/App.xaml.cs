using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DistributeCalculate.Client.Client;

namespace DistributeCalculate.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var clientActor = new ClientActor();

            clientActor.Start();

            base.OnStartup(e);
        }
    }
}
