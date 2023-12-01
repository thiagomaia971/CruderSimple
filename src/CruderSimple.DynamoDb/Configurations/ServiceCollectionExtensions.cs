using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using CruderSimple.Core.Interfaces;
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

    private static IServiceCollection LocalhostConfig(IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDefaultAWSOptions(configuration.GetAWSOptions())
            .AddSingleton<IAmazonDynamoDB>(c => new AmazonDynamoDBClient(
                new BasicAWSCredentials("fake", "fake"),
                new AmazonDynamoDBConfig
                {
                    UseHttp = true,
                    ServiceURL = configuration["AWS:Dynamodb:ServicRepositoryBase             }))
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