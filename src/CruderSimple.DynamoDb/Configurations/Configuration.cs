using CruderSimple.Api.Extensions;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.DynamoDb.Interfaces;
using CruderSimple.DynamoDb.Repositories;
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
        string assemblyStartsWithName,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddMediatR(typeof(Configuration))
            .AddDynamodbMapper(configuration, environment)
            .AddRepositories<IEntity>(assemblyStartsWithName, typeof(IRepository<>), typeof(Repository<>))
            .AddScoped<MultiTenantScoped>();
        
        return services;
    }
}