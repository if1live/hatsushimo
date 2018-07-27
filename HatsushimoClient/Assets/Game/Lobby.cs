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

                var worldID = roomField.text.Trim();
                if (worldID.Length == 0) { return; }

                var conn = ConnectionManager.Instance;
                var p = new WorldJoinPacket
                {
                    Nickname = nickname,
                    WorldID = worldID
                };
                conn.SendPacket(p);

                // 중복 클릭 방지
                joinButton.interactable = false;
            });

            var dispatcher = PacketDispatcher.Instance;
            dispatcher.WorldJoin.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"world id: {p.WorldID} / player id={p.PlayerID}");

                var info = ConnectionInfo.Info;
                info.PlayerID = p.PlayerID;
                info.WorldID = p.WorldID;
                info.Nickname = p.Nickname;

                // TOOD async scene loading
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }).AddTo(gameObject);
        }
    }
}
