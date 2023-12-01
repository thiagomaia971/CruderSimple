using CruderSimple.Core.Entities;

namespace CruderSimple.MySql.Entities;

public class TenantEntity : Entity, ITenantEntity
{
    public string UserId { get; set; }
}