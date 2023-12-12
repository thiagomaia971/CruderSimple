using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
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

        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("CanRead", policy =>
                policy.RequireAssertion(context =>
                {
                    if (context.Resource is RouteData rd)
                    {
                        var permission = $"{rd.PageType.Name.ToUpper()}:READ";
                        var permissions = context.User.Claims.FirstOrDefault(x => x.Type == "Permissions");
                        return permissions?.Value.Contains(permission) ?? false;
                    }

                    return false;
                })
            );
    
            options.AddPolicy("CanWrite", policy =>
                policy.RequireAssertion(context =>
                {
                    if (context.Resource is RouteData rd)
                    {
                        var permission = $"{rd.PageType.Name.ToUpper()}:WRITE";
                        var permissions = context.User.Claims.FirstOrDefault(x => x.Type == "Permissions");
                        return permissions?.Value.Contains(permission) ?? false;
                    }

                    return false;
                })
            );
        });
        
        return services;
    }
}