using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct NullPacket : IBitSerializable
{
    public void Deserialize(ref BitReader reader)
    {
    }

    public void Serialize(ref BitWriter writer)
    {
    }
}
