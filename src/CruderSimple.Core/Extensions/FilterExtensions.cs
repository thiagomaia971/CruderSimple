using System.Linq.Expressions;
using System.Reflection;

namespace CruderSimple.Core.Extensions;
    
public enum Op
{
    Equals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith
}

public class Filter
{
    public string PropertyName { get; set; }
    public Op Operation { get; set; }
    public object Value { get; set; }
}

public static class FilterExtensions
{
        public static Expression CreateWhereClause<T>
        (ParameterExpression target, Expression expression, Filter filter)
    {
        var predicate = Expression.Lambda(CreateComparison<T>(target, filter), target);

        return Expression.Call(typeof(Queryable), nameof(Queryable.Where), 
            new[] { target.Type }, expression, Expression.Quote(predicate));
    }

    public static Expression CreateComparison<T>(ParameterExpression target, 
        Filter filter)
    {
        Expression exp = null;

        var memberAccess = CreateMemberAccess(target, filter.PropertyName);
        var exp2 = GetExpression<T>(memberAccess, filter);

        exp = exp == null ? exp2 : Expression.Or(exp, exp2);

        return exp;
    }
    
    public static Expression GetSelector<T>(Filter filter)
{
  switch (filter.Value)
  {
    case int t1: return GetTypedSelector<T, int>(filter);
    case float f1: return GetTypedSelector<T, float>(filter);
    case double d1: return GetTypedSelector<T, double>(filter);
    case long l1: return GetTypedSelector<T, long>(filter);
    case DateTime dt1: return GetTypedSelector<T, DateTime>(filter);
    case bool b1: return GetTypedSelector<T, bool>(filter);
    case decimal d1: return GetTypedSelector<T, decimal>(filter);
    case char c1: return GetTypedSelector<T, char>(filter);
    case byte by1: return GetTypedSelector<T, byte>(filter);
    case short sh1: return GetTypedSelector<T, short>(filter);
    case ushort ush1: return GetTypedSelector<T, ushort>(filter);
    case uint ui1: return GetTypedSelector<T, uint>(filter);
    case ulong ul1: return GetTypedSelector<T, ulong>(filter);
    case string s1:
    {
      Expression<Func<string>> valueSelector = () => (string)filter.Value;
      return valueSelector.Body;
    }
    default: return null;
  }
}

public static Expression GetTypedSelector<T, R>(Filter filter) where R : struct
{
  // We actually need to get the property type, chaining from the container class,
  // and converting the value type to the property type using Expression.Convert
  var pi = GetPropertyInfo(typeof(T), filter.PropertyName);

  // This seems to be the preferred way.
  // Alternate: if (type.IsGenericType && type.GetGenericTypeDefinition() == 
  // typeof(Nullable<>))
  var propIsNullable = Nullable.GetUnderlyingType(pi.PropertyType) != null;

  Expression<Func<object>> valueSelector = () => filter.Value;
  Expression expr = propIsNullable ? Expression.Convert(valueSelector.Body, 
             typeof(R?)) : Expression.Convert(valueSelector.Body, typeof(R));

  return expr;
}

public static PropertyInfo GetPropertyInfo(Type baseType, string propertyName)
{
  string[] parts = propertyName.Split('.');

  return (parts.Length > 1)
    ? GetPropertyInfo(baseType.GetProperty(parts[0]).PropertyType, 
                      parts.Skip(1).Aggregate((a, i) => a + "." + i))
    : baseType.GetProperty(propertyName);
}

    
public static MethodInfo containsMethod = 
    typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
public static MethodInfo startsWithMethod = 
    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
public static MethodInfo endsWithMethod = 
    typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
    public static Expression GetExpression<T>(Expression member, Filter filter)
    {
        // How do we turn this into a SQL parameter 
        // so we're not susceptible to SQL injection attacks?
        // Like this: <a href="https://stackoverflow.com/a/71019524">
        // https://stackoverflow.com/a/71019524</a>
        //Expression<Func<object>> valueSelector = () => filter.Value;
        //var actualValue = valueSelector.Body;

        var actualValue = GetSelector<T>(filter);

        switch (filter.Operation)
        {
            case Op.Equals: return Expression.Equal(member, actualValue);
            case Op.GreaterThan: return Expression.GreaterThan(member, actualValue);
            case Op.GreaterThanOrEqual: 
                return Expression.GreaterThanOrEqual(member, actualValue);
            case Op.LessThan: return Expression.LessThan(member, actualValue);
            case Op.LessThanOrEqual: return Expression.LessThanOrEqual(member, actualValue);
            case Op.Contains: return Expression.Call(member, containsMethod, actualValue);
            case Op.StartsWith: return Expression.Call(member, startsWithMethod, actualValue);
            case Op.EndsWith: return Expression.Call(member, endsWithMethod, actualValue);
        }

        return null;
    }

    public static Expression CreateMemberAccess(Expression target, string selector)
    {
        return selector.Split('.').Aggregate(target, Expression.PropertyOrField);
    }
}