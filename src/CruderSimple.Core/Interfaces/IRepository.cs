using CruderSimple.Core.Entities;

namespace CruderSimple.Core.Interfaces;

public interface IRepository<T>
    where T : IEntity
{
    IRepository<T> Add(T entity);
    IRepository<T> Update(T entity);
    IRepository<T> Remove(T entity);
    Task Save();
    Task<T> FindById(string id);
    Task<T> FindBy(string propertyName, string value);
    Task<Pagination<T>> GetAll();
}