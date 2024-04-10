using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ChecksumDenied : IPacket
{
    public static PacketType PacketId => PacketType.ChecksumsDenied;

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
        writer.WritePacketType(PacketId);
        using var comp = writer.WriteLZ4();
        comp.Writer.WriteUInt32(Index);
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadPacketType();
        using var comp = reader.ReadLZ4(reader.BytesLeft);
        Index = comp.Reader.ReadUInt32();
    }
}
