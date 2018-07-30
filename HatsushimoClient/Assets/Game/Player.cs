using Assets.Game.Extensions;
using Assets.NetChan;
using Hatsushimo.Packets;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string nickname;

        public Vector3 TargetPos;
        public float Speed;

        public bool IsOwner
        {
            get
            {
                var info = ConnectionInfo.Info;
                var myid = info.PlayerID;
                return (myid == id);
            }
        }

        IObservable<ReplicationActionPacket> ReplicationReceived
        {
            get { return replication.Skip(1).AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>();

        IObservable<PlayerInitial> InitialReceived
        {
            get { return initial.Skip(1).AsObservable(); }
        }
        ReactiveProperty<PlayerInitial> initial = new ReactiveProperty<PlayerInitial>();

        IObservable<MoveNotify> Moved
        {
            get { return moved.Skip(1).AsObservable(); }
        }
        ReactiveProperty<MoveNotify> moved = new ReactiveProperty<MoveNotify>();

        public void ApplyReplication(ReplicationActionPacket p) { replication.Value = p; }
        public void ApplyInitial(PlayerInitial p) { initial.Value = p; }
        public void ApplyMove(MoveNotify m) { moved.Value = m; }


        private void Start()
        {
            ReplicationReceived.Subscribe(packet =>
            {
                Debug.Assert(packet.ID == id, $"id mismatch: my={id} packet={packet.ID} action={packet.Action}");

                nickname = packet.Extra;
                TargetPos = packet.TargetPos.ToVector3();
                Speed = packet.Speed;

                // 생성 요청의 경우는 즉시 소환하지만
                // 갱신 요청의 경우 즉수 움직이면 순간이동때문에 어색하다
                // 갱신 요청은 target pos를 이용해서 처리하기
                if (packet.Action == ReplicationAction.Create)
                {
                    var pos = packet.Pos.ToVector3();
                    transform.position = pos;
                }

            }).AddTo(this);

            InitialReceived.Subscribe(packet =>
            {
                id = packet.ID;
                nickname = packet.Nickname;
                TargetPos = packet.TargetPos.ToVector3();

                var pos = packet.Pos.ToVector3();
                transform.position = pos;

            }).AddTo(this);

            // 이동 관련 추가 작업이 필요하면 고치기
            Moved.Subscribe(move => {
                TargetPos = move.TargetPos.ToVector3();
                // TODO 이동 계산을 구현하기
                Speed = 10;
            }).AddTo(this);
        }
    }
}
