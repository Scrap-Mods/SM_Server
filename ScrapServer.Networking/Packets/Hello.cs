using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the client after <see cref="ClientAccepted"/> to confirm the connection.
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/hello"/>
public struct Hello : IBitSerializable
{
    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.Hello);
    }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
    }
}
