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
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Assets.NetChan
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        WebSocket ws;
        public string host = "ws://127.0.0.1";
        public int port = Config.ServerPort;

        // 네트워크 에러 발생하면 초기 화면으로 가서 접속 재시도
        public string initialScene = "ServerSelect";

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

        public IObservable<string> ErrorRaised { get { return _error.AsObservable(); } }
        Subject<string> _error = new Subject<string>();

        public IObservable<bool> ReadyObservable
        {
            get { return readySubject.AsObservable(); }
        }
        Subject<bool> readySubject = new Subject<bool>();

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

        IDisposable loop = null;

        void Start()
        {
            var bandwidthInterval = TimeSpan.FromMilliseconds(100);
            Observable.Interval(bandwidthInterval).Subscribe(_ =>
            {
                var ts = TimeUtils.NowTimestamp - 1000;
                bandwidth.Flush(ts);
                _receivedBytes.SetValueAndForceNotify(bandwidth.GetReceivedBytesPerSeconds(ts));
                _sentBytes.SetValueAndForceNotify(bandwidth.GetSentBytesPerSecond(ts));
            }).AddTo(this);

            var heartbeatInterval = TimeSpan.FromSeconds(Config.HeartbeatInterval);
            Observable.Interval(heartbeatInterval).SkipUntil(ReadyObservable).Subscribe(_ =>
            {
                SendImmediate(new HeartbeatPacket());
            }).AddTo(this);

            var flushInterval = TimeSpan.FromMilliseconds(300);
            Observable.Interval(flushInterval).SkipUntil(ReadyObservable).Subscribe(_ => Flush()).AddTo(this);

            ErrorRaised.Subscribe(msg =>
            {
                Debug.LogError($"Error: " + ws.error);

                if (loop != null)
                {
                    loop.Dispose();
                    loop = null;
                }

                // dont destory on load로 등록된 네트워크 관련 객체 삭제
                // 네트워크 접속 실패시 처음부터 재시작하는게 목적
                // TOOD 최초 접속 에러와 게임 도중 접속 에러를 분리하기
                Destroy(PingChecker.Instance.gameObject);
                Destroy(ConnectionManager.Instance.gameObject);

                SceneManager.LoadScene(initialScene, LoadSceneMode.Single);

            }).AddTo(this);

            loop = Observable.FromCoroutine(BeginLoop).Subscribe();
        }

        IEnumerator BeginLoop()
        {
            var url = ServerURL;
            Debug.Log($"connect to {url}");
            ws = new WebSocket(new Uri(url));
            yield return StartCoroutine(ws.Connect());

            if (ws.error != null)
            {
                _error.OnNext(ws.error);
                yield break;
            }

            // connection success
            readySubject.OnNext(true);
            SendImmediate(new ConnectPacket());

            while (ws != null)
            {
                byte[] bytes = null;
                while (ws != null && bytes == null)
                {
                    if (ws.error != null)
                    {
                        _error.OnNext(ws.error);
                    }

                    bytes = ws.Recv();
                    yield return null;
                }

                var now = TimeUtils.NowTimestamp;
                bandwidth.AddReceived(bytes.Length, now);

                var stream = new MemoryStream(bytes);
                var reader = new BinaryReader(stream);
                // 패킷 여러개가 붙어있을지도 모른다
                while (true)
                {
                    PacketType type = (PacketType)codec.ReadPacketType(reader);
                    if (type == PacketType.Invalid) { break; }
                    Dispatch(type, reader);
                }
            }
        }

        bool Dispatch(PacketType type, BinaryReader reader)
        {
            var dispatcher = PacketDispatcher.Instance;
            if (DispatchPacket(type, reader, dispatcher.Ping)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.Welcome)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.Disconnect)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.SignUp)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.Authentication)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.ReplicationAll)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.Replication)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.ReplicationBulk)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.WorldJoin)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.WorldLeave)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.PlayerReady)) { return true; }
            if (DispatchPacket(type, reader, dispatcher.Leaderboard)) { return true; }

            Debug.Log($"handler not found: packet_type={type}");
            return false;
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

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Close()
        {
            if (loop != null)
            {
                loop.Dispose();
                loop = null;
            }

            if (ws != null)
            {
                SendImmediate(new DisconnectPacket());

                ws.Close();
                ws = null;
            }
        }


        public void SendImmediate<T>(T p) where T : IPacket
        {
            var bytes = codec.Encode(p);
            ws.Send(bytes);

            var now = TimeUtils.NowTimestamp;
            bandwidth.AddSent(bytes.Length, now);
        }

        readonly List<byte[]> queue = new List<byte[]>();
        public void SendLazy<T>(T p) where T : IPacket
        {
            var bytes = codec.Encode(p);
            queue.Add(bytes);
        }

        public void Flush()
        {
            var bytes = ByteJoin.Combine(queue);
            queue.Clear();

            ws.Send(bytes);
            var now = TimeUtils.NowTimestamp;
            bandwidth.AddSent(bytes.Length, now);
        }
    }
}
