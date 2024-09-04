using GameServer.Model;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    public class SpaceManager : Singleton<SpaceManager>
    {

        //地图字典
        private Dictionary<int, Space> dict = new Dictionary<int, Space>();

        public SpaceManager() { }

        public void Init()
        {
            foreach (var kv in DataManager.Instance.Spaces)
            {
                dict[kv.Key] = new Space(kv.Value);
                Log.Information("初始化地图：{0}", kv.Value.Name);
            }
        }

        /// <summary>
        /// 获取场景，如果不存在则返回null
        /// </summary>
        /// <param name="spaceId"></param>
        /// <returns></returns>
        public Space GetSpace(int spaceId)
        {
            return dict.GetValueOrDefault(spaceId, null);
        }

        // 每一帧更新dict当中所有的地图
        public void Update()
        {
            foreach (var s in dict.Values)
            {
                s.Update();
            }
        }
    }
}
