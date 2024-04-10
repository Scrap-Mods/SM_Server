using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Utils;

internal static class BitReaderExtensions
{
    public static PacketType ReadPacketType(this ref BitReader reader)
    {
        return (PacketType)reader.ReadByte();
    }

    public static Gamemode ReadGamemode(this ref BitReader reader)
    {
        return (Gamemode)reader.ReadUInt32();
    }

    public static ServerFlags ReadServerFlags(this ref BitReader reader)
    {
        return (ServerFlags)reader.ReadByte();
    }
}
