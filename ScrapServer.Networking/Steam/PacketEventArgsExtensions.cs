using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Steam;

public static class PacketEventArgsExtensions
{
    /// <summary>
    /// Takes attached data and deserializes it into a <c>IBitSerializable</c> datatype
    /// </summary>
    public static T DeserializePlain<T>(this PacketEventArgs args) where T : IBitSerializable, new()
    {
        var packet = new T();
        var reader = BitReader.WithSharedPool(args.Packet);

        packet.Deserialize(ref reader);

        return packet;
    }
    public static T Deserialize<T>(this PacketEventArgs args) where T : IBitSerializable, new()
    {
        var packet = new T();
        var reader = BitReader.WithSharedPool(args.Packet);

        var compReader = reader.ReadLZ4();
        packet.Deserialize(ref compReader.Reader);
        compReader.Dispose();

        return packet;
    }
}