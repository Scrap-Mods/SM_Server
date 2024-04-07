using ScrapServer.Utility;
using ScrapServer.Utility.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Networking.Serialization;

/// <summary>
/// Represents decompressed LZ4 packet data.
/// </summary>
public ref struct DecompressedData
{
    /// <summary>
    /// Gets the packet reader for reading the decompressed data.
    /// </summary>
    /// <value>Packet reader.</value>
    [UnscopedRef]
    public ref PacketReader Reader => ref reader;
    
    private readonly BorrowedBuffer buffer;

    private PacketReader reader;
    private bool disposed;

    /// <summary>
    /// Creates a new instance of <see cref="DecompressedData"/>.
    /// </summary>
    /// <remarks>
    /// If the provided buffer is too small for decompression, its length will be 
    /// doubled until it fits the data or <paramref name="maxTryCount"/> is reached.
    /// </remarks>
    /// <param name="bufferPool">The buffer pool used for borrowing temporary buffers.</param>
    /// <param name="compressedData">The buffer with compressed data.</param>
    /// <param name="decompressedLength">The expected length of data after decompression.</param>
    /// <param name="maxTryCount">Maximum number of buffer allocation attempts (see remarks).</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="decompressedLength"/> is smaller than 
    /// the actual length of decompressed data.
    /// </exception>
    public DecompressedData(
        IBufferPool bufferPool,
        scoped ReadOnlySpan<byte> compressedData,
        int decompressedLength,
        int maxTryCount = 1)
    {
        buffer = bufferPool.GetBuffer(decompressedLength);
        
        for (int i = 0; i < maxTryCount; i++)
        {
            if (LZ4.TryDecompress(compressedData, buffer, out int actualLength))
            {
                buffer.Resize(actualLength);
                reader = new PacketReader(buffer, bufferPool);
                return;
            }
            buffer.Resize(buffer.Length * 2);
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
            buffer.Dispose();
        }
    }
}
