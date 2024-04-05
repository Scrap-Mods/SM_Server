namespace ScrapServer.Utility.Buffers;

/// <summary>
/// Represents a pool of byte buffers.
/// </summary>
public interface IBufferPool
{
    /// <summary>
    /// Borrows a buffer of the specified size.
    /// </summary>
    /// <param name="size">The desired size of the buffer.</param>
    /// <returns>The borrowed buffer.</returns>
    public BorrowedBuffer GetBuffer(int size);

    /// <summary>
    /// Extends or truncates a borrowed buffer.
    /// </summary>
    /// <param name="buffer">The borrowed buffer</param>
    /// <param name="size">The desired size of the buffer.</param>
    internal protected void ResizeBuffer(ref BorrowedBuffer buffer, int size);

    /// <summary>
    /// Returns a borrowed buffer to the pool.
    /// </summary>
    /// <param name="buffer">The borrowed buffer.</param>
    internal protected void ReturnBuffer(BorrowedBuffer buffer);
}
