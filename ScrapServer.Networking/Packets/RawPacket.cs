using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct RawPacket : IBitSerializable
{
    public byte[] Data;

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteBytes(Data);
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadBytes(Data);
    }
}
