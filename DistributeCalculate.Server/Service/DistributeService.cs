using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DistributeCalculate.Contract.Data;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Server.Service
{
    public class DistributeService
    {
        public TextBox InfoTextBox { get; private set; }

        public DistributeService(TextBox textBox)
        {
            InfoTextBox = textBox;
        }

        public void Info(string info)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var oriInfo = this.InfoTextBox.Text;
                this.InfoTextBox.Text = string.Format("{0}{1}{2}", oriInfo, Environment.NewLine, info);
            });
        }

        // receive a message from client...
        public void WcfRequest(WcfReqIdent req)
        {
            IWcfMessage response = new WcfErrorMessage(WcfErrorMessage.Code.AppRequestNotAcceptedByService, "");
            if (req.Message is ClientExecuteMessage)
            {
                var clientExecuteMessage = req.Message as ClientExecuteMessage;
                if (clientExecuteMessage.ExecuteType == ExecuteType.Join)
                {
                    Info("加入服务器");
                    response = new ClientExecuteBackMessage(executeType:ExecuteType.JoinCompleted);
                }
            }
            
            req.SendResponse(response);
        }// WcfRequest

    }
}
