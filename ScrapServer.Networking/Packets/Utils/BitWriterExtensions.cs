using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Utils;

internal static class BitWriterExtensions
{
    public static void WriteGamemode(this ref BitWriter writer, Gamemode gamemode)
    {
        writer.WriteUInt32((uint)gamemode);
    }

    public static void WriteServerFlags(this ref BitWriter writer, ServerFlags flags)
    {
        writer.WriteByte((byte)flags);
    }
}
