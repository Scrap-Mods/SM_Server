using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server during the join sequence
/// to indicate that the client has passed all the checks.
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/join-confirmation"/>
public struct JoinConfirmation : IBitSerializable
{
    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.JoinConfirmation);
    }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
    }
}
