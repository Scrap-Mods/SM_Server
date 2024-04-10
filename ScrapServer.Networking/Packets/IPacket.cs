using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public interface IPacket : IBitSerializable
{
    public virtual static PacketType PacketId => PacketType.Empty;
    public virtual static bool IsCompressable => false;

    internal string PacketName => GetType().Name;
}
