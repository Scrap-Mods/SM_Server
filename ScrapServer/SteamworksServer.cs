using ScrapServer.Networking;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;
using System.Diagnostics;

namespace ScrapServer.Vanilla;


/// <summary>
/// An implementation of <see cref="IServer"/> which uses the Steamworks socket manager.
/// </summary>
public sealed class SteamworksServer : IServer
{
    private readonly ref struct RawPacketEventArgs
    {
        public readonly IClient Client;
        public readonly ReadOnlySpan<byte> Data;

        public RawPacketEventArgs(IClient client, ReadOnlySpan<byte> data) { Client = client; Data = data; }
    }
    private delegate void RawPacketEventHandler(object? sender, RawPacketEventArgs args);

    private class SocketInterface : ISocketManager
    {
        private readonly SteamworksServer clientManager;

        public SocketInterface(SteamworksServer clientManager)
        {
            this.clientManager = clientManager;
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            var client = new SteamworksClient(connection, info.Identity, clientManager);

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

            clientManager.ReceiveRaw(client, dataSpan);
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

    private RawPacketEventHandler?[]? packetHandlers = null;

    private bool isDisposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamworksServer"/> which starts listening for connections.
    /// </summary>
    public SteamworksServer()
    {
        packetHandlers = new RawPacketEventHandler?[256];
        connectedClients = new List<SteamworksClient>();
        connectingClients = new List<SteamworksClient>();
        clients = new Dictionary<uint, SteamworksClient>();
        socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
        socketManager.Interface = new SocketInterface(this);
    }

    private void ReceiveRaw(IClient client, ReadOnlySpan<byte> dataSpan)
    {
        var id = dataSpan[0];

        if (packetHandlers == null)
        {
            throw new UnreachableException("Packet handlers has not been initialized");
        }
        else if (packetHandlers[id] == null)
        {
            throw new InvalidOperationException("Packet handler for this id is null");
        }
        else
        {
            packetHandlers[id].Invoke(this, new RawPacketEventArgs(client, dataSpan));
        }
    }

    /// <inheritdoc
    public void Handle<T>(PacketId id, PacketEventHandler<T> handler) where T : IBitSerializable, new()
    {
        if (packetHandlers == null)
        {
            throw new UnreachableException("Packet handlers has not been initialized");
        }

        packetHandlers[(byte)id] += (o, args) =>
        {
            var reader = BitReader.WithSharedPool(args.Data);
            var t = reader.ReadObject<T>();

            handler(o, new PacketEventArgs<T>(args.Client, (PacketId) args.Data[0], t));
        };
    }

    /// <inheritdoc
    public void Receive<T>(PacketEventArgs<T> args) where T : IBitSerializable, new()
    {
        // NOTE(ArcPrime): this isn't great for performance (serializing once more just to deserialize again), but you shouldn't be injecting packets much anyway

        var writer = BitWriter.WithSharedPool();
        writer.WriteByte((byte)args.PacketId);
        writer.WriteObject(args.Data);

        ReceiveRaw(args.Client, writer.Data);
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
