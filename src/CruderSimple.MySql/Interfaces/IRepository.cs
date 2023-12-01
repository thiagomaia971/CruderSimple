using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Entities;

namespace CruderSimple.MySql.Interfaces;

public interface IRepository<TEntity> : IRepositoryBase<TEntity>
    where TEntity : Entity
{
}