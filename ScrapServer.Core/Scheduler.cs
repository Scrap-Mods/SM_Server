public static class Scheduler
{
    public static UInt32 GameTick { get; private set; }

    public static void Tick()
    {
        GameTick += 1;
    }
}