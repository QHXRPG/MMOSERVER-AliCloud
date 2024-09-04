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
    /// 刷怪管理器
    /// </summary>
    public class SpawnManager
    {
        public List<Spawner> Rules = new List<Spawner>();
        public Space Space { get; set; }

        public void Init(Space space)
        {
            Space = space;
            //根据当前场景加载对应的规则
            var rules = DataManager.Instance.Spawns.Values
                .Where(r => r.SpaceId == space.Id);
            foreach (var r in rules)
            {
                Rules.Add(new Spawner(r, space));
            }
        }

        public void Update()
        {
            Rules.ForEach(r => r.Update());
        }
    }
}
