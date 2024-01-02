using CruderSimple.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Core.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionsAuthorization(this IServiceCollection services)
    {
        var permissionsService = new PermissionService();
        services.AddSingleton<PermissionService>(permissionsService);
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("CanRead", policy =>
                policy.RequireAssertion(context => IsAuthorized(services, context, "READ"))
            );
    
            options.AddPolicy("CanWrite", policy =>
                policy.RequireAssertion(context => IsAuthorized(services, context, "WRITE"))
            );
        });
        return services;
    }
    
    private static bool IsAuthorized(
        IServiceCollection services, 
        AuthorizationHandlerContext context, 
        string permissionType)
    {
        var routeEntity = string.Empty;
        if (context.Resource is RouteData rd)
        {
            var route = (string) rd.PageType.CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(RouteAttribute))
                .ConstructorArguments[0]
                .Value;
            var routeSplited = route.Split("/");
            routeEntity = string.IsNullOrEmpty(routeSplited[1]) ? "home" : routeSplited[1];
        }
        else
        {
            dynamic defaultHttpContext = (dynamic)context.Resource;
            routeEntity = defaultHttpContext.Request.Path.Value.Split("/")[2];
        }

        if (string.IsNullOrEmpty(routeEntity))
            return false;
            
        var permission = $"{routeEntity.ToUpper()}:{permissionType.ToUpper()}";
        var permissions = context.User.Claims.FirstOrDefault(x => x.Type == "Permissions");
        bool allowed = permissions?.Value.Contains(permission) ?? false;
        if (permissionType == "WRITE")
            services.BuildServiceProvider().GetService<PermissionService>().CanWrite = allowed;
        else
            services.BuildServiceProvider().GetService<PermissionService>().CanRead = allowed;
        return allowed;
    }
}