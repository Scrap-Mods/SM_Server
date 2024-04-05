namespace ScrapServer.Utility.Buffers;

public ref struct PooledBuffer
{
    public Span<byte> Span { get; }

    internal int Id { get; }

    internal IBufferPool? Pool { get; }

    private bool disposed = false;

    public PooledBuffer(int id, Span<byte> span, IBufferPool? owningPool = null)
    {
        Id = id;
        Span = span;
        Pool = owningPool;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            Pool?.ReturnBuffer(this);
        }
    }
}
