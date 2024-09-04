using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.FSM
{
    /// <summary>
    /// 状态基础类
    /// </summary>
    public class State<T>
    {
        public FsmSystem<T> fsm;
        public T P => fsm.P;

        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
    }
}
