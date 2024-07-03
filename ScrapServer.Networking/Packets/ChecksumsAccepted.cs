using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server in response to <see cref="FileChecksums"/> 
/// to indicate that all the checksums are valid.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/checksums-accepted"/>
public struct ChecksumsAccepted : IBitSerializable
{
    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) {
        writer.WriteByte((byte)PacketId.ChecksumsAccepted);

    }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) {
        reader.ReadByte();
    }
}
