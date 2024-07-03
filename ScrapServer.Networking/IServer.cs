using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// A delegate for handling incoming messages from the client.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">The event args.</param>
public delegate void PacketEventHandler<T>(object? sender, PacketEventArgs<T> args) where T : IBitSerializable, new();

/// <summary>
/// Represents a server listening for incoming connections.
/// </summary>
public interface IServer : IDisposable
{
    /// <summary>
    /// Gets the list of clients trying to establish a connection with this server.
    /// </summary>
    /// <value>List of connecting clients.</value>
    public IReadOnlyList<IClient> ConnectingClients { get; }

    /// <summary>
    /// Gets the list of clients connected to this server.
    /// </summary>
    /// <value>List of connected clients.</value>
    public IReadOnlyList<IClient> ConnectedClients { get; }

    /// <summary>
    /// Fired when a client is trying to connect to this server.
    /// Handlers should set call <see cref="IClient.AcceptConnection"/> 
    /// to accept the connection.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientConnecting;

    /// <summary>
    /// Fired when a client is connected to this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientConnected;

    /// <summary>
    /// Fired when a client is disconnected from this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

    /// <summary>
    /// Registers a handler for packets coming from any client 
    /// connected to this server.
    /// </summary>
    /// <param name="id">The id to subscribe this callback to.</param>
    /// <param name="handler">The delegate to be called when a packet is received.</param>
    public void Handle<T>(PacketId id, PacketEventHandler<T> handler) where T : IBitSerializable, new();

    /// <summary>
    /// Injects a packet into the handlers as if the server recieved it.
    /// </summary>
    /// <param name="args">The arguments of the packet event.</param>
    public void Receive<T>(PacketEventArgs<T> args) where T : IBitSerializable, new();

    /// <summary>
    /// Runs a single iteration of the event loop.
    /// </summary>
    public void Poll();

    /// <summary>
    /// Runs the server event loop.
    /// </summary>
    /// <remarks>
    /// Does not throw an <see cref="OperationCanceledException"/> 
    /// when <paramref name="cancellationToken"/> signals cancellation.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for stopping the loop.</param>
    public void RunLoop(CancellationToken cancellationToken = default);
}
