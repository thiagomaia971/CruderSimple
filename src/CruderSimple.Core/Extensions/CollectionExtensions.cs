using System.Linq.Expressions;
using System.Reflection;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Extensions;

public static class CollectionExtensions
{
    public static ICollection<TEntity> FromInput<TEntity, TInput>(this ICollection<TEntity> entities,
        ICollection<TInput> inputs)
        where TEntity : IEntity
        where TInput : InputDto
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
    public static IEnumerable<TEntity> FromInput<TEntity, TInput>(this IEnumerable<TEntity> entities,
        ICollection<TInput> inputs)
        where TEntity : IEntity
        where TInput : InputDto
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
    public static List<TEntity> FromInput<TEntity, TInput>(this List<TEntity> entities,
        ICollection<TInput> inputs)
        where TEntity : IEntity
        where TInput : InputDto
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
                .ApplyPagination(query)
                .ApplyFilter<TSource>(query)
                .SelectBy(query?.select ?? "*"));
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
        var filters = new List<Filter>();
        var f = query.filter.Split(",");
        foreach (var filter in f)
        {
            source = source.ApplyFilterContains(filter, $" {Op.Contains} ", Op.Contains);
            source = source.ApplyFilterContains(filter, $" {Op.Equals} ", Op.Equals);
            source = source.ApplyFilterContains(filter, $" {Op.GreaterThan} ", Op.GreaterThan);
            source = source.ApplyFilterContains(filter, $" {Op.GreaterThanOrEqual} ", Op.GreaterThanOrEqual);
            source = source.ApplyFilterContains(filter, $" {Op.EndsWith} ", Op.EndsWith);
            source = source.ApplyFilterContains(filter, $" {Op.StartsWith} ", Op.StartsWith);
            source = source.ApplyFilterContains(filter, $" {Op.LessThan} ", Op.LessThan);
            source = source.ApplyFilterContains(filter, $" {Op.LessThanOrEqual} ", Op.LessThanOrEqual);
        }

        return source;
    }

    public static IQueryable<TSource> ApplyFilterContains<TSource>(
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

    public static IQueryable<TSource> SkipIf<TSource>(this IQueryable<TSource> source, Func<bool> func, int count)
    {
        if (func.Invoke())
            return source.Skip(count - 1);
        return source;
    }

    public static IQueryable<TSource> TakeIf<TSource>(this IQueryable<TSource> source, Func<bool> func, int size)
    {
        if (func.Invoke())
            return source.Take(size);
        return source;
    }
}