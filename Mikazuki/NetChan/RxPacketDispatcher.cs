using System;
using System.IO;
using System.Reactive.Linq;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;

namespace Mikazuki.NetChan
{
    public class RxPacketObservable<TPacket> where TPacket : IPacket, new()
    {
        Action<PacketReceiveEventHandler<TPacket, Session>> addHandler;
        Action<PacketReceiveEventHandler<TPacket, Session>> removeHandler;

        public RxPacketObservable(
            Action<PacketReceiveEventHandler<TPacket, Session>> addHandler,
            Action<PacketReceiveEventHandler<TPacket, Session>> removeHandler
        )
        {
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IObservable<ReceivedPacket<TPacket>> Received
        {
            get
            {
                return Observable.FromEvent<
                    PacketReceiveEventHandler<TPacket, Session>,
                    PacketReceiveEventArgs<TPacket, Session>
                >(
                    addHandler,
                    removeHandler
                ).Select(x =>
                {
                    var codec = new PacketCodec();
                    var bytes = codec.Encode(x.Packet);
                    return new ReceivedPacket<TPacket>(x.Arg, bytes);
                });
            }
        }
    }

    public class RxPacketDispatcher
    {
        readonly PacketDispatcher<Session> dispatcher = new PacketDispatcher<Session>();

        public readonly RxPacketObservable<ConnectPacket> Connect;
        public readonly RxPacketObservable<DisconnectPacket> Disconnect;
        public readonly RxPacketObservable<PingPacket> Ping;
        public readonly RxPacketObservable<HeartbeatPacket> Heartbeat;
        public readonly RxPacketObservable<SignUpPacket> SignUp;
        public readonly RxPacketObservable<AuthenticationPacket> Authentication;
        public readonly RxPacketObservable<AttackPacket> Attack;
        public readonly RxPacketObservable<MovePacket> Move;
        public readonly RxPacketObservable<WorldJoinPacket> WorldJoin;
        public readonly RxPacketObservable<WorldLeavePacket> WorldLeave;
        public readonly RxPacketObservable<PlayerReadyPacket> PlayerReady;

        public RxPacketDispatcher()
        {
            Connect = new RxPacketObservable<ConnectPacket>(h => dispatcher.Connect += h, h => dispatcher.Connect -= h);
            Disconnect = new RxPacketObservable<DisconnectPacket>(h => dispatcher.Disconnect += h, h => dispatcher.Disconnect -= h);
            Ping = new RxPacketObservable<PingPacket>(h => dispatcher.Ping += h, h => dispatcher.Ping -= h);
            Heartbeat = new RxPacketObservable<HeartbeatPacket>(h => dispatcher.Heartbeat += h, h => dispatcher.Heartbeat -= h);
            SignUp = new RxPacketObservable<SignUpPacket>(h => dispatcher.SignUp += h, h => dispatcher.SignUp -= h);
            Authentication = new RxPacketObservable<AuthenticationPacket>(h => dispatcher.Authentication += h, h => dispatcher.Authentication -= h);
            Attack = new RxPacketObservable<AttackPacket>(h => dispatcher.Attack += h, h => dispatcher.Attack -= h);
            Move = new RxPacketObservable<MovePacket>(h => dispatcher.Move += h, h => dispatcher.Move -= h);
            WorldJoin = new RxPacketObservable<WorldJoinPacket>(h => dispatcher.WorldJoin += h, h => dispatcher.WorldJoin -= h);
            WorldLeave = new RxPacketObservable<WorldLeavePacket>(h => dispatcher.WorldLeave += h, h => dispatcher.WorldLeave -= h);
            PlayerReady = new RxPacketObservable<PlayerReadyPacket>(h => dispatcher.PlayerReady += h, h => dispatcher.PlayerReady -= h);
        }

        public void OnNext(Session session, byte[] data)
        {
            dispatcher.Dispatch(data, session);
        }
    }
}
