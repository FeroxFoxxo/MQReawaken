using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;

namespace Server.Base.Core.Extensions;

public static class RequiredServices
{
    public static IEnumerable<T> GetRequiredServices<T>(this IServiceProvider services, IEnumerable<Module> modules)
        where T : class => GetServices<T>(modules).Select(t => services.GetRequiredService(t) as T);

    public static IEnumerable<Type> GetServices<T>(IEnumerable<Module> modules) =>
        modules.Select(m => m.GetType().Assembly)
            .SelectMany(a =>
                a.GetTypes().Where(
                    t => typeof(T).IsAssignableFrom(t) &&
                         !t.IsInterface &&
                         !t.IsAbstract
                )
            );
}
