public static class SchedulerService
{
    public static UInt32 GameTick { get; private set; }
    public static event Action Tick;
    public static void Start()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var shift = 0;

        while (true)
        {
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (elapsed - shift >= 25)
            {
                GameTick += 1;

                Tick.Invoke();

                shift += 25;
            }
        }
    }
}
