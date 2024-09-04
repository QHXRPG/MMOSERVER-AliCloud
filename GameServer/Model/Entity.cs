using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Summer;

namespace GameServer.Model
{
    //在MMO世界进行同步的实体
    public class Entity
    {
        private int speed;              //移动速度
        private Vector3Int position;    //位置
        private Vector3Int direction;   //方向
        private NetEntity netObj;         //网络对象
        private long _lastUpdate;       //最后一次更新位置的时间戳
        
        public int entityId { get { return netObj.Id; } }
        public Vector3Int Position
        {
            get { return position; }
            set { 
                position = value; 
                netObj.Position = value;
                _lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }
        public Vector3Int Direction
        {
            get { return direction; }
            set { 
                direction = value; 
                netObj.Direction = value;
            }
        }
        public int Speed 
        {
            get { return speed; }
            set
            {
                speed = value;
                netObj.Speed = value;
            }
        }
        /// <summary>
        /// 距离上次位置更新的间隔（秒）
        /// </summary>
        public float PositionTime
        {
            get
            {
                return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastUpdate) * 0.001f;
            }
        }


        public Entity(Vector3Int pos,Vector3Int dir)
        {
            netObj = new NetEntity();
            Position = pos;
            Direction = dir;
        }

        public Proto.NetEntity EntityData
        {
            get { return netObj; }
            set
            {
                Position = value.Position;
                Direction = value.Direction;
                Speed = value.Speed;
            }
        }


        public virtual void Update()
        {

        }

    }
}
