namespace Assets.Game
{
    class ReadySender
    {
        const string EVENT_READY = "ready";

        public void Setup()
        {
            var socket = SocketManager.Instance.MySocket;
            socket.On(EVENT_READY, () =>
            {
                var conn = Connection.Instance;
                conn.IsReady.Value = true;
            });

            socket.Emit(EVENT_READY);
        }

        public void Cleanup()
        {
            var mgr = SocketManager.Instance;
            if (mgr == null) { return; }

            var socket = mgr.MySocket;
            socket.Off(EVENT_READY);
        }
    }
}
