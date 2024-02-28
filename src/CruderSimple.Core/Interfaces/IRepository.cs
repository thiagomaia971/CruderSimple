using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;

namespace CruderSimple.Core.Interfaces;

public interface IRepository<T>
    where T : IEntity
{
    IRepository<T> Add(T entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE);
    IRepository<T> Update(T entity, AutoDetachOptions autoDetach = AutoDetachOptions.BEFORE);
    IRepository<T> Remove(T entity);
    Task Save(bool withoutTracking = true);
    Task<T> FindById(string id, string select = "*", bool asNoTracking = false);
    Task<T> FindBy(string propertyName, string value);
    Task<Pagination<T>> GetAll(GetAllEndpointQuery query = null, bool asNoTracking = false);
}

public enum AutoDetachOptions
{
    NONE,
    AFTER,
    BEFORE
}