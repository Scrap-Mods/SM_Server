using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct RawPacket : IBitSerializable
{
    public byte[] Data;
    public byte PacketId;

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte(PacketId);

        using var compWriter = writer.WriteLZ4();
        compWriter.Writer.WriteBytes(Data);
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();

        var compReader = reader.ReadLZ4().Reader;
        compReader.ReadBytes(Data);
    }
}
