using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2.DataModel;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using CruderSimple.DynamoDb.Attributes;
using Newtonsoft.Json;

namespace CruderSimple.DynamoDb.Entities;

[ExcludeFromCodeCoverage]
public abstract class Entity : IEntity
{
    [DynamoDBHashKey("Id")]
    [JsonProperty("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [DynamoDBRangeKey("CreatedAt")]
    [DynamoDbGsi("GSI-CreatedAt")]
    [JsonProperty("CreatedAt")]
    public string CreatedAt { get; set; }

    [DynamoDbGsi("GSI-InheritedKey")]
    [JsonProperty("InheritedKey")]
    public string InheritedKey { get; set; }
    
    [DynamoDbGsi("GSI-EntityType")]
    [DynamoDBProperty("EntityType")]
    [JsonProperty("EntityType")]
    public string EntityType { get; set; }
    
    [DynamoDbGsi("GSI-InheritedType")]
    [DynamoDBProperty("InheritedType")]
    [JsonProperty("InheritedType")]
    public string InheritedType { get; set; }
    
    [DynamoDbGsi("GSI-PrimaryKey")]
    [DynamoDBProperty("GSI-PrimaryKey")]
    [JsonProperty("GSI-PrimaryKey")]
    public string PrimaryKey { get; set; }
    
    [DynamoDbGsi("GSI-PrimaryInheritedKey")]
    [DynamoDBProperty("GSI-PrimaryInheritedKey")]
    [JsonProperty("GSI-PrimaryInheritedKey")]
    public string PrimaryInheritedKey { get; set; }
    
    [DynamoDBProperty("UpdatedAt")]
    [JsonProperty("UpdatedAt")]
    public string UpdatedAt { get; set; }

    public Entity()
    {
        EntityType = GetType().FullName;
    }

    public virtual IEntity FromInput(InputDto input)
    {
        Id = input.Id;
        return this;
    }

    public OutputDto ConvertToOutput()
    {
        var output = new OutputDto(
            Id,
            CreatedAt,
            UpdatedAt);
        return output;
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
        => PrimaryKey;
}
