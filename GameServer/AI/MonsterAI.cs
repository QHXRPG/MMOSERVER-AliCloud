using GameServer.Core;
using GameServer.FSM;
using GameServer.Mgr;
using GameServer.Model;
using Org.BouncyCastle.Asn1.Mozilla;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    public class MonsterAI : AIBase
    {
        // 状态机
        public FsmSystem<Param> fsmSystem;

        public MonsterAI(Monster Owner) : base(Owner)
        {
            Param param = new Param();
            param.Owner = Owner;
            fsmSystem = new FsmSystem<Param>(param);
            fsmSystem.AddState("walk", new WalkState());
            fsmSystem.AddState("chase", new ChaseState());
            fsmSystem.AddState("goback", new GobackState());
        }

        public override void Update()
        {
            fsmSystem?.Update();
        }

        public class Param
        {
            public Monster Owner;
            public int viewRange = 8000;       //视野范围
            public int walkRange = 8000;       //相对于出生点的活动范围
            public int chaseRange = 12000;     //追击范围
            public Random rand = new Random();
        }

        //巡逻状态
        class WalkState : State<Param>
        {
            float lastTime = Time.time;
            float waitTime = 10f;  //等待时间
            public override void OnEnter()
            {
                P.Owner.StopMove();
            }
            public override void OnUpdate()
            {
                var mon = P.Owner;

                //查询8000范围内的玩家
                var chr = Game.RangeUnit(mon.Space.Id, mon.Position, P.viewRange)
                    .OfType<Character>()
                    .OrderBy(e => Vector3Int.Distance(mon.Position, e.Position))
                    .FirstOrDefault(a => !a.IsDeath);
                //Log.Information("最近的目标：{0}", chr);
                if (chr != null)
                {
                    mon.target = chr;
                    fsm.ChangeState("chase");
                    return;
                }

                if (mon.State == Proto.EntityState.Idle)
                {
                    if (lastTime + waitTime < Time.time)
                    {
                        lastTime = Time.time;
                        waitTime = (P.rand.NextSingle() * 20f) + 10f;
                        //移动到随机位置
                        var target = mon.RandomPointWithBirth(P.walkRange);
                        mon.MoveTo(target);
                    }
                }
            }
        }
        //追击状态
        class ChaseState : State<Param>
        {
            public override void OnUpdate()
            {
                var mon = P.Owner;
                if (mon.target == null || mon.target.IsDeath ||
                    !EntityManager.Instance.Exist(mon.target.entityId))
                {
                    mon.target = null;
                    fsm.ChangeState("walk");
                    return;
                }
                //自身与出生点的距离
                float m = Vector3.Distance(mon.initPosition, mon.Position);
                //自身和目标的距离
                float n = Vector3.Distance(mon.Position, mon.target.Position);
                if (m > P.chaseRange || n > P.viewRange)
                {
                    //返回出生点
                    fsm.ChangeState("goback");
                    return;
                }
                if (n < 1500)
                {
                    if (mon.State == Proto.EntityState.Move)
                    {
                        mon.StopMove();
                    }

                    //Log.Information("发起攻击");
                    mon.Attack(mon.target);
                }
                else
                {
                    mon.MoveTo(mon.target.Position);
                }
            }
        }

        //返回状态
        class GobackState : State<Param>
        {
            public override void OnEnter()
            {
                P.Owner.MoveTo(P.Owner.initPosition);
            }
            public override void OnUpdate()
            {
                var mon = P.Owner;
                if (Vector3.Distance(mon.initPosition, mon.Position) < 100)
                    fsm.ChangeState("walk");
            }
        }
    }
}
