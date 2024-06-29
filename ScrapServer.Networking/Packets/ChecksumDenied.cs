using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ChecksumDenied : IPacket
{
    public static PacketId PacketId => PacketId.ChecksumsDenied;
    public static bool IsCompressable => true;

    public UInt32 Index { get; set; }

    public ChecksumDenied()
    {
        Index = 0;
    }

    public ChecksumDenied(UInt32 index)
    {
        Index = index;
    }

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Index);
    }

    public void Deserialize(ref BitReader reader)
    {
        Index = reader.ReadUInt32();
    }
}
