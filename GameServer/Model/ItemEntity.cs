using GameServer.InventorySystem;
using GameServer.Mgr;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    /// <summary>
    /// 场景里的物品，实体模型 
    /// </summary>
    public class ItemEntity : Actor
    {
        //真正的物品对象，会进入玩家背包
        public Item Item { get; set; }


        public ItemEntity(EntityType type, int tid, int level, Vector3Int position, Vector3Int direction)
            : base(type, tid, level, position, direction)  // 按照 Actor方法构造物品实体
        {
        }

        /// <summary>
        /// 在场景里创建物品
        /// </summary>
        /// <param name="item">物品</param>
        /// <param name="pos">位置</param>
        /// <param name="dir">方向</param>
        /// <returns></returns>
        public static ItemEntity Create(Space space, Item item, Vector3Int pos, Vector3Int dir)
        {
            var entity = new ItemEntity(EntityType.Item, 0, 0, pos, dir);       // 构造 ItemEntity 实体模型
            entity.Item = item;
            entity.Info.ItemInfo = entity.Item.ItemInfo;
            EntityManager.Instance.AddEntity(space.Id, entity);
            space.EntityEnter(entity);                                          // 该物品进入场景
            return entity;
        }

        public static ItemEntity Create(int spaceId, int itemId, int itemAmount, Vector3Int pos, Vector3Int dir)
        {
            Space space1 = SpaceManager.Instance.GetSpace(spaceId);             // 拿到 spaceId 对应的场景
            var item = new Item(itemId, itemAmount);                            // 根据 itemId 和 item数量 构造item
            return ItemEntity.Create(space1, item, pos, dir);
        }
    }
}
