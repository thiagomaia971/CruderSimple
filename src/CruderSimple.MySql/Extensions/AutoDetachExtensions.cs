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
        if (detached is null)
            detached = new List<string>();
        else
            detached.Add($"{entity.GetType()}:{entity.Id}");
        
        var autoDetachProperties = entity.GetPropertiesWithAttribute<AutoDetachAttribute>();
        foreach (var autoDetachProperty in autoDetachProperties)
        {
            if (autoDetachProperty.GetValue(entity) is IEntity ent)
                dbContext.DetachLocal(ent, ent.Id);
            else
            {
                var list = (autoDetachProperty.GetValue(entity) as IEnumerable);
                if (list is null)
                    continue;
                foreach (var v in list)
                {
                    if (v is not null && !detached.Contains($"{v.GetType()}:{((IEntity) v).Id}"))
                        dbContext.AutoDetach((IEntity) v, detached);
                }
            }
        }
    }
}