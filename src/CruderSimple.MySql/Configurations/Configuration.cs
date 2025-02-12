using System.Reflection;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;
using CruderSimple.MySql.Interfaces;
using CruderSimple.MySql.Repositories;
using Mapster;
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
        string assemblyStartsWithName,
        Type multiTenantRepositoryInterface,
        Type multiTenantRepositoryImplementation)
        where TMultiTenant : IEntity
    {
        TypeAdapterConfig.GlobalSettings.Default
            //.MaxDepth(3)
            .PreserveReference(true)
            .AvoidInlineMapping(true)
            ;
        LoadAssemblies();
        services
            .AddMediatR(typeof(Configuration))
            // .AddDynamodbMapper(configuration, environment)
            .AddRepositories<IEntity>(assemblyStartsWithName, typeof(IRepositoryBase<>), typeof(Repository<>))
            .AddRepositories<IEntity>(assemblyStartsWithName, typeof(IRepository<>), typeof(Repository<>))
            .AddRepositories<TMultiTenant>(assemblyStartsWithName, multiTenantRepositoryInterface, multiTenantRepositoryImplementation)
            .AddScoped<MultiTenantScoped>(_ => new MultiTenantScoped(typeof(TMultiTenant)));
        
        return services;
    }

    private static void LoadAssemblies()
    {
        string path = AppDomain.CurrentDomain.BaseDirectory;
        
        foreach (string dll in Directory.GetFiles(path, "*.dll"))
        {
            try
            {
                Assembly.LoadFrom(dll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {dll}: {ex.Message}");
            }
        }
    }
}