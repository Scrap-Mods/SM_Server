namespace ScrapServer.Networking;

/// <summary>
/// Represents the state of an <see cref="IClient"/>.
/// </summary>
public enum ClientState
{
    /// <summary>
    /// The client is disconnected after previously being connected.
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// The client is requesting a connection.
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// The client is connected.
    /// </summary>
    Connected = 2
}
