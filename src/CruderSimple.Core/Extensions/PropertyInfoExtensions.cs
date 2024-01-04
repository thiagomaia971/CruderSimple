using System.Reflection;

namespace CruderSimple.Core.Extensions;

public static class PropertyInfoExtensions
{
    public static T? GetCustomAttribute<T>(this PropertyInfo property)
        where T : Attribute 
        => (T?) property.GetCustomAttributes(false)
            .FirstOrDefault(y => typeof(T).IsAssignableFrom(y.GetType()));

    public static bool IsPropertyEnumerableType(this Type type, string propertyName)
    {
        var property = type.GetProperties().FirstOrDefault(x => x.Name == propertyName);
        if (property is null)
            return false;
        return property.PropertyType.IsEnumerableType(out _);
    }
}