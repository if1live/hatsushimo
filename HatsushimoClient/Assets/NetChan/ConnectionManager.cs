using UniRx;
using UnityEngine;
using Hatsushimo;
using System.Collections;
using System;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;

namespace Assets.NetChan
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        WebSocket ws;
        public string host = "ws://127.0.0.1";
        public int port = Config.ServerPort;

        readonly PacketCodec codec = MyPacketCodec.Create();

        string ServerURL
        {
            get
            {
                var info = ConnectionInfo.Info;
                if (info.UseDefaultServer)
                {
                    return $"{host}:{Config.ServerPort}/game";
                }
                else
                {
                    return info.ServerURL;
                }
            }
        }

        public IObservable<bool> ReadyObservable
        {
            get { return ready.Where(x => x).AsObservable(); }
        }
        BoolReactiveProperty ready = new BoolReactiveProperty(false);

        private void Awake()
        {
            if (Instance == null)
            {
                Debug.Assert(Instance == null);
                Instance = this;

                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.Log("ConnectionManager is already exist, remove self");
                GameObject.Destroy(this.gameObject);
            }
        }

        IEnumerator Start()
        {
            var url = ServerURL;
            Debug.Log($"connect to {url}");
            ws = new WebSocket(new Uri(url));
            yield return StartCoroutine(ws.Connect());

            if (ws.error != null)
            {
                Debug.LogError($"Error: " + ws.error);
                yield break;
            }

            // connection success
            ready.SetValueAndForceNotify(true);
            SendPacket(new ConnectPacket());

            // heartbeat
            var heartbeatInterval = TimeSpan.FromSeconds(Config.HeartbeatInterval);
            Observable.Interval(heartbeatInterval).Subscribe(_ =>
            {
                var p = new HeartbeatPacket();
                SendPacket(p);
            }).AddTo(gameObject);

            while (true)
            {
                byte[] bytes = null;
                while (bytes == null)
                {
                    if (ws.error != null)
                    {
                        Debug.LogError($"Error: " + ws.error);
                    }

                    bytes = ws.Recv();
                    yield return null;
                }

                var packet = codec.Decode(bytes);
                var dispatcher = PacketDispatcher.Instance;
                dispatcher.Dispatch(packet);
            }
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;

            if (ws != null)
            {
                SendPacket(new DisconnectPacket());

                ws.Close();
                ws = null;
            }
        }


        public void SendPacket(IPacket p)
        {
            var bytes = codec.Encode(p);
            ws.Send(bytes);
        }

    }
}
