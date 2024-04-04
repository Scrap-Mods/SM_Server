namespace ScrapServer.Networking.Client;

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
    /// Handlers should set <see cref="ClientConnectingEventArgs.IsAccepted"/> 
    /// to <see langword="true"/> to accept the connection.
    /// </summary>
    public event EventHandler<ClientConnectingEventArgs>? ClientConnecting;

    /// <summary>
    /// Fired when a client is connected to this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientConnected;

    /// <summary>
    /// Fired when a client is disconnected from this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

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
