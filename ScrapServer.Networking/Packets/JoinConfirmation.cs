using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct JoinConfirmation : IPacket
{
    public static PacketType PacketId => PacketType.JoinConfirmation;
    public static bool IsCompressable => false;

    public readonly void Serialize(ref BitWriter writer) { }
    public readonly void Deserialize(ref BitReader reader) { }
}
