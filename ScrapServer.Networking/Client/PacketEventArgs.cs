using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking.Client;

/// <summary>
/// The arguments for an incoming client packet event (automatically parsed).
/// </summary>
public readonly struct PacketEventArgs<T> where T : IPacket
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
    /// Gets the packet data.
    /// </summary>
    /// <value>Packet object.</value>
    public T Packet { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RawPacketEventArgs"/>.
    /// </summary>
    /// <param name="client">The client that sent the packet.</param>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="packet">The packet data.</param>
    public PacketEventArgs(IClient client, PacketId packetId, T packet)
    {
        Client = client;
        PacketId = packetId;
        Packet = packet;
    }
}
