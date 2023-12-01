using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
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
    private IDictionary<Type, BatchWrite<object>> batchWrites = new Dictionary<Type, BatchWrite<object>>();

    public IRepositoryBase<T> Add(T entity)
    {
        var oldEntities = entity.Id is null ? new List<Entity>() : (FindById(entity.Id).GetAwaiter().GetResult())?.SegregateEntities() ?? new List<Entity>();
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

            var batchWrite = AddBatchWrite(entityToSave);
            batchWrite.AddPutItem(entityToSave);
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
            var batchWrite = AddBatchWrite(oldEntity.EntityRemoved);
            batchWrite.AddDeleteItem(oldEntity.EntityRemoved);
            
            // await amazonDynamoDb.DeleteItemAsync(new DeleteItemRequest(
            //     oldEntity.EntityRemoved.GetPropertyWithAttribute<DynamoDBTableAttribute>().TableName,
            //     new Dictionary<string, AttributeValue>
            //     {
            //         {nameof(Entity.Id),new AttributeValue(oldEntity.EntityRemoved.Id)} ,
            //         {nameof(Entity.CreatedAt),new AttributeValue(oldEntity.EntityRemoved.CreatedAt.ToString(CultureInfo.InvariantCulture))} 
            //     }));
        }

        return this;
    }

    public IRepositoryBase<T> Update(T entity) 
        => Add(entity);

    private BatchWrite<object> AddBatchWrite(Entity entityToSave)
    {
        BatchWrite<object> batchWrite = dynamoDbContext.CreateBatchWrite(GetType(), new DynamoDBOperationConfig());

        if (batchWrites.ContainsKey(entityToSave.GetType()))
            batchWrite = batchWrites[entityToSave.GetType()];
        else
            batchWrites.Add(entityToSave.GetType(), batchWrite);
        return batchWrite;
    }

    public virtual IRepositoryBase<T> Remove(T entity)
    {
        var batchWrite = AddBatchWrite(entity);
        batchWrite.AddDeleteItem(entity);
        return this;
    }

    public virtual async Task Save()
    {
        var multiTableBatchWrite = dynamoDbContext.CreateMultiTableBatchWrite(batchWrites.Values.ToArray());
        await multiTableBatchWrite.ExecuteAsync();
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

    public async Task<T> FindBy(string propertyName, string value) 
        => await CreateQuery()
            .ByGsi(propertyName, value)
            .ByInheritedType()
            .FindAsync();

    public virtual async Task<Pagination<T>> GetAll() 
        => await CreateQuery()
            .ByGsi(x => x.InheritedType, _entityType)
            .QueryAsync();
}
