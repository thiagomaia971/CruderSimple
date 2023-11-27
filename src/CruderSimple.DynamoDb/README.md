# Create Entity

```C#
[DynamoDBTable("YourTable")]
public class User : Entity
{
    [DynamoDBProperty("Name")]
    public string Name { get; set; }

    [DynamoDBIgnore]
    [DynamoDbInner(typeof(UserRole))]
    public List<UserRole> Roles { get; set; }
}

[DynamoDBTable("YourTable")]
public class UserRole : Entity
{
    [DynamoDBProperty("RoleId")]
    public string RoleId { get; set; }
}
```

# Create Repository

Your repository are optional, but if are created, will be registered in ServiceCollections in startup time.

```C#
public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<string>> GetRoles(string userId);
}

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly IRepository<UserRole> _UserRoleRepository;

    public UserRepository(
        IDynamoDBContext dynamoDbContext,
        IAmazonDynamoDB amazonDynamoDb,
        MultiTenantScoped multiTenant,
        IRepository<UserRole> UserRoleRepository) 
        : base(dynamoDbContext, amazonDynamoDb, multiTenant)
    {
        _UserRoleRepository = UserRoleRepository;
    }
    
    public async Task<IEnumerable<string>> GetRoles(string UserId) 
        => (await _UserRoleRepository
            .CreateQuery()
            .ById(UserId)
            .ByEntityType()
            .QueryAsync())
            .Data
            .Select(x => x.RoleId);
    
}
```

# GetAll

Return all Users and yours SubEntities.

```c#
var users = await _repository.GetAll();
```

or

```c#
var users = await _repository
                .CreateQuery()
                .ByGsi("GSI-InheritedType", "InheritedType", "User")
                .QueryAsync();
```

# FindById

Return User and yours SubEntities.

```c#
var users = await _repository.FindById("02a01310-d8a0-48c0-a655-9755a91b4aff");
```

or

```c#
var users = await _repository
                .CreateQuery()
                .ById("02a01310-d8a0-48c0-a655-9755a91b4aff")
                .ByEntityType(DynamoDbOperator.BeginsWith)
                .QueryAsync();
```

# Create

```C#
var user = new User {
    Name = "Anyone",
    Roles = new List<UserRole> {
        new UserRole {
            RoleId = "ac337365-e690-4c75-9f05-e5ea75caa1e5"
        }
    }
};
await _repository.Save(user);
```

# Update

> Warning: to update an entity with this structure, you have to retrieve the Id (hash key) and CreatedAt (range key). If the CreatedAt value is a milisecond different, will be created a new row in the database.

```C#
var user = await _repository.GetById("02a01310-d8a0-48c0-a655-9755a91b4aff");
user.Name = "New name";
await _repository.Save(user);
```

# Delete

> Warning: to remove an entity with this structure, you have to retrieve the Id (hash key) and CreatedAt (range key). If the CreatedAt value is a milisecond different, will be not deleted.

```C#
var user = await _repository.GetById("02a01310-d8a0-48c0-a655-9755a91b4aff");
await _repository.Remove(user);
```