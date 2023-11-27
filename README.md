# How to Use

## Configure
```c#
services
    .AddCruderSimpleServices(configuration, environment);

app
    .UseCruderSimpleServices();
```

## [Repository](./src/CruderSimple.DynamoDb/README.md)
## CRUD Endpoit Request

Just create a Record to be a Query, inhired to IEndpointQuery and create a Handler class with HttpHandlerBase<Query, Entity> and IRequestHandler<Query, IResult> implementations.  
All your endpoints will be registered with EndpointRequest attribute configurations.

### Get All

```C#
public static class GetAllUserRequest
{
    public record Query : GetAllRequest.Query;

    [EndpointRequest(
        method: EndpointMethod.GET, 
        version: "v1", 
        endpoint: "User",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : GetAllRequest.Handler<Query, Domain.Models.User, UserOutput, IUserRepository>(repository)
    {
    }
}
```

### Get By Id

```C#
public static class GetByIdUserRequest
{
    public record Query([FromRoute] string id) : GetByIdRequest.Query(id);

    [EndpointRequest(
        method: EndpointMethod.GET, 
        version: "v1", 
        endpoint: "User/{id}",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : GetByIdRequest.Handler<Query, Domain.Models.User, UserOutput, IUserRepository>(repository)
    {
    }
}
```

### Create

```C#
public static class CreateUserRequest
{
    public record Query([FromBody] UserInput payload) : CreateRequest.Query<UserInput>(payload);

    [EndpointRequest(
        method: EndpointMethod.POST, 
        version: "v1", 
        endpoint: "User",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : CreateRequest.Handler<Query, Domain.Models.User, UserInput, UserOutput, IUserRepository>(repository)
    {
    }
}
```

### Update

```C#
public static class UpdateUserRequest
{
    public record Query([FromRoute] string id, [FromBody] UserInput payload ) : UpdateRequest.Query<UserInput>(id, payload);

    [EndpointRequest(
        method: EndpointMethod.PUT, 
        version: "v1", 
        endpoint: "User/{id}",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : UpdateRequest.Handler<Query, Domain.Models.User, UserInput, UserOutput, IUserRepository>(repository)
    {
    }
}
```

### Delete

```C#
public static class DeleteUserRequest
{
    public record Query([FromRoute] string id) : DeleteRequest.Query(id);

    [EndpointRequest(
        method: EndpointMethod.DELETE, 
        version: "v1", 
        endpoint: "User/{id}",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : DeleteRequest.Handler<Query, Domain.Models.User, UserOutput, IUserRepository>(repository)
    {
    }
}
```

### Custom endpoint

```C#
public static class LoginEndpoint
{
    public record Query([FromBody] LoginViewModel payload) : IEndpointQuery;
    
    [EndpointRequest(
        method: EndpointMethod.DELETE, 
        version: "v1", 
        endpoint: "login",
        requireAuthorization: false)]
    public class Handler
        (IConfiguration configuration)
        : HttpHandlerBase<Query, User>, IRequestHandler<Query, IResult>
    {
        public override async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            (...)
            return Results.Ok();
        }
    }
}
```

### Override endpoint

```C#
public static class DeleteUserRequest
{
    public record Query([FromRoute] string id) : DeleteRequest.Query(id);

    [EndpointRequest(
        method: EndpointMethod.DELETE, 
        version: "v1", 
        endpoint: "User/{id}",
        requireAuthorization: true,
        roles: new []{ "YOUR_ROLES_ID" })]
    public class Handler
        (IUserRepository repository)
        : DeleteRequest.Handler<Query, Domain.Models.User, UserOutput, IUserRepository>(repository)
    {
        public override Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            (...)
            return Results.Ok();
        }
    }
}
```
