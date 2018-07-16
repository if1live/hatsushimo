using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Game
{
    class JoinButton : MonoBehaviour
    {
        const string EVENT_ROOM_JOIN_REQUEST = "room-join-req";
        const string EVENT_ROOM_JOIN_RESPONSE = "room-join-resp";

        struct RoomJoinRequest
        {
            public string nickname;
        }

        class RoomJoinResponse
        {
            public bool ok;
            public string room_id;
            public string player_id;
            public string nickname;
        }

        ReactiveProperty<RoomJoinResponse> OnRoomJoinResponse {
            get { return _onRoomJoinResponse; }
        }
        ReactiveProperty<RoomJoinResponse> _onRoomJoinResponse = new ReactiveProperty<RoomJoinResponse>(null);

        public Button button = null;
        public InputField nickField = null;

        void Awake()
        {
            Debug.Assert(button != null);
            Debug.Assert(nickField != null);
        }

        void Start()
        {
            var mgr = SocketManager.Instance;
            mgr.IsReady.ObserveOnMainThread().Where(isReady => isReady == true).Subscribe(_ =>
            {
                var socket = mgr.Socket;
                button.onClick.AddListener(OnSubmit);
                button.interactable = true;

                socket.On(EVENT_ROOM_JOIN_RESPONSE, (data) =>
                {
                    var str = data.ToString();
                    var ctx = JsonConvert.DeserializeObject<RoomJoinResponse>(str);
                    OnRoomJoinResponse.Value = ctx;
                });
            });

            OnRoomJoinResponse.ObserveOnMainThread().Where(resp => resp != null).Subscribe(ctx =>
            {
                Debug.Log($"room id: {ctx.room_id}");

                var player = PlayerModel.Instance;
                player.RoomID = ctx.room_id;
                player.PlayerID = ctx.player_id;
                player.Nickname = ctx.nickname;
                

                SceneManager.LoadScene("Game", LoadSceneMode.Single);
            });
        }

        void OnDestroy()
        {
            var mgr = SocketManager.Instance;
            if (mgr)
            {
                var socket = mgr.Socket;
                socket.Off(EVENT_ROOM_JOIN_RESPONSE);
            }
        }

        void OnSubmit()
        {
            var nickname = nickField.text.Trim();
            if (nickname.Length == 0)
            {
                return;
            }

            var socket = SocketManager.Instance.Socket;
            var ctx = new RoomJoinRequest { nickname = nickname };
            var json = JsonConvert.SerializeObject(ctx);
            socket.Emit(EVENT_ROOM_JOIN_REQUEST, json);

            // 중복 클릭 방지
            button.interactable = false;
        }
    }
}
