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
        public InputField uuidField = null;
        public InputField nicknameField = null;
        public InputField roomField = null;
        public Button joinButton = null;

        public Button genUUIDButton = null;

        private void Awake()
        {
            Debug.Assert(uuidField != null);
            Debug.Assert(nicknameField != null);
            Debug.Assert(roomField != null);
            Debug.Assert(joinButton != null);

            uuidField.text = SystemInfo.deviceUniqueIdentifier;
            Debug.Assert(genUUIDButton != null);
        }

        string UUID { get { return uuidField.text.Trim(); } }
        string Nickname { get { return nicknameField.text.Trim(); } }
        string WorldID { get { return roomField.text.Trim(); } }

        private void Start()
        {
            genUUIDButton.OnClickAsObservable().Subscribe(_ => {
                // 같은 클라에서 같은 uuid 생성되는게 싫을떄
                // 같은 컴퓨터에서 실행할 경우
                var uuid = Guid.NewGuid();
                uuidField.text = uuid.ToString();
            }).AddTo(gameObject);

            ConnectionManager.Instance.ReadyObservable.ObserveOnMainThread().Subscribe(conn =>
            {
                joinButton.interactable = true;
            }).AddTo(gameObject);

            joinButton.OnClickAsObservable().Subscribe(_ =>
            {
                if (UUID.Length == 0) { return; }
                if (Nickname.Length == 0) { return; }
                if (WorldID.Length == 0) { return; }

                var conn = ConnectionManager.Instance;
                var p = new SignUpPacket()
                {
                    Uuid = UUID,
                };
                conn.SendPacket(p);

                // 중복 클릭 방지
                joinButton.interactable = false;
            }).AddTo(gameObject);

            var dispatcher = PacketDispatcher.Instance;
            dispatcher.SignUp.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"sign up result: {p.ResultCode}");

                var auth = new AuthenticationPacket()
                {
                    Uuid = UUID,
                };

                var conn = ConnectionManager.Instance;
                conn.SendPacket(auth);
            }).AddTo(gameObject);

            dispatcher.Authentication.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"authentication result: {p.ResultCode}");

                if(p.ResultCode == 0)
                {
                    var join = new WorldJoinPacket
                    {
                        Nickname = Nickname,
                        WorldID = WorldID
                    };

                    var conn = ConnectionManager.Instance;
                    conn.SendPacket(join);
                }
            }).AddTo(gameObject);

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
