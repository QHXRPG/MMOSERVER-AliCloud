using GameServer.Battle;
using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// 投射物
    /// </summary>
    public class Missile
    {

        public Space Space { get; private set; }        //投射物所在场景
        public Skill Skill { get; private set; }        //所属技能
        public SCObject Target { get; private set; }    //追击目标
        public Vector3 InitPos { get; private set; }    //初始位置
        public Vector3 Position;                        //飞弹当前位置

        public FightMgr FightMgr => Space.FightMgr;

        public Missile(Skill skill, Vector3 initPos, SCObject target)
        {
            this.Space = skill.Owner.Space;
            this.Skill = skill;
            this.Target = target;
            this.InitPos = initPos;
            this.Position = initPos;
            Log.Information("Position:{0}", Position);
        }

        public void OnUpdate(float dt)
        {
            var a = this.Position;
            var b = this.Target.Position;
            Vector3 direction = (b - a).normalized;
            var dist = Skill.Def.MissileSpeed * dt;
            if (dist >= Vector3.Distance(a, b))
            {
                Position = b;                     // 飞弹当前位置设置为目标位置 
                Skill.OnHit(Target);              // 投射物打到人 并造成伤害 
                FightMgr.Missiles.Remove(this);   // 销毁投射物
            }
            else
            {
                Position = Position + direction * dist;
            }
        }
    }
}
