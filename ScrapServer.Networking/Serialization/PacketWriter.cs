using ScrapServer.Utility.Buffers;

namespace ScrapServer.Networking.Serialization;

public ref struct PacketWriter
{
    private readonly BorrowedBuffer buffer;
    private readonly IBufferPool bufferPool;

    private int index;
    private byte bitOffset;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketWriter"/>.
    /// </summary>
    /// <param name="buffer">The buffer for writing the packet data.</param>
    /// <param name="bufferPool">The buffer pool used for borrowing additional buffers.</param>
    internal PacketWriter(BorrowedBuffer buffer, IBufferPool bufferPool)
    {
        this.bufferPool = bufferPool;
        this.buffer = buffer;
    }
}
