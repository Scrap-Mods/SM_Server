using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server at the beginning of the join
/// sequence to indicate that the client is authorized to join.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/client-accepted"/>
public struct ClientAccepted : IBitSerializable
{
    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) {
        writer.WriteByte((byte)PacketId.ClientAccepted);
    }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) {
        reader.ReadByte();

    }
}
