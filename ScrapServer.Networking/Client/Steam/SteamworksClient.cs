using ScrapServer.Networking.Packets;
using ScrapServer.Utility;
using ScrapServer.Utility.Serialization;
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
    private ClientState state = ClientState.Connecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? StateChanged;

    private readonly List<(int packetId, PacketHandler handler)> packetHandlers;
    private readonly Connection connection;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamworksClient"/>.
    /// </summary>
    /// <param name="connection">The underlying steamworks connection.</param>
    public SteamworksClient(Connection connection)
    {
        packetHandlers = new List<(int, PacketHandler)>();
        this.connection = connection;
    }

    /// <inheritdoc/>
    public void HandlePacket<T>(EventHandler<PacketEventArgs<T>> handler) where T : IPacket, new()
    {
        var packetId = T.PacketId;

        PacketHandler outerHandler = span =>
        {
            var reader = new BitReader(span, ArrayPool<byte>.Shared);
            T packet;

            try
            {
                packet = reader.ReadObject<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Malformed packet:\n{e}");
                return;
            }

            var args = new PacketEventArgs<T>(this, packetId, packet);
            handler?.Invoke(this, args);
        };
        packetHandlers.Add((packetId, outerHandler));
    }

    /// <inheritdoc/>
    public void SendPacket<T>(T packet) where T : IPacket
    {
        ObjectDisposedException.ThrowIf(State == ClientState.Disconnected, this);

        using var writer = new BitWriter(ArrayPool<byte>.Shared);
        writer.WriteObject<T>(packet);

        connection.SendMessage(writer.Data);
    }

    /// <summary>
    /// Runs the registered packet handlers.
    /// </summary>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="data">The raw data of the packet.</param>
    internal void ReceivePacket(int packetId, ReadOnlySpan<byte> data)
    {
        foreach (var (id, handler) in packetHandlers)
        {
            if (id == packetId)
            {
                handler.Invoke(data);
            }
        }
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
