using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct FileChecksums : IPacket
{
    public static PacketId PacketId => PacketId.FileChecksums;
    public static bool IsCompressable => true;

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
        writer.WriteUInt32((UInt32)Checksums.Length);
        foreach (uint checksum in Checksums)
        {
            writer.WriteUInt32(checksum);
        }
    }

    public void Deserialize(ref BitReader reader)
    {
        uint length = reader.ReadUInt32();
        Checksums = new uint[length];
        for (int i = 0; i < length; i++)
        {
            Checksums[i] = reader.ReadUInt32();
        }
    }
}
