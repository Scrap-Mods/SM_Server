using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

public struct ModData : IBitSerializable
{
    ulong FileId { get; set; }
    Guid UUID { get; set; }

    public ModData()
    {
        FileId = 0;
        UUID = Guid.Empty;
    }

    public ModData(ulong fileId, Guid uuid)
    {
        FileId = fileId;
        UUID = uuid;
    }

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
