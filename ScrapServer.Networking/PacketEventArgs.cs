using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The arguments for an incoming client packet event.
/// </summary>
public readonly ref struct PacketEventArgs
{
    /// <summary>
    /// Gets the client that sent the packet.
    /// </summary>
    /// <value>The client.</value>
    public IClient Client { get; }

    /// <summary>
    /// Gets the packet data.
    /// </summary>
    /// <value>Packet object.</value>
    public ReadOnlySpan<byte> Packet { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RawPacketEventArgs"/>.
    /// </summary>
    /// <param name="client">The client that sent the packet.</param>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="packet">The packet data.</param>
    public PacketEventArgs(IClient client, ReadOnlySpan<byte> packet)
    {
        Client = client;
        Packet = packet;
    }

    public T Deserialize<T>() where T : IBitSerializable, new()
    {
        var t = new T();
        var reader = BitReader.WithSharedPool(Packet);

        t.Deserialize(ref reader);

        return t;
    }
}
