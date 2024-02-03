using System.Collections;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using Mapster;

namespace CruderSimple.MySql.Extensions;

public static class EntityExtensions
{
    public static Dictionary<string, IEntity> GetReferences(this IEntity entity, Dictionary<string, IEntity> references = null)
    {
        if (references == null)
            references = new Dictionary<string, IEntity>();
        if (references.ContainsKey(entity.Id))
            return references;
        
        references.Add(entity.Id, entity);
        var allEntities = entity.GetPropertiesFromInhiredType<IEntity>();

        foreach (var innerEntityProperty in allEntities)
        {
            if (innerEntityProperty.GetValue(entity) is IEntity innerEntity)
            {
                var innerReferences = GetReferences(innerEntity, references);
                foreach (var reference in innerReferences.Where(x => !references.ContainsKey(x.Key)))
                    references.Add(reference.Key, reference.Value);
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
                    var innerReferences = GetReferences(innerEntityList, references);
                    foreach (var reference in innerReferences.Where(x => !references.ContainsKey(x.Key)))
                        references.Add(reference.Key, reference.Value);
                }
            }
        }

        return references;
    }
}