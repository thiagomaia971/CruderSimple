using System.Collections;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Attributes;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CruderSimple.MySql.Extensions;

public static class AutoDetachExtensions
{
    public static IQueryable<TSource> AsNoTracking<TSource>(this IQueryable<TSource> source, bool asNoTracking)
        where TSource : class
    {
        if (asNoTracking)
            return source.AsNoTrackingWithIdentityResolution();
        return source;
    }

    public static IQueryable<TSource> AsNoTracking<TSource, TProperty>(this IIncludableQueryable<TSource, TProperty> source, bool asNoTracking)
        where TSource : class
    {
        if (asNoTracking)
            return source.AsNoTrackingWithIdentityResolution();
        return source;
    }

    public static void AutoDetach(this DbContext dbContext, IEntity entity, List<string> detached = null)
    {
        if (detached is not null && detached.Contains($"{entity.GetType()}:{entity.Id}"))
            return;
        
        if (detached is null)
            detached = new List<string>();
        detached.Add($"{entity.GetType()}:{entity.Id}");
        
        var allEntities = entity.GetPropertiesFromInhiredType<IEntity>();

        foreach (var innerEntityProperty in allEntities)
        {
            var autoDetachAttribute =
                innerEntityProperty.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(AutoDetachAttribute));

            if (innerEntityProperty.GetValue(entity) is IEntity innerEntity)
            {
                if (autoDetachAttribute is null)
                    dbContext.AutoDetach(innerEntity, detached);
                else if (!detached.Contains($"{innerEntity.GetType()}:{innerEntity.Id}"))
                {
                    // dbContext.DetachLocal(innerEntity);
                    innerEntityProperty.SetValue(entity, null);
                }
            }
            else
            {
                var list = (innerEntityProperty.GetValue(entity) as IEnumerable);
                if (list is null)
                    continue;
                foreach (var v in list.Adapt<IEnumerable>())
                {
                    if (v is null)
                        continue;
                    var innerEntityList = (IEntity)v;
                    if (autoDetachAttribute is null)
                        dbContext.AutoDetach(innerEntityList, detached);
                    else if (!detached.Contains($"{innerEntityList.GetType()}:{innerEntityList.Id}"))
                    {
                        dbContext.DetachLocal(innerEntityList);
                        // list.Remove(innerEntityList);
                    }
                }
            }
        }
        
    }
}