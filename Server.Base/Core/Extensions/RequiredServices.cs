using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;

namespace Server.Base.Core.Extensions;

public static class RequiredServices
{
    public static IEnumerable<T> GetRequiredServices<T>(this IServiceProvider services) where T : class
        => GetServices<T>().Select(t => services.GetRequiredService(t) as T);

    public static IEnumerable<Type> GetServices<T>() =>
        AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(a =>
                         a.GetTypes().Where(
                             t => typeof(T).IsAssignableFrom(t) &&
                                  !t.IsInterface &&
                                  !t.IsAbstract
                         )
                     );
}
