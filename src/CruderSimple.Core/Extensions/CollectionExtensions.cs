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
        return Pagination<TSource>.CreateSuccess(
            page: query?.page ?? 1,
            size: source.Count(),
            data: source
                    .ApplyOrderBy(query)
                    .ApplyFilter(query)
                    .ApplyPagination(query)
                    .SelectBy(query?.select ?? "*"));
    }

    public static IQueryable<TSource> ApplyOrderBy<TSource>(this IQueryable<TSource> source, GetAllEndpointQuery query = null)
    {
        if (query is null || string.IsNullOrEmpty(query.orderBy))
            return source;

        var target = Expression.Parameter(typeof(TSource));
        if (query.orderBy.Contains("Ascending"))
        {
            var propertyName = query.orderBy.Split(" Ascending")[0];
            return source.OrderBy(propertyName);
        }
        else if (query.orderBy.Contains("Descending"))
        {
            var propertyName = query.orderBy.Split(" Descending")[0];
            return source.OrderByDescending(propertyName);
        }

        return source;
    }

    public static IQueryable<TSource> ApplyPagination<TSource>(this IQueryable<TSource> source, GetAllEndpointQuery query = null)
    {
        if (query is null)
            return source;
        var size = query.size > 0 ? query.size : 10;
        var page = ((query.page > 0 ? query.page : 1) - 1) * size;
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
            var propertyName = filter.Split(key)[0];
            var value = filter.Split(key)[1];
            var query = CreateQuery(propertyName, value, operation);
            source = source.Where(query);
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
        string tenantId)
    where TEntity : IEntity
    {
        if (string.IsNullOrEmpty(tenantId))
            return source;
        
        var entityType = typeof(TEntity);
        var multiTenantProperty = entityType.GetPropertiesWithAttribute<MultiTenantAttribute>().FirstOrDefault();
        if (multiTenantProperty is null)
            return source;
        
        var propertyName = multiTenantProperty.Name;
        var target = Expression.Parameter(typeof(TEntity));
        return source.Provider.CreateQuery<TEntity>(FilterExtensions.CreateWhereClause<TEntity>
            (target, source.Expression, new Filter
            {
                PropertyName = propertyName,
                Operation = Op.Equals,
                Value = tenantId
            }));
    }
}