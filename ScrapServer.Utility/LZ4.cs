using K4os.Compression.LZ4;

namespace ScrapServer.Utility;

public static class LZ4
{
    public static byte[] Compress(ReadOnlySpan<byte> uncompressedData, out int compressedLength)
    {
        int maximumOutputSize = LZ4Codec.MaximumOutputSize(uncompressedData.Length);
        var compressedBuffer = new byte[maximumOutputSize];

        compressedLength = LZ4Codec.Encode(
            uncompressedData, compressedBuffer, LZ4Level.L00_FAST);

        return compressedBuffer;
    }

    public static byte[] CompressPacket(ReadOnlySpan<byte> uncompressedData, byte packetId, out int compressedLength)
    {
        int maximumOutputSize = LZ4Codec.MaximumOutputSize(uncompressedData.Length);
        var compressedBuffer = new byte[maximumOutputSize + 1];
        
        compressedBuffer[0] = packetId;
        compressedLength = 1 + LZ4Codec.Encode(
            uncompressedData, compressedBuffer.AsSpan(1), LZ4Level.L00_FAST);

        return compressedBuffer;
    }

    public static byte[] Decompress(ReadOnlySpan<byte> compressedData, out int decompressedLength)
    {
        var decompressedData = new byte[0xA00000];
        decompressedLength = LZ4Codec.Decode(compressedData, decompressedData);
        return decompressedData;
    }
}
