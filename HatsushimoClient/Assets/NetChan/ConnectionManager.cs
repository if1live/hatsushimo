using UniRx;
using UnityEngine;
using Hatsushimo;
using System.Collections;
using System;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System.IO;
using System.Threading.Tasks;
using Hatsushimo.Utils;

namespace Assets.NetChan
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        WebSocket ws;
        public string host = "ws://127.0.0.1";
        public int port = Config.ServerPort;

        static readonly PacketCodec codec = new PacketCodec();

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


        readonly BandwidthChecker bandwidth = new BandwidthChecker();
        public IObservable<int> SentBytes { get { return _sentBytes.AsObservable(); } }
        public IObservable<int> ReceivedBytes { get { return _receivedBytes; } }
        ReactiveProperty<int> _sentBytes = new ReactiveProperty<int>();
        ReactiveProperty<int> _receivedBytes = new ReactiveProperty<int>();

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

        void Start()
        {
            var bandwidthInterval = TimeSpan.FromMilliseconds(100);
            Observable.Interval(bandwidthInterval).Subscribe(_ =>
            {
                var ts = TimeUtils.NowTimestamp - 1000;
                bandwidth.Flush(ts);
                _receivedBytes.SetValueAndForceNotify(bandwidth.GetReceivedBytesPerSeconds(ts));
                _sentBytes.SetValueAndForceNotify(bandwidth.GetSentBytesPerSecond(ts));
            }).AddTo(gameObject);

            coroutine_start = StartCoroutine(BeginStart());
        }

        Coroutine coroutine_start;

        IEnumerator BeginStart()
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

            while (ws != null)
            {
                byte[] bytes = null;
                while (ws != null && bytes == null)
                {
                    if (ws.error != null)
                    {
                        Debug.LogError($"Error: " + ws.error);
                    }

                    bytes = ws.Recv();
                    yield return null;
                }

                var now = TimeUtils.NowTimestamp;
                bandwidth.AddReceived(bytes.Length, now);

                var stream = new MemoryStream(bytes);
                var reader = new BinaryReader(stream);
                var type = (PacketType)codec.ReadPacketType(reader);

                var dispatcher = PacketDispatcher.Instance;
                if (DispatchPacket(type, reader, dispatcher.Ping)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.Welcome)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.Disconnect)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.SignUp)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.Authentication)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.ReplicationAll)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.Replication)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.ReplicationBulk)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.WorldJoin)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.WorldLeave)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.PlayerReady)) { continue; }
                if (DispatchPacket(type, reader, dispatcher.Leaderboard)) { continue; }

                Debug.Log($"handler not found: packet_type={type}");
            }
        }

        bool DispatchPacket<TPacket>(PacketType type, BinaryReader reader, PacketObservable<TPacket> subject)
        where TPacket : IPacket, new()
        {
            TPacket packet;
            if (codec.TryDecode<TPacket>((short)type, reader, out packet))
            {
                subject.SetValueAndForceNotify(packet);
                return true;
            }
            return false;
        }


        private void OnDestroy()
        {
            Close();

            Debug.Assert(Instance == this);
            Instance = null;
        }

        public void Close()
        {
            if (coroutine_start != null)
            {
                StopCoroutine(coroutine_start);
                coroutine_start = null;
            }

            if (ws != null)
            {
                SendPacket(new DisconnectPacket());

                ws.Close();
                ws = null;
            }
        }


        public void SendPacket<T>(T p) where T : IPacket
        {
            var bytes = codec.Encode(p);
            ws.Send(bytes);

            var now = TimeUtils.NowTimestamp;
            bandwidth.AddSent(bytes.Length, now);
        }
    }
}
