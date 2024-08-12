using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

public struct UnreliableUpdate : IPacket
{
    public static PacketId PacketId => PacketId.UnreliableUpdate;
    public static bool IsCompressable => true;

    public UInt32 ServerTick;
    public UInt32 CurrentTick;
    public byte[] Updates;

    public void Deserialize(ref BitReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(ServerTick);
        writer.WriteUInt32(CurrentTick);
        writer.WriteBytes(Updates);
    }
}
