﻿using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CruderSimple.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static System.Net.WebRequestMethods;

namespace CruderSimple.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories<TEntity>(
        this IServiceCollection services,
        string assemblyStartsWithName,
        Type repositoryInterfaceType,
        Type repositoryImplementationType)
        where TEntity : IEntity
    {
        var types = GetTypes(assemblyStartsWithName);
        var entityTypes = GetByType<TEntity>(assemblyStartsWithName);

        foreach (var entityType in entityTypes)
        {
            var genericInterface = repositoryInterfaceType.MakeGenericType(entityType);
            var genericImplementation = repositoryImplementationType.MakeGenericType(entityType);

            var @interface = types.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
            if (@interface is null)
            {
                services.AddScoped(genericInterface, genericImplementation);
            }
            else
            {
                var implementation =
                    types.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                services.TryAddScoped(@interface, implementation);
                services.AddScoped(genericInterface, implementation);
            }
        }


        return services;
    }

    public static IEnumerable<Type> GetTypes(string assemblyStartsWithName, bool fromApi = true)
    {
        if (fromApi)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName.StartsWith("CruderSimple") || a.FullName.StartsWith(assemblyStartsWithName))
                .SelectMany(a => a.GetTypes());
        }

        try
        {
            return Assembly
                .GetEntryAssembly()
                .GetReferencedAssemblies()
                .Where(x => x.FullName.StartsWith("CruderSimple") || x.FullName.StartsWith(assemblyStartsWithName))
                .Select(Assembly.Load)
                .SelectMany(x => x.DefinedTypes)
                .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.DefinedTypes))
                .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes))
                .Distinct();
        }
        catch (Exception e)
        {
            throw;
        }

        return null;
    }

    public static IEnumerable<Type> GetByType<T>(string assemblyStartsWithName, bool fromApi = true)
    {
        return GetTypes(assemblyStartsWithName, fromApi)
            .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
    }
}

public class BootstrapInfo
{

    [JsonConstructor]
    public BootstrapInfo()
    {
        Resources = new BootstrapResourceInfo();
    }

    [JsonPropertyName("cacheBootResources")]
    public bool CacheBootResources { get; set; }

    [JsonPropertyName("config")] public List<string> Config { get; set; }

    [JsonPropertyName("debugBuild")] public bool DebugBuild { get; set; }

    [JsonPropertyName("entryAssembly")] public string EntryAssembly { get; set; }

    [JsonPropertyName("icuDataMode")] public long IcuDataMode { get; set; }

    [JsonPropertyName("linkerEnabled")] public bool LinkerEnabled { get; set; }

    [JsonPropertyName("resources")] public BootstrapResourceInfo Resources { get; set; }

    public static BootstrapInfo FromJson(string json) =>
        JsonSerializer.Deserialize<BootstrapInfo>(json, JsonSerializerOptions.Default.GetDefaultSerializerOptions());

    public static string ToJson(BootstrapInfo bootstrap) => JsonSerializer.Serialize(bootstrap, JsonSerializerOptions.Default.GetDefaultSerializerOptions());
}

public class BootstrapResourceInfo
{
    [JsonPropertyName("hash")] public string Hash { get; set; }

    [JsonPropertyName("assembly")] public IDictionary<string, string> Assembly { get; set; }
}