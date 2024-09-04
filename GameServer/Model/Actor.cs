using Common.Summer.Core;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Fight;
using GameServer.Mgr;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{


    /// <summary>
    /// 角色基类｛主角、怪物、NPC、陷阱｝
    /// </summary>
    public class Actor : Entity
    {
        public int Id { get { return Info.Id; } set { Info.Id = value; } }                  // 角色id 
        public string Name { get { return Info.Name; } set { Info.Name = value; } }         // 角色名称
        public Space Space { get; set; }                                                    // 角色所在的场景
        public EntityType Type { get { return Info.Type; } set { Info.Type = value; } }     // 角色类型（主角、怪物、NPC等）
        public NetActor Info { get; set; } = new NetActor();                                // 角色信息
        public UnitDefine Define { get; set; }                                              // 角色定义
        public EntityState State;                                                           // 角色当前状态
        public AttributesAssembly Attr { get; set; } = new AttributesAssembly();            // 角色属性（基础、成长、装备等）

        public UnitState UnitState;                                                         // 单位状态
        public bool IsDeath => UnitState == UnitState.Dead;                                 // 角色是否死亡


        public SkillManager skillMgr;                                                       // 技能管理器

        public Spell Spell;                                                                 // 技能释放器

        // 角色基础属性
        public float Hp => Info.Hp;
        public float Mp => Info.Mp;
        public float HPMax => Attr.Final.HPMax;
        public float MPMax => Attr.Final.MPMax;
        public long Gold => Info.Gold;
        public long Exp => Info.Exp;
        public int Level => Info.Level;

        // 角色构造函数，通过构造函数生成符合参数列表的角色
        // （角色类型、角色单位类型、等级、位置、朝向）
        public Actor(EntityType type, int tid, int level, Vector3Int position, Vector3Int direction)
            : base(position, direction)
        {

            this.Info.Tid = tid;
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.Entity = this.EntityData;

            if (DataManager.Instance.Units.TryGetValue(tid, out var def))
            {
                this.Define = def;
                this.Info.Name = Define.Name;
                this.Info.Hp = (int)Define.HPMax;
                this.Info.Mp = (int)Define.MPMax;

            }

            if (type != EntityType.Item)
            {
                this.skillMgr = new(this);
                this.Attr.Init(this);
                this.Spell = new Spell(this);
            }
        }

        // 角色进入场景
        public void OnEnterSpace(Space _space)
        {
            // 更改角色场景
            if (Space != null && _space != null)
            {
                EntityManager.Instance.ChangeSpace(this, Space.Id, _space.Id);
            }
            this.Space = _space;
            this.Info.SpaceId = _space.Id;

            // 当前 Actor 是 Character 类型，则更新数据库信息
            if (this is Character chr)
            {
                chr.Data.SpaceId = _space.Id;
            }
        }

        // 玩家复活
        public virtual void Revive()
        {
            Log.Information("Actor.Revive:{0}", entityId);
            if (!IsDeath) return;
            SetHp(Attr.Final.HPMax);
            SetMp(Attr.Final.MPMax);
            SetState(UnitState.Free);
        }

        // 传送
        public virtual void TelportSpace(Space _space, Vector3Int pos, Vector3Int dir = new())
        {
            if (this is not Character chr) return;
            if (_space != Space)  // 不同场景下的传送
            {
                Space.EntityLeave(chr);         //1.退出当前场景
                chr.Position = pos;             //2.设置坐标和方向
                chr.Direction = dir;
                _space.EntityEnter(chr);        //3.进入新场景
            }
            else   // 同场景下的传送
            {
                _space.Telport(chr, pos, dir);
            }
        }

        public override void Update()
        {
            this.skillMgr?.Update();
        }

        //该单位被 killer 杀死
        public virtual void Die(int killerID)
        {
            if (IsDeath) return;
            OnBeforeDie(killerID);   // 被杀之前
            SetHp(0);
            SetMp(0);
            SetState(UnitState.Dead);
            OnAfterDie(killerID);    // 被杀之后
        }

        protected virtual void OnBeforeDie(int killerID)
        {
            // 自爆、死歌等技能
        }

        protected virtual void OnAfterDie(int killerID)
        {
            // 掉落物品池（血包、蓝包等）
            int[] array = { 1001, 1002 };

            // 生成一个随机索引，获取随机索引对应的掉落物品
            Random random = new Random();
            int randomIndex = random.Next(0, array.Length);
            int itemId = array[randomIndex];
            ItemEntity.Create(Space.Id, itemId, 5, this.Position, Vector3Int.zero);

            //如果击杀者是玩家，给与奖励
            var killer = Game.GetUnit(killerID);
            if (killer != null && killer is Character chr)
            {
                chr.SetLevel(chr.Level + 1);  // 等级+1
                chr.SetGolds(chr.Gold + 10);  // 金币+10
                chr.SetExp(chr.Exp + 32);     // 经验+32
            }
        }

        public void RecvDamage(Damage dmg)
        {
            Log.Information("Actor:RecvDamage[{0}]", dmg);
            //添加广播
            Space.FightMgr.DamageQueue.Enqueue(dmg);
            //扣血或者死亡
            if (Hp > dmg.Amount)
            {
                SetHp(Hp - dmg.Amount);
            }
            else
            {
                Die(dmg.AttackerId);
            }
        }

        // 设置血量
        public void SetHp(float hp)
        {
            if (MathC.Equals(Info.Hp, hp)) return;
            if (hp <= 0)
            {
                hp = 0;
            }
            if (hp > Attr.Final.HPMax)
            {
                hp = Attr.Final.HPMax;
            }
            float oldValue = Info.Hp;
            Info.Hp = hp;

            // 把属性变动事件存入 PropertyUpdateQueue 下一帧发送 
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Hp,
                OldValue = new() { FloatValue = oldValue },
                NewValue = new() { FloatValue = Info.Hp },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }


        // 设置蓝量
        public void SetMp(float mp)
        {
            if (MathC.Equals(Info.Mp, mp)) return;
            if (mp <= 0)
            {
                mp = 0;
            }
            if (mp > Attr.Final.MPMax)
            {
                mp = Attr.Final.MPMax;
            }
            float oldValue = Info.Mp;
            Info.Mp = mp;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Mp,
                OldValue = new() { FloatValue = oldValue },
                NewValue = new() { FloatValue = Info.Mp },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

        // 设置状态
        public void SetState(UnitState unitState)
        {
            if (this.UnitState == unitState) return;
            UnitState oldValue = this.UnitState;
            this.UnitState = unitState;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.State,
                OldValue = new() { StateValue = oldValue },
                NewValue = new() { StateValue = unitState },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

        // 设置金币
        public void SetGolds(long value)
        {
            if (this.Info.Gold == value) return;
            long oldValue = Info.Gold;
            Info.Gold = value;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Golds,
                OldValue = new() { LongValue = oldValue },
                NewValue = new() { LongValue = Info.Gold },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }
        // 设置经验
        public void SetExp(long value)
        {
            if (this.Info.Exp == value) return;
            long oldValue = Info.Exp;
            Info.Exp = value;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Exp,
                OldValue = new() { LongValue = oldValue },
                NewValue = new() { LongValue = Info.Exp },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }
        // 设置等级
        public void SetLevel(int value)
        {
            if (this.Info.Level == value) return;
            int oldValue = Info.Level;
            Info.Level = value;
            Attr.Reload();
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Level,
                OldValue = new() { IntValue = oldValue },
                NewValue = new() { IntValue = Info.Level },
            };
            Space.FightMgr.PropertyUpdateQueue.Enqueue(po);
        }

        //通知客户端：HPMax变化
        public void OnHpMaxChanged(float value)
        {
            Info.Hpmax = (int)value;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Hpmax,
                OldValue = new() { FloatValue = 0 },
                NewValue = new() { FloatValue = value },
            };
            Space?.FightMgr?.PropertyUpdateQueue.Enqueue(po);
        }

        //通知客户端：MPMax变化
        public void OnMpMaxChanged(float value)
        {
            Info.Mpmax = (int)value;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Property.Mpmax,
                OldValue = new() { FloatValue = 0 },
                NewValue = new() { FloatValue = value },
            };
            Space?.FightMgr?.PropertyUpdateQueue.Enqueue(po);
        }

    }
}
