using GameServer.Model;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    /// <summary>
    /// 每个地图都有怪物管理器
    /// </summary>
    public class MonsterManager
    {
        // <EntityId,Monster>
        private Dictionary<int, Monster> _dict = new Dictionary<int, Monster> ();


        private Space _space;

        public void Init(Space space)
        {
            this._space = space;
        }


        public Monster Create(int tid, int level, Vector3Int pos, Vector3Int dir)
        {
            Monster monster = new Monster (tid, level, pos, dir);
            EntityManager.Instance.AddEntity(_space.Id, monster);
            monster.Info.SpaceId = _space.Id;
            monster.Info.Entity.Id = monster.entityId;
            _dict[monster.entityId] = monster;
            monster.Id = monster.entityId;
            this._space.EntityEnter(monster);
            return monster;
        }

    }
}
