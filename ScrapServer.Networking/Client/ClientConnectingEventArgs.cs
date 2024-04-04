namespace ScrapServer.Networking.Client;

/// <summary>
/// The arguments for <see cref="IClient"/> connection attempt events.
/// </summary>
public class ClientConnectingEventArgs
{
    /// <summary>
    /// Gets the connecting client.
    /// </summary>
    /// <value>The connecting client.</value>
    public IClient Client { get; }

    /// <summary>
    /// Gets or sets whether the connection should be accepted or not.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to accept the connection, 
    /// <see langword="false"/> to reject the connection.
    /// </value>
    public bool IsAccepted { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of <see cref="ClientConnectingEventArgs"/>.
    /// </summary>
    /// <param name="client">The connecting client.</param>
    public ClientConnectingEventArgs(IClient client)
    {
        Client = client;
    }
}
