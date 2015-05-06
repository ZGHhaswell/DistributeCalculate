using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SourceForge.AsyncWcfLib;

namespace DistributeCalculate.Contract.Data
{
    [DataContract(Namespace = WcfDefault.WsNamespace)]
    public class ClientExecuteBackMessage : WcfMessage
    {
        [DataMember]
        public ExecuteType ExecuteType { get; set; }

        public ClientExecuteBackMessage(ExecuteType executeType)
        {
            ExecuteType = executeType;
        }

    }
}
