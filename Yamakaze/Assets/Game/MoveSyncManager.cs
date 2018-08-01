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
            var conn = ConnectionManager.Instance;
            conn.MoveNotify.Received.ObserveOnMainThread().Subscribe(packet =>
            {
                foreach (var move in packet.list) { ApplyMove(move); }
            }).AddTo(this);
        }

        bool ApplyMove(MoveNotify move)
        {
            // TODO 플레이어 이외에 이동하는게 생기면 분기
            // 이동 정보 패킷은 immediate로 보내는데
            // 플레이서 소환 패킷은 lazy로 보낼지도 모른다
            // 두 패킷의 의존성이 없다고 가정하고 안전하게 예외처리
            var replication = ReplicationManager.Instance;
            Player player = null;
            if (replication.TryGetPlayer(move.ID, out player))
            {
                player.ApplyMove(move);
                return true;
            }
            return false;
        }


        void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;
        }
    }
}
