using Assets.Game.Extensions;
using Assets.NetChan;
using Hatsushimo;
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

        IObservable<PlayerStatus> StatusReceived
        {
            get { return status.Skip(1).AsObservable(); }
        }
        ReactiveProperty<PlayerStatus> status = new ReactiveProperty<PlayerStatus>();

        IObservable<MoveNotify> Moved
        {
            get { return moved.Skip(1).AsObservable(); }
        }
        ReactiveProperty<MoveNotify> moved = new ReactiveProperty<MoveNotify>();

        public void ApplyStatus(PlayerStatus p) { status.Value = p; }
        public void ApplyMove(MoveNotify m) { moved.Value = m; }


        private void Start()
        {
            StatusReceived.Subscribe(packet =>
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
                Speed = Config.PlayerSpeed;
            }).AddTo(this);
        }
    }
}
