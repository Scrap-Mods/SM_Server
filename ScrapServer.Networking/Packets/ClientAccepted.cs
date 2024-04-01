using System.IO;

namespace ScrapServer.Networking.Packets;
public class ClientAccepted : IPacket
{
    public static byte PacketId { get => 5; }

    // Constructor
    public ClientAccepted()
    {
    }

    public void Serialize(BinaryWriter writer)
    {
    }

    public void Deserialize(BinaryReader reader)
    {
    }
}
