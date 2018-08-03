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
        public Button joinPlayerButton = null;
        public Button joinObserverButton = null;


        public Button genUUIDButton = null;

        private void Awake()
        {
            Debug.Assert(uuidField != null);
            Debug.Assert(nicknameField != null);
            Debug.Assert(roomField != null);
            Debug.Assert(joinPlayerButton != null);
            Debug.Assert(joinObserverButton != null);

            uuidField.text = SystemInfo.deviceUniqueIdentifier;
            Debug.Assert(genUUIDButton != null);
        }

        string UUID { get { return uuidField.text.Trim(); } }
        string Nickname { get { return nicknameField.text.Trim(); } }
        string WorldID { get { return roomField.text.Trim(); } }

        private void Start()
        {
            var conn = ConnectionManager.Instance;

            genUUIDButton.OnClickAsObservable().Subscribe(_ =>
            {
                // 같은 클라에서 같은 uuid 생성되는게 싫을떄
                // 같은 컴퓨터에서 실행할 경우
                var uuid = Guid.NewGuid();
                uuidField.text = uuid.ToString();
            }).AddTo(this);

            ConnectionManager.Instance.ReadyObservable.ObserveOnMainThread().Subscribe(_ =>
            {
                joinPlayerButton.interactable = true;
                joinObserverButton.interactable = true;
            }).AddTo(this);

            joinPlayerButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnJoinButtonClicked(PlayerMode.Player);
            }).AddTo(this);

            joinObserverButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnJoinButtonClicked(PlayerMode.Observer);
            }).AddTo(this);

            conn.Welcome.Received.ObserveOnMainThread().Subscribe(p =>
            {
                var info = ConnectionInfo.Info;
                info.PlayerID = p.UserID;
            });

            conn.SignUp.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"sign up result: {p.ResultCode}");

                var auth = new AuthenticationPacket(UUID);
                conn.SendImmediate(auth);
            }).AddTo(this);

            conn.Authentication.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"authentication result: {p.ResultCode}");

                if (p.ResultCode == 0)
                {
                    Debug.Assert(mode != PlayerMode.None);

                    var info = ConnectionInfo.Info;
                    info.WorldID = WorldID;
                    info.Nickname = Nickname;
                    info.PlayerMode = mode;

                    var join = new WorldJoinPacket(WorldID, Nickname, mode);
                    conn.SendImmediate(join);
                }
            }).AddTo(this);

            conn.WorldJoin.Received.ObserveOnMainThread().Subscribe(p =>
            {
                Debug.Log($"player id={p.PlayerID}");
                if (p.ResultCode != 0)
                {
                    var info = ConnectionInfo.Info;
                    info.WorldID = "";
                    info.Nickname = "";
                }


                // TOOD async scene loading
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }).AddTo(this);
        }


        PlayerMode mode = PlayerMode.None;

        void OnJoinButtonClicked(PlayerMode mode)
        {
            if (UUID.Length == 0) { return; }
            if (Nickname.Length == 0) { return; }
            if (WorldID.Length == 0) { return; }

            this.mode = mode;

            var p = new SignUpPacket(UUID);
            var conn = ConnectionManager.Instance;
            conn.SendImmediate(p);

            // 중복 클릭 방지
            joinPlayerButton.interactable = false;
            joinObserverButton.interactable = false;
        }
    }
}
