using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;
    protected TEntity Saved { get; set; }

    public virtual IRepository<TEntity> Add(TEntity entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        Saved = entity;
        if (autoDetach == AutoDetachOptions.AFTER)
            DbContext.AutoDetach(Saved);
        DbContext.Add(entity); 
        if (autoDetach == AutoDetachOptions.BEFORE)
            DbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepository<TEntity> Update(TEntity entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Saved = entity;
        if (autoDetach == AutoDetachOptions.AFTER)
            DbContext.AutoDetach(Saved);
        DbContext.Update(entity);
        if (autoDetach == AutoDetachOptions.BEFORE)
            DbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepository<TEntity> Remove(TEntity entity)
    {
        // entity.DeleteMethod(0);
        Saved = entity;
        DbContext.Update(entity);
        return this;
    }

    public virtual async Task Save(bool withoutTracking = true)
    {
        Saved.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        
        if (withoutTracking)
        {
            var referencesId = Saved.GetReferences();
            var entries = dbContext.ChangeTracker.Entries().ToList();
            dbContext.ChangeTracker.Clear();
            var groupBy = entries
                .Where(x => referencesId.ContainsKey(((IEntity)x.Entity).Id))
                .GroupBy(x => ((IEntity)x.Entity).Id)
                .ToList();
            
            foreach (var group in groupBy)
            {
                var entry = group.FirstOrDefault();
                var entryEntity = (IEntity) entry.Entity;
                
                if (group.Count() > 1)
                {
                    if (entryEntity.DeletedAt.HasValue)
                        entryEntity.DeleteMethod(0);
                    dbContext.Entry(entry.Entity).State = EntityState.Modified;
                }
                else
                {
                    if (entryEntity.DeletedAt.HasValue)
                        entryEntity.DeleteMethod(0);
                    
                    if (entry.State == EntityState.Deleted)
                        dbContext.Entry(entry.Entity).State = EntityState.Modified;
                    else
                        dbContext.Entry(entry.Entity).State = entry.State;
                }
            }
            // var tracked = new List<string>();
            // foreach (var group in entries.GroupBy(x => ((IEntity)x.Entity).Id))
            // {
            //     var entry = group.FirstOrDefault();
            //     var entryEntity = (IEntity) entry.Entity;
            //     if (tracked.Contains(entryEntity.Id))
            //         continue;
            //     tracked.Add(entryEntity.Id);
            //     
            //     if (group.Count() > 1)
            //     {
            //         if (entryEntity.DeletedAt.HasValue)
            //             entryEntity.DeleteMethod(0);
            //         dbContext.Entry(entry.Entity).State = EntityState.Modified;
            //     }
            //     else
            //     {
            //         
            //         if (entryEntity.DeletedAt.HasValue)
            //             entryEntity.DeleteMethod(0);
            //         if (entry.State == EntityState.Deleted)
            //         {
            //             // ((IEntity) entry.Entity).DeleteMethod(0);
            //             dbContext.Entry(entry.Entity).State = EntityState.Modified;
            //         }
            //         else
            //             dbContext.Entry(entry.Entity).State = entry.State;
            //     }
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
            .ApplyMultiTenantFilter(multiTenant?.UserId ?? string.Empty, multiTenant?.Id ?? string.Empty, ignoreUser)
            .OrderBy(x => x.CreatedAt);
}