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

    public virtual BaseDto ConvertToOutput(IDictionary<string, object> cached = null)
    {
        if (cached is null)
            cached = new Dictionary<string, object>();
        if (cached.ContainsKey(Id))
            return null;
        cached.Add(Id, this);
        var output = new BaseDto(
            Id,
            CreatedAt,
            UpdatedAt);
        return output;
    }

    public virtual string GetPrimaryKey() 
        => Id;
}
