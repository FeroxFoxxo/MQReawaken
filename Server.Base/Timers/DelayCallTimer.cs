using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers;

public class DelayCallTimer : Timer
{
    public TimerCallback Callback { get; }

    public DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerCallback callback, TimerThread tThread)
        : base(delay, interval, count, tThread) => Callback = callback;

    public override void OnTick() => Callback?.Invoke();

    public override string ToString() => $"DelayCallTimer [{Callback.FormatDelegate()}]";
}
