using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct Hello : IPacket
{
    public static PacketId PacketId => PacketId.Hello;
    public static bool IsCompressable => false;

    public readonly void Serialize(ref BitWriter writer) { }

    public readonly void Deserialize(ref BitReader reader) { }
}
