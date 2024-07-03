using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

//TODO (doc): elaborate on what these files are
/// <summary>
/// The packet sent by the client during the join sequence
/// containing the checksums of the files it has. 
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/file-checksums"/>
public struct FileChecksums : IBitSerializable
{
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
        writer.WriteByte((byte)PacketId.FileChecksums);
        using var compWriter = writer.WriteLZ4().Writer;
        
        if (Checksums == null)
        {
            compWriter.WriteUInt32(0);
            return;
        }

        compWriter.WriteUInt32((UInt32)Checksums.Length);
        
        foreach (uint checksum in Checksums)
        {
            compWriter.WriteUInt32(checksum);
        }
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
        using var compReader = reader.ReadLZ4(reader.BytesLeft);
        
        uint length = compReader.Reader.ReadUInt32();
        Checksums = new uint[length];

        for (int i = 0; i < length; i++)
        {
            Checksums[i] = compReader.Reader.ReadUInt32();
        }
    }
}
