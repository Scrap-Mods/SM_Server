using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

/// <summary>
/// A character's customization options including gender and equipped items.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/structures/docs/character-customization/"/>
public struct CharacterCustomization : IBitSerializable
{
    /// <summary>
    /// The character's gender.
    /// </summary>
    public Gender Gender;

    /// <summary>
    /// The character's equipped items.
    /// </summary>
    /// <remarks>
    /// The type of the item is determined <see href="https://docs.scrapmods.io/docs/structures/docs/character-customization/#customization-options">by its index in the array</see>.
    /// </remarks>
    public CharacterItem[]? Items;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        // The purpose of this value is unknown (version maybe?)
        writer.WriteUInt32(2);
        writer.WriteByte((byte)Gender);
        if (Items == null)
        {
            writer.WriteByte(0);
            return;
        }
        if (Items.Length > byte.MaxValue)
        {
            throw new InvalidOperationException($"Too many character items: {Items.Length} (max is {byte.MaxValue}).");
        }
        writer.WriteByte((byte)Items.Length);
        foreach (var item in Items)
        {
            writer.WriteGuid(item.VariantId);
        }
        foreach (var item in Items)
        {
            writer.WriteUInt32(item.PaletteIndex);
        }
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        reader.ReadUInt32();
        Gender = (Gender)reader.ReadByte();
        int itemCount = reader.ReadByte();
        Items = new CharacterItem[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            // Arrays return elements by reference
            Items[i].VariantId = reader.ReadGuid();
        }
        for (int i = 0; i < itemCount; i++)
        {
            // ditto
            Items[i].PaletteIndex = reader.ReadUInt32();
        }
    }
}
