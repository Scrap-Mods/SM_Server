using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;
using System.Buffers;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScrapServer.Networking.Steam;

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
    public SteamworksClient(Connection connection, NetIdentity identity, SteamworksServer server)
    {
        this.connection = connection;
        if (identity.IsSteamId && identity.SteamId.IsValid)
        {
            username = new Friend(identity.SteamId).Name;
        }

        this.server = server;
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

    /// <inheritdoc/>
    public void Send<T>(PacketId id, T packet) where T : IBitSerializable, new()
    {
        ObjectDisposedException.ThrowIf(State == ClientState.Disconnected, this);

        var writer = BitWriter.WithSharedPool();
        writer.WriteByte((byte)id);

        if (packet != null)
        {
            using var compWriter = writer.WriteLZ4();
            packet.Serialize(ref compWriter.Writer);
        }

        connection.SendMessage(writer.Data);
    }
    /// <inheritdoc/>
    public void Receive(ReadOnlySpan<byte> data)
    {
        server.Receive(this, data);
    }

    ~SteamworksClient()
    {
        if (State != ClientState.Disconnected)
        {
            connection.Close(false, 0);
        }
    }
}
