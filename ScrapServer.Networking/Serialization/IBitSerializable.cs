namespace ScrapServer.Networking.Serialization;

/// <summary>
/// Represents an object that can be serialized to a bit stream.
/// </summary>
public interface IBitSerializable
{
    /// <summary>
    /// Deserializes the object.
    /// </summary>
    /// <param name="packetReader">The reader for reading the object data.</param>
    public void Deserialize(ref BitReader packetReader);

    /// <summary>
    /// Serializes the object.
    /// </summary>
    /// <param name="packetWriter">The writer for writing the object data.</param>
    public void Serialize(ref BitWriter packetWriter);
}
