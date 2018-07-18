using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class GameMain : MonoBehaviour
    {
        /*
        public Text waitingText = null;

        public Text statusText = null;

        ReadySender readySender = new ReadySender();

        private void Start()
        {
            readySender.Setup();

            var conn = Connection.Instance;
            conn.IsReady.ObserveOnMainThread()
                .Where(isReady => isReady)
                .Subscribe(_ =>
            {
                waitingText.gameObject.SetActive(false);
            });
        }

        public void Update()
        {
            // TODO 상태가 바뀌었을때만 객체를 다시 생성하면 성능이 좋아질것이다
            var conn = Connection.Instance;
            if(conn.IsReady.Value)
            {
                var lines = new string[]
                {
                    $"room_id: {conn.RoomID}",
                    $"player_id: {conn.PlayerID}",
                    $"nickname: {conn.Nickname}",
                };
                var msg = string.Join("\n", lines);
                statusText.text = msg;
            }
        }

        private void OnDestroy()
        {
            readySender.Cleanup();
        }
        */
    }
}
