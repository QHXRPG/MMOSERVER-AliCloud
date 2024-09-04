using GameServer.Model;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    // 刷怪笼
    public class Spawner
    {
        public SpawnDefine Define;
        public Space Space;
        public Monster mon;
        public Vector3Int Pos { get; private set; } //刷怪位置
        public Vector3Int Dir { get; private set; } //刷怪方向

        private bool reviving;  //是否正在复活倒计时
        private float reviveTime; //复活时间

        public Spawner(SpawnDefine define, Space space)
        {
            Define = define;
            Space = space;
            Pos = ParsePoint(define.Pos);
            Dir = ParsePoint(define.Dir);
            Log.Debug("New Spawner:场景[{0}],坐标[{1}],单位类型[{2}]，周期[{3}]秒",
                space.Name, Pos, define.TID, define.Period);
            this.spawn();
        }


        private Vector3Int ParsePoint(string text)
        {
            string pattern = @"\[(\d+),(\d+),(\d+)\]";
            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                int z = int.Parse(match.Groups[3].Value);
                return new Vector3Int(x, y, z);
            }
            return Vector3Int.zero;
        }

        private void spawn()
        {
            this.mon = this.Space.MonsterManager.Create(Define.TID, Define.Level, Pos, Dir);
        }

        public void Update()
        {
            if (mon != null && mon.IsDeath && !reviving)
            {
                reviveTime = Time.time + Define.Period;
                reviving = true;
            }
            if (reviving && reviveTime <= Time.time)
            {
                reviving = false;
                mon?.Revive();
            }
        }

    }
}
