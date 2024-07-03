using ScrapServer.Utility.Serialization;

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
    /// </summary
    /// <param name="id">The id of the packet.</param>
    /// <param name="data">The packet data.</param>
    public void Send<T>(PacketId id, T data) where T : IBitSerializable, new();

    /// <summary>
    /// Injects a packet into the handlers as if the client recieved it.
    /// </summary>
    /// <param name="id">The id of the packet.</param>
    /// <param name="data">The packet data.</param>
    public void Receive<T>(PacketId id, T data) where T : IBitSerializable, new();

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
