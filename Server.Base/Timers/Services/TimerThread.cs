using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Timers.Helpers;

namespace Server.Base.Timers.Services;

public class TimerThread : IService
{
    private readonly Dictionary<Timer, TimerChangeEntry> _changed;
    private readonly InternalServerConfig _config;
    private readonly ServerHandler _handler;
    private readonly double[] _nextPriorities;
    private readonly TimerChangePool _pool;
    private readonly Queue<Timer> _queue;
    private readonly AutoResetEvent _signal;
    private readonly EventSink _sink;
    private readonly List<Timer>[] _timers;
    private readonly Thread _timerThread;

    public TimerThread(InternalServerConfig config, TimerChangePool pool, EventSink sink, ServerHandler handler)
    {
        _config = config;
        _pool = pool;
        _sink = sink;
        _handler = handler;
        _changed = new Dictionary<Timer, TimerChangeEntry>();
        _queue = new Queue<Timer>();
        _signal = new AutoResetEvent(false);

        _nextPriorities = Enumerable.Repeat(default(double), config.Delays.Length).ToArray();
        _timers = Enumerable.Repeat(new List<Timer>(), config.Delays.Length).ToArray();

        _timerThread = new Thread(RunTimer)
        {
            Name = "Timer Thread"
        };
    }

    public void Initialize() => _sink.ServerStarted += _ => _timerThread.Start();

    public void AddTimer(Timer timer) => Change(timer, (int)timer.Priority, true);

    public void PriorityChange(Timer timer, int newPriority) => Change(timer, newPriority, false);

    public void RemoveTimer(Timer timer) => Change(timer, -1, false);

    public void Change(Timer timer, int index, bool adding)
    {
        lock (_changed)
            _changed[timer] = _pool.GetInstance(timer, index, adding);

        _signal.Set();
    }

    public void Set() => _signal.Set();

    private void ProcessChanged()
    {
        lock (_changed)
        {
            var ticks = GetTicks.Ticks;

            foreach (var timerChangeEntry in _changed.Values)
            {
                var timer = timerChangeEntry.Timer;
                var newIndex = timerChangeEntry.Index;

                timer.List?.Remove(timer);

                if (timerChangeEntry.Adding)
                {
                    timer.Next = ticks + timer.Delay;
                    timer.Index = 0;
                }

                if (newIndex >= 0)
                {
                    timer.List = _timers[newIndex];
                    timer.List.Add(timer);
                }
                else
                {
                    timer.List = null;
                }

                timerChangeEntry.Free();
            }

            _changed.Clear();
        }
    }

    public void Slice()
    {
        lock (_queue)
        {
            var index = 0;

            while (index < _config.BreakCount && _queue.Count != 0)
            {
                var timer = _queue.Dequeue();

                timer.OnTick();
                timer.Queued = false;
                index++;
            }
        }
    }

    public void RunTimer()
    {
        while (!_handler.IsClosing)
        {
            ProcessChanged();

            var loaded = false;

            for (var i = 0; i < _timers.Length; i++)
            {
                var now = GetTicks.Ticks;

                if (now < _nextPriorities[i])
                    break;

                _nextPriorities[i] = now + _config.Delays[i];

                foreach (var timer in _timers[i].Where(timer => !timer.Queued && !(now <= timer.Next)))
                {
                    timer.Queued = true;

                    lock (_queue)
                        _queue.Enqueue(timer);

                    loaded = true;

                    if (timer.Count != 0 && ++timer.Index >= timer.Count)
                        timer.Stop();
                    else
                        timer.Next = now + timer.Interval;
                }
            }

            if (loaded)
                _handler.Set();

            _signal.WaitOne(1, false);
        }
    }
}
