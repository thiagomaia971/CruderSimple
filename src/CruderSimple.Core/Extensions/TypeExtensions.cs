using System.Reflection;

namespace CruderSimple.Core.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypesWithHelpAttribute<T>(this IEnumerable<Type> types) {
            foreach(Type type in types) {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                    yield return (type);
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithInhiredType<T>(this T type)
            => type.GetType()
                .GetProperties()
                .Where(c => type.GetType().IsAssignableFrom(c.PropertyType));
        
        public static IEnumerable<PropertyInfo> cGetPropertiesWithInhiredType<T>(this T type)
            => type.GetType()
                .GetProperties()
                .Where(c => type.GetType().IsAssignableFrom(c.PropertyType));
        
        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(this object @object)
            where T : Attribute 
            => @object.GetType()
                .GetProperties()
                .Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(T)))
                .ToList();
    
        public static IEnumerable<PropertyInfo> GetPropertiesWithoutAttribute<T>(this object @object)
            where T : Attribute 
            => @object.GetType()
                .GetProperties()
                .Where(x => x.CustomAttributes.All(x => x.AttributeType != typeof(T)))
                .ToList();
    }
}
