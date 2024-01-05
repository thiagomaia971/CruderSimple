using System.Collections;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Attributes;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Extensions;

public static class AutoDetachExtensions
{
    public static void AutoDetach(this DbContext dbContext, IEntity entity, List<string> detached = null)
    {
        if (detached is not null && detached.Contains($"{entity.GetType()}:{entity.Id}"))
            return;
        
        if (detached is null)
            detached = new List<string>();
        else
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
                else
                    dbContext.DetachLocal(innerEntity, innerEntity.Id);
            }
            else
            {
                var list = (innerEntityProperty.GetValue(entity) as IEnumerable);
                if (list is null)
                    continue;
                foreach (var v in list)
                {
                    if (v is null)
                        continue;
                    if (autoDetachAttribute is null)
                        dbContext.AutoDetach((IEntity) v, detached);
                    else 
                        dbContext.DetachLocal((IEntity) v, ((IEntity) v).Id);
                }
            }
        }
        
    }
}