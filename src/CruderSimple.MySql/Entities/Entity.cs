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
    public string CreatedAt { get; set; }
    
    [JsonProperty("UpdatedAt")]
    public string UpdatedAt { get; set; }

    public virtual IEntity FromInput(InputDto input)
    {
        Id = input.Id;
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

    public string GetPrimaryKey() 
        => Id;
}
