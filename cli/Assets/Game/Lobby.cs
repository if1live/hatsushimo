using Assets.Game.Packets;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using UnityEngine.SceneManagement;

namespace Assets.Game
{
    class Lobby : MonoBehaviour
    {
        public InputField nicknameField;
        public InputField roomField;
        public Button joinButton;

        IObservable<JoinResponsePacket> JoinResponseObservable {
            get { return joinResp.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<JoinResponsePacket> joinResp = new ReactiveProperty<JoinResponsePacket>(null);


        private void Awake()
        {
            Debug.Assert(nicknameField != null);
            Debug.Assert(roomField != null);
            Debug.Assert(joinButton != null);
        }

        private void Start()
        {
            ConnectionManager.Instance.ReadyObservable.ObserveOnMainThread().Subscribe(conn =>
            {
                joinButton.interactable = true;
                conn.On<JoinResponsePacket>(Events.ROOM_JOIN, (ctx) => joinResp.Value = ctx);
            });

            joinButton.OnClickAsObservable().Subscribe(_ =>
            {
                var nickname = nicknameField.text.Trim();
                if (nickname.Length == 0) { return; }

                var roomID = roomField.text.Trim();
                if (roomID.Length == 0) { return; }

                var conn = ConnectionManager.Instance.Conn;
                var ctx = new JoinRequestPacket
                {
                    nickname = nickname,
                    room_id = roomID
                };
                conn.Emit(Events.ROOM_JOIN, ctx);

                // 중복 클릭 방지
                joinButton.interactable = false;
            });

            JoinResponseObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Log($"room id: {ctx.room_id}");

                var conn = ConnectionManager.Instance.Conn;
                conn.PlayerID = ctx.player_id;
                conn.RoomID = ctx.room_id;
                conn.Nickname = ctx.nickname;

                // TOOD async scene loading
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            });
        }

        private void OnDestroy()
        {
            var mgr = ConnectionManager.Instance;
            if(mgr)
            {
                var conn = mgr.Conn;
                conn.Off(Events.ROOM_JOIN);
            }
        }
    }
}
