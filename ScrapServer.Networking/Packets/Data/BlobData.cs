using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

public struct BlobData : IBitSerializable
{
    public Guid UUID;
    public byte[]? Key;
    public UInt16 WorldID;
    public byte Flags;
    public byte[]? Data;

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteGuid(UUID);

        if (Key == null)
        {
            writer.WriteUInt16(0);
        }
        else
        {
            writer.WriteUInt16((ushort)Key.Length);
            writer.WriteBytes(Key);
        }

        writer.WriteUInt16(WorldID);
        writer.WriteByte(Flags);

        if (Data == null)
        {
            writer.WriteUInt32(1);
            writer.WriteByte(0);
        }
        else
        {
            using var comp = writer.WriteLZ4(true);
            comp.Writer.WriteBytes(Data);
        }
    }

    public void Deserialize(ref BitReader reader)
    {
        UUID = reader.ReadGuid();

        ushort keyLength = reader.ReadUInt16();
        Key = new byte[keyLength];
        reader.ReadBytes(Key);

        WorldID = reader.ReadUInt16();
        Flags = reader.ReadByte();

        var compressedLength = reader.ReadInt32();
        using var decomp = reader.ReadLZ4(compressedLength);
        Data = new byte[decomp.DecompressedLength]; 
        decomp.Reader.ReadBytes(Data);
    }
}
