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

    public virtual IEntity FromInput(InputDto input)
    {
        Id = string.IsNullOrEmpty(input.Id) ? Guid.NewGuid().ToString() : input.Id;
        CreatedAt = CreatedAt == DateTime.MinValue ? DateTime.UtcNow : CreatedAt;
        UpdatedAt= DateTime.UtcNow;
        return this;
    }

    public virtual OutputDto ToOutput()
    {
        var output = new OutputDto(
            Id,
            CreatedAt.ToString("O"),
            UpdatedAt?.ToString("O"));
        return output;
    }

    public virtual string GetPrimaryKey() 
        => Id;
}
