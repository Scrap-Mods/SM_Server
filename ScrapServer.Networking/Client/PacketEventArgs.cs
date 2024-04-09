using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking.Client;

/// <summary>
/// The arguments for an incoming client packet event.
/// </summary>
/// <typeparam name="T">The type of the packet.</typeparam>
public struct PacketEventArgs<T> where T : IPacket
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
    public PacketType PacketId { get; }

    /// <summary>
    /// Gets the packet data.
    /// </summary>
    /// <value>The packet.</value>
    public T Packet { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PacketEventArgs{T}"/>.
    /// </summary>
    /// <param name="client">The client that sent the packet.</param>
    /// <param name="packetId">The id of the packet.</param>
    /// <param name="packet">The packet data.</param>
    public PacketEventArgs(IClient client, PacketType packetId, T packet)
    {
        Client = client;
        PacketId = packetId;
        Packet = packet;
    }
}
