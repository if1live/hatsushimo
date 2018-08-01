using System;

namespace Mikazuki.NetChan
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

        // raw_socket.recv()로 부터 데이터를 받으면 이벤트가 발생
        IObservable<byte[]> Received { get; }

        void Close();
    }

    public interface ITransportLayer<ClientID>
    {
        void Send(ClientID id, byte[] data);
        IObservable<Datagram<ClientID>> Received { get; }
        void Close(ClientID id);
    }
}
