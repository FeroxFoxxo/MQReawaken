namespace Server.Base.Timers.Helpers;

public class TimerChangeEntry
{
    private readonly TimerChangePool _pool;
    public bool Adding;

    public int Index;
    public Timer Timer;

    public TimerChangeEntry(Timer timer, int newIndex, bool adding, TimerChangePool pool)
    {
        Timer = timer;
        Index = newIndex;
        Adding = adding;
        _pool = pool;
    }

    public void Free()
    {
        Timer = null;

        lock (_pool.InstancePool)
        {
            if (_pool.InstancePool.Count < 512)
                _pool.InstancePool.Enqueue(this);
        }
    }
}
