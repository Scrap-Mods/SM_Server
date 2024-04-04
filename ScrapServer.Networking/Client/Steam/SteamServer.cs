using ScrapServer.Utility;
using Steamworks;
using Steamworks.Data;

namespace ScrapServer.Networking.Client.Steam;

/// <summary>
/// An implementation of <see cref="IServer"/> which uses the Steamworks socket manager.
/// </summary>
public sealed class SteamServer : IServer
{
    private class SocketInterface : ISocketManager
    {
        private readonly SteamServer clientManager;

        public SocketInterface(SteamServer clientManager)
        {
            this.clientManager = clientManager;
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            var client = new SteamClient(connection);

            clientManager.clients.Add(connection.Id, client);
            clientManager.connectedClients.Add(client);

            var args = new ClientConnectingEventArgs(client);

            client.State = ClientState.Connecting;
            clientManager.ClientConnecting?.Invoke(clientManager, args);

            if (args.IsAccepted)
            {
                connection.Accept();
            }
            else
            {
                connection.Close();
            }
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            if (!clientManager.clients.TryGetValue(connection.Id, out var client))
            {
                return;
            }

            clientManager.connectingClients.Remove(client);
            clientManager.connectedClients.Add(client);

            client.State = ClientState.Connected;
            clientManager.ClientConnected?.Invoke(clientManager, new ClientEventArgs(client));
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            if (!clientManager.clients.TryGetValue(connection.Id, out var client))
            {
                return;
            }

            if (client.State == ClientState.Connected)
            {
                clientManager.connectedClients.Remove(client);
            }
            else if (client.State == ClientState.Connecting)
            {
                clientManager.connectingClients.Remove(client);
            }

            client.State = ClientState.Disconnected;
            clientManager.ClientDisconnected?.Invoke(clientManager, new ClientEventArgs(client));
        }

        public void OnMessage(Connection connection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel)
        {
            if (!clientManager.clients.TryGetValue(connection.Id, out var client))
            {
                return;
            }

            ReadOnlySpan<byte> dataSpan;
            unsafe
            {
                dataSpan = new ReadOnlySpan<byte>((void*)data, size);
            }

            byte packetId = dataSpan[0];
            var decompressedData = LZ4.Decompress(dataSpan, out int decompressedLength);
            using var stream = new MemoryStream(decompressedData, 0, decompressedLength);
            using var reader = new BigEndianBinaryReader(stream);

            client.ReceivePacket(packetId, reader);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ClientConnectingEventArgs>? ClientConnecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? ClientConnected;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

    /// <inheritdoc/>
    public IReadOnlyList<IClient> ConnectedClients => connectedClients;
    private readonly List<SteamClient> connectedClients;

    /// <inheritdoc/>
    public IReadOnlyList<IClient> ConnectingClients => connectingClients;
    private readonly List<SteamClient> connectingClients;

    private readonly Dictionary<uint, SteamClient> clients;
    private readonly SocketManager socketManager;

    private bool isDisposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamServer"/> which starts listening for connections.
    /// </summary>
    public SteamServer()
    {
        connectedClients = new List<SteamClient>();
        connectingClients = new List<SteamClient>();
        clients = new Dictionary<uint, SteamClient>();
        socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
        socketManager.Interface = new SocketInterface(this);
    }

    /// <inheritdoc/>
    public void RunLoop(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            socketManager.Receive();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Steam client {GetHashCode()}'";
    }

    ~SteamServer()
    {
        if (!isDisposed)
        {
            socketManager.Close();
        }
    }

    /// <summary>
    /// Closes the underlying socket.
    /// </summary>
    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            socketManager.Close();
            GC.SuppressFinalize(this);
        }
    }
}
