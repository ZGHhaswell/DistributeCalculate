using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DistributeCalculate.Client.Client;
using DistributeCalculate.Contract.Data;

namespace DistributeCalculate.Client.ClientStepViews
{
    /// <summary>
    /// Interaction logic for ClientJoinInWindow.xaml
    /// </summary>
    public partial class ClientJoinInWindow : Window
    {
        public ClientJoinInWindow(ClientActor clientActor)
        {
            _clientActor = clientActor;

            InitializeComponent();
        }

        private ClientActor _clientActor;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _clientActor.ActorInput.PostInput(new ClientExecuteMessage(executeType: ExecuteType.Join));
        }
    }
}
