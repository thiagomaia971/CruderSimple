using System.Reflection;

namespace CruderSimple.Core.Extensions;

public static class PropertyInfoExtensions
{
    public static T? GetCustomAttribute<T>(this PropertyInfo property)
        where T : Attribute 
        => (T?) property.GetCustomAttributes(false)
            .FirstOrDefault(y => typeof(T).IsAssignableFrom(y.GetType()));
}