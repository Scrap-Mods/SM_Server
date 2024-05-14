using ScrapServer.Networking.Packets;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Client;

/// <summary>
/// Extension methods for subscribing to client packets and sending packets with automatic serialization and parsing.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Sends a packet to the client.
    /// </summary>
    /// <typeparam name="T">The type of the packet.</typeparam>
    /// <param name="client">The client to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    public static void SendPacket<T>(this IClient client, T packet) where T : IPacket
    {
        var writer = BitWriter.WithSharedPool();
        try
        {
            if (T.IsCompressable)
            {
                using var comp = writer.WriteLZ4();
                comp.Writer.WriteObject(packet);
            }
            else
            {
                writer.WriteObject(packet);
            }
            client.SendRawPacket(T.PacketId, writer.Data);
        }
        finally
        {
            writer.Dispose();
        }
    }

    /// <summary>
    /// Registers a handler for packets of specified type coming from the client.
    /// </summary>
    /// <typeparam name="T">The type of packets handled by <paramref name="handler"/>.</typeparam>
    /// <param name="client">The client to handle incoming packets from.</param>
    /// <param name="handler">The delegate to be called when a matching packet is received.</param>
    public static void HandlePacket<T>(this IClient client, EventHandler<PacketEventArgs<T>> handler) where T : IPacket, new()
    {
        client.HandleRawPacket(T.PacketId, (o, args) =>
        {
            var packet = new T();
            var reader = BitReader.WithSharedPool(args.Data);
            if (T.IsCompressable)
            {
                using var decomp = reader.ReadLZ4();
                packet.Deserialize(ref decomp.Reader);
            }
            else
            {
                packet.Deserialize(ref reader);
            }
            handler(o, new PacketEventArgs<T>(args.Client, args.PacketId, packet));
        });
    }
}
