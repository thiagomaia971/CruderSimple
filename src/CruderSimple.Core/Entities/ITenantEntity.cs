namespace CruderSimple.Core.Entities;

public interface ITenantEntity : IEntity
{
    public string UserId { get; set; }
}