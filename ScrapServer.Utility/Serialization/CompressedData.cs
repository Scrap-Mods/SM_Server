using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Represents compressed LZ4 data.
/// </summary>
public ref struct CompressedData
{
    /// <summary>
    /// Gets the writer for writing the data to be compressed.
    /// </summary>
    /// <value>The writer.</value>
    [UnscopedRef]
    public ref BitWriter Writer => ref childWriter;

    private BitWriter childWriter;
    private bool disposed = false;

    private readonly ArrayPool<byte> arrayPool;
    private ref BitWriter parentWriter;
    private readonly bool writeLength;

    /// <summary>
    /// Initializes a new instance of <see cref="CompressedData"/>.
    /// </summary>
    /// <param name="parentWriter">The <see cref="BitWriter"/> to write the compressed data with.</param>
    /// <param name="arrayPool">The array pool for renting temporary buffers.</param>
    /// <param name="writeLength">Should the length of the compressed block written as a <see cref="uint"/> before the data.</param>
    public CompressedData(ref BitWriter parentWriter, ArrayPool<byte> arrayPool, bool writeLength)
    {
        this.writeLength = writeLength;
        this.parentWriter = ref parentWriter;
        this.arrayPool = arrayPool;
        childWriter = new BitWriter(arrayPool);
    }

    /// <summary>
    /// Compresses and writes the data with the original <see cref="BitWriter"/> and releases the temporary buffers.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            var array = arrayPool.Rent(LZ4.MaxCompressedSize(Writer.Data.Length));
            LZ4.TryCompress(Writer.Data, array, out int compressedLength);
            if (writeLength)
            {
                parentWriter.WriteInt32(compressedLength);
            }
            parentWriter.WriteBytes(array.AsSpan(0, compressedLength));
        }
    }
}
