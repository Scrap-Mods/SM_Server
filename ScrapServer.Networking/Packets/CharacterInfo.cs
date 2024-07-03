using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using System.Text;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the client during the join sequence that contains 
/// the player's name and their character customization options.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/character-info"/>
public struct CharacterInfo : IBitSerializable
{
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
        writer.WriteByte((byte)PacketId.CharacterInfo);
        using var compWriter = writer.WriteLZ4().Writer;

        if (Name != null)
        {
            var byteLen = Encoding.UTF8.GetByteCount(Name);
            if (byteLen > UInt16.MaxValue)
            {
                throw new ArgumentException($"Character name too long: {byteLen} bytes (max is {UInt16.MaxValue}).");
            }
            compWriter.WriteUInt16((UInt16)byteLen);
            compWriter.WriteString(Name);
        }
        else
        {
            compWriter.WriteUInt16(0);
        }
        compWriter.WriteObject(Customization);
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
        var compReadeer = reader.ReadLZ4().Reader;

        var byteLen = compReadeer.ReadUInt16();
        Name = compReadeer.ReadString(byteLen);
        Customization = compReadeer.ReadObject<CharacterCustomization>();
    }
}
