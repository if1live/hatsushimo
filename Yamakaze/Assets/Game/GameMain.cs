using Assets.NetChan;
using Hatsushimo.Packets;
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

        private void Start()
        {
            var info = ConnectionInfo.Info;

            if (info.PlayerMode == PlayerMode.Player)
            {
                StartForPlayer();
            }
            else if (info.PlayerMode == PlayerMode.Observer)
            {
                StartForObserver();
            }

            // 접속 정보는 접속후에 내용이 바뀔일은 없을것이다
            var lines = new string[]
            {
                    $"room_id: {info.WorldID}",
                    $"player_id: {info.PlayerID}",
                    $"nickname: {info.Nickname}",
            };
            var msg = string.Join("\n", lines);
            statusText.text = msg;
        }

        void StartForPlayer()
        {
            var mgr = ConnectionManager.Instance;
            var info = ConnectionInfo.Info;

            mgr.PlayerReady.Received.ObserveOnMainThread().Subscribe(_ =>
            {
                waitingText.gameObject.SetActive(false);
            }).AddTo(this);

            mgr.SendImmediate(new PlayerReadyPacket());
        }

        void StartForObserver()
        {
            waitingText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // TOOD
            // ready의 특정 콜백만 제거하는 방법이 있나?
        }
    }
}
