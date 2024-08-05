using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct InitNetworkUpdate : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.InitNetworkUpdate;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public UInt32 GameTick;
    public byte[] Updates;

    public void Deserialize(ref BitReader reader)
    {
        GameTick = reader.ReadUInt32();
        Updates = new byte[reader.BytesLeft];
        reader.ReadBytes(Updates);
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(GameTick);
        writer.WriteBytes(Updates);
    }
}
