using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct GenericDataS2C : IPacket
{
    public static PacketId PacketId => PacketId.GenericDataS2C;
    public static bool IsCompressable => false;

    public UInt32 Tick;
    public BlobData Data;

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Tick);
        Data.Serialize(ref writer);
    }
    public void Deserialize(ref BitReader reader)
    {
        Tick = reader.ReadUInt32();
        Data.Deserialize(ref reader);
    }
}
