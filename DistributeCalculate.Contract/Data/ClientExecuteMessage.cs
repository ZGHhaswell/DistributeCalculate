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
    public class ClientExecuteMessage : WcfMessage
    {
        [DataMember]
        public ExecuteType ExecuteType { get; set; }

        public ClientExecuteMessage(ExecuteType executeType)
        {
            ExecuteType = executeType;
        }

        public static void AddKnownTypes()
        {
            AddKnownType(typeof(ClientExecuteMessage));
            AddKnownType(typeof(ClientExecuteBackMessage));
        }
    }
}
