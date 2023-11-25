using CruderSimple.Core.Entities;
using CruderSimple.DynamoDb.Entities;

namespace CruderSimple.DynamoDb.Interfaces;

public interface IRepository<T>
    where T : Entity
{
    
    Task<T> Save(T entity);
    DynamoDbQueryBuilder<T> CreateQuery();
    Task<T> FindById(string id);
    Task<Pagination<T>> GetAll();
    Task Remove(T entity);
}