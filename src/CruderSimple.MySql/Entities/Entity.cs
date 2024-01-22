using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Mapster;
using Newtonsoft.Json;

namespace CruderSimple.MySql.Entities;

[ExcludeFromCodeCoverage]
public abstract class Entity : IEntity
{
    [JsonProperty("Id")]
    [StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    protected BaseDto FromOutputBase<TEntity, TDto>()
        where TEntity : IEntity
        where TDto : BaseDto
    {
        var userConfig = TypeAdapterConfig<TEntity, TDto>
            .NewConfig()
            .ShallowCopyForSameType(true)
            .Config;
        
        return this.Adapt<TDto>(userConfig);
    }

    public abstract IEntity FromInput(BaseDto input);
    public abstract BaseDto ConvertToOutput();

    public virtual string GetPrimaryKey() 
        => Id;
}
