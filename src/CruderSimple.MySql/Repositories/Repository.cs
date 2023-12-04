using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Entities;
using CruderSimple.MySql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CruderSimple.MySql.Repositories;

public class Repository<TEntity>(DbContext dbContext, MultiTenantScoped multiTenant) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

    protected MultiTenantScoped MultiTenant { get; } = multiTenant;

    public virtual IRepositoryBase<TEntity> Add(TEntity entity)
    {
        dbContext.Add(entity);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Update(TEntity entity)
    {
        dbContext.Update(entity);
        return this;
    }

    public virtual IRepositoryBase<TEntity> Remove(TEntity entity)
    {
        dbContext.Remove(entity);
        return this;
    }

    public virtual async Task Save()
    {
        await dbContext.SaveChangesAsync();
    }

    public virtual Task<TEntity> FindById(string id) 
        => Query().FirstOrDefaultAsync(x => x.Id == id);

    public virtual Task<TEntity> FindBy(string propertyName, string value)
        => FindById(value);

    public virtual Task<Pagination<TEntity>> GetAll()
    {
        var pagination = new Pagination<TEntity>
        {
            Size = Query().Count(),
            Data = Query()
        };
        
        return Task.FromResult(pagination);
    }

    protected virtual IQueryable<TEntity> Query()
    {
        return dbSet;
    }
}
