using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct Compressed<T> : IBitSerializable where T : IBitSerializable, new()
{
    public T Packet;

    public void Deserialize(ref BitReader reader)
    {
        using var compReader = reader.ReadLZ4();
        Packet.Deserialize(ref compReader.Reader);
    }

    public void Serialize(ref BitWriter writer)
    {
        using var compWriter = writer.WriteLZ4();
        Packet.Serialize(ref compWriter.Writer);
    }

    public Compressed(T packet) { this.Packet = packet; }
}
