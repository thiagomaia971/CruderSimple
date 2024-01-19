namespace CruderSimple.Core.Extensions
{
    public static class DictionaryExtensions
    {

        public static T FromCache<T>(this IDictionary<string, object> cached, string id)
            where T : class
        {
            if (cached == null)
                return null;
            if (cached.ContainsKey(id))
                return (T) cached[id];
            return null;

        }
    }
}
