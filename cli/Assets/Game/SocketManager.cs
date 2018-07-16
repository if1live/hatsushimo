using Quobject.SocketIoClientDotNet.Client;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class SocketManager : MonoBehaviour
    {
        public static SocketManager Instance { get; private set; }

        public string address = "http://127.0.0.1:3000";

        Socket _socket;
        public Socket Socket { get { return _socket; } }

        public ReactiveProperty<bool> IsReady {
            get { return _isReady; }
            private set { _isReady = value; }
        }
        ReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);


        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);

            DoOpen();
        }

        private void OnDestroy()
        {
            DoClose();

            Debug.Assert(Instance == this);
            Instance = null;
        }

        bool DoOpen()
        {
            if (_socket != null)
            {
                return false;
            }

            _socket = IO.Socket(address);
            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                Debug.Log("connect");
                IsReady.Value = true;
            });

            return true;
        }

        bool DoClose()
        {
            if (_socket == null)
            {
                return false;
            }

            _socket.Disconnect();
            _socket = null;
            return true;
        }
    }
}
