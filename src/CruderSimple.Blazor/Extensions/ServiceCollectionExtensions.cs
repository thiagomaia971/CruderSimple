using System;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using CruderSimple.Blazor.Components;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Blazor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCruderSimpleBlazor<TAuthorizeApi>(this IServiceCollection services)
        where TAuthorizeApi : IAuthorizeApi
    {
        services.AddBlazoredLocalStorage();
        services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            } )
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();

        services.AddScoped<IdentityAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
        services.AddScoped(typeof(IAuthorizeApi), typeof(TAuthorizeApi));
        services.AddTransient<IRequestService, RequestService>();
        var permissionsService = new PermissionService();
        services.AddSingleton<PermissionService>(permissionsService);
        services.AddSingleton<PageHistoryState>();
        services.AddScoped<DebounceService>();
        services.AddCruderServices();

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
        if (context.Resource is RouteData rd)
        {
            var route = (string) rd.PageType.CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(RouteAttribute))
                .ConstructorArguments[0]
                .Value;
            var routeSplited = route.Split("/");
            var routeEntity = string.IsNullOrEmpty(routeSplited[1]) ? "home" : routeSplited[1];
            var permission = $"{routeEntity.ToUpper()}:{permissionType.ToUpper()}";
            var permissions = context.User.Claims.FirstOrDefault(x => x.Type == "Permissions");
            bool allowed = permissions?.Value.Contains(permission) ?? false;
            if (permissionType == "WRITE")
                services.BuildServiceProvider().GetService<PermissionService>().CanWrite = allowed;
            else
                services.BuildServiceProvider().GetService<PermissionService>().CanRead = allowed;
            return allowed;
        }

        return false;
    }

    private static IServiceCollection AddCruderServices(this IServiceCollection services) 
    {
        var types = Core.Extensions.ServiceCollectionExtensions.GetTypes();
        var entityTypes = Core.Extensions.ServiceCollectionExtensions.GetByType<IEntity>().ToList();

        Console.WriteLine(string.Join(",", entityTypes.Select(x => x.Name)));

        foreach (var entityType in entityTypes)
        {
            var inputDto = types.FirstOrDefault(x => x.Name == $"{entityType.Name}Input");
            var outputDto = types.FirstOrDefault(x => x.Name == $"{entityType.Name}Output");
            if (inputDto is not null)
                AddCruderService(services, types, entityType, inputDto);
            if (outputDto is not null)
                AddCruderService(services, types, entityType, outputDto);
        }
        return services;
    }

    private static void AddCruderService(IServiceCollection services, IEnumerable<Type> types, Type? entityType, Type? inputDto)
    {
        var genericInterface = typeof(ICrudService<,>).MakeGenericType(entityType, inputDto);
        var genericImplementation = typeof(CrudService<,>).MakeGenericType(entityType, inputDto);
        services.AddScoped(genericInterface, genericImplementation);

        var @interface = types.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
        if (@interface is null)
            return;

        var implementation = types.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        services.AddScoped(@interface, implementation);
    }
}
