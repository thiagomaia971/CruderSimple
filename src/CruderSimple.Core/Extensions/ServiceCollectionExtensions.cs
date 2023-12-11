using System.Reflection;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CruderSimple.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        Type repositoryInterfaceType,
        Type repositoryImplementationType)
    {
        var types = Assembly
                        .GetEntryAssembly()
                        .GetReferencedAssemblies()
                        .Select(Assembly.Load)
                        .SelectMany(x => x.DefinedTypes)
                        .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes))
                        .Distinct();

        var entityTypes = types
            .Where(x => typeof(IEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        Console.WriteLine(string.Join(",", entityTypes.Select(x => x.Name)));

        foreach (var entityType in entityTypes)
        {
            var genericInterface = repositoryInterfaceType.MakeGenericType(entityType);
            var genericImplementation = repositoryImplementationType.MakeGenericType(entityType);
            services.AddScoped(genericInterface, genericImplementation);

            var @interface = types.FirstOrDefault(x => genericInterface.IsAssignableFrom(x) && x.IsInterface);
            if (@interface is null)
                continue;
            
            var implementation = types.FirstOrDefault(x => @interface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            services.AddScoped(@interface, implementation);
        }
        
        return services;
    }
}