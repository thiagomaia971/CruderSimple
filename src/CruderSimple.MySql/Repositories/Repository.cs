using System.Collections;
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

public class Repository<TEntity>(DbContext managementDbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext ManagementDbContext = managementDbContext;
    protected readonly DbSet<TEntity> DbSet = managementDbContext.Set<TEntity>();

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;
    protected TEntity Saved { get; set; }

    public virtual IRepositoryBase<TEntity> Add(TEntity entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE)
    {
        entity.CreateMethod(0);
        Saved = entity;
        if (autoDetach == AutoDetachOptions.AFTER)
            ManagementDbContext.AutoDetach(Saved);
        ManagementDbContext.Add(entity); 
        if (autoDetach == AutoDetachOptions.BEFORE)
            ManagementDbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Update(TEntity entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE)
    {
        entity.UpdateMethod(0);
        Saved = entity;
        if (autoDetach == AutoDetachOptions.AFTER)
            ManagementDbContext.AutoDetach(Saved);
        ManagementDbContext.Update(entity);
        if (autoDetach == AutoDetachOptions.BEFORE)
            ManagementDbContext.AutoDetach(Saved);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Remove(TEntity entity)
    {
        entity.DeleteMethod(0);
        Saved = entity;
        ManagementDbContext.Update(entity);
        return this;
    }

    public virtual async Task Save(bool withoutTracking = true)
    {
        Saved.SetAllMultiTenant(MultiTenant.MultiTenantType, MultiTenant.Id);
        
        if (withoutTracking)
        {
            var referencesId = Saved.GetReferences();
            var entries = managementDbContext.ChangeTracker.Entries().ToList();
            managementDbContext.ChangeTracker.Clear();
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
                    managementDbContext.Entry(entry.Entity).State = EntityState.Modified;
                }
                else
                {
                    if (entryEntity.DeletedAt.HasValue)
                        entryEntity.DeleteMethod(0);
                    
                    if (entry.State == EntityState.Deleted)
                        managementDbContext.Entry(entry.Entity).State = EntityState.Modified;
                    else
                        managementDbContext.Entry(entry.Entity).State = entry.State;
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
        await ManagementDbContext.SaveChangesAsync();
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