using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;
using System.Text;

namespace ScrapServer.Networking;

/// <summary>
/// The packet sent by the client during the join sequence that contains 
/// the player's name and their character customization options.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/character-info"/>
public struct CharacterInfo : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.CharacterInfo;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// The players's name displayed in chat and above the character model.
    /// </summary>
    public string? Name;

    /// <summary>
    /// The players's character customization options.
    /// </summary>
    public CharacterCustomization Customization;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        if (Name != null)
        {
            var byteLen = Encoding.UTF8.GetByteCount(Name);
            if (byteLen > UInt16.MaxValue)
            {
                throw new ArgumentException($"Character name too long: {byteLen} bytes (max is {UInt16.MaxValue}).");
            }
            writer.WriteUInt16((UInt16)byteLen);
            writer.WriteString(Name);
        }
        else
        {
            writer.WriteUInt16(0);
        }
        writer.WriteObject(Customization);
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        var byteLen = reader.ReadUInt16();
        Name = reader.ReadString(byteLen);
        Customization = reader.ReadObject<CharacterCustomization>();
    }
}
