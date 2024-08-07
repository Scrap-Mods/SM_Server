using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;

namespace ScrapServer.Networking;

/// <summary>
/// An implementation of <see cref="IServer"/> which uses the Steamworks socket manager.
/// </summary>
public sealed class SteamworksServer : IServer
{
    private class SocketInterface : ISocketManager
    {
        private readonly SteamworksServer clientManager;

        public SocketInterface(SteamworksServer clientManager)
        {
            this.clientManager = clientManager;
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            var client = new SteamworksClient(clientManager, connection, info.Identity);

            clientManager.clients.Add(connection.Id, client);
            clientManager.connectedClients.Add(client);

            client.State = ClientState.Connecting;
            clientManager.ClientConnecting?.Invoke(clientManager, new ClientEventArgs(client));
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

            var packetId = (PacketId)dataSpan[0];

            clientManager.ReceiveCompressed(client, packetId, dataSpan[1..]);
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? ClientConnecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? ClientConnected;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

    /// <inheritdoc/>
    public IReadOnlyList<IClient> ConnectedClients => connectedClients;
    private readonly List<SteamworksClient> connectedClients;

    /// <inheritdoc/>
    public IReadOnlyList<IClient> ConnectingClients => connectingClients;
    private readonly List<SteamworksClient> connectingClients;

    private readonly Dictionary<uint, SteamworksClient> clients;
    private readonly SocketManager socketManager;

    private RawPacketEventHandler?[] uncompressedPacketHandlers = new RawPacketEventHandler?[256];
    private RawPacketEventHandler?[] compressedPacketHandlers = new RawPacketEventHandler?[256];

    private bool isDisposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamworksServer"/> which starts listening for connections.
    /// </summary>
    public SteamworksServer()
    {
        connectedClients = new List<SteamworksClient>();
        connectingClients = new List<SteamworksClient>();
        clients = new Dictionary<uint, SteamworksClient>();
        socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
        socketManager.Interface = new SocketInterface(this);
    }

    /// <inheritdoc/>
    public void Handle<T>(PacketEventHandler<T> handler) where T : IPacket, new()
    {
        compressedPacketHandlers[(byte)T.PacketId] += (o, args) =>
        {
            var reader = BitReader.WithSharedPool(args.Data);

            T packet;

            if (T.IsCompressable)
            {
                using var decomp = reader.ReadLZ4();
                packet = decomp.Reader.ReadObject<T>();
            }
            else
            {
                packet = reader.ReadObject<T>();
            }

            var eventArgs = new PacketEventArgs<T>(args.Client, args.PacketId, packet);
            handler(this, eventArgs);
        };
        uncompressedPacketHandlers[(byte)T.PacketId] += (o, args) =>
        {
            var reader = BitReader.WithSharedPool(args.Data);

            T packet = reader.ReadObject<T>();

            var eventArgs = new PacketEventArgs<T>(args.Client, args.PacketId, packet);
            handler(this, eventArgs);
        };
    }

    internal void ReceiveCompressed(SteamworksClient client, PacketId packetId, ReadOnlySpan<byte> data)
    {
        var args = new RawPacketEventArgs(client, packetId, data);

        compressedPacketHandlers[(byte)packetId]?.Invoke(this, args);
    }

    internal void ReceiveUncompressed(SteamworksClient client, PacketId packetId, ReadOnlySpan<byte> data)
    {
        var args = new RawPacketEventArgs(client, packetId, data);

        uncompressedPacketHandlers[(byte)packetId]?.Invoke(this, args);
    }

    /// <inheritdoc/>
    public void Poll()
    {
        socketManager.Receive();
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

    ~SteamworksServer()
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
