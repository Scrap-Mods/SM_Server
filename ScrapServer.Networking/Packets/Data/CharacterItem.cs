using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

/// <summary>
/// A character's equipped item (e.g. face, hair, torso, pants).
/// </summary>
public struct CharacterItem : IBitSerializable
{
    /// <summary>
    /// The uuid of the selected item variant.
    /// </summary>
    public Guid VariantId;

    /// <summary>
    /// The index of the selected item palette.
    /// </summary>
    public UInt32 PaletteIndex;

    public void Deserialize(ref BitReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ref BitWriter writer)
    {
        throw new NotImplementedException();
    }
}
