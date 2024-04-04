using ScrapServer.Networking.Packets;
using ScrapServer.Utility;
using Steamworks.Data;

namespace ScrapServer.Networking.Client.Steam;

/// <summary>
/// An implementation of <see cref="IClient"/> used by <see cref="SteamServer"/>.
/// </summary>
internal sealed class SteamClient : IClient
{
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
    private ClientState state = ClientState.Connecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? StateChanged;

    private readonly List<(int packetId, Action<BinaryReader> handler)> packetHandlers;
    private readonly Connection connection;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamClient"/>.
    /// </summary>
    /// <param name="connection">The underlying steamworks connection.</param>
    public SteamClient(Connection connection)
    {
        packetHandlers = new List<(int, Action<BinaryReader>)>();
        this.connection = connection;
    }

    /// <inheritdoc/>
    public void HandlePacket<T>(EventHandler<PacketEventArgs<T>> handler) where T : IPacket, new()
    {
        var packetId = T.PacketId;

        Action<BinaryReader> outerHandler = reader =>
        {
            var packet = new T();
            try
            {
                packet.Deserialize(reader);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Malformed packet:\n{e}");
            }

            var args = new PacketEventArgs<T>(this, packetId, packet);
            handler?.Invoke(this, args);
        };
        packetHandlers.Add((packetId, outerHandler));
    }

    /// <inheritdoc/>
    public void SendPacket<T>(T packet) where T : IPacket
    {
        if (State == ClientState.Disconnected)
        {
            throw new ObjectDisposedException(ToString());
        }

        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);
        
        writer.Write(T.PacketId);

        using var cStream = new MemoryStream();
        using var cWriter = new BigEndianBinaryWriter(cStream);

        packet.Serialize(cWriter);

        if (cStream.Length > 0)
        {
            var compressedData = LZ4.Compress(cStream.AsSpan());

            writer.Write(compressedData);
        }

        connection.SendMessage(stream.ToArray(), 0, (int)stream.Length);
    }

    /// <summary>
    /// Runs the registered packet handlers.
    /// </summary>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="reader">The binary reader for reading the decompressed data of the packet.</param>
    internal void ReceivePacket(int packetId, BinaryReader reader)
    {
        foreach (var (id, handler) in packetHandlers)
        {
            if (id == packetId)
            {
                handler.Invoke(reader);
            }
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is SteamClient client && client.connection.Id == connection.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return unchecked((int)connection.Id);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Steam client '{connection.ConnectionName}'";
    }

    ~SteamClient()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }

    /// <summary>
    /// Closes the underlying connection.
    /// </summary>
    public void Disconnect()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }
}
