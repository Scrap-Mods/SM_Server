namespace ScrapServer.Networking.Packets.Data;

/// <summary>
/// A character's equipped item (e.g. face, hair, torso, pants).
/// </summary>
public struct CharacterItem
{
    /// <summary>
    /// The uuid of the selected item variant.
    /// </summary>
    public Guid VariantId;

    /// <summary>
    /// The index of the selected item palette.
    /// </summary>
    public UInt32 PaletteIndex;
}
