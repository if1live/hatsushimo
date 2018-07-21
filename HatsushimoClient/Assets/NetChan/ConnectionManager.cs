using UnityEngine;
using HatsushimoShared;
using System.Collections;
using System;

namespace Assets.NetChan
{
    public class ConnectionManager : MonoBehaviour
    {
        WebSocket ws;
        public string host = "ws://127.0.0.1";

        readonly PacketFactory factory = PacketFactory.Create();

        string ServerURL { get { return $"{host}:{Config.ServerPort}/game"; } }

        IEnumerator Start()
        {
            ws = new WebSocket(new Uri(ServerURL));
            yield return StartCoroutine(ws.Connect());

            while(true)
            {
                SendPing();

                // TODO 패킷 올때까지 대기하는 더 좋은 방법?
                byte[] bytes = null;
                while(bytes == null)
                {
                    bytes = ws.Recv();
                    yield return null;
                }

                // TODO TODO handler?
                var p = factory.Deserialize(bytes);
                if(p.Type == PacketType.Ping)
                {
                    HandlePing((PingPacket)p);
                }

                yield return new WaitForSeconds(1);
            }
        }

        private void OnDestroy()
        {
            if(ws != null)
            {
                ws.Close();
                ws = null;
            }
        }

        void SendPing()
        {
            var p = new PingPacket()
            {
                millis = TimeUtils.NowMillis,
            };
            SendPacket(p);
        }

        void SendPacket(IPacket p)
        {
            var bytes = factory.Serialize(p);
            ws.Send(bytes);
        }

        void HandlePing(PingPacket p)
        {
            var now = TimeUtils.NowMillis;
            var diff = now - p.millis;
            Debug.Log($"ping: {diff}ms");
        }
    }
}