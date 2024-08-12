using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Data;

public struct ModData : IBitSerializable
{
    ulong FileId { get; set; }
    Guid UUID { get; set; }

    public readonly void Serialize(ref BitWriter writer)
    {
        using var comp = writer.WriteLZ4();
        comp.Writer.WriteUInt64(FileId);
        comp.Writer.WriteGuid(UUID);
    }

    public void Deserialize(ref BitReader reader)
    {
        using var decomp = reader.ReadLZ4(reader.BytesLeft);
        FileId = decomp.Reader.ReadUInt64();
        UUID = decomp.Reader.ReadGuid();
    }
}
