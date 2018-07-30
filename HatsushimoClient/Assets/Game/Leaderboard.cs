using Assets.NetChan;
using Hatsushimo.Packets;
using System;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class Leaderboard : MonoBehaviour
    {
        // TODO render leaderboard
        public Text message = null;

        private void Awake()
        {
            Debug.Assert(message != null);
        }

        private void Start()
        {
            var dispatcher = PacketDispatcher.Instance;
            dispatcher.Leaderboard.Received.ObserveOnMainThread().Subscribe(p =>
            {
                var replication = ReplicationManager.Instance;
                Debug.Assert(replication != null);

                var sb = new StringBuilder();
                foreach (var r in p.Top)
                {
                    var player = replication.FindPlayer(r.ID);
                    Debug.Assert(player != null, $"cannot player id={r.ID}");
                    sb.AppendLine($"{r.Ranking} name={player.nickname}: id={r.ID} score={r.Score}");
                }
                sb.AppendLine($"players = {p.Players}");
                message.text = sb.ToString();

            }).AddTo(this);
        }
    }
}
