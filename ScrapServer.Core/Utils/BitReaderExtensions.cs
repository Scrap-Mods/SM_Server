using ScrapServer.Networking;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Core.Utils;

internal static class BitReaderExtensions
{
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