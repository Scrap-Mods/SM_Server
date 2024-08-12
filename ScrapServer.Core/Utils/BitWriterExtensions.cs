using ScrapServer.Networking;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Core.Utils;

internal static class BitWriterExtensions
{
  public static void WritePacket<T>(this ref BitWriter writer, T packet) where T : IPacket, new()
    {
        writer.WriteByte((byte)T.PacketId);

        if (T.IsCompressable)
        {
            using var compWriter = writer.WriteLZ4();
            compWriter.Writer.WriteObject(packet);
        }
        else
        {
            writer.WriteObject(packet);
        }
    }
}