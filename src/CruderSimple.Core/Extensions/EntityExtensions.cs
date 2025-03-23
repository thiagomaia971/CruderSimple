using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Mapster;

namespace CruderSimple.Core.Extensions;

public static class EntityExtensions
{
    public static TOutput ToOutput<TOutput>(this IEntity entity)
        where TOutput : BaseDto 
        => (TOutput) entity?.ConvertToOutput();
    
    public static ICollection<TOutput> ToOutput<TEntity, TOutput>(this ICollection<TEntity> entities)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static List<TOutput> ToOutput<TEntity, TOutput>(this List<TEntity> entities)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static IEnumerable<TOutput> ToOutput<TEntity, TOutput>(this IEnumerable<TEntity> entities)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>()).ToList();
    
    public static IQueryable<TOutput> ToOutput<TEntity, TOutput>(this IQueryable<TEntity> entities)
        where TEntity : IEntity
        where TOutput : BaseDto 
        => entities.Select(x => x.ToOutput<TOutput>());
    
    public static IEntity ParseWithContext<TEntity, TDto>(this TEntity entityBase, BaseDto input, TypeAdapterConfig typeConfig= null)
        where TEntity : IEntity
        where TDto : BaseDto
    {
        if (typeConfig == null)
            typeConfig = TypeAdapterConfig<TDto, TEntity>.NewConfig()
            // .IgnoreNullValues(false)
            .Config;
        
        var entityToSave = input.Adapt<TEntity>(typeConfig);
        return entityToSave.BuildAdapter(TypeAdapterConfig<TEntity, TEntity>.NewConfig()
                // .ShallowCopyForSameType(false)
                // .PreserveReference(false)
                .Config)
            .AdaptTo(entityBase);
    }
}
