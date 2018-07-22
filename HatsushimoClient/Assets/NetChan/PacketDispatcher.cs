using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.NetChan
{
    public class PacketDispatcher : MonoBehaviour 
    {
        public static PacketDispatcher Instance;

        public IObservable<PingPacket> PingReceived {
            get { return ping.AsObservable(); }
        }
        ReactiveProperty<PingPacket> ping = new ReactiveProperty<PingPacket>();

        public IObservable<WelcomePacket> WelcomeReceived {
            get { return welcome.AsObservable(); }
        }
        ReactiveProperty<WelcomePacket> welcome = new ReactiveProperty<WelcomePacket>();

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }

        // TODO
        internal void Dispatch(IPacket p)
        {
            switch((PacketType)p.Type)
            {
                case PacketType.Ping:
                    ping.SetValueAndForceNotify((PingPacket)p);
                    break;
                case PacketType.Welcome:
                    welcome.SetValueAndForceNotify((WelcomePacket)p);
                    break;
                default:
                    break;
            }
        }
    }
}
