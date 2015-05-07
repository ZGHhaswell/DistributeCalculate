using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributeCalculate.Contract.StateMachine
{
    public interface IStateMachine<T> where T:class 
    {
        void Enter(T t);

        void Execute(T t);

        void Exit(T t);
    }
}
