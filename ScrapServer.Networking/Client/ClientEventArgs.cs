namespace ScrapServer.Networking.Client;

/// <summary>
/// The arguments for generic <see cref="IClient"/> events.
/// </summary>
public struct ClientEventArgs
{
    /// <summary>
    /// Gets the related client.
    /// </summary>
    /// <value>The client.</value>
    public IClient Client { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ClientEventArgs"/>.
    /// </summary>
    /// <param name="client">The related client.</param>
    public ClientEventArgs(IClient client)
    {
        Client = client;
    }
}
