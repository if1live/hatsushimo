using Newtonsoft.Json;
using UniRx;

namespace Assets.Game
{
    class ReadySender
    {
        const string EVENT_READY_REQUEST = "ready-req";
        const string EVENT_READY_RESPONSE = "ready-resp";

        struct ReadyResponse
        {
            public float posX;
            public float posY;
        }

        public void Setup()
        {
            var socket = SocketManager.Instance.Socket;
            socket.On(EVENT_READY_RESPONSE, (data) =>
            {
                var str = data.ToString();
                var ctx = JsonConvert.DeserializeObject<ReadyResponse>(str);

                var player = PlayerModel.Instance;
                player.IsReady.Value = true;
                player.PosX = ctx.posX;
                player.PosY = ctx.posY;
            });

            socket.Emit(EVENT_READY_REQUEST);
        }

        public void Cleanup()
        {
            var mgr = SocketManager.Instance;
            if (mgr == null) { return; }

            var socket = mgr.Socket;
            socket.Off(EVENT_READY_RESPONSE);
        }
    }
}
