using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game
{
    class GameMain : MonoBehaviour
    {
        public Text waitingText = null;

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

        private void OnDestroy()
        {
            readySender.Cleanup();
        }
    }
}
