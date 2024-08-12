using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the server in response to <see cref="FileChecksums"/> 
/// to indicate that a checksum is invalid.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/checksums-denied"/>
public struct ChecksumDenied : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ChecksumsDenied;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// The index of the invalid checksum.
    /// </summary>
    public UInt32 Index;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChecksumDenied"/> struct.
    /// </summary>
    /// <param name="index">The index of the invalid checksum.</param>
    public ChecksumDenied(UInt32 index)
    {
        Index = index;
    }

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Index);
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        Index = reader.ReadUInt32();
    }
}
