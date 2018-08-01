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
            var conn = ConnectionManager.Instance;
            conn.Leaderboard.Received.ObserveOnMainThread().Subscribe(p =>
            {
                var replication = ReplicationManager.Instance;
                Debug.Assert(replication != null);

                var sb = new StringBuilder();
                foreach (var r in p.Top)
                {
                    Player player = null;
                    if (replication.TryGetPlayer(r.ID, out player))
                    {
                        sb.AppendLine($"{r.Ranking} name={player.nickname}: id={r.ID} score={r.Score}");
                    }
                }
                sb.AppendLine($"players = {p.Players}");
                message.text = sb.ToString();

            }).AddTo(this);
        }
    }
}
