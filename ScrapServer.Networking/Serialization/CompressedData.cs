using ScrapServer.Utility;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Networking.Serialization;

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
    public ref PacketWriter Writer => ref childWriter;

    private readonly ArrayPool<byte> arrayPool;
    private ref PacketWriter parentWriter;
    private PacketWriter childWriter;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="CompressedData"/>.
    /// </summary>
    /// <param name="parentWriter">The <see cref="PacketWriter"/> to write the compressed data with.</param>
    /// <param name="arrayPool">The array pool for renting temporary buffers.</param>
    public CompressedData(ref PacketWriter parentWriter, ArrayPool<byte> arrayPool)
    {
        this.parentWriter = ref parentWriter;
        this.arrayPool = arrayPool;
        childWriter = new PacketWriter(arrayPool);
    }

    /// <summary>
    /// Compresses and writes the data with the original <see cref="PacketWriter"/> and releases the temporary buffers.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            var array = arrayPool.Rent(LZ4.MaxCompressedSize(Writer.Data.Length));
            LZ4.TryCompress(Writer.Data, array, out int compressedLength);
            parentWriter.WriteBytes(array.AsSpan(0, compressedLength));
        }
    }
}
