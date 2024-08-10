using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

//TODO (doc): elaborate on what these files are
/// <summary>
/// The packet sent by the client during the join sequence
/// containing the checksums of the files it has. 
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/file-checksums"/>
public struct FileChecksums : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.FileChecksums;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// The checksum array.
    /// </summary>
    public UInt32[]? Checksums;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileChecksums"/> struct.
    /// </summary>
    /// <param name="checksums">The checksum array.</param>
    public FileChecksums(UInt32[] checksums)
    {
        Checksums = checksums;
    }

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        if (Checksums == null)
        {
            writer.WriteUInt32(0);
            return;
        }
        writer.WriteUInt32((UInt32)Checksums.Length);
        foreach (uint checksum in Checksums)
        {
            writer.WriteUInt32(checksum);
        }
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        uint length = reader.ReadUInt32();
        if (length != 344) return;

        Checksums = new uint[length];
        for (int i = 0; i < length; i++)
        {
            Checksums[i] = reader.ReadUInt32();
        }
    }
}
