using System.Buffers;

namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Convenience methods for reading and writing <see cref="IBitSerializable"/> to byte arrays.
/// </summary>
public static class BitSerializable
{
    /// <summary>
    /// Converts a serializable object to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to convert to an array.</param>
    /// <returns>The resulting byte array.</returns>
    public static byte[] ToByteArray<T>(this T obj) where T : IBitSerializable
    {
        using var writer = BitWriter.WithSharedPool();
        writer.WriteObject(obj);
        return writer.Data.ToArray();
    }

    /// <summary>
    /// Converts a byte array to a serializable object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="data">The byte array.</param>
    /// <returns>The resulting object.</returns>
    public static T FromByteArray<T>(byte[] data) where T : IBitSerializable, new()
    {
        var reader = new BitReader(data, ArrayPool<byte>.Shared);
        return reader.ReadObject<T>();
    }
}
