using Server.Base.Timers.Enums;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers;

public class Timer
{
    public delegate void TimerCallback();

    private readonly TimerThread _timerThread;

    public readonly int Count;

    private TimerPriority _priority;
    private bool _prioritySet;
    private bool _running;

    public double Delay;
    public int Index;
    public long Interval;
    public List<Timer> List;
    public double Next;
    public bool Queued;

    public TimerPriority Priority
    {
        get => _priority;
        set
        {
            if (!_prioritySet)
                _prioritySet = true;

            if (_priority == value)
                return;

            _priority = value;

            if (_running)
                _timerThread.PriorityChange(this, (int)_priority);
        }
    }

    public Timer(TimeSpan delay, TimeSpan interval, int count, TimerThread timerThread)
    {
        Delay = (long)delay.TotalMilliseconds;
        Interval = (long)interval.TotalMilliseconds;
        Count = count;
        _timerThread = timerThread;

        if (_prioritySet)
            return;

        _priority = (count == 1 ? delay : interval).ComputePriority();
        _prioritySet = true;
    }

    public void Start()
    {
        if (_running)
            return;

        _running = true;

        _timerThread.AddTimer(this);
    }

    public void Stop()
    {
        if (!_running)
            return;

        _running = false;

        _timerThread.RemoveTimer(this);
    }

    public virtual void OnTick()
    {
    }
}
