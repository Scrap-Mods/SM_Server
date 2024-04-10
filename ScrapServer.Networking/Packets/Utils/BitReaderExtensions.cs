using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Utils;

internal static class BitReaderExtensions
{
    public static Gamemode ReadGamemode(this ref BitReader reader)
    {
        return (Gamemode)reader.ReadUInt32();
    }

    public static ServerFlags ReadServerFlags(this ref BitReader reader)
    {
        return (ServerFlags)reader.ReadByte();
    }

    public static T ReadPacket<T>(this ref BitReader reader) where T : IPacket, new()
    {
        reader.ReadByte();
        if (T.IsCompressable)
        {
            using var decomp = reader.ReadLZ4();
            return decomp.Reader.ReadObject<T>();
        }
        else
        {
            return reader.ReadObject<T>();
        }
    }
}
