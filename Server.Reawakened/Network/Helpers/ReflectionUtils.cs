using System.Reflection;

namespace Server.Reawakened.Network.Helpers;

/// <summary>
///     Based on Discord.Net Reflection Utils.
///     https://github.com/discord-net
/// </summary>
public class ReflectionUtils
{
    private readonly TypeInfo _objectTypeInfo;

    public ReflectionUtils() => _objectTypeInfo = typeof(object).GetTypeInfo();

    public Func<IServiceProvider, T> CreateBuilder<T>(TypeInfo typeInfo)
    {
        var constructor = GetConstructor(typeInfo);
        var parameters = constructor.GetParameters();
        var properties = GetProperties(typeInfo);

        return services =>
        {
            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                args[i] = GetMember(services, parameters[i].ParameterType, typeInfo);
            var obj = InvokeConstructor<T>(constructor, args, typeInfo);

            foreach (var property in properties)
                property.SetValue(obj, GetMember(services, property.PropertyType, typeInfo));
            return obj;
        };
    }

    private static T InvokeConstructor<T>(ConstructorInfo constructor, object[] args, Type ownerType)
    {
        try
        {
            return (T)constructor.Invoke(args);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create \"{ownerType.FullName}\".", ex);
        }
    }

    private static ConstructorInfo GetConstructor(TypeInfo ownerType)
    {
        var constructors = ownerType.DeclaredConstructors.Where(x => !x.IsStatic).ToArray();

        return constructors.Length switch
        {
            0 => throw new InvalidOperationException($"No constructor found for \"{ownerType.FullName}\"."),
            > 1 => throw new InvalidOperationException($"Multiple constructors found for \"{ownerType.FullName}\"."),
            _ => constructors[0]
        };
    }

    private PropertyInfo[] GetProperties(TypeInfo ownerType)
    {
        var result = new List<PropertyInfo>();
        while (ownerType != _objectTypeInfo)
        {
            result.AddRange(
                ownerType.DeclaredProperties.Where(
                    prop => prop.SetMethod is { IsStatic: false, IsPublic: true }
                )
            );

            if (ownerType.BaseType != null)
                ownerType = ownerType.BaseType.GetTypeInfo();
        }

        return result.ToArray();
    }

    private static object GetMember(IServiceProvider services, Type memberType, Type ownerType)
    {
        if (memberType == typeof(IServiceProvider) || memberType == services.GetType())
            return services;

        var service = services.GetService(memberType);

        return service ?? throw new InvalidOperationException(
            $"Failed to create \"{ownerType.FullName}\", dependency \"{memberType.Name}\" was not found.");
    }
}
