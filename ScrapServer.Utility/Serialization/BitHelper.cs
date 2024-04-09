namespace ScrapServer.Utility.Serialization;

internal static class BitHelper
{
    public static void ApplyByteOrder(Span<byte> bytes, ByteOrder byteOrder)
    {
        if (BitConverter.IsLittleEndian != (byteOrder != ByteOrder.BigEndian))
        {
            bytes.Reverse();
        }
    }

    public static int Bit(int index)
    {
        return 0b10000000 >>> index;
    }
}
