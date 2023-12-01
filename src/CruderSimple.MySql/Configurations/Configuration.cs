using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Interfaces;
using CruderSimple.MySql.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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
            .AddRepositories(typeof(IRepository<>), typeof(Repository<>))
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