using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct Hello : IPacket
{
    public static PacketType PacketId => PacketType.Hello;

    public readonly void Serialize(ref BitWriter writer) 
    {
        writer.WritePacketType(PacketId);
    }

    public readonly void Deserialize(ref BitReader reader) 
    {
        reader.ReadPacketType();
    }
}
