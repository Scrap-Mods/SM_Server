using ScrapServer.Networking.Packets;

namespace ScrapServer.Networking.Client;

/// <summary>
/// Represents a client connected to a <see cref="IServer"/> for sending and receiving packets.
/// </summary>
public interface IClient : IDisposable
{
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
    /// Registers a handler for packets of specified type coming from the client.
    /// </summary>
    /// <typeparam name="T">The type of handled packets.</typeparam>
    /// <param name="handler">The delegate to be called when a matching packet is received.</param>
    public void HandlePacket<T>(EventHandler<PacketEventArgs<T>> handler) where T : IPacket, new();

    /// <summary>
    /// Sends a packet to the client.
    /// </summary>
    /// <typeparam name="T">The type of the packet.</typeparam>
    /// <param name="packet">The packet to be sent.</param>
    public void SendPacket<T>(T packet) where T : IPacket;

    /// <summary>
    /// Disconnects the client from the server.
    /// </summary>
    public void Disconnect();

#pragma warning disable CA1816 // GC.SuppressFinalize is intended to be called in Disconnect.
    void IDisposable.Dispose() => Disconnect();
#pragma warning restore CA1816
}
