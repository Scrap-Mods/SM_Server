using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server during the join sequence
/// to indicate that the client has passed all the checks.
/// </summary>
/// <seealso cref="https://docs.scrapmods.io/docs/networking/packets/join-confirmation"/>
public struct JoinConfirmation : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.JoinConfirmation;

    /// <inheritdoc/>
    public static bool IsCompressable => false;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer) { }

    /// <inheritdoc/>
    public readonly void Deserialize(ref BitReader reader) { }
}
