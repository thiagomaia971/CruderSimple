using Amazon.DynamoDBv2.DataModel;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.DynamoDb.Entities;

public abstract class TenantEntity : Entity, ITenantEntity
{
    [DynamoDBProperty("UserId")]
    public string UserId { get; set; }
}