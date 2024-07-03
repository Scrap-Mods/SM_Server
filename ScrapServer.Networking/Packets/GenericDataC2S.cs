using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using System.Runtime.InteropServices;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the client to the server containing generic game data.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/generic-data-c2s"/>
public struct GenericDataC2S : IBitSerializable
{
    /// <summary>
    /// The generic game data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.GenericDataC2S);
        using var compWriter = writer.WriteLZ4();
        if (Data == null)
        {
            compWriter.Writer.WriteUInt32(0);
            return;
        }
        foreach (var data in Data)
        {
            data.Serialize(ref writer);
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
