namespace ScrapServer.Core.Utils;

public class UniqueIdProvider
{
    private volatile uint currentId = 0;

    public uint GetNextId()
    {
        return Interlocked.Increment(ref this.currentId);
    }
}
