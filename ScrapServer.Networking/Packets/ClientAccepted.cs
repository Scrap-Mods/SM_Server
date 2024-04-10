using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ClientAccepted : IPacket
{
    public static PacketType PacketId => PacketType.ClientAccepted;

    public readonly void Serialize(ref BitWriter writer) 
    {
        writer.WritePacketType(PacketId);
    }

    public readonly void Deserialize(ref BitReader reader) 
    {
        reader.ReadPacketType();
    }
}
