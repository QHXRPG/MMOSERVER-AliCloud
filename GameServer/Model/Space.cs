using Serilog;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using System.Runtime.ConstrainedExecution;
using GameServer.Mgr;
using GameServer.Core;
using GameServer.Fight;
using Summer;
using Google.Protobuf;

namespace GameServer.Model
{
    //场景
    public class Space
    {
        public int Id { get; set; }             // 场景Id
        public string Name { get; set; }        // 场景名字
        public SpaceDefine Def { get; set; }    // 场景信息

        public FightMgr FightMgr { get; set; }                                                  // 战斗管理器 
        private Dictionary<int, Character> CharacterDict = new Dictionary<int, Character>();    // 当前场景中全部的主角 <ChrId,ChrObj>
        private Dictionary<int, Actor> ActorDict = new();                                       // 当前场景中的全部非主角 <entityId,Actor>
        public MonsterManager MonsterManager = new MonsterManager();                            // 怪物管理器
        public SpawnManager SpawnManager = new SpawnManager();                                  // 刷怪管理器


        public Space(SpaceDefine def)
        {
            this.Def = def;                               // 场景信息
            this.Id = def.SID;                            // 场景id
            this.Name = def.Name;                         // 场景名字
            this.FightMgr = new FightMgr(this);           // 场景战斗管理器
            MonsterManager.Init(this);
            SpawnManager.Init(this);
        }

        // 非主角进入场景
        public void EntityEnter(Actor actor)
        {
            Log.Information("character enters the scene : eid=" + actor.entityId);
            ActorDict[actor.entityId] = actor;
            actor.OnEnterSpace(this);
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id; //场景ID
            resp.CharacterList.Add(actor.Info);
            Broadcast(resp);
            //如果是主角
            if (actor is Character chr)
            {
                CharacterJoin(chr);
            }
        }

        //非主角离开场景
        public void EntityLeave(Actor actor)
        {
            Log.Information("character Leaves the scene : " + actor.entityId);
            ActorDict.Remove(actor.entityId);
            SpaceCharacterLeaveResponse resp = new SpaceCharacterLeaveResponse();
            resp.EntityId = actor.entityId;
            Broadcast(resp);
            //如果是主角
            if (actor is Character chr)
            {
                CharacterDict.Remove(chr.entityId);
            }
        }

        //主角进入场景
        private void CharacterJoin(Character chr)
        {
            Log.Information("玩家进入场景 Chr[{0}],Entity[{1}]", chr.characterId, chr.entityId);
            CharacterDict[chr.entityId] = chr;                  //记录到主角字典
            SpaceEnterResponse ser = new SpaceEnterResponse();  //拉取附近的非主角信息
            ser.Character = chr.Info;
            foreach (var kv in ActorDict)
            {
                ser.List.Add(kv.Value.Info);
            }
            chr.conn.Send(ser);
        }

        // 同场景传送
        public void Telport(Actor actor, Vector3Int pos, Vector3Int dir = new())
        {
            actor.Position = pos;
            actor.Direction = dir;
            SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
            resp.EntitySync = new NetEntitySync();
            resp.EntitySync.Entity = actor.EntityData;
            resp.EntitySync.Force = true;
            Broadcast(resp);
        }

        // 广播更新Entity信息
        public void UpdateEntity(NetEntitySync entitySync)
        {
            //Log.Information("UpdateEntity {0}", entitySync);
            foreach (var kv in CharacterDict)
            {
                if (kv.Value.entityId == entitySync.Entity.Id)
                {
                    kv.Value.EntityData = entitySync.Entity;
                }
                else
                {
                    SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                    resp.EntitySync = entitySync;
                    kv.Value.conn.Send(resp);
                }
            }
        }

        // 广播Proto消息给场景的全体玩家
        public void Broadcast(IMessage msg)
        {
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(msg);
            }
        }

        public void Update()
        {
            this.SpawnManager.Update();
            this.FightMgr.OnUpdate(Time.deltaTime);
        }
    }
}
