using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Entities;
using CruderSimple.MySql.Extensions;
using CruderSimple.MySql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

    public IRepositoryBase<TEntity> Add(TEntity entity)
    {
        var userIdProp = entity.GetType().GetProperties().FirstOrDefault(x => x.Name == "UserId");
        if (userIdProp != null) 
            userIdProp.SetValue(entity, multiTenant.UserId);

        dbContext.Add(entity);
        return this;
    }

    public IRepositoryBase<TEntity> Update(TEntity entity)
    {
        var userIdProp = entity.GetType().GetProperties().FirstOrDefault(x => x.Name == "UserId");
        if (userIdProp != null) 
            userIdProp.SetValue(entity, multiTenant.UserId);

        dbContext.Update(entity);
        return this;
    }

    public IRepositoryBase<TEntity> Remove(TEntity entity)
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