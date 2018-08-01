using UniRx;
using UnityEngine;
using Hatsushimo;
using Hatsushimo.NetChan;
using System.Collections;
using System;
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

        readonly PacketDispatcher<int> dispatcher = new PacketDispatcher<int>();

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
            InitializePacketObservables();

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
                dispatcher.Dispatch(bytes, 0);
            }
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

        public UniRxPacketObservable<PingPacket> Ping;
        public UniRxPacketObservable<WelcomePacket> Welcome;
        public UniRxPacketObservable<DisconnectPacket> Disconnect;

        public UniRxPacketObservable<SignUpResultPacket> SignUp;
        public UniRxPacketObservable<AuthenticationResultPacket> Authentication;

        public UniRxPacketObservable<ReplicationAllPacket> ReplicationAll;

        public UniRxPacketObservable<ReplicationCreatePlayerPacket> CreatePlayer;
        public UniRxPacketObservable<ReplicationCreateFoodPacket> CreateFood;
        public UniRxPacketObservable<ReplicationCreateProjectilePacket> CreateProjectile;

        public UniRxPacketObservable<ReplicationRemovePacket> ReplicationRemove;
        public UniRxPacketObservable<ReplicationBulkRemovePacket> ReplicationBulkRemove;

        public UniRxPacketObservable<WorldJoinResultPacket> WorldJoin;
        public UniRxPacketObservable<WorldLeaveResultPacket> WorldLeave;

        public UniRxPacketObservable<PlayerReadyPacket> PlayerReady;
        public UniRxPacketObservable<LeaderboardPacket> Leaderboard;

        public UniRxPacketObservable<MoveNotifyPacket> MoveNotify;
        public UniRxPacketObservable<AttackNotifyPacket> AttackNotify;

        void InitializePacketObservables()
        {
            // packet event
            Ping = new UniRxPacketObservable<PingPacket>(
                h => (arg) => h(arg),
                h => dispatcher.Ping += h,
                h => dispatcher.Ping -= h
            );

            Welcome = new UniRxPacketObservable<WelcomePacket>(
                h => (arg) => h(arg),
                h => dispatcher.Welcome += h,
                h => dispatcher.Welcome -= h
            );

            Disconnect = new UniRxPacketObservable<DisconnectPacket>(
                h => (arg) => h(arg),
                h => dispatcher.Disconnect += h,
                h => dispatcher.Disconnect -= h
            );

            SignUp = new UniRxPacketObservable<SignUpResultPacket>(
                h => (arg) => h(arg),
                h => dispatcher.SignUpResult += h,
                h => dispatcher.SignUpResult -= h
            );

            Authentication = new UniRxPacketObservable<AuthenticationResultPacket>(
                h => (arg) => h(arg),
                h => dispatcher.AuthenticationResult += h,
                h => dispatcher.AuthenticationResult -= h
            );

            ReplicationAll = new UniRxPacketObservable<ReplicationAllPacket>(
                h => (arg) => h(arg),
                h => dispatcher.ReplicationAll += h,
                h => dispatcher.ReplicationAll -= h
            );

            CreateFood = new UniRxPacketObservable<ReplicationCreateFoodPacket>(
                h => (arg) => h(arg),
                h => dispatcher.CreateFood += h,
                h => dispatcher.CreateFood -= h
            );

            CreatePlayer = new UniRxPacketObservable<ReplicationCreatePlayerPacket>(
                h => (arg) => h(arg),
                h => dispatcher.CreatePlayer += h,
                h => dispatcher.CreatePlayer -= h
            );

            CreateProjectile = new UniRxPacketObservable<ReplicationCreateProjectilePacket>(
                h => (arg) => h(arg),
                h => dispatcher.CreateProjectile += h,
                h => dispatcher.CreateProjectile -= h
            );

            ReplicationRemove = new UniRxPacketObservable<ReplicationRemovePacket>(
                h => (arg) => h(arg),
                h => dispatcher.ReplicationRemove += h,
                h => dispatcher.ReplicationRemove -= h
            );

            ReplicationBulkRemove = new UniRxPacketObservable<ReplicationBulkRemovePacket>(
                h => (arg) => h(arg),
                h => dispatcher.ReplicationBulkRemove += h,
                h => dispatcher.ReplicationBulkRemove -= h
            );

            WorldJoin = new UniRxPacketObservable<WorldJoinResultPacket>(
                h => (arg) => h(arg),
                h => dispatcher.WorldJoinResult += h,
                h => dispatcher.WorldJoinResult -= h
            );

            WorldLeave = new UniRxPacketObservable<WorldLeaveResultPacket>(
                h => (arg) => h(arg),
                h => dispatcher.WorldLeaveResult += h,
                h => dispatcher.WorldLeaveResult -= h
            );

            PlayerReady = new UniRxPacketObservable<PlayerReadyPacket>(
                h => (arg) => h(arg),
                h => dispatcher.PlayerReady += h,
                h => dispatcher.PlayerReady -= h
            );

            Leaderboard = new UniRxPacketObservable<LeaderboardPacket>(
                h => (arg) => h(arg),
                h => dispatcher.Leaderboard += h,
                h => dispatcher.Leaderboard -= h
            );

            MoveNotify = new UniRxPacketObservable<MoveNotifyPacket>(
                h => (arg) => h(arg),
                h => dispatcher.MoveNotify += h,
                h => dispatcher.MoveNotify -= h
            );

            AttackNotify = new UniRxPacketObservable<AttackNotifyPacket>(
                h => (arg) => h(arg),
                h => dispatcher.AttackNotify += h,
                h => dispatcher.AttackNotify -= h
            );
        }
    }

    public class UniRxPacketObservable<TPacket> where TPacket : IPacket, new()
    {
        Func<Action<PacketReceiveEventArgs<TPacket, int>>, PacketReceiveEventHandler<TPacket, int>> conversion;
        Action<PacketReceiveEventHandler<TPacket, int>> addHandler;
        Action<PacketReceiveEventHandler<TPacket, int>> removeHandler;

        public UniRxPacketObservable(
            Func<Action<PacketReceiveEventArgs<TPacket, int>>, PacketReceiveEventHandler<TPacket, int>> conversion,
            Action<PacketReceiveEventHandler<TPacket, int>> addHandler,
            Action<PacketReceiveEventHandler<TPacket, int>> removeHandler
        )
        {
            this.conversion = conversion;
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IObservable<TPacket> Received
        {
            get
            {
                return Observable.FromEvent<
                    PacketReceiveEventHandler<TPacket, int>,
                    PacketReceiveEventArgs<TPacket, int>
                >(
                    conversion,
                    addHandler,
                    removeHandler
                ).Select(x => x.Packet);
            }
        }
    }
}
