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
    }
}
