using Server.Base.Timers.Services;

namespace Server.Base.Timers.Extensions;

public static class CreateDelayTimer
{
    public static Timer DelayCall(this TimerThread thread, Timer.TimerCallback callback) =>
        thread.DelayCall(callback, TimeSpan.Zero, TimeSpan.Zero, 1);

    public static Timer DelayCall(this TimerThread thread, Timer.TimerCallback callback, TimeSpan delay,
        TimeSpan interval, int count)
    {
        Timer timer = new DelayCallTimer(delay, interval, count, callback, thread)
        {
            Priority = (count == 1 ? delay : interval).ComputePriority()
        };

        timer.Start();

        return timer;
    }
}
