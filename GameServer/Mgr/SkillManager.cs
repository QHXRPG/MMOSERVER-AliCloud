using GameServer.Battle;
using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr;

/// <summary>
/// 技能管理器，每个Actor都有独立的技能管理器
/// </summary>
public class SkillManager
{
    // 技能列表归属的角色
    private Actor owner;

    // 技能列表
    public List<Skill> Skills = new();

    public SkillManager(Actor owner)
    {
        this.owner = owner;
        this.InitSkills();
    }

    // 角色初始技能加载
    public void InitSkills()
    {
        //初始化技能信息，正常是通过读取数据库来加载技能信息
        int job = this.owner.Define.TID;
        if (job == 1) //战士
        {
            loadSkill(1001, 1002, 1003, 2001, 2002);
        }
        if (job == 2) //法师
        {
            loadSkill(2001, 2002);
        }
        //如果是山贼或土匪，添加默认101号技能
        if (job == 1002 || job == 1003)
        {
            loadSkill(101);
        }
    }

    // 加载 ids 技能列表
    private void loadSkill(params int[] ids)
    {
        foreach (int skid in ids)
        {
            owner.Info.Skills.Add(new Proto.SkillInfo() { Id = skid });
            var skill = new Skill(owner, skid);
            Skills.Add(skill);
            Log.Information("角色[{0}]加载技能[{1}-{2}]", owner.Name, skill.Def.ID, skill.Def.Name);
        }
    }

    public Skill GetSkill(int id)
    {
        return Skills.FirstOrDefault(s => s.Def.ID == id);
    }

    public void Update()
    {
        foreach (Skill skill in Skills)
        {
            skill.Update();
        }
    }
}
