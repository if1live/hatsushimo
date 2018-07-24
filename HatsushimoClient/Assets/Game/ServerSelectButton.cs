using UnityEngine;
using UnityEngine.UI;
using Assets.NetChan;
using UniRx;

namespace Assets.Game
{
    class ServerSelectButton  : MonoBehaviour
    {
        public Button button = null;
        public InputField addressInputField = null;
        public string address = "ws://127.0.0.1";

        void Awake()
        {
            Debug.Assert(button != null);
        }

        void Start()
        {
            button.OnClickAsObservable().Subscribe(_ => {
                var info = ConnectionInfo.Info;
                addressInputField.text = address;
                info.ServerHost = address;
            }).AddTo(gameObject);
        }
    }
}
