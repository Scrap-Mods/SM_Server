namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Represents an object that can be serialized to a bit stream.
/// </summary>
public interface IBitSerializable
{
    /// <summary>
    /// Deserializes the object.
    /// </summary>
    /// <param name="reader">The reader for reading the object data.</param>
    public void Deserialize(ref BitReader reader);

    /// <summary>
    /// Serializes the object.
    /// </summary>
    /// <param name="writer">The writer for writing the object data.</param>
    public void Serialize(ref BitWriter writer);
}
