using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using UnityEngine.SceneManagement;
using Assets.NetChan;
using Hatsushimo.Packets;

namespace Assets.Game
{
    class Lobby : MonoBehaviour
    {
        public InputField nicknameField = null;
        public InputField roomField = null;
        public Button joinButton = null;

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
            }).AddTo(gameObject);

            joinButton.OnClickAsObservable().Subscribe(_ =>
            {
                var nickname = nicknameField.text.Trim();
                if (nickname.Length == 0) { return; }

                var roomID = roomField.text.Trim();
                if (roomID.Length == 0) { return; }

                var conn = ConnectionManager.Instance;
                var p = new RoomJoinRequestPacket
                {
                    Nickname = nickname,
                    RoomID = roomID
                };
                conn.SendPacket(p);

                // 중복 클릭 방지
                joinButton.interactable = false;
            });

            var dispatcher = PacketDispatcher.Instance;
            dispatcher.RoomJoinReceived.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"room id: {p.RoomID} / player id={p.PlayerID}");

                var conn = ConnectionManager.Instance.Conn;
                conn.PlayerID = p.PlayerID;
                conn.RoomID = p.RoomID;
                conn.Nickname = p.Nickname;

                // TOOD async scene loading
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }).AddTo(gameObject);
        }
    }
}
