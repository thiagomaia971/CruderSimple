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
    protected TEntity Saved { get; set; }

    public virtual IRepositoryBase<TEntity> Add(TEntity entity, bool autoDetach = true)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        Saved = entity;
        DbContext.Add(entity); 
        if (autoDetach)
            DbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Update(TEntity entity, bool autoDetach = true)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Saved = entity;
        DbContext.Update(entity);
        if (autoDetach)
            DbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Remove(TEntity entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        Saved = entity;
        DbContext.Update(entity);
        return this;
    }

    public virtual async Task Save(bool withoutTracking = true)
    {
        Saved.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        
        if (withoutTracking)
        {
            var entries = dbContext.ChangeTracker.Entries().ToList();
            dbContext.ChangeTracker.Clear();
            foreach (var group in entries.GroupBy(x => ((IEntity)x.Entity).Id))
            {
                var entry = group.FirstOrDefault();
                if (group.Count() > 1)
                {
                    dbContext.Entry(entry.Entity).State = EntityState.Modified;
                }
                else
                {
                    if (entry.State == EntityState.Deleted)
                        dbContext.Entry(entry.Entity).State = EntityState.Modified;
                    else
                        dbContext.Entry(entry.Entity).State = entry.State;
                }
            }
            
            // foreach (var entry in entries.DistinctBy(x => ((IEntity)x.Entity).Id))
            // {
            //     if (entry.State == EntityState.Deleted)
            //         dbContext.Entry(entry.Entity).State = EntityState.Modified;
            //     else
            //         dbContext.Entry(entry.Entity).State = entry.State;
            // }
        }
        await DbContext.SaveChangesAsync();
    }

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
            .Where(x => !x.DeletedAt.HasValue)
            .ApplyMultiTenantFilter(multiTenant?.UserId ?? string.Empty, multiTenant?.Id ?? string.Empty, ignoreUser)
            .OrderBy(x => x.CreatedAt);
}
