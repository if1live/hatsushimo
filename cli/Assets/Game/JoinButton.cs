using Assets.Game.Types;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Game
{
    class JoinButton : MonoBehaviour
    {
        const string EVENT_ROOM_JOIN = "room-join";

        ReactiveProperty<JoinResponseResponse> OnRoomJoinResponse {
            get { return _onRoomJoinResponse; }
        }
        ReactiveProperty<JoinResponseResponse> _onRoomJoinResponse = new ReactiveProperty<JoinResponseResponse>(null);

        public Button joinButton = null;
        public InputField nickField = null;
        public InputField roomField = null;

        void Awake()
        {
            Debug.Assert(joinButton != null);
            Debug.Assert(nickField != null);
            Debug.Assert(roomField != null);
        }

        void Start()
        {
            var mgr = SocketManager.Instance;
            mgr.IsReady.ObserveOnMainThread().Where(isReady => isReady == true).Subscribe(_ =>
            {
                var socket = mgr.MySocket;
                joinButton.onClick.AddListener(OnSubmit);
                joinButton.interactable = true;

                socket.On<JoinResponseResponse>(EVENT_ROOM_JOIN, (ctx) => OnRoomJoinResponse.Value = ctx);
            });

            OnRoomJoinResponse.ObserveOnMainThread().Where(resp => resp != null).Subscribe(ctx =>
            {
                Debug.Log($"room id: {ctx.room_id}");

                var conn = Connection.Instance;
                conn.RoomID = ctx.room_id;
                conn.PlayerID = ctx.player_id;
                conn.Nickname = ctx.nickname;

                // TOOD async scene loading
                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            });
        }

        void OnDestroy()
        {
            var mgr = SocketManager.Instance;
            if (mgr)
            {
                var socket = mgr.MySocket;
                socket.Off(EVENT_ROOM_JOIN);
            }
        }

        void OnSubmit()
        {
            var nickname = nickField.text.Trim();
            if (nickname.Length == 0)
            {
                return;
            }

            var roomID = roomField.text.Trim();
            if (roomID.Length == 0)
            {
                return;
            }

            var socket = SocketManager.Instance.MySocket;
            var ctx = new JoinRequestPacket { nickname = nickname, room_id = roomID };
            socket.Emit(EVENT_ROOM_JOIN, ctx);

            // 중복 클릭 방지
            joinButton.interactable = false;
        }
    }
}
