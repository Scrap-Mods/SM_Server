using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ClientAccepted : IPacket
{
    public static PacketType PacketId => PacketType.ClientAccepted;
    public static bool IsCompressable => false;

    public readonly void Serialize(ref BitWriter writer) { }

    public readonly void Deserialize(ref BitReader reader) { }
}
