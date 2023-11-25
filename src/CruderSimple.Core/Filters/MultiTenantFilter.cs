using System.Security.Claims;
using CruderSimple.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Core.Filters;

public class MultiTenantActionFilter([FromServices] MultiTenantScoped multiTenant) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var claims = context.HttpContext.User.Identity as ClaimsIdentity;
        if (claims.Claims.Any(x => x.Type == "UserId"))
        {
            var userId = claims.Claims.First(x => x.Type == "UserId").Value;
            multiTenant.UserId = userId;
        }
        return next(context);
    }
}