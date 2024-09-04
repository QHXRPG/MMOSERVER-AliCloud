using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Model;
using Summer;

namespace GameServer.Mgr
{
    /// <summary>
    /// Entity管理器（角色，怪物，NPC，陷阱）
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        private int index = 1;

        //记录全部的Entity对象，<EntityId,Entity>
        private ConcurrentDictionary<int, Entity> AllEntities = new();

        //记录场景里的Entity列表，<SpaceId,EntityList>
        private ConcurrentDictionary<int, List<Entity>> SpaceEntities = new();


        public EntityManager() { }

        public void AddEntity(int spaceId, Entity entity)
        {
            lock (this)
            {
                //统一管理的对象分配ID
                entity.EntityData.Id = NewEntityId;
                AllEntities[entity.entityId] = entity;
                if (!SpaceEntities.ContainsKey(spaceId))
                {
                    SpaceEntities[spaceId] = new List<Entity>();
                }
                ForUnits(spaceId, (list) => list.Add(entity));
            }
        }

        public void RemoveEntity(int spaceId, Entity entity)
        {
            lock (this)
            {
                AllEntities.TryRemove(entity.entityId, out var item);
                ForUnits(spaceId, (list) => list.Remove(entity));
            }
        }

        private void ForUnits(int spaceId, Action<List<Entity>> action)
        {
            if (SpaceEntities.TryGetValue(spaceId, out var list))
            {
                if (list == null) return;
                lock (list)
                {
                    action.Invoke(list);   // 执行传入的操作（添加或移除实体）
                }
            }
        }

        /// <summary>
        /// 更改角色所在场景
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldSpaceId"></param>
        /// <param name="newSpaceId"></param>
        // (list) => list.Remove(entity) : 表示一个接受 list 参数并从该 list 中移除 entity 的操作。
        public void ChangeSpace(Entity entity, int oldSpaceId, int newSpaceId)
        {
            if (oldSpaceId == newSpaceId) return;
            ForUnits(oldSpaceId, (list) => list.Remove(entity));
            ForUnits(newSpaceId, (list) => list.Add(entity));
        }

        public bool Exist(int entityId)
        {
            return AllEntities.ContainsKey(entityId);
        }

        public Entity GetEntity(int entityId)
        {
            return AllEntities.GetValueOrDefault(entityId, null);
        }

        //查找Entity对象
        public List<T> GetEntityList<T>(int spaceId, Predicate<T> match) where T : Entity
        {
            if (!SpaceEntities.TryGetValue(spaceId, out var list)) return null;
            return list?.OfType<T>()
                .Where(entity => match.Invoke(entity))
                .ToList();
        }


        public int NewEntityId
        {
            get
            {
                lock (this)
                {
                    return index++;
                }
            }
        }

        public void Update()
        {
            foreach (var entity in AllEntities)
            {
                entity.Value.Update();
            }
        }

    }
}
