using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the client after <see cref="ClientAccepted"/> to confirm the connection.
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/hello"/>
public struct Hello : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.Hello;

    /// <inheritdoc/>
    public static bool IsCompressable => false;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) { }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) { }
}
