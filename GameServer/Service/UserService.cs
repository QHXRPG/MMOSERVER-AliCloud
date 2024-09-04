using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Summer.Network;
using Proto;
using Serilog;
using GameServer.Model;
using GameServer.Mgr;
using Summer;
using GameServer.Database;
using GameServer.Core;
using GameServer.Fight;

namespace GameServer.Service
{
    /// <summary>
    /// 玩家服务
    /// 注册，登录，创建角色，进入游戏
    /// </summary>
    public class UserService : Singleton<UserService>
    {


        public void Start()
        {
            MessageRouter.Instance.Subscribe<GameEnterRequest>(_GameEnterRequest);
            MessageRouter.Instance.Subscribe<UserLoginRequest>(_UserLoginRequest);
            MessageRouter.Instance.Subscribe<UserRegisterRequest>(_UserRegisterRequest);
            MessageRouter.Instance.Subscribe<CharacterCreateRequest>(_CharacterCreateRequest);
            MessageRouter.Instance.Subscribe<CharacterListRequest>(_CharacterListRequest);
            MessageRouter.Instance.Subscribe<CharacterDeleteRequest>(_CharacterDeleteRequest);
            MessageRouter.Instance.Subscribe<ReviveRequest>(_ReviveRequest);                      // 复活请求
            MessageRouter.Instance.Subscribe<PickupItemRequest>(_PickupItemRequest);              // 拾取物品请求
            MessageRouter.Instance.Subscribe<InventoryRequest>(_InventoryRequest);                // 获取背包信息请求
            MessageRouter.Instance.Subscribe<ItemPlacementRequest>(_ItemPlacementRequest);        // 放置物品请求
            MessageRouter.Instance.Subscribe<ItemUseRequest>(_ItemUseRequest);                    // 使用物品请求
            MessageRouter.Instance.Subscribe<ItemDiscardRequest>(_ItemDiscardRequest);            // 丢弃物品请求

        }


        //丢弃物品
        private void _ItemDiscardRequest(Connection sender, ItemDiscardRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            chr.knapsack.Discard(msg.SlotIndex, msg.Count);
            chr.SendInventoty(true);
        }

        //使用物品
        private void _ItemUseRequest(Connection sender, ItemUseRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            chr.UseItem(msg.SlotIndex);
        }

        private void _ItemPlacementRequest(Connection conn, ItemPlacementRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            chr.knapsack.Exchange(msg.OriginIndex, msg.TargetIndex);
            //发送背包数据
            chr.SendInventoty(true);
        }

        private void _InventoryRequest(Connection conn, InventoryRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            //发送背包数据
            chr.SendInventoty(msg.QueryKnapsack, msg.QueryWarehouse, msg.QueryEquipment);
        }

        //玩家拾取物品
        private void _PickupItemRequest(Connection conn, PickupItemRequest msg)
        {
            var chr = conn.Get<Session>()?.Character;
            if (chr == null) return;

            // 把当前玩家附近3000距离内选择最近的物品
            var itemEntity = Game.RangeUnit(chr.Space.Id, chr.Position, 3000)
                .OfType<ItemEntity>()                                                   // 选的实体类型是物品
                .MinBy(entity => Vector3Int.Distance(entity.Position, chr.Position));   // 选最近的

            if (itemEntity == null) return;

            //如果添加失败则结束
            if (!chr.knapsack.AddItem(itemEntity.Item.Id, itemEntity.Item.amount)) return;

            //物品模型移出场景
            chr.Space.EntityLeave(itemEntity);
            EntityManager.Instance.RemoveEntity(itemEntity.Space.Id, itemEntity);
            Log.Information("Players pick up items, Chr[{0}], Bag[{1}]", chr.characterId, chr.knapsack.InventoryInfo);

            //发送背包数据
            chr.SendInventoty(true);
        }


        //复活请求
        private void _ReviveRequest(Connection sender, ReviveRequest msg)
        {
            var actor = Game.GetUnit(msg.EntityId);
            if (actor is Character chr && chr.IsDeath && chr.conn == sender)
            {
                // 将玩家复活在新手村的起始坐标上
                var sp = SpaceManager.Instance.GetSpace(1);
                chr.TelportSpace(sp, Vector3Int.zero);
                chr.Revive();
            }
        }

        private void _UserRegisterRequest(Connection conn, UserRegisterRequest msg)
        {
            // 计算玩家取的名字在数据库中是否已存在
            var count = Db.fsql.Select<DbPlayer>().Where(p => p.Username == msg.Username).Count();

            Log.Information("新用户注册：" + msg.Username);
            UserRegisterResponse resp = new UserRegisterResponse();
            if (count > 0)
            {
                resp.Code = 1;
                resp.Message = "用户名已被占用";
            }
            else
            {
                DbPlayer dbPlayer = new DbPlayer()
                {
                    Username = msg.Username,
                    Password = msg.Password
                };
                Db.fsql.Insert(dbPlayer).ExecuteAffrows();
                resp.Code = 6;
                resp.Message = "注册成功";
            }
            conn.Send(resp);
        }

        /// <summary>
        /// 删除角色的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void _CharacterDeleteRequest(Connection conn, CharacterDeleteRequest msg)
        {
            var player = conn.Get<Session>().DbPlayer;
            Db.fsql.Delete<DbCharacter>()
                .Where(t => t.Id == msg.CharacterId)
                .Where(t => t.PlayerId == player.Id)
                .ExecuteAffrows();
            //给客户端响应
            CharacterDeleteResponse cdr = new CharacterDeleteResponse();
            cdr.Success = true;
            cdr.Message = "执行完成";
            conn.Send(cdr);
        }

        /// <summary>
        /// 查询角色列表的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void _CharacterListRequest(Connection conn, CharacterListRequest msg)
        {
            var player = conn.Get<Session>().DbPlayer;
            //从数据库查询出当前玩家的全部角色
            var list = Db.fsql.Select<DbCharacter>().Where(t => t.PlayerId == player.Id).ToList();
            CharacterListResponse listResp = new CharacterListResponse();
            foreach (var item in list)
            {
                listResp.CharacterList.Add(new NetActor()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Tid = item.JobId,
                    //EntityId
                    Level = item.Level,
                    Exp = item.Exp,
                    SpaceId = item.SpaceId,
                    Gold = item.Gold,
                    //NetEntity entity = 9;
                });
            }
            conn.Send(listResp);
        }

        public static void GateBuild(int spaceId, int tid, Vector3Int position, string GateName, int targetSpaceId, Vector3Int targetPosition)
        {
            Gate gate = new Gate(spaceId, tid, position);
            gate.Name = GateName;
            gate.SetTarget(targetSpaceId, targetPosition);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="con"></param>
        /// <param name="msg"></param>
        private void _CharacterCreateRequest(Connection conn, CharacterCreateRequest msg)
        {
            Log.Information("创建角色：{0}", msg);
            ChracterCreateResponse resp = new ChracterCreateResponse();
            var player = conn.Get<Session>().DbPlayer;
            if (player == null)
            {
                //未登录，不能创建角色
                Log.Information("未登录，不能创建角色");
                resp.Success = false;
                resp.Message = "未登录，不能创建角色";
                conn.Send(resp);
                return;
            }
            long count = Db.fsql.Select<DbCharacter>().Where(t => t.PlayerId.Equals(player.Id)).Count();
            if (count >= 4)
            {
                //角色数量最多4个
                Log.Information("角色数量最多4个");
                resp.Success = false;
                resp.Message = "角色数量最多4个";
                conn.Send(resp);
                return;
            }

            //判断角色名是否为空
            if (string.IsNullOrWhiteSpace(msg.Name))
            {
                Log.Information("创建角色失败，角色名不能为空");
                resp.Success = false;
                resp.Message = "角色名不能为空";
                conn.Send(resp);
                return;
            }
            string name = msg.Name.Trim();
            //角色名最长7个字
            if (name.Length > 7)
            {
                Log.Information("创建角色失败，名字长度最大为7");
                resp.Success = false;
                resp.Message = "名字长度最大为7";
                conn.Send(resp);
                return;
            }
            //检验角色名是否存在
            if (Db.fsql.Select<DbCharacter>().Where(t => t.Name.Equals(name)).Count() > 0)
            {
                Log.Information("创建角色失败，名字已被占用");
                resp.Success = false;
                resp.Message = "名字已被占用";
                conn.Send(resp);
                return;
            }

            //出生点坐标
            // 1. 新手村
            // 2. 森林
            // 3. 篝火营地
            Vector3Int birthPos = new Vector3Int(5393, 0, -1336);

            DbCharacter dc = new DbCharacter()
            {
                Name = msg.Name,
                JobId = msg.JobType,
                Hp = 100,
                Mp = 100,
                Level = 1,
                Exp = 0,
                SpaceId = 1,
                X = birthPos.x,
                Y = birthPos.y,
                Z = birthPos.z,
                Gold = 0,
                PlayerId = player.Id
            };
            int aff = Db.fsql.Insert(dc).ExecuteAffrows();
            if (aff > 0)
            {
                resp.Success = true;
                resp.Message = "角色创建成功";
                conn.Send(resp);
            }
        }

        private void _UserLoginRequest(Connection conn, UserLoginRequest msg)
        {
            var dbPlayer = Db.fsql.Select<DbPlayer>()
                            .Where(p => p.Username == msg.Username)
                            .Where(p => p.Password == msg.Password)
                            .First();
            Log.Information("登录结果：" + dbPlayer);
            UserLoginResponse resp = new UserLoginResponse();
            if (dbPlayer != null)
            {
                resp.Success = true;
                resp.Message = "登录成功";
                //登录成功，记录用户信息
                conn.Get<Session>().DbPlayer = dbPlayer;
            }
            else
            {
                resp.Success = false;
                resp.Message = "用户名或密码不正确";
            }
            conn.Send(resp);
        }

        private void _GameEnterRequest(Connection conn, GameEnterRequest msg)
        {
            Log.Information($"有玩家进入游戏，角色ID={msg.CharacterId}");

            // 获取当前玩家
            var player = conn.Get<Session>().DbPlayer;
            // 查询数据库的角色
            var dbRole = Db.fsql.Select<DbCharacter>()
                .Where(t => t.PlayerId == player.Id)
                .Where(t => t.Id == msg.CharacterId)
                .First();

            Log.Information("dbRole={0}", dbRole);

            // 把数据库角色变成游戏角色
            Character chr = CharacterManager.Instance.CreateCharacter(dbRole);

            //角色与conn关联
            chr.conn = conn;
            //角色存入session
            chr.conn.Get<Session>().Character = chr;


            /*//通知玩家登录成功
            GameEnterResponse resp = new GameEnterResponse();
            resp.Success = true;
            resp.Entity = chr.EntityData;
            resp.Character = chr.Info;
            conn.Send(resp);*/
            //将新角色加入到地图
            var space = SpaceService.Instance.GetSpace(dbRole.SpaceId);
            space.EntityEnter(chr);
        }
    }
}
