using Assets.Game;
using Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

public class Demo : MonoBehaviour
{
    Socket socket;

    readonly PingSender pingSender = new PingSender();

    void Start()
    {
        var host = "http://127.0.0.1:3000";
        socket = IO.Socket(host);
        socket.On(Socket.EVENT_CONNECT, () =>
        {
            Debug.Log("connection!");

            pingSender.Initialize(socket);
        });
    }

    private void OnDestroy()
    {
        socket.Disconnect();
        socket = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
