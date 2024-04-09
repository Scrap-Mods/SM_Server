namespace ScrapServer.Utility.Serialization;

internal static class Exceptions
{
    public static InvalidOperationException NotEnoughtData =>
        new InvalidOperationException("Not enough data to fill the buffer.");

    public static ArgumentException BufferTooSmall =>
        new ArgumentException("Output buffer too small.");
}
