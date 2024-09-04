using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.FSM
{
    public class FsmSystem<T>
    {

        public FsmSystem(T param)
        {
            this.P = param;
        }


        private Dictionary<string,State<T>> _dict = new Dictionary<string,State<T>>();

        public string CurrentStateId { get; private set; }
        public State<T> CurrentState { get; private set; }

        public T P; //共享参数Params

        //添加状态
        public void AddState(string id, State<T> state)
        {
            if(CurrentStateId is null)
            {
                CurrentStateId = id;
                CurrentState = state;
            }
            _dict[id] = state;
            state.fsm = this;
        }
        //移除状态
        public void RemoveState(string id)
        {
            if (_dict.ContainsKey(id))
            {
                _dict[id].fsm = null;
                _dict.Remove(id);
            }
                
        }
        //切换状态

        public void ChangeState(string id)
        {
            if (CurrentStateId == id) return;
            if (!_dict.ContainsKey(id)) return;
            if (CurrentState != null)
                CurrentState.OnLeave();
            CurrentStateId = id;
            CurrentState = _dict[id];
            CurrentState.OnEnter();
        }

        public void Update()
        {
            CurrentState?.OnUpdate();
        }
    }
}
