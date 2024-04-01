using ScrapServer.Networking.Packets;

namespace ScrapServer.Networking.Client;

public interface IClient : IDisposable
{
    public ClientState State { get; }
    public event EventHandler<ClientEventArgs>? StateChanged;

    public void SubscribeToPacket<T>(EventHandler<IncomingPacketEventArgs<T>> handler) where T : IPacket, new();

    public T ReceivePacket<T>(int timeoutMillis = 5000) where T : IPacket, new();
    public void SendPacket<T>(T packet) where T : IPacket;
}
