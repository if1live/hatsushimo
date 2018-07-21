using UniRx;
using UnityEngine;
using HatsushimoShared;
using System.Collections;
using System;

namespace Assets.NetChan
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        WebSocket ws;
        public string host = "ws://127.0.0.1";

        readonly PacketFactory factory = PacketFactory.Create();

        string ServerURL { get { return $"{host}:{Config.ServerPort}/game"; } }

        public IObservable<bool> ReadyObservable {
            get { return ready.Where(x => x).AsObservable(); }
        }
        BoolReactiveProperty ready = new BoolReactiveProperty(false);

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        IEnumerator Start()
        {
            ws = new WebSocket(new Uri(ServerURL));
            yield return StartCoroutine(ws.Connect());

            // TODO error check?
            ready.SetValueAndForceNotify(true);

            while (true)
            {
                // TODO 패킷 올때까지 대기하는 더 좋은 방법?
                byte[] bytes = null;
                while (bytes == null)
                {
                    bytes = ws.Recv();
                    yield return null;
                }

                // TODO TODO handler?
                var p = factory.Deserialize(bytes);
                var dispatcher = PacketDispatcher.Instance;
                dispatcher.Dispatch(p);
            }
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;

            if (ws != null)
            {
                ws.Close();
                ws = null;
            }
        }


        public void SendPacket(IPacket p)
        {
            var bytes = factory.Serialize(p);
            ws.Send(bytes);
        }

        
    }
}
