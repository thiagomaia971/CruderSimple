using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CruderSimple.Core.Attributes;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Extensions;

public static class CollectionExtensions
{

    public static IEnumerable<TItem> AddItem<TItem>(this IEnumerable<TItem> values, TItem item)
    {
        var valuesNew = values.ToList();
        valuesNew.Add(item);
        values = valuesNew;
        return values;
    }

    public static IEnumerable<TItem> RemoveItem<TItem>(this IEnumerable<TItem> values, Func<TItem, bool> predicate)
    {
        var valueToRemove = values.FirstOrDefault(predicate);
        if (valueToRemove == null)
            return values;

        var valuesNew = values.ToList();
        valuesNew.Remove(valueToRemove);
        values = valuesNew;
        return values;
    }

    public static IEnumerable<TItem> ReplaceItem<TItem>(this IEnumerable<TItem> values, TItem item, Func<TItem, bool> predicate, bool addIfNotExists = true)
    {
        if (values == null)
            return Enumerable.Empty<TItem>();
        var valuesNew = values.ToList();
        var itemFounded = valuesNew.FirstOrDefault(predicate);
        if (itemFounded != null)
        {
            var index = valuesNew.IndexOf(itemFounded);
            valuesNew.Remove(itemFounded);
            valuesNew.Insert(index, item);
        }
        else if (addIfNotExists)
            valuesNew.Add(item);

        values = valuesNew;
        return values;
    }

    public static ICollection<TEntity> FromInput<TEntity, TDto>(this ICollection<TEntity> entities,
        ICollection<TDto> inputs)
        where TEntity : IEntity
        where TDto : BaseDto
    {
        var inner = from xinput in inputs
            join xxinput in entities on xinput.Id equals xxinput.Id into joined
            from entity in joined.DefaultIfEmpty()
            select new
            {
                Input = xinput,
                Entity = entity
            };

        return inner
            .Select(x => (x.Entity?.FromInput(x.Input) ?? Activator.CreateInstance<TEntity>().FromInput(x.Input)))
            .Cast<TEntity>()
            .ToList();
    }
    public static IEnumerable<TEntity> FromInput<TEntity, TDto>(this IEnumerable<TEntity> entities,
        ICollection<TDto> inputs)
        where TEntity : IEntity
        where TDto : BaseDto
    {
        if (inputs is null)
            return entities;
        
        var inner = from xinput in inputs
            join xxinput in entities on xinput.Id equals xxinput.Id into joined
            from entity in joined.DefaultIfEmpty()
            select new
            {
                Input = xinput,
                Entity = entity
            };

        return inner
            .Select(x => (x.Entity?.FromInput(x.Input) ?? Activator.CreateInstance<TEntity>().FromInput(x.Input)))
            .Cast<TEntity>()
            .ToList();
    }
    public static List<TEntity> FromInput<TEntity, TDto>(this List<TEntity> entities,
        ICollection<TDto> inputs)
        where TEntity : IEntity
        where TDto : BaseDto
    {
        var inner = from xinput in inputs
            join xxinput in entities on xinput.Id equals xxinput.Id into joined
            from entity in joined.DefaultIfEmpty()
            select new
            {
                Input = xinput,
                Entity = entity
            };

        return inner
            .Select(x => (x.Entity?.FromInput(x.Input) ?? Activator.CreateInstance<TEntity>().FromInput(x.Input)))
            .Cast<TEntity>()
            .ToList();
    }

    public static Pagination<TSource> ApplyQuery<TSource>(this IQueryable<TSource> source,
        GetAllEndpointQuery query = null)
    {
        var beforePagination = source
            .ApplyOrderBy(query)
            .ApplyFilter(query);
        return Pagination<TSource>.CreateSuccess(
            page: query?.page ?? 1,
            size: beforePagination.Count(),
            data: beforePagination
                    .ApplyPagination(query)
                    .SelectBy(query?.select ?? "*"));
    }

    public static IQueryable<TSource> ApplyOrderBy<TSource>(this IQueryable<TSource> source, GetAllEndpointQuery query = null)
    {
        if (query is null || string.IsNullOrEmpty(query.orderBy))
            return source;

        if (query.orderBy.Contains("Ascending"))
            return source.OrderBy(query.orderBy.Split(" Ascending")[0]);
        else if (query.orderBy.Contains("Descending"))
        {
            var propertyName = query.orderBy.Split(" Descending")[0];
            return source.OrderBy($"{propertyName} desc");
        }

        return source;
    }

    public static IQueryable<TSource> ApplyPagination<TSource>(this IQueryable<TSource> source, GetAllEndpointQuery query = null)
    {
        if (query is null)
            return source;

        return source.ApplyPagination(query.size, query.page, query.skip);
    }

    public static IQueryable<TSource> ApplyPagination<TSource>(this IQueryable<TSource> source, int size = 0, int page = 0, int skip = 0)
    {
        size = size > 0 ? size : 10;
        page = skip > 0 ? skip : ((page > 0 ? page : 1) - 1) * size;
        return source
            .Skip(page)
            .Take(size);
    }

    public static IQueryable<TSource> ApplyFilter<TSource>(
        this IQueryable<TSource> source, 
        GetAllEndpointQuery query)
    {
        if (query is null || string.IsNullOrEmpty(query.filter))
            return source;
        var f = query.filter.Split(",");
        foreach (var filter in f)
        {
            source = source.ApplyFilterGeneric(filter, $" {Op.Contains} ", Op.Contains);
            source = source.ApplyFilterGeneric(filter, $" {Op.Equals} ", Op.Equals);
            source = source.ApplyFilterGeneric(filter, $" {Op.GreaterThan} ", Op.GreaterThan);
            source = source.ApplyFilterGeneric(filter, $" {Op.GreaterThanOrEqual} ", Op.GreaterThanOrEqual);
            source = source.ApplyFilterGeneric(filter, $" {Op.EndsWith} ", Op.EndsWith);
            source = source.ApplyFilterGeneric(filter, $" {Op.StartsWith} ", Op.StartsWith);
            source = source.ApplyFilterGeneric(filter, $" {Op.LessThan} ", Op.LessThan);
            source = source.ApplyFilterGeneric(filter, $" {Op.LessThanOrEqual} ", Op.LessThanOrEqual);
            source = source.ApplyFilterGeneric(filter, $" {Op.AnyEquals} ", Op.AnyEquals);
            source = source.ApplyFilterGeneric(filter, $" {Op.AnyContains} ", Op.AnyContains);
            
            // source = source.ApplyFilterType(filter, $" {Op.Equals} ", Op.Equals);
            // source = source.ApplyFilterType(filter, $" {Op.GreaterThan} ", Op.GreaterThan);
            // source = source.ApplyFilterType(filter, $" {Op.GreaterThanOrEqual} ", Op.GreaterThanOrEqual);
            // source = source.ApplyFilterType(filter, $" {Op.EndsWith} ", Op.EndsWith);
            // source = source.ApplyFilterType(filter, $" {Op.StartsWith} ", Op.StartsWith);
            // source = source.ApplyFilterType(filter, $" {Op.LessThan} ", Op.LessThan);
            // source = source.ApplyFilterType(filter, $" {Op.LessThanOrEqual} ", Op.LessThanOrEqual);
        }

        return source;
    }

    public static IQueryable<TSource> ApplyFilterGeneric<TSource>(
        this IQueryable<TSource> source,
        string filter,
        string key,
        Op operation)
    {
        if (filter.Contains(key))
        {
            // Handle OR situation
            var filterSplitedByOr = filter.Split(" OR ");
            var queryStringBuilder = new List<string>();
            foreach (var expression in filterSplitedByOr)
            {
                var propertyName = expression.Split(key)[0];
                var value = expression.Split(key)[1];
                queryStringBuilder.Add(CreateQuery(propertyName, value, operation));
            }
            source = source.Where(string.Join(" OR ", queryStringBuilder));
        }
        
        return source;
    }

    private static string CreateQuery(string propertyName, string value, Op operation)
    {
        var query = new StringBuilder();
        switch (operation)
        {
            case Op.Equals:
                query.Append($"{propertyName} == \"{value}\"");
                break;
            case Op.GreaterThan:
                query.Append($"{propertyName} > \"{value}\"");
                break;
            case Op.LessThan:
                query.Append($"{propertyName} < \"{value}\"");
                break;
            case Op.GreaterThanOrEqual:
                query.Append($"{propertyName} >= \"{value}\"");
                break;
            case Op.LessThanOrEqual:
                query.Append($"{propertyName} <= \"{value}\"");
                break;
            case Op.Contains:
                query.Append($"{propertyName}.Contains(\"{value}\")");
                break;
            case Op.StartsWith:
                query.Append($"{propertyName}.StartsWith(\"{value}\")");
                break;
            case Op.EndsWith:
                query.Append($"{propertyName}.EndsWith(\"{value}\")");
                break;
            case Op.AnyEquals:
                var propertiesAnyEquals = propertyName.Split(".").ToList();
                var lastPropertyAnyEquals = propertiesAnyEquals[propertiesAnyEquals.Count - 1];
                propertiesAnyEquals.RemoveAt(propertiesAnyEquals.Count - 1);
                query.Append($"{string.Join(".", propertiesAnyEquals)}.Any({lastPropertyAnyEquals} == \"{value}\")");
                break;
            case Op.AnyContains:
                var properties = propertyName.Split(".").ToList();
                var lastProperty = properties[properties.Count - 1];
                properties.RemoveAt(properties.Count - 1);
                query.Append($"{string.Join(".", properties)}.Any({lastProperty}.Contains(\"{value}\"))");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        return query.ToString();
    }

    public static IQueryable<TSource> ApplyFilterType<TSource>(
        this IQueryable<TSource> source,
        string filter, 
        string key, 
        Op operation)
    {
        if (filter.Contains(key))
        {
            var target = Expression.Parameter(typeof(TSource));
            var propertyName = filter.Split(key)[0];
            var value = filter.Split(key)[1];
            if (string.IsNullOrEmpty(value))
                return source;

            return source.Provider.CreateQuery<TSource>(FilterExtensions.CreateWhereClause<TSource>
                (target, source.Expression, new Filter
                {
                    PropertyName = propertyName,
                    Operation = operation,
                    Value = value
                }));
        }
        return source;
    }

    public static IQueryable<TEntity> ApplyMultiTenantFilter<TEntity>(
        this IQueryable<TEntity> source,
        string userId,
        string tenantId,
        bool ignoreUser = false)
    where TEntity : IEntity
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return source;
        if (ignoreUser && userId == "02a01310-d8a0-48c0-a655-9755a91b4aff")
            return source;

        var entityType = typeof(TEntity);
        var multiTenantProperty = Activator.CreateInstance(entityType).GetPropertiesWithAttribute<MultiTenantAttribute>().FirstOrDefault();
        if (multiTenantProperty is null)
            return source;
        
        var propertyName = multiTenantProperty.Name;
        var target = Expression.Parameter(entityType);
        return source.Provider.CreateQuery<TEntity>(FilterExtensions.CreateWhereClause<TEntity>
            (target, source.Expression, new Filter
            {
                PropertyName = propertyName,
                Operation = Op.Equals,
                Value = tenantId
            }));
    }
}