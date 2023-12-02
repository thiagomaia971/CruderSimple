using System.Diagnostics.CodeAnalysis;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Newtonsoft.Json;

namespace CruderSimple.MySql.Entities;

[ExcludeFromCodeCoverage]
public abstract class Entity : IEntity
{
    [JsonProperty("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [JsonProperty("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual IEntity FromInput(InputDto input)
    {
        Id = string.IsNullOrEmpty(input.Id) ? Guid.NewGuid().ToString() : input.Id;
        CreatedAt = CreatedAt == DateTime.MinValue ? DateTimeOffset.UtcNow : CreatedAt;
        UpdatedAt= DateTimeOffset.UtcNow;
        return this;
    }

    public virtual OutputDto ToOutput()
    {
        var output = new OutputDto(
            Id,
            CreatedAt,
            UpdatedAt);
        return output;
    }

    public virtual string GetPrimaryKey() 
        => Id;
}
