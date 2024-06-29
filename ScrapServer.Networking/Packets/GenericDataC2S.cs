using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct GenericDataC2S : IPacket
{
    public static PacketId PacketId => PacketId.GenericDataC2S;
    public static bool IsCompressable => false;

    public BlobData Data;

    public readonly void Serialize(ref BitWriter writer)
    {
        Data.Serialize(ref writer);
    }
    public readonly void Deserialize(ref BitReader reader)
    {
        Data.Deserialize(ref reader);
    }
}
