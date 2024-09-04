using FreeSql.DataAnnotations;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    /// <summary>
    /// 数据库玩家信息
    /// </summary>
    ///
    [Table(Name = "player")]
    public class DbPlayer
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coin { get; set; }
    }

    /// <summary>
    /// 玩家的角色
    /// </summary>
    [Table(Name = "character")]  // 映射到数据库中的 character 表。
    public class DbCharacter
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; } = 100;
        public int Mp { get; set; } = 100;
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int SpaceId { get; set; }
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;
        public long Gold { get; set; } = 0;
        public int PlayerId { get; set; }

        [Column(DbType = "blob")]
        public byte[] Knapsack { get; set; }

    }


}
