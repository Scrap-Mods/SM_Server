using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;
using System.Buffers;

namespace ScrapServer.Networking.Client.Steam;

/// <summary>
/// An implementation of <see cref="IClient"/> used by <see cref="SteamworksServer"/>.
/// </summary>
internal sealed class SteamworksClient : IClient
{
    private delegate void PacketHandler(ReadOnlySpan<byte> data);

    /// <inheritdoc/>
    public ClientState State
    {
        get => state;
        internal set
        {
            if (state != value)
            {
                state = value;

                if (state == ClientState.Disconnected)
                {
#pragma warning disable CA1816 // The finalizer should not be called when the client is disconnected by the server.
                    GC.SuppressFinalize(this);
#pragma warning restore CA1816
                }

                StateChanged?.Invoke(this, new ClientEventArgs(this));
            }
        }
    }

    /// <inheritdoc/>
    public string? Username => username;
    private readonly string? username;

    private ClientState state = ClientState.Connecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? StateChanged;

    private readonly PacketEventHandler?[] packetHandlers;
    private readonly Connection connection;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamworksClient"/>.
    /// </summary>
    /// <param name="connection">The underlying steamworks connection.</param>
    /// <param name="identity">The network identity of the client.</param>
    public SteamworksClient(Connection connection, NetIdentity identity)
    {
        this.connection = connection;

        packetHandlers = new PacketEventHandler?[256];
        username = null;
        if (identity.IsSteamId && identity.SteamId.IsValid)
        {
            username = new Friend(identity.SteamId).Name;
        }
    }

    /// <inheritdoc/>
    public void HandleRawPacket(PacketId packetId, PacketEventHandler handler)
    {
        packetHandlers[(byte)packetId] += handler;
    }

    /// <inheritdoc/>
    public void SendRawPacket(PacketId packetId, ReadOnlySpan<byte> data)
    {
        ObjectDisposedException.ThrowIf(State == ClientState.Disconnected, this);
        using BitWriter writer = BitWriter.WithSharedPool();
        writer.WriteByte((byte)packetId);
        writer.WriteBytes(data);
        connection.SendMessage(writer.Data);
    }

    /// <summary>
    /// Runs the registered packet handlers.
    /// </summary>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="data">The raw data of the packet excluding <paramref name="packetId"/>.</param>
    internal void ReceivePacket(PacketId packetId, ReadOnlySpan<byte> data)
    {
        packetHandlers[(byte)packetId]?.Invoke(this, new RawPacketEventArgs(this, packetId, data));
    }

    /// <inheritdoc/>
    public void AcceptConnection()
    {
        if (State == ClientState.Connecting)
        {
            connection.Accept();
        }
    }

    /// <inheritdoc/>
    public void Disconnect()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is SteamworksClient client && client.connection.Id == connection.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return unchecked((int)connection.Id);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Steam client '{connection.Id}'";
    }

    ~SteamworksClient()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }
}
