using System.Collections;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Attributes;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Extensions;

public static class AutoDetachExtensions
{
    public static void AutoDetach(this DbContext dbContext, IEntity entity)
    {
        var autoDetachProperties = entity.GetPropertiesWithAttribute<AutoDetachAttribute>();
        foreach (var autoDetachProperty in autoDetachProperties)
        {
            if (autoDetachProperty.GetValue(entity) is IEntity ent)
                dbContext.DetachLocal(ent, ent.Id);
            else
            {
                var list = (autoDetachProperty.GetValue(entity) as IEnumerable);
                foreach (var v in list)
                {
                    if (v is not null)
                        dbContext.AutoDetach((IEntity) v);
                }
            }
        }
    }
}