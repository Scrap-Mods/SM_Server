using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the server in response to <see cref="FileChecksums"/> 
/// to indicate that all the checksums are valid.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/checksums-accepted"/>
public struct ChecksumsAccepted : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ChecksumsAccepted;

    /// <inheritdoc/>
    public static bool IsCompressable => false;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) { }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) { }
}
