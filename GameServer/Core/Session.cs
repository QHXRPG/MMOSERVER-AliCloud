using GameServer.Database;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    /// <summary>
    /// 用户会话
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 当前登录的角色
        /// </summary>
        public Character Character;

        /// <summary>
        /// 当前所在地图
        /// </summary>
        public Space Space => Character?.Space;

        /// <summary>
        /// 数据库玩家信息
        /// </summary>
        public DbPlayer DbPlayer;



    }
}
