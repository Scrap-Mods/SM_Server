using K4os.Compression.LZ4;

namespace ScrapServer.Utility;

public static class LZ4
{
    public static ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> uncompressedData)
    {
        int maximumOutputSize = LZ4Codec.MaximumOutputSize(uncompressedData.Length);
        var compressedBuffer = new byte[maximumOutputSize];

        int compressedLength = LZ4Codec.Encode(
            uncompressedData, compressedBuffer, LZ4Level.L00_FAST);

        return compressedBuffer.AsSpan(0, compressedLength);
    }

    public static ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> compressedData)
    {
        var decompressedData = new byte[0xA00000];
        int decompressedLength = LZ4Codec.Decode(compressedData, decompressedData);

        return decompressedData.AsSpan(0, decompressedLength);
    }
}
