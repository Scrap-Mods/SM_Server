using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the server at the beginning of the join
/// sequence to indicate that the client is authorized to join.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/client-accepted"/>
public struct ClientAccepted : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ClientAccepted;

    /// <inheritdoc/>
    public static bool IsCompressable => false;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) { }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) { }
}
