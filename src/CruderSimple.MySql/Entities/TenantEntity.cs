using CruderSimple.Core.Entities;

namespace CruderSimple.MySql.Entities;

public class TenantEntity<TUser> : Entity, ITenantEntity
    where TUser : IEntity
{
    public string UserId { get; set; }
    public TUser User { get; set; }
}