namespace ScrapServer.Utility;

public static class MemoryStreamExtensions
{
    public static Span<byte> AsSpan(this MemoryStream stream)
    {
        return stream.GetBuffer().AsSpan(0, (int)stream.Length);
    }
    public static Memory<byte> AsMemory(this MemoryStream stream)
    {
        return stream.GetBuffer().AsMemory(0, (int)stream.Length);
    }
}
