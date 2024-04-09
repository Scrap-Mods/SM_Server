using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public class ClientAccepted : IPacket
{
    public static PacketType PacketId => PacketType.ClientAccepted;

    public void Serialize(ref BitWriter writer) 
    {
        writer.WritePacketType(PacketId);
    }

    public void Deserialize(ref BitReader reader) { }
}
