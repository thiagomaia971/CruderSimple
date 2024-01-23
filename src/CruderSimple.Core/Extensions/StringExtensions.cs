using Newtonsoft.Json;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Text;

namespace CruderSimple.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Join(this string left, params string[] right)
        {
            if (string.IsNullOrEmpty(left))
                return string.Join(",", right);
            return $"{left},{string.Join(",", right)}";
        }
        public static string Join(this bool left, params bool[] right)
        {
            if (left)
                return string.Join(",", right);
            return $"{left},{string.Join(",", right)}";
        }

        public static string ToJson(this object obj)
        {
            if (obj == null)
                return "null";
            if (IsEnumerableType(obj.GetType(), out _))
            {
                var list = obj as IEnumerable;
                var builder = new StringBuilder($"{obj.GetType().Name}: ");
                builder.Append("[");
                var objs = new List<string>();
                foreach (var item in list)
                {
                    objs.Add(JsonConvert.SerializeObject(item, Formatting.Indented));
                }
                builder.Append(string.Join(",", objs));
                builder.Append("]");
                return builder.ToString();
            }
            else
                return $"{obj.GetType().Name}: {JsonConvert.SerializeObject(obj, Formatting.Indented)}";
        }
        private static bool IsEnumerableType(this Type type, out Type elementType)
        {
            if (type.IsGenericType && (
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            elementType = null;
            return false;
        }
    }
}

