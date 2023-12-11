using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.MySql.Interfaces;
using CruderSimple.MySql.Repositories;
using MediatR;
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
            // .AddDynamodbMapper(configuration, environment)
            .AddRepositories(typeof(IRepository<>), typeof(Repository<>))
            .AddScoped<MultiTenantScoped>();
        
        return services;
    }
}