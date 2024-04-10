using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct FileChecksums : IPacket
{
    public static PacketType PacketId => PacketType.FileChecksums;
    public UInt32[] Checksums { get; set; }

    public FileChecksums()
    {
        Checksums = Array.Empty<UInt32>();
    }

    public FileChecksums(UInt32[] checksums)
    {
        Checksums = checksums;
    }

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WritePacketType(PacketId);
        using var comp = writer.WriteLZ4();
        comp.Writer.WriteUInt32((UInt32)Checksums.Length);
        foreach (uint checksum in Checksums)
        {
            comp.Writer.WriteUInt32(checksum);
        }
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadPacketType();
        using var decomp = reader.ReadLZ4();
        uint length = decomp.Reader.ReadUInt32();
        Checksums = new uint[length];
        for (int i = 0; i < length; i++)
        {
            Checksums[i] = decomp.Reader.ReadUInt32();
        }
    }
}
