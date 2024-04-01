using SMServer.Packets;
using Steamworks.Data;

namespace SMServer.src.Client;

internal sealed class SteamworksClient : IClient
{
    public ClientState State
    {
        get => state;
        internal set
        {
            if (state != value)
            {
                state = value;
                StateChanged?.Invoke(this, new ClientEventArgs(this));
            }
        }
    }
    private ClientState state;

    public event EventHandler<ClientEventArgs>? StateChanged;

    internal Connection Connection { get; }
    internal SteamworksClientManager ClientManager { get; }

    public SteamworksClient(SteamworksClientManager clientManager, Connection connection)
    {
        State = ClientState.Connecting;
        ClientManager = clientManager;
        Connection = connection;
    }

    public T ReceivePacket<T>(int timeoutMillis = 5000) where T : IPacket, new()
    {
        if (State == ClientState.Disconnected)
        {
            throw new ObjectDisposedException(this.ToString());
        }
        throw new NotImplementedException();
    }

    public void SendPacket<T>(T packet) where T : IPacket
    {
        if (State == ClientState.Disconnected)
        {
            throw new ObjectDisposedException(this.ToString());
        }
        ClientManager.EnqueueOutcomingPacket(Connection, packet);
    }

    public void SubscribeToPacket<T>(EventHandler<IncomingPacketEventArgs<T>> handler) where T : IPacket, new()
    {
        throw new NotImplementedException();
    }

    ~SteamworksClient()
    {
        Connection.Close(false, 0);
    }

    public void Dispose()
    {
        if (State != ClientState.Disconnected)
        {
            Connection.Close(false, 0);
            GC.SuppressFinalize(this);
        }
    }

    public override string ToString()
    {
        return $"Steamworks client '{Connection.ConnectionName}'";
    }
}
