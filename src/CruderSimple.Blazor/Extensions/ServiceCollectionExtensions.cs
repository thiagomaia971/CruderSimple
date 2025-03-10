﻿using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazorise.LoadingIndicator;
using CruderSimple.Blazor.Components;
using CruderSimple.Blazor.DelegationHandlers;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Blazor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCruderSimpleBlazor<TAuthorizeApi>(this IServiceCollection services, string assemblyStartsWithName)
        where TAuthorizeApi : IAuthorizeApi
    {
        services.AddBlazoredLocalStorage();
        services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            } )
            .AddBootstrapProviders()
            .AddFontAwesomeIcons()
            .AddLoadingIndicator();

        services.AddScoped<IdentityAuthenticationStateProvider>();
        services.AddScoped<IIdentityAuthenticationStateProvider, IdentityAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
        services.AddScoped<AuthHeaderHandler>();
        services.AddScoped(typeof(IAuthorizeApi), typeof(TAuthorizeApi));
        services.AddTransient<IRequestService, RequestService>();
        var permissionsService = new PermissionService();
        services.AddSingleton<PermissionService>(permissionsService);
        services.AddSingleton<PageHistoryState>();
        services.AddScoped<DebounceService>();
        services.AddSingleton<BrowserService>();
        //var pageParameter = new PageParameter();
        //services.AddSingleton(pageParameter);
        services.AddCruderServices(assemblyStartsWithName);
        services.AddPermissionsAuthorization();

        return services;
    }

    private static IServiceCollection AddCruderServices(this IServiceCollection services, string assemblyStartsWithName) 
    {
        var types = Core.Extensions.ServiceCollectionExtensions.GetTypes(assemblyStartsWithName, false);
        var entityTypes = Core.Extensions.ServiceCollectionExtensions.GetByType<IEntity>(assemblyStartsWithName, false).ToList();

        foreach (var entityType in entityTypes)
        {
            var inputDto = types.FirstOrDefault(x => x.Name == $"{entityType.Name}Input");
            var outputDto = types.FirstOrDefault(x => x.Name == $"{entityType.Name}Output");
            var dto = types.FirstOrDefault(x => x.Name == $"{entityType.Name}Dto");
            if (inputDto is not null)
                AddCruderService(services, types, entityType, inputDto);
            if (outputDto is not null)
                AddCruderService(services, types, entityType, outputDto);
            if (dto is not null)
                AddCruderService(services, types, entityType, dto);
        }
        return services;
    }

    private static void AddCruderService(IServiceCollection services, IEnumerable<Type> types, Type? entityType, Type? inputDto)
    {
        var genericInterface = typeof(ICrudService<,>).MakeGenericType(entityType, inputDto);
        var genericImplementation = typeof(CrudService<,>).MakeGenericType(entityType, inputDto);
        services.AddTransient(genericInterface, genericImplementation);

        var @interface = types.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
        if (@interface is null)
            return;

        var implementation = types.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        services.AddTransient(@interface, implementation);
    }
}
