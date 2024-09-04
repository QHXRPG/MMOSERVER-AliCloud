using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    /// <summary>
    /// Server-Client-Object，可以代表一个人或者位置
    /// </summary>
    public abstract class SCObject
    {

        protected object realObj;
        public SCObject(object realObj)
        {
            this.realObj = realObj;
        }
        //对外可以访问的属性
        public int Id => GetId();                        // 
        public object RealObj => GetRealObj();           // 
        public Vector3 Position => GetPosition();        // 
        public Vector3 Direction => GetDirection();      // 

        //子类可以重写的属性
        protected virtual int GetId() => 0;
        protected virtual object GetRealObj() => realObj;
        protected virtual Vector3 GetPosition() => Vector3.zero;
        protected virtual Vector3 GetDirection() => Vector3.zero;

    }

    // 定义SCEntity类，继承自SCObject
    public class SCEntity : SCObject
    {
        private Entity Obj { get => (Entity)realObj; }
        public SCEntity(Entity realObj) : base(realObj) { }


        protected override int GetId()
        {
            return Obj.entityId;
        }
        protected override Vector3 GetPosition()
        {
            return Obj.Position;
        }
        protected override Vector3 GetDirection()
        {
            return Obj.Direction;
        }

    }

    public class SCPosition : SCObject
    {
        public SCPosition(Vector3 realObj) : base(realObj)
        {
        }

        protected override Vector3 GetPosition()
        {
            return (Vector3)realObj;
        }
    }


}
