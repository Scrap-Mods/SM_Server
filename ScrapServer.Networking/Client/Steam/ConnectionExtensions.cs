using Steamworks;
using Steamworks.Data;

namespace ScrapServer.Networking.Client.Steam;

internal static class ConnectionExtensions
{
    public static Result SendMessage(this Connection connection, ReadOnlySpan<byte> data, SendType sendType = SendType.Reliable)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                return connection.SendMessage((nint)ptr, data.Length, sendType);
            }
        }
    }

    public static Result SendMessage(this Connection connection, ReadOnlyMemory<byte> data, SendType sendType = SendType.Reliable)
    {
        unsafe
        {
            using var handle = data.Pin();
            return connection.SendMessage((nint)handle.Pointer, data.Length, sendType);
        }
    }
}
