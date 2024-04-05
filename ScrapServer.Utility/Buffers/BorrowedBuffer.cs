namespace ScrapServer.Utility.Buffers;

/// <summary>
/// Represents a byte buffer borrowed from an <see cref="IBufferPool"/>.
/// </summary>
public ref struct BorrowedBuffer
{
    /// <summary>
    /// Gets or sets a byte in the buffer.
    /// </summary>
    /// <param name="index">The index of the byte.</param>
    /// <returns>The byte at the specified index.</returns>
    public readonly ref byte this[int index] => ref span[index];

    /// <summary>
    /// Gets the length of this buffer.
    /// </summary>
    /// <value>The length of the buffer.</value>
    public readonly int Length => span.Length;

    /// <summary>
    /// Gets whether the length of the buffer is equal to zero.
    /// </summary>
    /// <value>Whether the buffer is empty.</value>
    public readonly bool IsEmpty => span.Length == 0;

    /// <summary>
    /// Gets the span corresponding to this buffer.
    /// </summary>
    /// <value>The span.</value>
    public readonly Span<byte> Span => span;

    /// <summary>
    /// Gets the pool which owns this buffer.
    /// </summary>
    /// <value>The owner pool.</value>
    internal readonly IBufferPool Pool => pool;

    /// <summary>
    /// Gets the number that uniquely identifies this buffer within an <see cref="IBufferPool"/>.
    /// </summary>
    /// <value>The buffer id.</value>
    internal readonly int Id => id;

    private readonly Span<byte> span;
    private readonly IBufferPool pool;
    private readonly int id;

    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="BorrowedBuffer"/>.
    /// </summary>
    /// <param name="id">The unique id of the buffer in the pool.</param>
    /// <param name="span">The span corresponding to the buffer.</param>
    /// <param name="owningPool">The pool owning the buffer.</param>
    internal BorrowedBuffer(int id, Span<byte> span, IBufferPool owningPool)
    {
        this.id = id;
        this.span = span;
        this.pool = owningPool;
    }

    /// <summary>
    /// Returns an enumerator for this buffer.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public readonly Span<byte>.Enumerator GetEnumerator() => span.GetEnumerator();

    /// <summary>
    /// Forms a slice of the buffer starting at a specified index.
    /// </summary>
    /// <param name="start">The starting index of the slice.</param>
    /// <returns>The slice.</returns>
    public readonly Span<byte> Slice(int start) => span.Slice(start);

    /// <summary>
    /// Forms a slice of the buffer starting at a specified index for a specified length.
    /// </summary>
    /// <param name="start">The starting index of the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>The slice.</returns>
    public readonly Span<byte> Slice(int start, int length) => span.Slice(start, length);

    /// <summary>
    /// Copies the contents of this buffer into a destination span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    public readonly void CopyTo(Span<byte> destination) => span.CopyTo(destination);

    /// <summary>
    /// Tries to copy the contents of this buffer into a destination span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    /// <returns><see langword="true"/> on success, <see langword="false"/> on failure (if the span was too small).</returns>
    public readonly bool TryCopyTo(Span<byte> destination) => span.TryCopyTo(destination);

    /// <summary>
    /// Returns a reference for pinning.
    /// </summary>
    /// <returns>A reference to the first element or <see langword="null"/> if <see cref="Length"/> is 0.</returns>
    public readonly ref byte GetPinnableReference() => ref span.GetPinnableReference();

    /// <summary>
    /// Copies the contents of this buffer to a new array.
    /// </summary>
    /// <returns>The created array.</returns>
    public readonly byte[] ToArray() => span.ToArray();

    /// <summary>
    /// Extends or truncates the buffer.
    /// </summary>
    /// <param name="size">The desired size.</param>
    public void Resize(int size)
    {
        Pool.ResizeBuffer(ref this, size);
    }

    /// <summary>
    /// Returns the buffer to the pool.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            Pool?.ReturnBuffer(this);
        }
    }

    public static implicit operator Span<byte>(BorrowedBuffer buffer) => buffer.span;

    public static implicit operator ReadOnlySpan<byte>(BorrowedBuffer buffer) => buffer.span;
}
