using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Hatsushimo.Packets;
using Hatsushimo.Utils;

namespace Assets.NetChan
{
    public class PingChecker : MonoBehaviour
    {
        public static PingChecker Instance;

        public int intervalMillis = 1000;

        public const int INF_LATENCY = 99999;

        public IObservable<int> LatencyObservable {
            get { return latency.AsObservable(); }
        }
        IntReactiveProperty latency = new IntReactiveProperty(INF_LATENCY);

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            var dispatcher = PacketDispatcher.Instance;
            dispatcher.Ping.Received.Subscribe(HandlePing).AddTo(gameObject);

            var mgr = ConnectionManager.Instance;
            ConnectionManager.Instance.ReadyObservable.Subscribe(_ =>
            {
                StartCoroutine(BeginPingLoop());
            }).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }

        void SendPing()
        {
            var mgr = ConnectionManager.Instance;
            if (mgr == null) { return; }
            var p = new PingPacket()
            {
                millis = TimeUtils.NowMillis,
            };
            mgr.SendPacket(p);
        }

        void HandlePing(PingPacket p)
        {
            var now = TimeUtils.NowMillis;
            var diff = now - p.millis;
            latency.SetValueAndForceNotify(diff);
        }

        IEnumerator BeginPingLoop()
        {
            while (true)
            {
                SendPing();
                yield return new WaitForSeconds(intervalMillis * 0.001f);
            }
        }
    }
}
