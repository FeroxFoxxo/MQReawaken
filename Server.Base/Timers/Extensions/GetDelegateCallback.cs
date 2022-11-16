namespace Server.Base.Timers.Extensions;

public static class GetDelegateCallback
{
    public static string FormatDelegate(this Delegate callback)
    {
        if (callback == null)
            return "null";

        return callback.Method.DeclaringType == null
            ? callback.Method.Name
            : $"{callback.Method.DeclaringType.FullName}.{callback.Method.Name}";
    }
}
