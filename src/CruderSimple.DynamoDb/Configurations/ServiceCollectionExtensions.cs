using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using CruderSimple.DynamoDb.Entities;
using CruderSimple.DynamoDb.Interfaces;
using CruderSimple.DynamoDb.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CruderSimple.DynamoDb.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamodbMapper(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            return LocalhostConfig(services, configuration);
        
        return services
            .AddDefaultAWSOptions(configuration.GetAWSOptions())
            .AddAWSService<IAmazonDynamoDB>()
            .AddTransient<IDynamoDBContext, DynamoDBContext>();
    }
    
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        params Type[] entityScanMarkers)
    {
        foreach (var marker in entityScanMarkers)
        {
            var entityTypes = marker.Assembly.ExportedTypes
                .Where(x => typeof(Entity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            
            foreach (var entityType in entityTypes)
            {
                var genericInterface = typeof(IRepository<>).MakeGenericType(entityType);
                var genericImplementation = typeof(Repository<>).MakeGenericType(entityType);
                services.AddScoped(genericInterface, genericImplementation);

                var @interface = marker.Assembly.ExportedTypes.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
                if (@interface is null)
                    continue;
                
                var implementation = marker.Assembly.ExportedTypes.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                services.AddScoped(@interface, implementation);
            }
        }
        
        return services;
    }

    private static IServiceCollection LocalhostConfig(IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDefaultAWSOptions(configuration.GetAWSOptions())
            .AddSingleton<IAmazonDynamoDB>(c => new AmazonDynamoDBClient(
                new BasicAWSCredentials("fake", "fake"),
                new AmazonDynamoDBConfig
                {
                    UseHttp = true,
                    ServiceURL = configuration["AWS:Dynamodb:ServiceUrl"]
                }))
            .AddSingleton<IDynamoDBContext>(provider =>
            {
                return new DynamoDBContext(
                    new AmazonDynamoDBClient(new BasicAWSCredentials("fake", "fake"),
                        new AmazonDynamoDBConfig
                        {
                            UseHttp = true,
                            ServiceURL = configuration["AWS:Dynamodb:ServiceUrl"]
                        }));
            });
    }
}