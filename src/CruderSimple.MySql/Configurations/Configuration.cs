using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CruderSimple.MySql.Configurations;

public static class Configuration
{
    public static IServiceCollection AddCruderSimpleServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddMediatR(typeof(Configuration))
            .AddRequestDefinitions()
            // .AddDynamodbMapper(configuration, environment)
            .AddRepositories(typeof(Repository<>))
            .AddScoped<MultiTenantScoped>();
        
        return services;
    }
    
    public static WebApplication UseCruderSimpleServices(
        this WebApplication app)
    {
        app.UseRequestDefinitions();
        
        return app;
    }
}