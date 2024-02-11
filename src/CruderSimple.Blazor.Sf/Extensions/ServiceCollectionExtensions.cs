//using CruderSimple.Blazor.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;
using System.Net.Http.Headers;
using CruderSimple.Blazor.Sf.Interfaces.Services;
using CruderSimple.Blazor.Sf.Services;
using CruderSimple.Core.Entities;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;

namespace CruderSimple.Blazor.Sf.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCruderSimpleBlazorSf/*<TAuthorizeApi>*/(this IServiceCollection services, IConfiguration configuration)
        //where TAuthorizeApi : IAuthorizeApi
    {
        //CruderSimple.Blazor.Extensions.ServiceCollectionExtensions.AddCruderSimpleBlazor<TAuthorizeApi>(services);
        var syncfusionToken = configuration["SYNCFUSION_TOKEN"];
        SyncfusionLicenseProvider.RegisterLicense(syncfusionToken);
        services.AddScoped<SfDialogService>();
        services.AddSyncfusionBlazor((config) =>
        {

        });

        services.AddTransient<IRequestService, RequestService>();
        services.AddScoped(typeof(CruderLogger<>));
        services.AddCruderServices();
        return services;
    }

    private static IServiceCollection AddCruderServices(this IServiceCollection services)
    {
        var types = Core.Extensions.ServiceCollectionExtensions.GetTypes();
        var entityTypes = Core.Extensions.ServiceCollectionExtensions.GetByType<IEntity>().ToList();

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

    public static WebAssemblyHost UseCruderSimpleBlazor(this WebAssemblyHost app)
    {
        return app;
    }
    public static IServiceCollection AddOdontoManagementClient(this IServiceCollection services)
    {
        Action<HttpClient> configureClient = client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new("http://localhost:5228/");
            //client.BaseAddress = new("http://192.168.68.117:5228/");

        };
        services.AddHttpClient("OdontoManagement.Api", configureClient);
        services.AddHttpClient<RequestService>(configureClient);
        return services;
    }
    public static HttpClient CreateOdontoManagementClient(this IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateHttpClient("OdontoManagement.Api");
        return httpClient;
    }
    public static HttpClient CreateHttpClient(this IHttpClientFactory httpClientFactory, string name, string token = null)
    {
        var httpClient = httpClientFactory.CreateClient(name);
        if (!string.IsNullOrEmpty(token))
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return httpClient;
    }
}
