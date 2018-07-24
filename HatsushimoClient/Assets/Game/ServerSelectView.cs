using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Assets.NetChan;
using UnityEngine.SceneManagement;

namespace Assets.Game
{
    public class ServerSelectView : MonoBehaviour
    {
        public InputField hostInputField = null;
        public InputField portInputField = null;
        public Button connectButton = null;

        void Awake()
        {
            Debug.Assert(hostInputField != null);
            Debug.Assert(portInputField != null);
            Debug.Assert(connectButton != null);

            var info = ConnectionInfo.Info;
            hostInputField.text = info.ServerHost;
            portInputField.text = info.ServerPort.ToString();
        }

        void Start()
        {
            connectButton.OnClickAsObservable().Subscribe(_ => {
                // TODO validate?
                var hostText = hostInputField.text;
                var portText = portInputField.text;

                var host = hostText;
                var port = int.Parse(portText);

                var info = ConnectionInfo.Info;
                info.ServerHost = host;
                info.ServerPort = port;

                SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            });
        }
    }
}
