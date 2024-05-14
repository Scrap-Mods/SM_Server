using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ChecksumsAccepted : IPacket
{
    public static PacketId PacketId => PacketId.ChecksumsAccepted;
    public static bool IsCompressable => false;

    public readonly void Serialize(ref BitWriter writer) { }

    public readonly void Deserialize(ref BitReader reader) { }
}
