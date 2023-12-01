using CruderSimple.Core.Interfaces;
using CruderSimple.DynamoDb.Entities;

namespace CruderSimple.DynamoDb.Interfaces;

public interface IDynamodbRepository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    DynamoDbQueryBuilder<TEntity> CreateQuery();
}