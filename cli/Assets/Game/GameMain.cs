using Assets.Game.Packets;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class GameMain : MonoBehaviour
    {
        public Text waitingText = null;
        public Text statusText = null;

        IObservable<bool> ReadyObservable {
            get { return ready.Where(x => x == true).AsObservable(); }
        }
        ReactiveProperty<bool> ready = new ReactiveProperty<bool>(false);

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;
            conn.On(Events.PLAYER_READY, () => ready.Value = true);

            ReadyObservable.ObserveOnMainThread().Subscribe(_ =>
            {
                waitingText.gameObject.SetActive(false);

                // 접속 정보는 접속후에 내용이 바뀔일은 없을것이다
                var lines = new string[]
                {
                    $"room_id: {conn.RoomID}",
                    $"player_id: {conn.PlayerID}",
                    $"nickname: {conn.Nickname}",
                };
                var msg = string.Join("\n", lines);
                statusText.text = msg;
            });

            conn.Emit(Events.PLAYER_READY);
        }

        private void OnDestroy()
        {
            // TOOD
            // ready의 특정 콜백만 제거하는 방법이 있나?
        }
    }
}
