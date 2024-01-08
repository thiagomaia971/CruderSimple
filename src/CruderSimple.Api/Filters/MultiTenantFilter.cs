using System.Security.Claims;
using System.Text.Json;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CruderSimple.Api.Filters;

public class MultiTenantActionFilter([FromServices] MultiTenantScoped multiTenant) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var headers = context.HttpContext.Request.Headers;
        var claims = context.HttpContext.User.Identity as ClaimsIdentity;

        if (headers.Any(c => c.Key == "tenantid"))
            multiTenant.Id = headers["tenantid"];
        else if (claims.Claims.Any(x => x.Type == "TenantId"))
                multiTenant.Id = claims.Claims.First(x => x.Type == "TenantId").Value;
        
        if (headers.Any(c => c.Key == "userid"))
            multiTenant.UserId= headers["userid"];
        else if (claims.Claims.Any(x => x.Type == "UserId"))
            multiTenant.UserId = claims.Claims.First(x => x.Type == "UserId").Value;

        return next(context);
    }
}
public class PermissionAuthorizationActionFilter<TUser> : IEndpointFilter
    where TUser : IUser
{
    private readonly IRepositoryBase<TUser> _repository;
    private readonly IMemoryCache _memoryCache;
    private bool _disableCache = true;
    private int MillisecondsAbsoluteExpiration { get; set; } = 7200;

    public PermissionAuthorizationActionFilter(IRepositoryBase<TUser> repository, IMemoryCache memoryCache)
    {
        _repository = repository;
        _memoryCache = memoryCache;
    }
    
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userId =
            (context.HttpContext.User.Identity as ClaimsIdentity).Claims.FirstOrDefault(x => x.Type == "UserId").Value;
        TUser user;
        if (_disableCache || !_memoryCache.TryGetValue(userId, out user))
        {
            user = await _repository.FindById(userId);
            _memoryCache.Set(userId, user, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(MillisecondsAbsoluteExpiration)
            });
        }
        
        var routeEntity = context.HttpContext.Request.Path.Value.Split("/")[2];
        var permission = $"{routeEntity.ToUpper()}:{(context.HttpContext.Request.Method == "GET" ? "READ" : "WRITE")}";

        var allowed = user.GetPermissions().Any(x => permission.Contains(x));
        
        if (allowed)
            return await next(context);

        var result = Result.CreateError("Usuário não autorizado", 403, "Usuário não autorizado"); 
        return Results.Json(result, JsonSerializerOptions.Default, null, 403);
    }
}