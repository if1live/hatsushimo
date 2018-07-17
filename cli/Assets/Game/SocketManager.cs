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
        public MySocket MySocket { get; private set; }

        public ReactiveProperty<bool> IsReady {
            get { return _isReady; }
            private set { _isReady = value; }
        }
        ReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);


        private void Awake()
        {
            if(Instance == null)
            {
                Debug.Assert(Instance == null);
                Instance = this;

                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.Log("SocketManager is already exist, remove self");
                GameObject.Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            DoOpen();
        }

        private void OnDestroy()
        {
            DoClose();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        bool DoOpen()
        {
            if (_socket != null)
            {
                return false;
            }

            _socket = IO.Socket(address);
            MySocket = new MySocket(_socket);
            MySocket.On(Socket.EVENT_CONNECT, () =>
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

            MySocket = null;
            _socket.Disconnect();
            _socket = null;
            return true;
        }
    }
}
