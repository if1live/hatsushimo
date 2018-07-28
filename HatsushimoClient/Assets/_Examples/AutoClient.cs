using UnityEngine;
using UniRx;
using Assets.NetChan;
using Hatsushimo;
using Hatsushimo.Packets;
using UnityEditor;

enum AutoClientState
{
    Init,

    WelcomeReceived,

    PingSent,
    PingReceived,

    SignUpSent,
    SignUpReceived,

    AuthenticationSent,
    AuthenticationReceived,

    WorldJoinSent,
    WorldJoinReceived,

    InGame,

    WorldLeaveSent,
    WorldLeaveReceived,

    // TODO
    // 명시적인 disconnect

    PlayerReadySent,
    PlayerReadyReceived,

    //LeaderboardReceived,

    Finished,
}

class AutoClient : MonoBehaviour
{
    AutoClientState state = AutoClientState.Init;

    int ping = 1234;
    string botUuid = "bot-uuid";
    string worldID = "default";
    string nickname = "bot-nick";

    void RegisterHandler()
    {
        var dispatcher = PacketDispatcher.Instance;
        dispatcher.Welcome.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test welcome");
            Debug.Assert(state == AutoClientState.Init);
            Debug.Assert(p.Version == Config.Version);
            state = AutoClientState.WelcomeReceived;
        }).AddTo(gameObject);

        dispatcher.Ping.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test ping");
            Debug.Assert(state == AutoClientState.PingSent);
            Debug.Assert(p.millis == ping);
            state = AutoClientState.PingReceived;
        }).AddTo(gameObject);

        dispatcher.SignUp.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test sign up");
            Debug.Assert(state == AutoClientState.SignUpSent);
            Debug.Assert(0 == p.ResultCode);
            state = AutoClientState.SignUpReceived;
        }).AddTo(gameObject);
        dispatcher.Authentication.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test authentication");
            Debug.Assert(state == AutoClientState.AuthenticationSent);
            Debug.Assert(0 == p.ResultCode);
            state = AutoClientState.AuthenticationReceived;
        }).AddTo(gameObject);

        // dispatcher.Replication.Received.ObserveOnMainThread().Subscribe(p =>
        // {
        //     Debug.Log("test replication?");
        // }).AddTo(gameObject);
        // dispatcher.ReplicationAll.Received.ObserveOnMainThread().Subscribe(p =>
        // {
        //     Debug.Log("test replication all?");
        // }).AddTo(gameObject);
        // dispatcher.ReplicationBulk.Received.ObserveOnMainThread().Subscribe(p =>
        // {
        //     Debug.Log("replication bulk?");
        // }).AddTo(gameObject);

        dispatcher.WorldJoin.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test world join");
            Debug.Assert(state == AutoClientState.WorldJoinSent);
            Debug.Assert(nickname == p.Nickname);
            Debug.Assert(worldID == p.WorldID);
            state = AutoClientState.WorldJoinReceived;
        }).AddTo(gameObject);
        dispatcher.WorldLeave.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test world leave");
            Debug.Assert(state == AutoClientState.WorldLeaveSent);
            state = AutoClientState.WorldLeaveReceived;
        }).AddTo(gameObject);

        dispatcher.PlayerReady.Received.ObserveOnMainThread().Subscribe(p =>
        {
            Debug.Log("test player ready");
            Debug.Assert(state == AutoClientState.PlayerReadySent);
            state = AutoClientState.PlayerReadyReceived;
        }).AddTo(gameObject);

        // dispatcher.Leaderboard.Received.ObserveOnMainThread().Subscribe(p =>
        // {
        //     Debug.Log("test leaderboard?");
        // }).AddTo(gameObject);
    }

    void Start()
    {
        var conn = ConnectionManager.Instance;
        conn.ReadyObservable.Subscribe(_ => RegisterHandler());

    }

    void Shutdown()
    {
        running = false;
        ConnectionManager.Instance.Close();
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#endif
    }

    bool running = true;
    void Update()
    {
        if (!running) { return; }
        var conn = ConnectionManager.Instance;
        switch (state)
        {
            case AutoClientState.WelcomeReceived:
                conn.SendPacket(new PingPacket() { millis = ping });
                state = AutoClientState.PingSent;
                break;

            case AutoClientState.PingReceived:
                conn.SendPacket(new SignUpPacket() { Uuid = botUuid });
                state = AutoClientState.SignUpSent;
                break;

            case AutoClientState.SignUpReceived:
                conn.SendPacket(new AuthenticationPacket() { Uuid = botUuid });
                state = AutoClientState.AuthenticationSent;
                break;

            case AutoClientState.AuthenticationReceived:
                conn.SendPacket(new WorldJoinPacket()
                {
                    WorldID = worldID,
                    Nickname = nickname,
                });
                state = AutoClientState.WorldJoinSent;
                break;

            case AutoClientState.WorldJoinReceived:
                conn.SendPacket(new PlayerReadyPacket());
                state = AutoClientState.PlayerReadySent;
                break;

            case AutoClientState.PlayerReadyReceived:
                // TODO world leave 반응 패킷이 없다
                conn.SendPacket(new WorldLeavePacket());
                state = AutoClientState.WorldLeaveSent;
                break;

            case AutoClientState.WorldLeaveReceived:
                state = AutoClientState.Finished;
                break;

            // TODO world leave의 리턴 구현하기
            case AutoClientState.WorldLeaveSent:
                Debug.Log("complete test?");
                Shutdown();
                break;

            case AutoClientState.Finished:
                Debug.Log("complete test");
                Shutdown();
                break;


            default:
                break;
        }

    }
}
