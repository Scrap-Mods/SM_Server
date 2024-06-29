using K4os.Compression.LZ4;

namespace ScrapServer.Utility;

public static class LZ4
{
    public static int MaxCompressedSize(int uncompressedSize)
    {
        return LZ4Codec.MaximumOutputSize(uncompressedSize);
    }

    public static bool TryDecompress(ReadOnlySpan<byte> compressedData, Span<byte> decompressedData, out int decompressedLength)
    {
        decompressedLength = LZ4Codec.Decode(compressedData, decompressedData);
        if (decompressedLength < 0)
        {
            decompressedLength = 0;
            return false;
        }
        return true;
    }

    public static bool TryCompress(ReadOnlySpan<byte> uncompressedData, Span<byte> compressedData, out int compressedLength)
    {
        compressedLength = LZ4Codec.Encode(uncompressedData, compressedData, LZ4Level.L00_FAST);
        if (compressedLength < 0)
        {
            compressedLength = 0;
            return false;
        }
        return true;
    }
}
