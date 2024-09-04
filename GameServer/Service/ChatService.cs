using GameServer.Core;
using GameServer.Mgr;
using Proto;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    // 聊天服务 
    public class ChatService : Singleton<ChatService>
    {
        public void Start()
        {
            MessageRouter.Instance.Subscribe<ChatRequest>(_ChatRequest);
        }

        private void _ChatRequest(Connection conn, ChatRequest msg)
        {
            //获取当前主角对象
            var session = conn.Get<Session>();
            var chr = session.Character;

            //广播聊天消息
            var resp = new ChatResponse();
            resp.SenderId = chr.entityId;
            resp.TextValue = msg.TextValue;
            chr.Space.Broadcast(resp);

            if (msg.TextValue == "to 新手村")
            {
                var sp = SpaceManager.Instance.GetSpace(1);
                chr.TelportSpace(sp, Vector3Int.zero);
            }
            if (msg.TextValue == "to 森林")
            {
                var sp = SpaceManager.Instance.GetSpace(2);
                chr.TelportSpace(sp, new Vector3Int(354947, 1660, 308498));
            }
            if (msg.TextValue == "to 山贼")
            {
                var sp = SpaceManager.Instance.GetSpace(2);
                chr.TelportSpace(sp, new Vector3Int(263442, 5457, 306462));
            }
        }
    }
}
