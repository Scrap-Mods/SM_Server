using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

public class GenericData : IBitSerializable
{
    Guid UUID { get; set; }
    byte[] Key { get; set; }

    public GenericData()
    {
        UUID = Guid.Empty;
        Key = Array.Empty<byte>();
    }

    public GenericData(Guid uuid, byte[] key)
    {
        UUID = uuid;
        Key = key;
    }

    public void Serialize(ref BitWriter writer)
    {
        using var comp = writer.WriteLZ4();
        comp.Writer.WriteGuid(UUID);
        comp.Writer.WriteUInt16((ushort)Key.Length);
        comp.Writer.WriteBytes(Key);
    }

    public void Deserialize(ref BitReader reader)
    {
        using var decomp = reader.ReadLZ4(reader.BytesLeft);
        UUID = decomp.Reader.ReadGuid();
        ushort keyLength = decomp.Reader.ReadUInt16();
        Key = new byte[keyLength];
        decomp.Reader.ReadBytes(Key);
    }
}
