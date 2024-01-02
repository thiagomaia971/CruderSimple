using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Entities;
using CruderSimple.MySql.Extensions;
using CruderSimple.MySql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;

    public virtual IRepositoryBase<TEntity> Add(TEntity entity)
    {
        entity.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        DbContext.AutoDetach(entity);
        DbContext.Add(entity);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Update(TEntity entity)
    {
        entity.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        DbContext.AutoDetach(entity);
        DbContext.Update(entity);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Remove(TEntity entity)
    {
        DbContext.Remove(entity);
        return this;
    }

    public virtual async Task Save() 
        => await DbContext.SaveChangesAsync();

    public virtual Task<TEntity> FindById(string id) 
        => Query().FirstOrDefaultAsync(x => x.Id == id);

    public virtual Task<TEntity> FindBy(string propertyName, string value)
        => FindById(value);

    public virtual Task<Pagination<TEntity>> GetAll(GetAllEndpointQuery query = null) 
        => Task.FromResult(Query().ApplyQuery(query));

    protected virtual IQueryable<TEntity> Query() 
        => DbSet.OrderBy(x => x.CreatedAt);
}
