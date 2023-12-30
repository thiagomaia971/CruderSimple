namespace CruderSimple.Core.Entities;

public class MultiTenantScoped(Type multiTenantType)
{
    public Type MultiTenantType { get; } = multiTenantType;

    public string Id { get; set; }
}