using ScrapServer.Utility;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Represents decompressed LZ4 data.
/// </summary>
public ref struct DecompressedData
{
    /// <summary>
    /// Gets the reader for reading the decompressed data.
    /// </summary>
    /// <value>The reader.</value>
    [UnscopedRef]
    public ref BitReader Reader => ref reader;

    private BitReader reader;
    private bool disposed;

    private readonly ArrayPool<byte> arrayPool;
    private readonly byte[] buffer;

    /// <summary>
    /// Creates a new instance of <see cref="DecompressedData"/>.
    /// </summary>
    /// <remarks>
    /// If the provided buffer is too small for decompression, its length will be 
    /// doubled until it fits the data or <paramref name="maxTryCount"/> is reached.
    /// </remarks>
    /// <param name="arrayPool">The array pool for renting temporary buffers.</param>
    /// <param name="compressedData">The buffer with compressed data.</param>
    /// <param name="decompressedLength">The expected length of data after decompression.</param>
    /// <param name="maxTryCount">Maximum number of buffer allocation attempts (see remarks).</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="decompressedLength"/> is smaller than 
    /// the actual length of decompressed data.
    /// </exception>
    public DecompressedData(
        scoped ReadOnlySpan<byte> compressedData,
        ArrayPool<byte> arrayPool,
        int decompressedLength,
        int maxTryCount = 1)
    {
        this.arrayPool = arrayPool;
        buffer = arrayPool.Rent(decompressedLength);

        for (int i = 0; i < maxTryCount; i++)
        {
            if (LZ4.TryDecompress(compressedData, buffer, out int actualLength))
            {
                reader = new BitReader(buffer.AsSpan(0, actualLength), arrayPool);
                return;
            }
            arrayPool.Return(buffer);
            buffer = arrayPool.Rent(buffer.Length * 2);
        }
        throw Exceptions.BufferTooSmall;
    }

    /// <summary>
    /// Releases the temporary buffer used for decompression.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            arrayPool.Return(buffer);
        }
    }
}
