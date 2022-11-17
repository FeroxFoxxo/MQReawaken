namespace Server.Base.Timers.Helpers;

public class TimerChangePool
{
    public readonly Queue<TimerChangeEntry> InstancePool;

    public TimerChangePool() => InstancePool = new Queue<TimerChangeEntry>();

    public TimerChangeEntry GetInstance(Timer timer, int index, bool adding)
    {
        TimerChangeEntry timerChangeEntry = null;

        lock (InstancePool)
        {
            if (InstancePool.Count > 0)
                timerChangeEntry = InstancePool.Dequeue();
        }

        if (timerChangeEntry != null)
        {
            timerChangeEntry.Timer = timer;
            timerChangeEntry.Index = index;
            timerChangeEntry.Adding = adding;
        }
        else
        {
            timerChangeEntry = new TimerChangeEntry(timer, index, adding, this);
        }

        return timerChangeEntry;
    }
}
