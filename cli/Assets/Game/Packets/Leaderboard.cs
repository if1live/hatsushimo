using System;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Packets
{
    class Leaderboard : MonoBehaviour
    {
        IObservable<LeaderboardPacket> LeaderboardObservable {
            get { return leaderboard.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<LeaderboardPacket> leaderboard = new ReactiveProperty<LeaderboardPacket>(null);

        // TODO render leaderboard
        public Text message;

        private void Awake()
        {
            Debug.Assert(message != null);
        }

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            conn.On<LeaderboardPacket>(Events.LEADERBOARD, (p => leaderboard.Value = p));

            LeaderboardObservable.ObserveOnMainThread().Subscribe(p =>
            {
                var replication = ReplicationManager.Instance;
                Debug.Assert(replication != null);

                var sb = new StringBuilder();
                foreach (var r in p.top)
                {
                    var player = replication.FindPlayer(r.id);
                    Debug.Assert(player != null, $"cannot player id={r.id}");
                    sb.AppendLine($"{r.rank} name={player.nickname}: id={r.id} score={r.score}");
                }
                sb.AppendLine($"players = {p.players}");
                message.text = sb.ToString();

            }).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            var mgr = ConnectionManager.Instance;
            if (mgr == null) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.LEADERBOARD);
        }
    }
}
