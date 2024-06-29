namespace ScrapServer.Utility.Serialization;

internal static class BitHelper
{
    public const int MSB = 0b10000000;
    public const int LSB = 0b00000001;

    public static void ApplyByteOrder(Span<byte> bytes, ByteOrder byteOrder)
    {
        if (BitConverter.IsLittleEndian != (byteOrder != ByteOrder.BigEndian))
        {
            bytes.Reverse();
        }
    }

    public static int Bit(int index)
    {
        return MSB >>> index;
    }

    public static int LeftBitMask(int bitCount)
    {
        int result = 0;
        for (int i = 0; i < bitCount; i++)
        {
            result = (result >>> 1) | MSB;
        }
        return result;
    }
}
