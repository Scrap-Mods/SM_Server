using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Utility.Buffers;

/// <summary>
/// An thread-safe implementation of <see cref="IBufferPool"/> which creates new buffers as needed.
/// </summary>
public class BufferPool : IBufferPool
{
    private BorrowedBuffer EmptyBuffer => new BorrowedBuffer(0, new Span<byte>(null), this);

    /// <summary>
    /// The list of arrays owned by this buffer pool.
    /// </summary>
    private readonly List<byte[]> ownedArrays;

    /// <summary>
    /// The list of indices of unused arrays in <see cref="ownedArrays"/>. 
    /// The indices are sorted by ascending size of the corresponding array.
    /// </summary>
    private readonly List<int> availableArrays;

    /// <summary>
    /// The object for locking on when accessing the lists.
    /// </summary>
    private readonly object lockObject = new object();

    /// <summary>
    /// Initializes a new instance of <see cref="BufferPool"/> with no preallocated buffers.
    /// </summary>
    public BufferPool()
    {
        ownedArrays = new List<byte[]>();
        availableArrays = new List<int>();
    }

    /// <inheritdoc/>
    public BorrowedBuffer GetBuffer(int size)
    {
        if (size == 0)
        {
            return EmptyBuffer;
        }
        if (TryGetAvailableBuffer(size, out var buffer))
        {
            return buffer;
        }
        return CreateBuffer(size);
    }

    /// <inheritdoc/>
    public void ResizeBuffer(ref BorrowedBuffer buffer, int size)
    {
        if (size == 0)
        {
            ReturnBuffer(buffer);
            buffer = EmptyBuffer;
            return;
        }
        if (buffer.IsEmpty)
        {
            buffer = GetBuffer(size);
            return;
        }

        int id = buffer.Id;
        byte[] array = GetArrayForBuffer(buffer);
        if (array.Length >= size)
        {
            buffer = new BorrowedBuffer(id, array.AsSpan(0, size), this);
            return;
        }

        var newBuffer = GetBuffer(size);
        buffer[0..int.Min(buffer.Length, size)].CopyTo(newBuffer);
        ReturnBuffer(buffer);
        buffer = newBuffer;
    }

    /// <inheritdoc/>
    void IBufferPool.ReturnBuffer(BorrowedBuffer buffer) => ReturnBuffer(buffer);

    private bool TryGetAvailableBuffer(int size, [MaybeNullWhen(false)] out BorrowedBuffer buffer)
    {
        buffer = default;
        lock (lockObject)
        {
            for (int i = 0; i < availableArrays.Count; i++)
            {
                var id = availableArrays[i];
                var array = ownedArrays[id];
                if (array.Length >= size)
                {
                    availableArrays.RemoveAt(i);
                    buffer = new BorrowedBuffer(id, array.AsSpan(0, size), this);
                    return true;
                }
            }
        }
        return false;
    }

    private BorrowedBuffer CreateBuffer(int size)
    {
        size = int.Max(16, size);
        var array = new byte[int.IsPow2(size) ? size : 2 << int.Log2(size)];
        int id;
        lock (lockObject)
        {
            id = ownedArrays.Count;
            ownedArrays.Add(array);
        }
        return new BorrowedBuffer(id, array.AsSpan(0, size), this);
    }

    private void ReturnBuffer(BorrowedBuffer buffer)
    {
        if (buffer.IsEmpty)
        {
            return;
        }

        lock (lockObject)
        {
            int id = buffer.Id;
            byte[] array = GetArrayForBuffer(buffer);

            int availableCount = availableArrays.Count;
            for (int i = 0; i < availableCount; i++)
            {
                var availableId = availableArrays[i];
                if (availableId == id)
                {
                    throw new ArgumentException("Buffer returned twice.");
                }
                var availableArray = ownedArrays[availableId];
                if (availableArray.Length > array.Length)
                {
                    availableArrays.Insert(i, id);
                    return;
                }
            }
            availableArrays.Add(id);
        }
    }

    private byte[] GetArrayForBuffer(BorrowedBuffer buffer)
    {
        lock (lockObject)
        {
            int id = buffer.Id;
            if (buffer.Pool == this && id < ownedArrays.Count)
            {
                var array = ownedArrays[id];
                if (array.AsSpan().Overlaps(buffer))
                {
                    return array;
                }
            }
        }
        throw new ArgumentException("The buffer is not owned by this pool.");
    }
}
