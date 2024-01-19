using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Extensions;

public static class EntityExtensions
{
    public static TOutput ToOutput<TOutput>(this IEntity entity, IDictionary<string, object> cached = null)
        where TOutput : BaseDto 
        => (TOutput) entity?.ConvertToOutput(cached);
    
    public static ICollection<TOutput> ToOutput<TEntity, TOutput>(this ICollection<TEntity> entities, IDictionary<string, object> cached = null)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>(cached)).ToList();
    
    public static List<TOutput> ToOutput<TEntity, TOutput>(this List<TEntity> entities, IDictionary<string, object> cached = null)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>(cached)).ToList();
    
    public static IEnumerable<TOutput> ToOutput<TEntity, TOutput>(this IEnumerable<TEntity> entities, IDictionary<string, object> cached = null)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>(cached)).ToList();
    
    public static IQueryable<TOutput> ToOutput<TEntity, TOutput>(this IQueryable<TEntity> entities, IDictionary<string, object> cached = null)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>(cached));
}
