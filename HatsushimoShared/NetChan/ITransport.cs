namespace Hatsushimo.NetChan
{
    public struct Datagram<ClientID>
    {
        // from / to
        public ClientID ID;
        public byte[] Data;

        public Datagram(ClientID id, byte[] data)
        {
            this.ID = id;
            this.Data = data;
        }
    }

    // 연결마다 생성되는 객체
    public interface ITransport<ClientID>
    {
        ClientID ID { get; }

        // Send() -> raw socket.send()
        void Send(byte[] data);

        // raw_socket.recv() -> Recv()
        void Recv(byte[] data);

        void Close();
    }
}
