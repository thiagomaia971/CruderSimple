using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CruderSimple.Core.Entities;
using CruderSimple.DynamoDb.Entities;
using CruderSimple.DynamoDb.Extensions;
using CruderSimple.DynamoDb.Interfaces;

namespace CruderSimple.DynamoDb.Repositories;

public class Repository<T>(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB amazonDynamoDb,
        MultiTenantScoped multiTenant)
    : IRepository<T>
    where T : Entity
{
    protected readonly IDynamoDBContext _dynamoDbContext = dynamoDbContext;
    protected string _entityType = typeof(T).FullName;

    public virtual async Task<T> Save(T entity)
    {
        var oldEntities = entity.Id is null ? new List<Entity>() : (await FindById(entity.Id))?.SegregateEntities() ?? new List<Entity>();
        var entitiesToSave = entity.SegregateEntities();
        
        foreach (var entityToSave in entitiesToSave)
        {
            entityToSave.UpdatedAt = DateTime.Now.ToString("O");
            if (string.IsNullOrEmpty(entityToSave.Id))
                entityToSave.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(entityToSave.CreatedAt))
                entityToSave.CreatedAt = DateTime.Now.ToString("O"); 
            if (entityToSave is TenantEntity m)
                m.UserId = multiTenant.UserId;
            
            await amazonDynamoDb.PutItemAsync(
                new PutItemRequest(
                    entityToSave.GetPropertyWithAttribute<DynamoDBTableAttribute>().TableName, 
                    entityToSave.AttributeValues()));
        }
        var entitiesToDelete =(
            from xinput in oldEntities
            join xxinput in entitiesToSave on xinput.Id equals xxinput.Id into joined
            from xentity  in joined.DefaultIfEmpty()
            select new
            {
                EntityRemoved = xinput,
                EntityEmpty = xentity
            }).Where(x => x.EntityEmpty is null);
        foreach (var oldEntity in entitiesToDelete)
        {
            await amazonDynamoDb.DeleteItemAsync(new DeleteItemRequest(
                oldEntity.EntityRemoved.GetPropertyWithAttribute<DynamoDBTableAttribute>().TableName,
                new Dictionary<string, AttributeValue>
                {
                    {nameof(Entity.Id),new AttributeValue(oldEntity.EntityRemoved.Id)} ,
                    {nameof(Entity.CreatedAt),new AttributeValue(oldEntity.EntityRemoved.CreatedAt.ToString(CultureInfo.InvariantCulture))} 
                }));
        }

        return entity;
    }

    public DynamoDbQueryBuilder<T> CreateQuery()
    {
        var multiTenantUserId = typeof(TenantEntity).IsAssignableFrom(typeof(T)) ? multiTenant.UserId : null;
        return DynamoDbQueryBuilder<T>
            .CreateQuery(_dynamoDbContext, multiTenantUserId);
    }

    public virtual async Task<T> FindById(string id) 
        => await CreateQuery()
            .ById(id)
            .FindAsync();

    public virtual async Task<Pagination<T>> GetAll() 
        => await CreateQuery()
            .ByGsi(x => x.InheritedType, _entityType)
            .QueryAsync();

    public virtual async Task Remove(T entity) 
        => await _dynamoDbContext.DeleteAsync(entity);
}
