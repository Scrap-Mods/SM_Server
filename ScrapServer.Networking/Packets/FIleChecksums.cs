using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public class FileChecksums : IPacket
{
    public static PacketType PacketId => PacketType.FileChecksums;
    public uint[] Checksums { get; set; }

    public FileChecksums()
    {
        Checksums = Array.Empty<uint>();
    }

    public FileChecksums(uint[] checksums)
    {
        Checksums = checksums;
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WritePacketType(PacketId);
        using var comp = writer.WriteLZ4();
        comp.Writer.WriteUInt32((uint)Checksums.Length);
        foreach (var checksum in Checksums)
        {
            comp.Writer.WriteUInt32(checksum);
        }
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadPacketType();
        using var decomp = reader.ReadLZ4(reader.BytesLeft);
        uint length = decomp.Reader.ReadUInt32();
        Checksums = new uint[length];
        for (int i = 0; i < length; i++)
        {
            Checksums[i] = decomp.Reader.ReadUInt32();
        }
    }
}
