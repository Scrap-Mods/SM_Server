using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server when it sends a Lua network request or updates client data.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/script-data-c2s"/>
public struct ScriptDataC2S : IBitSerializable
{
    /// <summary>
    /// The script data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.ScriptDataC2S);
        using var compWriter = writer.WriteLZ4();

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

        var dataList = new List<BlobData>();
        while (compReader.Reader.BytesLeft > 0)
        {
            dataList.Add(compReader.Reader.ReadObject<BlobData>());
        }
        Data = dataList.ToArray();
    }
}
