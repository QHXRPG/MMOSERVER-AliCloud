using FreeSql;
using GameServer.Database;
using GameServer.Model;
using Google.Protobuf;
using Proto;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    /// <summary>
    /// 统一管理全部的角色（创建，移除，获取），每隔五秒保存一次角色信息到数据库 
    /// </summary>
    public class CharacterManager : Singleton<CharacterManager>
    {
        //游戏里全部的角色 <ChrId,ChrObj>
        private ConcurrentDictionary<int, Character> Characters = new ConcurrentDictionary<int, Character>();

        // IBaseRepository：泛型接口，用于初始化仓库实例，以便后续进行数据库的增删改查操作。（仓库模式）
        // <DbCharacter>: 这是这个接口的泛型参数，表示这个存储库将会操作 DbCharacter 类型的实体。
        // repo: 这是变量的名称，用于引用这个基础存储库实例。
        IBaseRepository<DbCharacter> repo = Db.fsql.GetRepository<DbCharacter>();

        // 构造
        public CharacterManager()
        {
            //每隔5秒保存Data到数据库
            Scheduler.Instance.AddTask(Save, 5);
        }

        public Character CreateCharacter(DbCharacter dbchr)
        {
            Character chr = new Character(dbchr);
            Characters[chr.Id] = chr;
            EntityManager.Instance.AddEntity(dbchr.SpaceId, chr);
            return chr;
        }

        //   玩家断开连接时调用
        public void RemoveCharacter(int chrId)
        {
            Character chr;
            if (Characters.TryRemove(chrId, out chr))
            {
                EntityManager.Instance.RemoveEntity(chr.Data.SpaceId, chr);
            }
        }

        public Character GetCharacter(int chrId)
        {
            return Characters.GetValueOrDefault(chrId, null);
        }

        public void Clear()
        {
            Characters.Clear();
        }


        private void Save()
        {
            // 游戏玩家在数据库中的更新，5秒一次
            foreach (var chr in Characters.Values)
            {
                //角色属性存入数据库
                chr.Data.X = chr.Position.x;
                chr.Data.Y = chr.Position.y;
                chr.Data.Z = chr.Position.z;
                chr.Data.JobId = chr.Info.Tid;
                chr.Data.Hp = (int)chr.Info.Hp;
                chr.Data.Mp = (int)chr.Info.Mp;
                chr.Data.Exp = (int)chr.Info.Exp;
                chr.Data.Level = chr.Info.Level;
                chr.Data.Gold = chr.Info.Gold;
                chr.Data.SpaceId = chr.Info.SpaceId;
                chr.Data.Knapsack = chr.knapsack.InventoryInfo.ToByteArray();
                repo.UpdateAsync(chr.Data);
            }
        }
    }
}
