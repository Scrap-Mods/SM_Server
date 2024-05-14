using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking.Client;

/// <summary>
/// The arguments for an incoming client packet event.
/// </summary>
public readonly ref struct RawPacketEventArgs
{
    /// <summary>
    /// Gets the client that sent the packet.
    /// </summary>
    /// <value>The client.</value>
    public IClient Client { get; }

    /// <summary>
    /// Gets the id of the packet.
    /// </summary>
    /// <value>Packet id.</value>
    public PacketId PacketId { get; }

    /// <summary>
    /// Gets the data of the packet excluding the packet id.
    /// </summary>
    /// <remarks>The data is as received from the client; it is NOT decompressed.</remarks>
    /// <value>Packet data.</value>
    public ReadOnlySpan<byte> Data { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RawPacketEventArgs"/>.
    /// </summary>
    /// <param name="client">The client that sent the packet.</param>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="data">The packet data.</param>
    public RawPacketEventArgs(IClient client, PacketId packetId, ReadOnlySpan<byte> data)
    {
        Client = client;
        PacketId = packetId;
        Data = data;
    }
}
