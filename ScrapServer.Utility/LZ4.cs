using K4os.Compression.LZ4;

namespace ScrapServer.Utility;

public static class LZ4
{
    public static byte[] Compress(byte[] uncompressedData)
    {
        int maximumOutputSize = LZ4Codec.MaximumOutputSize(uncompressedData.Length);
        Span<byte> compressedDataSpan = new byte[maximumOutputSize];

        int compressedDataSize = LZ4Codec.Encode(
            uncompressedData, compressedDataSpan, LZ4Level.L00_FAST);

        return compressedDataSpan.Slice(0, compressedDataSize).ToArray();
    }

    public static byte[] CompressPacket(byte[] uncompressedData, byte packetId)
    {
        Span<byte> compressedData = Compress(uncompressedData);
        byte[] packetData = new byte[compressedData.Length + 1];

        packetData[0] = packetId;

        compressedData.CopyTo(packetData.AsSpan(1));
        return packetData;
    }

    public static byte[] Decompress(byte[] compressedData)
    {
        Span<byte> uncompressedDataSpan = new byte[0xA00000];
        int uncompressedDataSize = LZ4Codec.Decode(
                           compressedData, uncompressedDataSpan);
        return uncompressedDataSpan.Slice(0, uncompressedDataSize).ToArray();
    }

}
