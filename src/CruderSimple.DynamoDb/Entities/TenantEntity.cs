using Amazon.DynamoDBv2.DataModel;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.DynamoDb.Entities;

public class TenantEntity : Entity
{
    [DynamoDBProperty("UserId")]
    public string UserId { get; set; }

    public override Entity FromInput(InputDto input)
    {
        throw new NotImplementedException();
    }

    public override OutputDto ToOutput()
    {
        throw new NotImplementedException();
    }
}