using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CruderSimple.DynamoDb.Configurations;

public static class Configuration
{
    public static IServiceCollection AddCruderSimpleServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        params Type[] entityScanMarkers)
    {
        services
            .AddMediatR(typeof(Configuration))
            .AddRequestDefinitions(entityScanMarkers)
            .AddDynamodbMapper(configuration, environment)
            .AddRepositories(entityScanMarkers)
            .AddScoped<MultiTenantScoped>();
        
        return services;
    }
    
    public static WebApplication UseCruderSimpleServices(
        this WebApplication app,
        params Type[] entityScanMarkers)
    {
        app.UseRequestDefinitions(entityScanMarkers);
        
        return app;
    }
}