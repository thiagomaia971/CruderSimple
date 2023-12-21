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
    public static IServiceCollection AddCruderSimpleServices<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Type multiTenantRepositoryInterface,
        Type multiTenantRepositoryImplementation)
    where T : IEntity
    {
        services
            .AddMediatR(typeof(Configuration))
            // .AddDynamodbMapper(configuration, environment)
            .AddRepositories<IEntity>(typeof(IRepository<>), typeof(Repository<>))
            .AddRepositories<T>(multiTenantRepositoryInterface, multiTenantRepositoryImplementation)
            .AddScoped<MultiTenantScoped>();
        
        return services;
    }
}