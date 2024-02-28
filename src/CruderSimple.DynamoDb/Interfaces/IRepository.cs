using CruderSimple.Core.Interfaces;
using CruderSimple.DynamoDb.Entities;

namespace CruderSimple.DynamoDb.Interfaces;

public interface IRepository<TEntity> : Core.Interfaces.IRepository<TEntity>
    where TEntity : Entity
{
    DynamoDbQueryBuilder<TEntity> CreateQuery();
}