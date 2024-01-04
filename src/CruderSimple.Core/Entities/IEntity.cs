using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public interface IEntity
{
    public string Id { get; set; }
    public IEntity FromInput(InputDto input);
    OutputDto ConvertToOutput();
    public string GetPrimaryKey();
}

public static class EntityExtensions
{
    public static TOutput ToOutput<TOutput>(this IEntity entity)
        where TOutput : OutputDto 
        => (TOutput) entity?.ConvertToOutput();
    
    public static ICollection<TOutput> ToOutput<TEntity, TOutput>(this ICollection<TEntity> entities)
        where TEntity : IEntity
        where TOutput : OutputDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static List<TOutput> ToOutput<TEntity, TOutput>(this List<TEntity> entities)
        where TEntity : IEntity
        where TOutput : OutputDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static IEnumerable<TOutput> ToOutput<TEntity, TOutput>(this IEnumerable<TEntity> entities)
        where TEntity : IEntity
        where TOutput : OutputDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static IQueryable<TOutput> ToOutput<TEntity, TOutput>(this IQueryable<TEntity> entities)
        where TEntity : IEntity
        where TOutput : OutputDto 
        => entities.Select(x => x.ToOutput<TOutput>());
}
