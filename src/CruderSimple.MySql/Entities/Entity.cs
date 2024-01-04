using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Newtonsoft.Json;

namespace CruderSimple.MySql.Entities;

[ExcludeFromCodeCoverage]
public abstract class Entity : IEntity
{
    [JsonProperty("Id")]
    [StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    public virtual IEntity FromInput(BaseDto input)
    {
        Id = string.IsNullOrEmpty(input.Id) ? Guid.NewGuid().ToString() : input.Id;
        CreatedAt = CreatedAt == DateTime.MinValue ? DateTime.UtcNow : CreatedAt;
        UpdatedAt= DateTime.UtcNow;
        return this;
    }

    public virtual BaseDto ConvertToOutput()
    {
        var output = new BaseDto(
            Id,
            CreatedAt,
            UpdatedAt);
        return output;
    }

    public virtual string GetPrimaryKey() 
        => Id;
}
