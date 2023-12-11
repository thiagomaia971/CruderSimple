using System.Security.Claims;
using CruderSimple.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Filters;

public class MultiTenantActionFilter([FromServices] MultiTenantScoped multiTenant) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var claims = context.HttpContext.User.Identity as ClaimsIdentity;
        if (claims.Claims.Any(x => x.Type == "TenantId"))
        {
            var userId = claims.Claims.First(x => x.Type == "TenantId").Value;
            multiTenant.Id = userId;
        }
        return next(context);
    }
}