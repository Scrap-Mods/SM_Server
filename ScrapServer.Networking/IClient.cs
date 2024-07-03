using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking;

/// <summary>
/// Represents a client connected to a <see cref="IServer"/> for sending and receiving packets.
/// </summary>
public interface IClient : IDisposable
{
    /// <summary>
    /// Gets the username of the client.
    /// </summary>
    /// <value>The username or <see langword="null"/> if unknown.</value>
    public string? Username { get; }

    /// <summary>
    /// Gets the current state of the client.
    /// </summary>
    /// <value>Client state.</value>
    public ClientState State { get; }

    /// <summary>
    /// Fired when the state of client is changed.
    /// </summary>
    public event EventHandler<ClientEventArgs>? StateChanged;

    /// <summary>
    /// Sends a packet to the client.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public void Send<T>(T packet) where T : IPacket;

    /// <summary>
    /// Injects a raw packet to be sent to the handlers
    /// </summary>
    /// <remarks>
    /// <paramref name="data"> is uncompressed and does not contain a packet ID.
    /// </remarks>
    /// <param name="id">The id of the packet.</param>
    /// <param name="data">The raw packet.</param>
    public void Inject(PacketId id, ReadOnlySpan<byte> data);

    /// <summary>
    /// Accepts the incoming connection.
    /// </summary>
    /// <remarks>
    /// Does nothing when <see cref="State"/> has any value other than <see cref="ClientState.Connecting"/>.
    /// </remarks>
    public void AcceptConnection();

    /// <summary>
    /// Disconnects the client from the server.
    /// </summary>
    public void Disconnect();

#pragma warning disable CA1816 // GC.SuppressFinalize is intended to be called in Disconnect.
    void IDisposable.Dispose() => Disconnect();
#pragma warning restore CA1816
}
