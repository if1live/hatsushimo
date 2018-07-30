using UnityEngine;
using Assets.NetChan;
using UniRx;
using Hatsushimo.Packets;
using System.Linq;

namespace Assets.Game
{
    public class MoveSyncManager : MonoBehaviour
    {
        public static MoveSyncManager Instance;

        void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        void Start()
        {
            var dispatcher = PacketDispatcher.Instance;
            dispatcher.MoveNotify.Received.ObserveOnMainThread().Subscribe(packet =>
            {
                foreach (var move in packet.list) { ApplyMove(move); }
            }).AddTo(this);
        }

        void ApplyMove(MoveNotify move)
        {
            // TODO 플레이어 이외에 이동하는게 생기면 분기
            var replication = ReplicationManager.Instance;
            var player = replication.FindPlayer(move.ID);
            player.ApplyMove(move);
        }


        void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }
    }
}
