using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server when it sends a Lua network request or updates client data.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/script-data-c2s"/>
public struct ScriptDataC2S : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ScriptDataC2S;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// The script data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        if (Data == null)
        {
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
        var dataList = new List<BlobData>();
        while (reader.BytesLeft > 0)
        {
            dataList.Add(reader.ReadObject<BlobData>());
        }
        Data = dataList.ToArray();
    }
}
