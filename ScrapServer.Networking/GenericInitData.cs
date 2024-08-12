using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the server to the client containing generic game data.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/generic-initialization-data"/>
public struct GenericInitData : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.GenericInitData;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// The current game tick.
    /// </summary>
    public UInt32 GameTick;

    /// <summary>
    /// The generic game data.
    /// </summary>
    public BlobData[]? Data;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(GameTick);
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
        GameTick = reader.ReadUInt32();
        var dataList = new List<BlobData>();
        while (reader.BytesLeft > 0)
        {
            dataList.Add(reader.ReadObject<BlobData>());
        }
        Data = dataList.ToArray();
    }
}
