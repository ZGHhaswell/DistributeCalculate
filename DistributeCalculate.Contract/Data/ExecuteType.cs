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
    public enum ExecuteType
    {
        /// <summary>
        /// 加入
        /// </summary>
        [EnumMember]
        Join,



        /// <summary>
        /// 加入成功
        /// </summary>
        [EnumMember]
        JoinCompleted,
    }
}
