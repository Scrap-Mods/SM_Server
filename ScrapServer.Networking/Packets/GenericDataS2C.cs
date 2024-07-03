using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server to the client containing generic game data.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/generic-data-s2c"/>
public struct GenericDataS2C : IBitSerializable
{
    /// <summary>
    /// The current game tick.
    /// </summary>
    public UInt32 Tick;

    /// <summary>
    /// The generic game data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.GenericDataS2C);
        using var compWriter = writer.WriteLZ4();

        compWriter.Writer.WriteUInt32(Tick);
        if (Data == null)
        {
            compWriter.Writer.WriteUInt32(0);
            return;
        }
        foreach (var data in Data)
        {
            data.Serialize(ref compWriter.Writer);
        }
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
        using var compReader = reader.ReadLZ4(reader.BytesLeft);

        Tick = compReader.Reader.ReadUInt32();
        var dataList = new List<BlobData>();
        while (compReader.Reader.BytesLeft > 0)
        {
            dataList.Add(compReader.Reader.ReadObject<BlobData>());
        }
        Data = dataList.ToArray();
    }
}
