﻿
using GameServer.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Summer.Network;
using Proto;
using Common;
using Serilog;
using GameServer.Service;
using GameServer.Database;
using GameServer.Mgr;
using Summer;
using GameServer.Model;
using GameServer.AI;
using GameServer.InventorySystem;
using System.Runtime.InteropServices;
using FreeSql;
using System.Security.Cryptography;
using GameServer.Fight;
#pragma execution_character_set("UTF-8")

namespace GameServer
{


    internal class Program
    {

        static void Main(string[] args)
        {

            //初始化日志环境
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() //debug , info , warn , error
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("logs\\server-log.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            //加载JSON配置文件
            DataManager.Instance.Init();

            //网路服务模块
            NetService netService = new NetService();
            netService.Start();
            Log.Debug("NetService Success");

            UserService userService = UserService.Instance;
            userService.Start();
            Log.Debug("UserService Success");

            SpaceService spaceService = SpaceService.Instance;
            spaceService.Start();
            Log.Debug("SpaceService Success");

            BattleService.Instance.Start();
            Log.Debug("BattleService Success");

            Scheduler.Instance.Start();
            Log.Debug("Scheduler Success");

            // 启动聊天服务 
            ChatService.Instance.Start();
            Log.Information("ChatService Success");

            // 每秒执行 50 次
            Scheduler.Instance.AddTask(() =>
            {
                EntityManager.Instance.Update();
                SpaceManager.Instance.Update();
            }, 0.02f);

            // 设置新手村相关道具
            ItemEntity.Create(1, 1001, 10, new Vector3Int(0, 0, 0), Vector3Int.zero);
            ItemEntity.Create(1, 1002, 5, new Vector3Int(3000, 0, 3000), Vector3Int.zero);
            ItemEntity.Create(1, 1003, 1, new Vector3Int(6000, 0, 4000), Vector3Int.zero);


            // 传送门1：新手村=>森林
            UserService.GateBuild(
                spaceId: 1,
                tid: 4001001,
                position: new Vector3Int(10000, 0, 10000),
                GateName: "Portal-Forest entrance",
                targetSpaceId: 2,
                targetPosition: new Vector3Int(354947, 1660, 308498));

            // 山贼附近
            UserService.GateBuild(
                spaceId: 1,
                tid: 4001001,
                position: new Vector3Int(15000, 0, 12000),
                GateName: "Portal-ShanZei",
                targetSpaceId: 2,
                targetPosition: new Vector3Int(263442, 5457, 306462));

            //传送门2：森林=>新手村
            UserService.GateBuild(
                spaceId: 2,
                tid: 4001001,
                position: new Vector3Int(346318, 1870, 319313),
                GateName: "Portal - Newbie Village",
                targetSpaceId: 1,
                targetPosition: new Vector3Int(0, 0, 0));

            // 传送门3：森林=>篝火营地
            UserService.GateBuild(
                spaceId: 2,
                tid: 4001001,
                position: new Vector3Int(305260, 6220, 302866),
                GateName: "Portal - Campfire site.",
                targetSpaceId: 5,
                targetPosition: new Vector3Int(5992, 156, -19555));

            // 传送门4：新手村=>篝火营地
            UserService.GateBuild(
                spaceId: 1,
                tid: 4001001,
                position: new Vector3Int(-8000, 0, 1870),
                GateName: "Portal - Campfire site.",
                targetSpaceId: 5,
                targetPosition: new Vector3Int(5992, 156, -19555));

            // 传送门5：篝火营地=>新手村
            UserService.GateBuild(
                spaceId: 5,
                tid: 4001001,
                position: new Vector3Int(5460, 60, -22530),
                GateName: "Portal - Newbie Village.",
                targetSpaceId: 1,
                targetPosition: new Vector3Int(0, 0, 0));


            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}