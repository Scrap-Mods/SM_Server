using System.Diagnostics.CodeAnalysis;

namespace ScrapServer.Utility.Buffers;

public class BufferPool : IBufferPool
{
    private static PooledBuffer EmptyBuffer => new PooledBuffer(0, new Span<byte>(null));

    private int currentArrayId = 0;
    
    private readonly Dictionary<int, byte[]> ownedArrays;
    private readonly List<KeyValuePair<int, byte[]>> availableArrays;

    public BufferPool()
    {
        ownedArrays = new Dictionary<int, byte[]>();
        availableArrays = new List<KeyValuePair<int, byte[]>>();
    }

    public PooledBuffer GetBuffer(int size)
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

    public void ResizeBuffer(ref PooledBuffer buffer, int size)
    {
        if (size == 0)
        {
            ReturnBuffer(buffer);
            buffer = EmptyBuffer;
            return;
        }
        if (buffer.Span.Length == 0)
        {
            buffer = GetBuffer(size);
            return;
        }

        int id = buffer.Id;
        byte[] array = GetArrayForBuffer(buffer);
        if (array.Length >= size)
        {
            buffer = new PooledBuffer(id, array.AsSpan(0, size));
            return;
        }

        var newBuffer = GetBuffer(size);
        buffer.Span[0..int.Min(buffer.Span.Length, size)].CopyTo(newBuffer.Span);
        ReturnBuffer(buffer);
        buffer = newBuffer;
    }

    void IBufferPool.ReturnBuffer(PooledBuffer buffer) => ReturnBuffer(buffer);

    private bool TryGetAvailableBuffer(int size, [MaybeNullWhen(false)] out PooledBuffer buffer)
    {
        buffer = default;
        lock (availableArrays)
        {
            for (int i = 0; i < availableArrays.Count; i++)
            {
                var (id, array) = availableArrays[i];
                if (array.Length >= size)
                {
                    availableArrays.RemoveAt(i);
                    buffer = new PooledBuffer(id, array.AsSpan(0, size));
                    return true;
                }
            }
        }
        return false;
    }

    private PooledBuffer CreateBuffer(int size)
    {
        size = int.Min(16, size);
        var array = new byte[int.IsPow2(size) ? size : 2 << int.Log2(size)];
        var id = Interlocked.Increment(ref currentArrayId);
        lock (ownedArrays)
        {
            ownedArrays.Add(id, array);
        }
        return new PooledBuffer(id, array);
    }

    private void ReturnBuffer(PooledBuffer buffer)
    {
        if (buffer.Span.Length == 0)
        {
            return;
        }

        int id = buffer.Id;
        byte[]? array = GetArrayForBuffer(buffer);

        var kvp = new KeyValuePair<int, byte[]>(id, array);
        lock (availableArrays)
        {
            int availableCount = availableArrays.Count;
            for (int i = 0; i < availableCount; i++)
            {
                var (availableId, availableBuffer) = availableArrays[i];
                if (availableId == id)
                {
                    throw new ArgumentException("Buffer returned twice.");
                }
                if (availableBuffer.Length > array.Length)
                {
                    availableArrays.Insert(i, kvp);
                    return;
                }
            }
            availableArrays.Add(kvp);
        }
    }

    private byte[] GetArrayForBuffer(PooledBuffer buffer)
    {
        lock (ownedArrays)
        {
            if (buffer.Pool == this
                && ownedArrays.TryGetValue(buffer.Id, out var array)
                && array.AsSpan().Overlaps(buffer.Span))
            {
                return array;
            }
        }
        throw new ArgumentException("The buffer is not owned by this pool.");
    }
}
