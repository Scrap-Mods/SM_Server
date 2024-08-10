using ScrapServer.Networking.Packets.Data;
using Steamworks;
using Steamworks.Data;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Utility.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScrapServer.Networking;

/// <summary>
/// An implementation of <see cref="IClient"/> used by <see cref="SteamworksServer"/>.
/// </summary>
public sealed class SteamworksClient : IClient
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

    private readonly SteamworksServer server;

    private readonly UInt64 steamid;
    public UInt64 Id => steamid;

    /// <inheritdoc/>
    public string? Username => username;
    private readonly string? username = null;

    private ClientState state = ClientState.Connecting;

    /// <inheritdoc/>
    public event EventHandler<ClientEventArgs>? StateChanged;

    private readonly Connection connection;

    /// <summary>
    /// Initializes a new instance of <see cref="SteamworksClient"/>.
    /// </summary>
    /// <param name="connection">The underlying steamworks connection.</param>
    /// <param name="identity">The network identity of the client.</param>
    public SteamworksClient(SteamworksServer server, Connection connection, NetIdentity identity)
    {
        this.connection = connection;
        if (identity.IsSteamId && identity.SteamId.IsValid)
        {
            steamid = identity.SteamId;
            username = new Friend(identity.SteamId).Name;
        }
        this.server = server;
    }

    /// <inheritdoc/>
    public void Send<T>(T packet) where T : IPacket
    {
        Console.WriteLine($"[!] Sending Packet: {packet.GetType().Name}\tTo: {Id} ({username ?? "Unknown"})");
        var writer = BitWriter.WithSharedPool();
        writer.WriteByte((byte)T.PacketId);
        try
        {
            if (T.IsCompressable)
            {
                using var comp = writer.WriteLZ4();
                comp.Writer.WriteObject(packet);
            }
            else
            {
                writer.WriteObject(packet);
            }

            unsafe
            {
                fixed (byte* ptr = writer.Data)
                {
                    connection.SendMessage((nint)ptr, writer.Data.Length, SendType.Reliable);
                }
            }
        }
        finally
        {
            writer.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Inject(PacketId packetId, ReadOnlySpan<byte> data)
    {
        server.ReceiveUncompressed(this, packetId, data);
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
        return $"Steam client '{username ?? connection.Id.ToString()}'";
    }

    ~SteamworksClient()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }
}
