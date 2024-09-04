
using GameServer.Battle;
using GameServer.Core;
using GameServer.Mgr;
using GameServer.Model;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using Proto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// 技能释放器,每个人身上都携带一个技能释放器s
    /// </summary>
    public class Spell
    {

        public Actor Owner { get; private set; }

        public Spell(Actor Owner)
        {
            this.Owner = Owner;
        }


        /// <summary>
        /// 吟唱技能
        /// </summary>
        /// <param name="skill"></param>
        public void Intonate(Skill skill)
        {

        }

        //统一施法
        public void RunCast(CastInfo info)
        {
            // 通过 技能id 在玩家的技能管理器 中拿到对应的技能
            var skill = Owner.skillMgr.GetSkill(info.SkillId);

            if (skill.IsUnitTarget)   // 单位技能
            {
                SpellTarget(info.SkillId, info.TargetId);
            }
            if (skill.IsPointTarget)  // 点技能
            {
                SpellPosition(info.SkillId, info.TargetLoc);
            }
            if (skill.IsNoneTarget)   // 无目标技能
            {
                SpellNoTarget(info.SkillId);
            }
        }

        //施放无目标技能
        private void SpellNoTarget(int skill_id)
        {
            Log.Information("Spell::SpellNoTarget():Caster[{0}]:Skill[{1}]", Owner.entityId, skill_id);
            //检查技能
            var skill = Owner.skillMgr.GetSkill(skill_id);
            if (skill == null)
            {
                Log.Warning("Spell::SpellNoTarget():Owner[{0}]:Skill={1} not found", Owner.entityId, skill_id);
                return;
            }

            //执行技能
            SCObject sco = new SCEntity(Owner);  // sco：技能目标
            var res = skill.CanUse(sco);
            if (res != CastResult.Success)
            {
                Log.Warning("Cast Fail Skill {0} {1}", skill.Def.ID, res);
                OnSpellFailure(skill_id, res);
                return;
            }
            skill.Use(sco);

            CastInfo info = new CastInfo()
            {
                CasterId = Owner.entityId,
                SkillId = skill_id
            };
            Owner.Space.FightMgr.SpellQueue.Enqueue(info);
        }

        //施放单位目标技能
        public void SpellTarget(int skill_id, int target_id)
        {
            Log.Information("Spell::SpellTarget():Caster[{0}]:Skill[{1}]:Target[{2}]", Owner.entityId, skill_id, target_id);

            // 根据 skill_id 拿到技能
            var skill = Owner.skillMgr.GetSkill(skill_id);
            if (skill == null)
            {
                Log.Warning("Spell::SpellTarget():Owner[{0}]:Skill={1} not found", Owner.entityId, skill_id);
                return;
            }

            // 根据 target_id 目标
            var target = Game.GetUnit(target_id);
            if (target == null)
            {
                Log.Warning("Spell::SpellTarget():Owner[{0}]:Target={1} not found", Owner.entityId, target_id);
                return;
            }

            // 执行技能
            SCObject sco = new SCEntity(target);   // sco：技能目标
            var res = skill.CanUse(sco);           // 检查技能是否可用
            if (res != CastResult.Success)
            {
                Log.Warning("Cast Fail Skill {0} {1}", skill.Def.ID, res);
                OnSpellFailure(skill_id, res);     // 通知施法者施法失败
                return;
            }
            skill.Use(sco);                        // 使用技能，通知施法者施法成功

            CastInfo info = new CastInfo()
            {
                CasterId = Owner.entityId,
                TargetId = target_id,
                SkillId = skill_id
            };
            Owner.Space.FightMgr.SpellQueue.Enqueue(info);
        }

        //施放点目标技能
        public void SpellPosition(int skill_id, Vector3 position)
        {
            Log.Information("Spell::SpellPosition():Caster[{0}]:Skill[{1}]:Pos[{2}]", Owner.entityId, skill_id, position);
            SCObject sco = new SCPosition(position);
        }

        //通知玩家技能失败
        public void OnSpellFailure(int skill_id, CastResult reason)
        {
            if (Owner is Character chr)
            {
                SpellFailResponse resp = new SpellFailResponse()
                {
                    CasterId = Owner.entityId,
                    SkillId = skill_id,
                    Reason = reason
                };
                chr.conn.Send(resp);
            }

        }

    }
}
