using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the client when it sends a Lua network request.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/script-data-s2c"/>
public struct ScriptDataS2C : IBitSerializable
{
    /// <summary>
    /// The current game tick.
    /// </summary>
    public UInt32 Tick;

    /// <summary>
    /// The script data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Tick);
        if (Data == null)
        {
            writer.WriteUInt32(0);
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
        Tick = reader.ReadUInt32();
        var dataList = new List<BlobData>();
        while (reader.BytesLeft > 0)
        {
            dataList.Add(reader.ReadObject<BlobData>());
        }
        Data = dataList.ToArray();
    }
}
