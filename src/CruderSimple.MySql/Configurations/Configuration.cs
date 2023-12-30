﻿using CruderSimple.Core.Entities;
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
    public static IServiceCollection AddCruderSimpleServices<TMultiTenant>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Type multiTenantRepositoryInterface,
        Type multiTenantRepositoryImplementation)
        where TMultiTenant : IEntity
    {
        services
            .AddMediatR(typeof(Configuration))
            // .AddDynamodbMapper(configuration, environment)
            .AddRepositories<IEntity>(typeof(IRepository<>), typeof(Repository<>))
            .AddRepositories<TMultiTenant>(multiTenantRepositoryInterface, multiTenantRepositoryImplementation)
            .AddScoped<MultiTenantScoped>(_ => new MultiTenantScoped(typeof(TMultiTenant)));
        
        return services;
    }
}