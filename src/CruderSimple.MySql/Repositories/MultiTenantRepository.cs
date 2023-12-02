using CruderSimple.Core.Entities;
using CruderSimple.MySql.Entities;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class MultiTenantRepository<TEntity, TTenantEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : Repository<TEntity>(dbContext, multiTenant)
    where TEntity : TenantEntity<TTenantEntity>
    where TTenantEntity : IEntity
{
    protected override IQueryable<TEntity> Query() 
        => base.Query().Where(x => x.UserId == MultiTenant.UserId);
}