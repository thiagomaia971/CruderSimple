using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;

namespace CruderSimple.Core.Interfaces;

public interface IRepositoryBase<T>
    where T : IEntity
{
    IRepositoryBase<T> Add(T entity, bool autoDetach = true);
    IRepositoryBase<T> Update(T entity, bool autoDetach = true);
    IRepositoryBase<T> Remove(T entity);
    Task Save();
    Task<T> FindById(string id, string select = "*", bool asNoTracking = false);
    Task<T> FindBy(string propertyName, string value);
    Task<Pagination<T>> GetAll(GetAllEndpointQuery query = null, bool asNoTracking = false);
}