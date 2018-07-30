using UnityEngine;
using UniRx;
using Assets.NetChan;
using Hatsushimo;
using Hatsushimo.Packets;
using UnityEditor;
using System.Threading.Tasks;
using Hatsushimo.NetChan;
using System.Collections.Generic;
using System;

class AutoClient : MonoBehaviour
{
    int ping = 1234;
    string botUuid = "bot-uuid";
    string worldID = "default";
    string nickname = "bot-nick";

    void HandleWelcome(WelcomePacket p)
    {
        Debug.Assert(p.Version == Config.Version);
    }

    void HandlePing(PingPacket p)
    {
        Debug.Assert(p.millis == ping);
    }

    void HandleSignUp(SignUpResultPacket p)
    {
        Debug.Assert(0 == p.ResultCode);
    }

    void HandleAuthentication(AuthenticationResultPacket p)
    {
        Debug.Assert(0 == p.ResultCode);
    }

    void HandleWorldJoin(WorldJoinResultPacket p)
    {
        Debug.Assert(nickname == p.Nickname);
        Debug.Assert(worldID == p.WorldID);
    }

    void HandleWorldLeave(WorldLeaveResultPacket p)
    {
    }

    void HandlePlayerReady(PlayerReadyPacket p)
    {
    }

    void HandleDisconnect(DisconnectPacket p)
    {
    }

    readonly Queue<IPacket> queue = new Queue<IPacket>();

    void RegisterHandler()
    {
        var d = PacketDispatcher.Instance;
        RegisterHandler(d.Welcome);
        RegisterHandler(d.Ping);
        RegisterHandler(d.Disconnect);
        RegisterHandler(d.SignUp);
        RegisterHandler(d.Authentication);
        RegisterHandler(d.WorldJoin);
        RegisterHandler(d.WorldLeave);
        RegisterHandler(d.PlayerReady);
    }

    void RegisterHandler<TPacket>(PacketObservable<TPacket> subject) where TPacket : IPacket
    {
        subject.Received.ObserveOnMainThread().Subscribe(p => queue.Enqueue(p)).AddTo(this);
    }

    void Start()
    {
        var conn = ConnectionManager.Instance;
        conn.ReadyObservable.Subscribe(_ =>
        {
            RegisterHandler();
            RunTest();
        });
    }

    void Shutdown()
    {
        ConnectionManager.Instance.Close();
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#endif
    }

    async void RunTest()
    {
        var conn = ConnectionManager.Instance;

        var welcome = await Recv<WelcomePacket>();
        HandleWelcome(welcome);

        conn.SendImmediate(new PingPacket() { millis = this.ping });
        var ping = await Recv<PingPacket>();
        HandlePing(ping);

        conn.SendImmediate(new SignUpPacket() { Uuid = botUuid });
        var signup = await Recv<SignUpResultPacket>();
        HandleSignUp(signup);

        conn.SendImmediate(new AuthenticationPacket() { Uuid = botUuid });
        var auth = await Recv<AuthenticationResultPacket>();
        HandleAuthentication(auth);

        conn.SendImmediate(new WorldJoinPacket()
        {
            WorldID = worldID,
            Nickname = nickname,
        });
        var join = await Recv<WorldJoinResultPacket>();
        HandleWorldJoin(join);

        conn.SendImmediate(new PlayerReadyPacket());
        var ready = await Recv<PlayerReadyPacket>();
        HandlePlayerReady(ready);

        // TODO game loop 테스트 구현?

        conn.SendImmediate(new WorldLeavePacket());
        var leave = await Recv<WorldLeaveResultPacket>();
        HandleWorldLeave(leave);

        conn.SendImmediate(new DisconnectPacket());
        var disconnect = await Recv<DisconnectPacket>();
        HandleDisconnect(disconnect);

        Debug.Log("test completed");
        Shutdown();
    }

    async Task<TPacket> Recv<TPacket>() where TPacket : IPacket, new()
    {
        var dummy = new TPacket();
        var expectedType = (PacketType)dummy.Type;
        var tryCount = 100;
        for (int i = 0; i < tryCount; i++)
        {
            while (queue.Count > 0)
            {
                var packet = queue.Dequeue();
                var t = (PacketType)packet.Type;
                if (t == expectedType)
                {
                    return (TPacket)packet;
                }
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
        Debug.Assert(false, $"timeout : expected packet type: {expectedType}");
        return dummy;
    }
}
