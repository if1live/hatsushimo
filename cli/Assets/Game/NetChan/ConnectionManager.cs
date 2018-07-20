using Assets.Game.Packets;
using Quobject.SocketIoClientDotNet.Client;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game.NetChan
{
    class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        public string address = "http://127.0.0.1:3000";
        public int version = 1234;

        Socket _socket;
        public Connection Conn { get { return _conn; } }
        Connection _conn = new Connection();

        public IObservable<Connection> ReadyObservable {
            get
            {
                return isReady.Where(x => x == true).Select(_ => Conn).AsObservable();
            }
        }
        ReactiveProperty<bool> isReady = new ReactiveProperty<bool>(false);

        private void Awake()
        {
            if (Instance == null)
            {
                Debug.Assert(Instance == null);
                Instance = this;

                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.Log("ConnectionManager is already exist, remove self");
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
            if (_socket != null) { return false; }

            _socket = IO.Socket(address);
            Conn.RawSocket = _socket;

            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                Debug.Log("connect...");
            });

            Conn.On<WelcomePacket>(Events.WELCOME, (packet) =>
            {
                var cli = version;
                var svr = packet.version;
                if (cli == svr)
                {
                    Debug.Log($"api version={svr}");
                    isReady.Value = true;
                }
                else
                {
                    Debug.LogWarning($"version mismatch: cli={cli} / svr={svr}");
                }
            });

            return true;
        }

        bool DoClose()
        {
            if (_socket == null) { return false; }

            Conn.RawSocket = null;
            _socket.Disconnect();
            _socket = null;
            return true;
        }
    }
}
