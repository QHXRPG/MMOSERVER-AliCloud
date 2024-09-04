using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    /// <summary>
    /// 属性数据
    /// </summary>
    public class Attributes
    {
        /// <summary>
        /// 速度
        /// </summary>
        public float Speed;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public float HPMax;

        /// <summary>
        /// 最大魔法值
        /// </summary>
        public float MPMax;

        /// <summary>
        /// 物理攻击力
        /// </summary>
        public float AD;

        /// <summary>
        /// 魔法攻击力
        /// </summary>
        public float AP;

        /// <summary>
        /// 物理防御力
        /// </summary>
        public float DEF;

        /// <summary>
        /// 魔法防御力
        /// </summary>
        public float MDEF;

        /// <summary>
        /// 暴击率
        /// </summary>
        public float CRI;

        /// <summary>
        /// 暴击伤害
        /// </summary>
        public float CRD;

        /// <summary>
        /// 力量
        /// </summary>
        public float STR;

        /// <summary>
        /// 智力
        /// </summary>
        public float INT;

        /// <summary>
        /// 敏捷
        /// </summary>
        public float AGI;

        /// <summary>
        /// 命中率
        /// </summary>
        public float HitRate;

        /// <summary>
        /// 闪避率
        /// </summary>
        public float DodgeRate;

        /// <summary>
        /// 生命恢复
        /// </summary>
        public float HpRegen;

        /// <summary>
        /// 伤害吸血%
        /// </summary>
        public float HpSteal;


        /// <summary>
        /// 增加属性
        /// </summary>
        /// <param name="data"></param>
        public void Add(Attributes data)
        {
            this.Speed += data.Speed;
            this.HPMax += data.HPMax;
            this.MPMax += data.MPMax;
            this.AD += data.AD;
            this.AP += data.AP;
            this.DEF += data.DEF;
            this.MDEF += data.MDEF;
            this.CRI += data.CRI;
            this.CRD += data.CRD;
            this.STR += data.STR;
            this.INT += data.INT;
            this.AGI += data.AGI;
            this.HitRate += data.HitRate;
            this.DodgeRate += data.DodgeRate;
            this.HpRegen += data.HpRegen;
            this.HpSteal += data.HpSteal;
        }
        /// <summary>
        /// 减少属性
        /// </summary>
        /// <param name="data"></param>
        public void Sub(Attributes data)
        {
            this.Speed -= data.Speed;
            this.HPMax -= data.HPMax;
            this.MPMax -= data.MPMax;
            this.AD -= data.AD;
            this.AP -= data.AP;
            this.DEF -= data.DEF;
            this.MDEF -= data.MDEF;
            this.CRI -= data.CRI;
            this.CRD -= data.CRD;
            this.STR -= data.STR;
            this.INT -= data.INT;
            this.AGI -= data.AGI;
            this.HitRate -= data.HitRate;
            this.DodgeRate -= data.DodgeRate;
            this.HpRegen -= data.HpRegen;
            this.HpSteal -= data.HpSteal;
        }
        /// <summary>
        /// 重置属性
        /// </summary>
        public void Reset()
        {
            this.Speed = 0;
            this.HPMax = 0;
            this.MPMax = 0;
            this.AD = 0;
            this.AP = 0;
            this.DEF = 0;
            this.MDEF = 0;
            this.CRI = 0;
            this.CRD = 0;
            this.STR = 0;
            this.INT = 0;
            this.AGI = 0;
            this.HitRate = 0;
            this.DodgeRate = 0;
            this.HpRegen = 0;
            this.HpSteal = 0;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
