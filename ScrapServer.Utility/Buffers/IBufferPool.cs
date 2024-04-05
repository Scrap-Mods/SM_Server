namespace ScrapServer.Utility.Buffers;

public interface IBufferPool
{
    public PooledBuffer GetBuffer(int size);

    public void ResizeBuffer(ref PooledBuffer buffer, int size);

    internal protected void ReturnBuffer(PooledBuffer buffer);
}
