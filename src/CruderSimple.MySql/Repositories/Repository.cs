using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.ViewModels;
using CruderSimple.MySql.Entities;
using CruderSimple.MySql.Extensions;
using CruderSimple.MySql.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;

    public virtual IRepositoryBase<TEntity> Add(TEntity entity, bool autoDetach = true)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        if (autoDetach)
            DbContext.AutoDetach(entity);
        DbContext.Add(entity);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Update(TEntity entity, bool autoDetach = true)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        if (autoDetach)
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

    public virtual Task<TEntity> FindById(string id, string select = "*", bool asNoTracking = false) 
        => Query(asNoTracking, true)
            .SelectBy(select)
            .FirstOrDefaultAsync(x => x.Id == id);

    public virtual Task<TEntity> FindBy(string propertyName, string value)
        => FindById(value);

    public virtual Task<Pagination<TEntity>> GetAll(GetAllEndpointQuery query = null, bool asNoTracking = false) 
        => Task.FromResult(Query(asNoTracking, false)
            .ApplyQuery(query));

    protected virtual IQueryable<TEntity> Query(bool asNoTracking = false, bool ignoreUser = false) 
        => DbSet
            .IgnoreAutoIncludes()
            .AsNoTracking(asNoTracking)
            .ApplyMultiTenantFilter(multiTenant?.UserId ?? string.Empty, multiTenant?.Id ?? string.Empty, ignoreUser)
            .OrderBy(x => x.CreatedAt);
}
