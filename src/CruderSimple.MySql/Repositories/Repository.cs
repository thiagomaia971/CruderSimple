using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Entities;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    public IRepository<TEntity> Add(TEntity entity)
    {
        if (entity is TenantEntity m)
            m.UserId = multiTenant.UserId;

        dbContext.Add(entity);
        return this;
    }

    public IRepository<TEntity> Update(TEntity entity)
    {
        if (entity is TenantEntity m)
            m.UserId = multiTenant.UserId;

        dbContext.Update(entity);
        return this;
    }

    public IRepository<TEntity> Remove(TEntity entity)
    {
        dbContext.Remove(entity);
        return this;
    }

    public async Task Save()
    {
        await dbContext.SaveChangesAsync();
    }

    public Task<TEntity> FindById(string id) 
        => dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);

    public Task<TEntity> FindBy(string propertyName, string value)
        => FindById(value);

    public Task<Pagination<TEntity>> GetAll()
    {
        var pagination = new Pagination<TEntity>
        {
            Size = dbContext.Set<TEntity>().Count(),
            Data = dbContext.Set<TEntity>()
        };
        
        return Task.FromResult(pagination);
    }
}