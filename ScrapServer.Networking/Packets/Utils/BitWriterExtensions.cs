using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

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

    public static void WriteVector3XYZ(this ref BitWriter writer, Vector3 vector)
    {
        writer.WriteSingle(vector.X);
        writer.WriteSingle(vector.Y);
        writer.WriteSingle(vector.Z);
    }
}
