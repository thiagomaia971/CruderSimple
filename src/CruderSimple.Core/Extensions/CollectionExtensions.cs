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

    public static IQueryable<TSource> ApplyFilter<TSource>(this IQueryable<TSource> source, string propertyName, string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return source;
        return source
            .ApplyFilterContains(propertyName, filter, "contains ", Op.Contains);
    }

    public static IQueryable<TSource> ApplyFilterContains<TSource>(this IQueryable<TSource> source, string propertyName, string filter, string key, Op operation)
    {
        if (filter.Contains(key))
        {
            var target = Expression.Parameter(typeof(TSource));
            var value = filter.Split(key)[1];

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