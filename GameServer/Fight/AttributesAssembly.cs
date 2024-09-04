using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    /// <summary>
    /// 属性组装
    /// </summary>
    public class AttributesAssembly
    {
        private Attributes Initial = new(); //初始属性（来自于Excel的属性）
        private Attributes Growth = new();  //成长属性
        //private Attributes Basic;  //基础属性（初始+成长）
        private Attributes Equip = new();    //装备属性
        private Attributes Buffs = new();    //Buff属性
        public  Attributes Final = new();    //最终属性

        public Actor actor {  get; private set; }


        public void Init(Actor actor)
        {
            this.actor = actor;
            Reload();
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        public void Reload()
        {
            //Initial.Reset();
            Growth.Reset();
            Equip.Reset();
            Buffs.Reset();
            Final.Reset();

            var define = this.actor.Define;
            var level = this.actor.Level;

            //初始化属性
            Initial.Speed = define.Speed;
            Initial.HPMax = define.HPMax;
            Initial.MPMax = define.MPMax;
            Initial.AD = define.AD;
            Initial.AP = define.AP;
            Initial.DEF = define.DEF;
            Initial.MDEF = define.MDEF;
            Initial.CRI = define.CRI;
            Initial.CRD = define.CRD;
            Initial.STR = define.STR;
            Initial.INT = define.INT;
            Initial.AGI = define.AGI;
            Initial.HitRate = define.HitRate;
            Initial.DodgeRate = define.DodgeRate;
            Initial.HpRegen = define.HpRegen;
            Initial.HpSteal = define.HpSteal;

            //成长属性
            Growth.STR = define.GSTR * level;// 力量成长
            Growth.INT = define.GINT * level;// 智力成长
            Growth.AGI = define.GAGI * level;// 敏捷成长

            //基础属性（初始+成长）
            //Basic.Add(Initial);
            //Basic.Add(Growth);

            //todo 处理装备和buff

            //合并到最终属性
            Final.Add(Initial);
            Final.Add(Growth);
            Final.Add(Equip);
            Final.Add(Buffs);

            //附加属性
            var Extra = new Attributes();
            Extra.HPMax = Final.STR * 5;    //力量加生命上限
            Extra.AP = Final.INT * 1.5f;    //智力加法攻
            Final.Add(Extra);
            /*
            Log.Information("初始属性：{0}", Initial);
            Log.Information("成长属性：{0}", Growth);
            Log.Information("装备属性：{0}", Equip);
            Log.Information("Buff属性：{0}", Buffs);
            Log.Information("属性附加：{0}", Extra);
            */
            Log.Information("最终属性：{0}", Final);
            //赋值与同步
            actor.Speed = (int)Final.Speed;
            actor.Info.Hpmax = Final.HPMax;
            actor.Info.Mpmax = Final.MPMax;
            actor.OnHpMaxChanged(Final.HPMax);
            actor.OnMpMaxChanged(Final.MPMax);
        }

    }
}
