using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Summer;
using Common;
using Serilog;
using GameServer.Model;
using Proto;
using GameServer.Mgr;
using GameServer.Core;

namespace GameServer.Network
{
    /// <summary>
    /// 网络服务
    /// </summary>
    public class NetService
    {

        TcpServer tcpServer;

        public NetService()
        {
            tcpServer = new TcpServer("0.0.0.0", 32510);
            tcpServer.Connected += OnClientConnected;   // += 用来注册事件处理程序的方法
            tcpServer.Disconnected += OnDisconnected;
        }
        //记录conn最后一次心跳包的时间
        private Dictionary<Connection, DateTime> heartBeatPairs = new Dictionary<Connection, DateTime>();


        public void Start()
        {
            //启动网络监听，指定消息包装类型
            tcpServer.Start();
            //启动消息分发器
            MessageRouter.Instance.Start(4);

            MessageRouter.Instance.Subscribe<HeartBeatRequest>(_HeartBeatRequest);


            Timer timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }


        void TimerCallback(object state)
        {
            //Log.Information("执行检查");
            var now = DateTime.Now;
            foreach (var kv in heartBeatPairs)
            {
                var cha = now - kv.Value;
                if (cha.TotalSeconds > 10)
                {
                    //关闭超时的客户端连接
                    Connection conn = kv.Key;
                    conn.Close();
                    heartBeatPairs.Remove(conn);
                }
            }
        }




        //收到心跳包
        private void _HeartBeatRequest(Connection conn, HeartBeatRequest msg)
        {
            heartBeatPairs[conn] = DateTime.Now;
            //Log.Information("收到心跳包：" + conn);
            HeartBeatResponse resp = new HeartBeatResponse();
            conn.Send(resp);
        }


        // 当客户端接入
        private void OnClientConnected(Connection conn)
        {
            Log.Information("客户端接入");
            heartBeatPairs[conn] = DateTime.Now;
            conn.Set<Session>(new Session());
        }

        private void OnDisconnected(Connection conn)
        {
            heartBeatPairs.Remove(conn);
            Log.Information("连接断开:" + conn);
            var chr = conn.Get<Session>().Character;
            var space = chr?.Space;
            if (space != null)
            {
                space.EntityLeave(chr);
                CharacterManager.Instance.RemoveCharacter(chr.Id);
            }
        }

    }
}
