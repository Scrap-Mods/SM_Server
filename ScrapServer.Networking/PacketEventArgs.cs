using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The arguments for an incoming client packet event.
/// </summary>
public readonly ref struct PacketEventArgs<T> where T : IBitSerializable
{
    /// <summary>
    /// Gets the client that sent the packet.
    /// </summary>
    /// <value>The client.</value>
    readonly public IClient Client { get; }

    /// <summary>
    /// Gets the packet id.
    /// </summary>
    /// <value>The id.</value>
   readonly public PacketId PacketId { get; }   

    /// <summary>
    /// Gets the packet data.
    /// </summary>
    /// <value>Packet object.</value>
    readonly public T Data { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PacketEventArgs"/>.
    /// </summary>
    /// <param name="client">The client that sent the packet.</param>
    /// <param name="data">The packet data.</param>
    public PacketEventArgs(IClient client, PacketId id, T data)
    {
        Client = client;
        PacketId = id;
        Data = data;
    }
}
