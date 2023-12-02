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

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;

    public IRepositoryBase<TEntity> Add(TEntity entity)
    {
        var userIdProp = entity.GetType().GetProperties().FirstOrDefault(x => x.Name == "UserId");
        if (userIdProp != null) 
            userIdProp.SetValue(entity, MultiTenant.UserId);

        dbContext.Add(entity);
        return this;
    }

    public IRepositoryBase<TEntity> Update(TEntity entity)
    {
        var userIdProp = entity.GetType().GetProperties().FirstOrDefault(x => x.Name == "UserId");
        if (userIdProp != null) 
            userIdProp.SetValue(entity, MultiTenant.UserId);

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
        => Query().FirstOrDefaultAsync(x => x.Id == id);

    public Task<TEntity> FindBy(string propertyName, string value)
        => FindById(value);

    public Task<Pagination<TEntity>> GetAll()
    {
        var pagination = new Pagination<TEntity>
        {
            Size = Query().Count(),
            Data = Query()
        };
        
        return Task.FromResult(pagination);
    }

    protected virtual IQueryable<TEntity> Query() 
        => dbContext.Set<TEntity>();
}
