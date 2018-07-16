using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class GameMain : MonoBehaviour
    {
        public Text waitingText = null;

        public Text statusText = null;

        ReadySender readySender = new ReadySender();

        private void Start()
        {
            readySender.Setup();

            var player = PlayerModel.Instance;
            player.IsReady.ObserveOnMainThread()
                .Where(isReady => isReady)
                .Subscribe(_ =>
            {
                waitingText.gameObject.SetActive(false);
            });
        }

        public void Update()
        {
            // TODO 상태가 바뀌었을때만 객체를 다시 생성하면 성능이 좋아질것이다
            var player = PlayerModel.Instance;
            if(player.IsReady.Value)
            {
                var lines = new string[]
                {
                    $"room_id: {player.RoomID}",
                    $"player_id: {player.PlayerID}",
                    $"nickname: {player.Nickname}",
                    $"pos_x: {player.PosX}",
                    $"pos_y: {player.PosY}",
                };
                var msg = string.Join("\n", lines);
                statusText.text = msg;
            }
        }



        private void OnDestroy()
        {
            readySender.Cleanup();
        }
    }
}
