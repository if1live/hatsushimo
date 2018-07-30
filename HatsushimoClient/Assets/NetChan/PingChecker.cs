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

        public IObservable<int> LatencyObservable
        {
            get { return latency.AsObservable(); }
        }
        Subject<int> latency = new Subject<int>();

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
                Debug.Log("PingChecker is already exist, remove self");
                GameObject.Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            var dispatcher = PacketDispatcher.Instance;
            dispatcher.Ping.Received.Subscribe(HandlePing).AddTo(gameObject);

            var conn = ConnectionManager.Instance;
            var pingInterval = TimeSpan.FromMilliseconds(intervalMillis);
            Observable.Interval(pingInterval)
            .SkipUntil(conn.ReadyObservable).Subscribe(_ =>
            {
                SendPing();
            }).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
            latency.OnNext(diff);
        }
    }
}
