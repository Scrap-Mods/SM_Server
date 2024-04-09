namespace ScrapServer.Networking.Serialization;

/// <summary>
/// Represents an object that can be serialized to a bit stream.
/// </summary>
public interface INetworkObject
{
    /// <summary>
    /// Deserializes the object.
    /// </summary>
    /// <param name="packetReader">The reader for reading the object data.</param>
    public void Deserialize(ref PacketReader packetReader);

    /// <summary>
    /// Serializes the object.
    /// </summary>
    /// <param name="packetWriter">The writer for writing the object data.</param>
    public void Serialize(ref PacketWriter packetWriter);
}
