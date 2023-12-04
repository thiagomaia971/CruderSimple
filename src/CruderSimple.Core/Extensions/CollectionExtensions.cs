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
    
    public static ICollection<TOutput> ToOutput<TEntity, TOutput>(this ICollection<TEntity> entities)
        where TEntity : IEntity
        where TOutput : OutputDto 
        => entities.Select(x => x.ToOutput()).Cast<TOutput>().ToList();
    
    public static TOutput ToOutput<TOutput>(this IEntity entity)
        where TOutput : OutputDto 
        => (TOutput) entity.ToOutput();
}