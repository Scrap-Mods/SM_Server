using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// Generic packet to be used by clients to broadcast data to other clients, used by QM's proximity voice addon.
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/hello"/>
public struct Broadcast : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.Broadcast;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) { }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) { }
}
