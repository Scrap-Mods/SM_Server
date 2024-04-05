namespace ScrapServer.Networking.Serialization;

/// <summary>
/// Represents a byte order.
/// </summary>
public enum ByteOrder
{
    /// <summary>
    /// Big endian byte order (MSB first, LSB last).
    /// </summary>
    BigEndian = 0,

    /// <summary>
    /// Little endian byte order (LSB first, MSB last).
    /// </summary>
    LittleEndian = 1
}
