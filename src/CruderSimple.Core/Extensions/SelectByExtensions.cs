using System.Linq.Expressions;

namespace CruderSimple.Core.Extensions
{
    public static class SelectByExtensions
    {
        public static IQueryable<TSource> SelectBy<TSource>(this IQueryable<TSource> source, string members)
        {
            if (members == "*")
                return source;
            return source.Select(BuildSelector<TSource, TSource>(members));
        }

        public static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(string members) =>
            BuildSelector<TSource, TTarget>(members.Split(',').Select(m => m.Trim()));

        public static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(IEnumerable<string> members)
        {
            var parameter = Expression.Parameter(typeof(TSource), "e");
            var body = NewObject(typeof(TTarget), parameter, members.Select(m => m.Split('.').ToList()));
            return Expression.Lambda<Func<TSource, TTarget>>(body, parameter);
        }

        static Expression NewObject(Type targetType, Expression source, IEnumerable<List<string>> memberPaths, int depth = 0)
        {
            var bindings = new List<MemberBinding>();
            var target = Expression.Constant(null, targetType);
            List<List<string>> custom = memberPaths.ToList();

            var inner = memberPaths.Where(x => x.Count > 1).ToList();
            foreach (var item in inner)
            {
                custom.Add(CreateDefaultSelect("Id", item));
                custom.Add(CreateDefaultSelect("CreatedAt", item));
                custom.Add(CreateDefaultSelect("UpdatedAt", item));
            }

            if (depth == 0)
            {
                custom = new List<List<string>>
                {
                    new() { "Id" },
                    new() { "CreatedAt" },
                    new() { "UpdatedAt" },
                };
                custom.AddRange(memberPaths);
            }

            foreach (var memberGroup in custom.GroupBy(path => path[depth]))
            {
                var memberName = memberGroup.Key;
                var targetMember = Expression.PropertyOrField(target, memberName);
                var sourceMember = Expression.PropertyOrField(source, memberName);
                var childMembers = memberGroup.Where(path => depth + 1 < path.Count).ToList();

                Expression targetValue = null;
                if (!childMembers.Any())
                    targetValue = sourceMember;
                else
                {
                    if (IsEnumerableType(targetMember.Type, out var sourceElementType) &&
                        IsEnumerableType(targetMember.Type, out var targetElementType))
                    { var sourceElementParam = Expression.Parameter(sourceElementType, "e");

                        // childMembers.Add(new string[] { memberName, "Id" });
                        // childMembers.Add(new string[] { memberName, "CreatedAt" });
                        // childMembers.Add(new string[] { memberName, "UpdatedAt" });

                        targetValue = NewObject(targetElementType, sourceElementParam, childMembers, depth + 1);
                        targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
                            new[] { sourceElementType, targetElementType }, sourceMember,
                            Expression.Lambda(targetValue, sourceElementParam));

                        targetValue = CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type);
                    }
                    else
                    {
                        targetValue = NewObject(targetMember.Type, sourceMember, childMembers, depth + 1);
                    }
                }

                bindings.Add(Expression.Bind(targetMember.Member, targetValue));
            }
            return Expression.MemberInit(Expression.New(targetType), bindings);
        }

        private static List<string> CreateDefaultSelect(string property, List<string> item)
        {
            
            var defaultList = item.Where(x => x != item.LastOrDefault()).ToList();
            if (!defaultList.Contains(property))
                defaultList.Add(property);
            return defaultList;
        }

        static bool IsEnumerableType(Type type, out Type elementType)
        {
        
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            elementType = null;
            return false;
            /*
            foreach (var intf in type.GetInterfaces())
            {
                if (intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = intf.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;*/
        }

        static bool IsSameCollectionType(Type type, Type genericType, Type elementType)
        {
            var result = genericType.MakeGenericType(elementType).IsAssignableFrom(type);
            return result;
        }

        static Expression CorrectEnumerableResult(Expression enumerable, Type elementType, Type memberType)
        {
            if (memberType == enumerable.Type)
                return enumerable;

            if (memberType.IsArray)
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { elementType }, enumerable);

            if (IsSameCollectionType(memberType, typeof(List<>), elementType)
                || IsSameCollectionType(memberType, typeof(ICollection<>), elementType)
                || IsSameCollectionType(memberType, typeof(IReadOnlyList<>), elementType)
                || IsSameCollectionType(memberType, typeof(IReadOnlyCollection<>), elementType))
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { elementType }, enumerable);

            throw new NotImplementedException($"Not implemented transformation for type '{memberType.Name}'");
        }
    }
}
