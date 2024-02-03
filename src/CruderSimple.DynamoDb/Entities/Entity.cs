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
    private DateTime _createdAt;
    private DateTime? _updatedAt;

    [DynamoDBHashKey("Id")]
    [JsonProperty("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    DateTime IEntity.CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value;
    }

    DateTime? IEntity.UpdatedAt
    {
        get => _updatedAt;
        set => _updatedAt = value;
    }

    public DateTime? DeletedAt { get; set; }

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

    public void DeleteMethod(int modifiedBy)
    {
        throw new NotImplementedException();
    }

    public virtual IEntity FromInput(BaseDto input)
    {
        Id = input.Id;
        return this;
    }

    public BaseDto ConvertToOutput()
    {
        throw new NotImplementedException();
    }

    public BaseDto ConvertToOutput(IDictionary<string, bool> cached = null)
    {
        var output = new BaseDto(
            Id,
            DateTime.Parse(CreatedAt),
            string.IsNullOrEmpty(UpdatedAt) ? DateTime.Parse(UpdatedAt) : null);
        return output;
    }

    public string GetPrimaryKey() 
        => PrimaryKey;
}
