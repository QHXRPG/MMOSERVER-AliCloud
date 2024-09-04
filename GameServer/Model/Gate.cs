using GameServer.Core;
using GameServer.Mgr;
using Proto;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{

    /// <summary>
    /// 传送门
    /// </summary>
    public class Gate : Actor
    {

        //传送到哪个场景
        public Space TargetSpace { get; set; }

        //传送到什么位置
        public Vector3Int TargetPosition { get; set; }


        public Gate(int spaceId, int tid, Vector3Int position, Vector3Int direction=default)
            : base(EntityType.Gate, tid, 0, position, direction)
        {
            EntityManager.Instance.AddEntity(spaceId, this);
            Scheduler.Instance.AddTask(Telport, 0.5f);
            SpaceManager.Instance.GetSpace(spaceId)?.EntityEnter(this);
        }


        /// <summary>
        /// 执行传送
        /// </summary>
        private void Telport()
        {
            if(this.Space==null || TargetSpace==null) return;
            var list = Game.RangeUnit(this.Space.Id, this.Position, 2000).OfType<Character>().ToList();
            foreach (Character chr in list)
            {
                chr.TelportSpace(TargetSpace, TargetPosition);
            }
        }

        /// <summary>
        /// 设置传送目标
        /// </summary>
        /// <param name="targetSpace"></param>
        /// <param name="targetPosition"></param>
        public void SetTarget(Space targetSpace, Vector3Int targetPosition)
        {
            TargetSpace = targetSpace;
            TargetPosition = targetPosition;
        }
        public void SetTarget(int targetSpaceId, Vector3Int targetPosition)
        {
            this.SetTarget(SpaceManager.Instance.GetSpace(targetSpaceId), targetPosition);
        }

    }
}
