using GameServer.Battle;
using GameServer.Mgr;
using GameServer.Model;
using Proto;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// 战斗管理器，负责接收用户的技能请求，处理扣血信息，技能释放结果，角色被杀死等战斗相关的逻辑
    /// </summary>
    public class FightMgr
    {
        private Space Space { get; }  // 单向获取，只能get不能set

        // 绑定当前场景到该战斗管理器
        public FightMgr(Space space)
        {
            this.Space = space;
        }

        public List<Missile> Missiles = new List<Missile>();                            //投射物列表
        public ConcurrentQueue<CastInfo> CastQueue = new ConcurrentQueue<CastInfo>();   //技能施法队列
        public ConcurrentQueue<CastInfo> SpellQueue = new();                            //等待广播的释放技能队列
        public ConcurrentQueue<Damage> DamageQueue = new();                             //等待广播的伤害队列
        public ConcurrentQueue<PropertyUpdate> PropertyUpdateQueue = new();             //角色属性变动
        private SpellResponse SpellResponse = new();                                    //施法响应对象，每帧发送一次
        private DamageResponse DamageResponse = new();                                  //伤害消息，每帧发送一次
        private PropertyUpdateResponse PropertyUpdateResponse = new();                  //角色属性变化

        // 每一帧都会处理相关战斗信息
        public void OnUpdate(float delta)
        {
            // 处理技能
            while (CastQueue.TryDequeue(out var cast))
            {
                Log.Information("执行施法：{0}", cast);
                RunCast(cast);
            }

            // 处理投射物
            for (int i = 0; i < Missiles.Count; i++)
            {
                Missiles[i].OnUpdate(delta);
            }

            // 把 战斗相关信息 给 对应的场景 去发送
            BroadcastSpell();        // 广播施法信息：处理 SpellQueue 中的施法请求
            BroadcastDamage();       // 广播受到伤害的信息： 处理 DamageQueue 中的受到伤害请求
            BroadcastProperties();   // 广播角色属性变动的信息：处理 PropertyUpdateQueue
        }

        private void BroadcastProperties()
        {
            while (PropertyUpdateQueue.TryDequeue(out var item))
            {
                PropertyUpdateResponse.List.Add(item);
            }
            if (PropertyUpdateResponse.List.Count > 0)
            {
                Space.Broadcast(PropertyUpdateResponse);
                PropertyUpdateResponse.List.Clear();
            }
        }

        // 广播伤害
        private void BroadcastDamage()
        {
            while (DamageQueue.TryDequeue(out var item))
            {
                DamageResponse.List.Add(item);
            }
            if (DamageResponse.List.Count > 0)
            {
                Space.Broadcast(DamageResponse);
                DamageResponse.List.Clear();
            }
        }

        //广播施法信息
        private void BroadcastSpell()
        {
            while (SpellQueue.TryDequeue(out var item))
            {
                SpellResponse.CastList.Add(item);   // 把 CastInfo 加入到 SpellResponse 中
            }
            if (SpellResponse.CastList.Count > 0)
            {
                Space.Broadcast(SpellResponse);
                SpellResponse.CastList.Clear();     // 把已经广播的消息清空
            }
        }

        private void RunCast(CastInfo cast)
        {
            var caster = EntityManager.Instance.GetEntity(cast.CasterId) as Actor;
            if (caster == null)
            {
                Log.Error("RunCast: Caster is null {0}", cast.CasterId);
                return;
            }
            caster.Spell.RunCast(cast);
        }
    }
}
